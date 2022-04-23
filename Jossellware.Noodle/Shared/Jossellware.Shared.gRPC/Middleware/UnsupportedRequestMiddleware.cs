namespace Jossellware.Shared.gRPC.Middleware
{
	using System;
	using System.Collections.Generic;
	using System.Net;
	using System.Threading;
	using System.Threading.Tasks;
	using Jossellware.Shared.Extensions;
	using Jossellware.Shared.gRPC.Extensions;
	using Jossellware.Shared.gRPC.Models;
	using Microsoft.AspNetCore.Http;
	using Microsoft.Extensions.Logging;
	using Microsoft.Extensions.Primitives;

	public class UnsupportedRequestMiddleware
	{
		private readonly RequestDelegate next;
		private readonly ILogger<UnsupportedRequestMiddleware> logger;

		public UnsupportedRequestMiddleware(RequestDelegate next, ILogger<UnsupportedRequestMiddleware> logger)
		{
			logger.ThrowIfNull(nameof(logger));

			this.next = next;
			this.logger = logger;
		}

		public async Task InvokeAsync(HttpContext context)
		{
			if (!context.Request.IsGrpcRequest())
			{
                this.logger.LogInformation("Intercepting non-gRPC request");
				var error = UnsupportedRequestMiddleware.BuildNonGrpcRequestErrorModel(context.Request, context.TraceIdentifier);
				context.Response.StatusCode = (int)error.StatusCode;
				try
				{
					using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15)))
					{
						await context.Response.WriteAsJsonAsync<HttpError>(error, cts.Token);
					}
				}
				catch (Exception ex)
				{
                    this.logger.LogError(ex, "Caught exception trying to write non-gRPC request error");
					throw;
				}
				finally
				{
					await context.Response.CompleteAsync();
				}
			}
			else
			{
				await this.next(context);
			}
		}

		public static HttpError BuildNonGrpcRequestErrorModel(HttpRequest request, string traceId)
		{
			const HttpStatusCode statusCode = HttpStatusCode.BadRequest;

			var headers = new Dictionary<string, object>();

			if (request.Headers.TryGetValue("Content-Type", out StringValues ctVal) &&
				!string.IsNullOrWhiteSpace(ctVal.ToString()))
			{
				headers.Add("Content-Type", ctVal.ToString());
			}

			if (request.Headers.TryGetValue("Accept", out StringValues accVal) &&
				!string.IsNullOrWhiteSpace(accVal.ToString()))
			{
				headers.Add("Accept", accVal.ToString());
			}

			var info = new Dictionary<string, object>
			{
				{ "detail", "This endpoint does not support requests of this type." },
				{ "request-headers", headers }
			};

			return new HttpError(statusCode)
			{
				TraceId = traceId,
				Information = info,
			};
		}
	}
}
