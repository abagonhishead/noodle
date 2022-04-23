namespace Jossellware.Noodle.Web.Api
{
    using System.Reflection;
    using System.Text.RegularExpressions;
    using FluentValidation.AspNetCore;
    using Jossellware.Noodle.Shared.CQRS.Handlers.MagicPacket;
    using Jossellware.Shared.AspNetCore.Extensions;
    using Jossellware.Shared.AspNetCore.Extensions.DependencyInjection.Mapping;
    using Jossellware.Shared.AspNetCore.Options;
    using Jossellware.Shared.Extensions;
    using Jossellware.Shared.Interop.Enums;
    using Jossellware.Shared.Interop.Helpers;
    using Jossellware.Shared.Networking.Udp.Send;
    using MediatR;
    using MicroElements.Swashbuckle.FluentValidation.AspNetCore;
    using Microsoft.AspNetCore.HttpOverrides;
    using Microsoft.AspNetCore.Server.Kestrel.Core;
    using Microsoft.Extensions.Configuration.CommandLine;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.OpenApi.Models;

    public class Bootstrap
	{
		private static readonly Platform platform = InteropHelper.GetOSPlatform();

		private static ILogger<Bootstrap> logger;
        private static IConfigurationProvider commandLineArgs;
		private static LogLevel bootstrapLogLevel = LogLevel.Trace;
		private static bool unixSocketCreated;
        private static UnixLifetimeOptions? unixOptions;
        private static bool isContainer;

		public static async Task<int> Main(string[] args)
		{
            AppDomain.CurrentDomain.ProcessExit += Bootstrap.OnProcessExit;

            Console.WriteLine($"Bootstrapping from {typeof(Bootstrap).FullName} - platform is {platform}");
            commandLineArgs = new CommandLineConfigurationProvider(args);
            if (commandLineArgs.TryGet("BootstrapLogLevel", out var value) &&
                Enum.TryParse<LogLevel>(value, true, out var level))
            {
                bootstrapLogLevel = level;
            }

			Console.WriteLine($"Building bootstrap logger...");
			logger = Bootstrap.BuildBootstrapLogger(bootstrapLogLevel);

            var returnCode = 1;
			using (logger.BeginScope("Bootstrap:{BootstrapperName}", typeof(Bootstrap).FullName))
			{
				logger.LogInformation("Bootstrap logger created successfully");
				logger.LogInformation("Bootstrapping from {BootstrapperClass} - platform is {Platform}", typeof(Bootstrap).FullName, platform);
                if (args?.Any() == true)
                {
                    logger.LogTrace("CommandLine args: {Arguments}", args);
                }

                try
				{
					logger.LogInformation("Building host...");
					await using (var app = Bootstrap.BuildWebApplication(args))
					using (var cts = new CancellationTokenSource())
					{
						/* ctrl+c doesn't appear to be respected properly on Linux/docker at the moment, so register a 
						 * cancel keypress event handler too */
						Bootstrap.RegisterCancelKeypressHandler(cts);
						logger.LogInformation("Starting host...");
						await app.RunAsync(cts.Token);
					}

					returnCode = 0;
				}
				catch (Exception ex)
				{
					logger.LogCritical(ex, "Caught unhandled exception");
					returnCode = ex.HResult == 0 ? 1 : ex.HResult;
					throw;
				}
				finally
				{
					if (unixSocketCreated)
					{
						Bootstrap.DoUnixSocketCleanup(unixOptions.ManagedSocketPath);
					}

					logger.LogInformation("Terminating with return code {ReturnCode}", returnCode);
				}
			}


			return returnCode;
		}

		private static WebApplication? BuildWebApplication(string[] args)
		{
			using (logger.BeginMethodScope<Bootstrap>(nameof(BuildWebApplication)))
			{
				var builder = WebApplication.CreateBuilder(args);

                unixOptions = builder.Configuration.BindSection<UnixLifetimeOptions>("UnixLifetime");
                isContainer = builder.Configuration.GetBoolValue("DOTNET_RUNNING_IN_CONTAINER");

				Bootstrap.ConfigureHost(builder.Host, builder.Environment, args);
                Bootstrap.ConfigureServices(builder.Services, builder.Environment);
				Bootstrap.ConfigureWebHost(builder.WebHost, builder.Environment);
				Bootstrap.ConfigureLogging(builder.Logging);
				return Bootstrap.BuildPipeline(builder);
			}
		}

		#region Bootstrap config/setup methods
		private static void ConfigureServices(IServiceCollection services, IHostEnvironment environment)
		{
			using (logger.BeginMethodScope<Bootstrap>(nameof(Bootstrap.ConfigureServices)))
			{
                logger.LogDebug("Registering controllers...");
				services.AddControllers();
                services.AddRouting(x =>
                {
                    x.LowercaseUrls = true;
                });

				logger.LogDebug("Registering FluentValidation...");
				services.AddFluentValidation(x =>
				{
					x.AutomaticValidationEnabled = true;
					x.DisableDataAnnotationsValidation = true;
					x.RegisterValidatorsFromAssembly(Assembly.GetExecutingAssembly());
				});

				if (!environment.IsProduction())
				{
					// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
					logger.LogDebug("Nonprod environment -- registering Swagger/OpenAPI...");
					services.AddEndpointsApiExplorer();
					services.AddSwaggerGen(x =>
					{
						x.SwaggerDoc("v1", new OpenApiInfo
						{
							Version = "v1",
							Title = "MagicPacket API",
							Contact = new OpenApiContact
							{
								Email = "russell@jossell.com",
								Name = "Russell Webster, Jossellware",
								Url = new Uri("https://jossell.dev/")
							},
							Description = "A REST API for sending Wake-on-LAN 'magic' packets on a local network",
						});
						x.DescribeAllParametersInCamelCase();
						x.IgnoreObsoleteActions();
						x.IgnoreObsoleteProperties();
						x.IncludeXmlComments(Regex.Replace(Assembly.GetExecutingAssembly().Location, "\\.dll$", ".xml", RegexOptions.IgnoreCase));
					});

					logger.LogDebug("Registering FluentValidation rules with Swagger/OpenAPI...");
					services.AddFluentValidationRulesToSwagger(
						configureRegistration: x =>
						{
						/* These are all default values, they're just here so I don't forget about them */
							x.RegisterFluentValidationRules = true;
							x.RegisterJsonSerializerOptions = true;
							x.RegisterSystemTextJsonNameResolver = true;
							x.ServiceLifetime = ServiceLifetime.Transient;
						});
				}

				logger.LogDebug("Registering MediatR & handlers...");
				services.AddMediatR(Assembly.GetExecutingAssembly(), Assembly.GetAssembly(typeof(MagicPacketRequestHandler)));

				logger.LogDebug("Registering mappings...");
				services.AddClassMapProvider();
				services.AddAllClassMaps(Assembly.GetExecutingAssembly());

				logger.LogDebug("Registering other services...");
                services.AddScoped<IUdpTransmitter, UdpTransmitter>(x => new UdpTransmitter(System.Net.Sockets.AddressFamily.InterNetwork, true));
                services.AddHttpContextAccessor();
			}
		}

		private static void ConfigureHost(IHostBuilder builder, IHostEnvironment environment, string[] args)
		{
			using (logger.BeginMethodScope<Bootstrap>(nameof(Bootstrap.ConfigureHost)))
			{
                builder.UseSystemd();
				builder.ConfigureAppConfiguration((context, configBuilder) =>
				{
					using (logger.BeginMethodScope<IHostBuilder>(nameof(IHostBuilder.ConfigureAppConfiguration)))
					{
						if (environment.IsDevelopment())
						{
							logger.LogDebug("Dev environment -- adding user secrets...");
							configBuilder.AddUserSecrets<Bootstrap>();
						}

                        configBuilder.AddCommandLine(args);
					}
				});
			}
		}

		private static void ConfigureWebHost(IWebHostBuilder builder, IHostEnvironment environment)
		{
			using (logger.BeginMethodScope<Bootstrap>(nameof(Bootstrap.ConfigureWebHost)))
			{
                
				builder.UseKestrel((ctx, opts) =>
				{
					using (logger.BeginMethodScope<IWebHostBuilder>(nameof(WebHostBuilderKestrelExtensions.UseKestrel)))
					{
						if (platform == Platform.Linux)
						{
                            unixOptions = unixOptions ?? new UnixLifetimeOptions();

                            if (unixOptions.UseSystemdSocket == false)
                            {
                                if (string.IsNullOrWhiteSpace(unixOptions.ManagedSocketPath))
                                {
                                    if (!environment.IsDevelopment())
                                    {
                                        logger.LogWarning("Configured to bind Kestrel directly to a TCP interface. Make sure we're behind a reverse proxy!");
                                    }
                                }
                                else
                                {

                                    logger.LogInformation("Managing our own UNIX socket");
                                    logger.LogInformation("Using managed socket at path: {UnixSocketPath}", unixOptions.ManagedSocketPath);

                                    opts.ListenUnixSocket(unixOptions.ManagedSocketPath, x =>
                                    {
                                        x.Protocols = HttpProtocols.Http1AndHttp2;
                                        if (environment.IsDevelopment())
                                        {
                                            x.UseConnectionLogging();
                                        }
                                    });
                                    unixSocketCreated = true;
                                    Bootstrap.SetUnixSocketPermissions(unixOptions.ManagedSocketPath);
                                }
                            }
                            else
                            {
                                logger.LogInformation("Using systemd socket activation");
                                opts.UseSystemd(x =>
                                {
                                    logger.LogDebug("systemd delegate was called");
                                    x.Protocols = HttpProtocols.Http1AndHttp2;
                                    if (environment.IsDevelopment())
                                    {
                                        x.UseConnectionLogging();
                                    }
                                });
                            }
						}

						opts.AddServerHeader = false;
					}
				});
			}
		}

		private static WebApplication? BuildPipeline(WebApplicationBuilder builder)
		{
			using (logger.BeginMethodScope<Bootstrap>(nameof(Bootstrap.BuildPipeline)))
			{
				logger.LogDebug("Building {WebAppTypeName} instance...", typeof(WebApplication).FullName);
				var application = builder.Build();

				logger.LogDebug("Configuring HTTP request pipeline/middleware...");

                application.UseForwardedHeaders(new ForwardedHeadersOptions
                {
                    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
                });

				if (application.Environment.IsDevelopment() && platform != Platform.Linux)
				{
					logger.LogDebug("Using HTTPS redirection...");
					application.UseHttpsRedirection();
				}

				logger.LogDebug("Using authorisation...");
				application.UseAuthorization();

				logger.LogDebug("Using endpoint routing & mapping controllers...");
                application.UseRouting();
                application.UseEndpoints(x =>
                {
                    x.MapControllers();
                });

                if (!application.Environment.IsProduction())
                {
                    logger.LogInformation("Nonprod environment -- using Swagger/OpenAPI...");
                    application.UseSwagger();
                    application.UseSwaggerUI();
                }

                logger.LogDebug("Build method finished, returning control to caller");

				return application;
			}
		}

		private static void ConfigureLogging(ILoggingBuilder builder)
		{
			builder.Configure(cfg => cfg.ActivityTrackingOptions = ActivityTrackingOptions.None);
			if (platform == Platform.Linux)
			{
				builder.AddSystemdConsole(opts =>
				{
					opts.IncludeScopes = true;
					opts.UseUtcTimestamp = true;
					opts.TimestampFormat = "[dd/MM/yyyy HH:mm:ss] ";
				});
			}
			else
			{
				builder.AddSimpleConsole(opts =>
				{
					opts.IncludeScopes = true;
					opts.SingleLine = false;
					opts.TimestampFormat = "[dd/MM/yyyy HH:mm:ss] ";
				});
			}
		}
		#endregion

		private static void DoUnixSocketCleanup(string path)
		{
			using (logger.BeginMethodScope<Bootstrap>(nameof(Bootstrap.DoUnixSocketCleanup)))
			{
                // TODO: this is old -- need to check if it is still an issue?
				// Kestrel leaves the UNIX socket in-place (https://github.com/aspnet/KestrelHttpServer/issues/419)
				// If we're on *nix and the file exists, try to remove it
				if (File.Exists(path))
				{
					logger.Log(LogLevel.Information, "Trying to clean up UNIX socket at {path}", path);
					try
					{
						File.Delete(path);
					}
					catch (Exception ex)
					{
						logger.Log(LogLevel.Error, ex, "Couldn't delete UNIX socket at {path}", path);
						throw;
					}
				}
			}
		}

		/// <summary>
		/// Fire and forget task to set socket permissions to 0770 octal. This is because Kestrel sets them
		/// to 0755 without offering a config option - making the socket world readable is not necessarily
		/// a good thing.
		/// <remarks>
        /// Note that this is only needed if you're not using systemd socket activation. Systemd socket activation is always preferable unless you're using container orchestration.
		/// This uses a P/Invoke method in the shared project that is mostly ripped from here:
		/// https://stackoverflow.com/questions/60272453/set-kestrel-unix-socket-file-permissions-in-asp-net-core
		/// </remarks>
		/// </summary>
		private static void SetUnixSocketPermissions(string path)
		{
			Task.Run(() =>
			{
				using (logger.BeginMethodScope<Bootstrap>(nameof(Bootstrap.SetUnixSocketPermissions)))
				{
					logger.LogDebug("Background task waiting to set UNIX socket permissions...");
					Thread.Sleep(TimeSpan.FromSeconds(5));
					var permissions = UnixPermission.UserExec | UnixPermission.UserRead | UnixPermission.UserWrite | UnixPermission.GroupExec | UnixPermission.GroupRead | UnixPermission.GroupWrite;
					try
					{
						logger.LogDebug("Trying to set UNIX permissions to {Permissions} for path {Path}", permissions, path);
						UnixHelper.SetFilePermissions(path, permissions);
						logger.LogDebug("Setting UNIX socket permissions succeeded");
					}
					catch (Exception ex)
					{
						logger.LogError(ex, "Couldn't set UNIX permissions on socket!");
						throw;
					}
				}
			});
		}

		private static ILogger<Bootstrap> BuildBootstrapLogger(LogLevel level)
		{
			return LoggerFactory.Create(x =>
			{
				Bootstrap.ConfigureLogging(x);
                x.SetMinimumLevel(level);
            }).CreateLogger<Bootstrap>();
		}

		private static void RegisterCancelKeypressHandler(CancellationTokenSource cts)
		{
			using (logger.BeginMethodScope<Bootstrap>(nameof(Bootstrap.RegisterCancelKeypressHandler)))
			{
				logger.LogDebug("Registering cancel keypress event handler...");
				Console.TreatControlCAsInput = false;
				Console.CancelKeyPress += (s, e) => Bootstrap.ConsoleCancelKeypressHandler(s, e, cts);
			}
		}

		private static void ConsoleCancelKeypressHandler(object sender, ConsoleCancelEventArgs e, CancellationTokenSource cts)
		{
			using (logger.BeginMethodScope<Bootstrap>(nameof(Bootstrap.ConsoleCancelKeypressHandler)))
			{
				logger.LogInformation("Cancel keypress handler called");
				if (cts?.IsCancellationRequested == false)
				{
					logger.LogDebug("Caught cancel keypress -- cancelling CancellationToken...");
					if (!cts.IsCancellationRequested)
					{
						cts.Cancel();
					}
				}
				else
				{
					logger.LogDebug("CancellationTokenSource was null or already cancelled -- nothing to do");
				}
			}
		}

        private static void OnProcessExit(object? sender, EventArgs? e)
        {
            using (logger.BeginMethodScope<Bootstrap>(nameof(Bootstrap.OnProcessExit)))
            {
                logger.LogTrace("Process exiting");
            }
        }
	}
}
