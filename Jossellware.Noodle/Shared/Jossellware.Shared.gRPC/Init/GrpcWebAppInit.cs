namespace Jossellware.Shared.gRPC.Init
{
	using System;
	using System.IO.Compression;
	using System.Threading.Tasks;
	using Grpc.Net.Compression;
	using Jossellware.Shared.AspNetCore.Init;
	using Jossellware.Shared.gRPC.Middleware;
	using Microsoft.AspNetCore.Builder;
	using Microsoft.Extensions.DependencyInjection;
	using Microsoft.Extensions.Hosting;
	using Microsoft.AspNetCore.Routing;

	public class GrpcWebAppInit : WebAppInit
	{
		public GrpcWebAppInit(string[] args) 
			: base(args)
		{
		}

		public GrpcWebAppInit(TimeSpan stopTimeout, string[] args) 
			: base(stopTimeout, args)
		{
		}

		public GrpcWebAppInit(int stopTimeoutMs, string[] args) 
			: base(stopTimeoutMs, args)
		{
		}

		protected override Task DisposeAsyncImpl()
		{
			return base.DisposeAsyncImpl();
		}

		protected override void PreBuild(IServiceCollection services)
		{
			services.AddGrpc(x =>
			{
				x.CompressionProviders.Add(new GzipCompressionProvider(CompressionLevel.Fastest));
				x.EnableDetailedErrors = this.Environment.IsDevelopment();
				x.IgnoreUnknownServices = true;
				x.ResponseCompressionLevel = CompressionLevel.Fastest;
			});

			base.PreBuild(services);
		}

		protected override void ConfigureImplementation(IApplicationBuilder builder)
		{
			builder.UseHttpsRedirection();
			builder.UseMiddleware<UnsupportedRequestMiddleware>();
		}

		protected override void ConfigureRoutingImplementation(IEndpointRouteBuilder builder)
		{
			this.MapGrpcServices(builder);
		}

		protected virtual void MapGrpcServices(IEndpointRouteBuilder builder)
		{
		}
	}
}
