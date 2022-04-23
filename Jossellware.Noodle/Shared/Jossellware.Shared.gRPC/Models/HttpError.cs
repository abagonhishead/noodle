namespace Jossellware.Shared.gRPC.Models
{
	using System;
	using System.Collections.Generic;
	using System.Net;
	using System.Text.Json.Serialization;

	public class HttpError
	{
		[JsonPropertyName("statusCode")]
		public int StatusCode { get; set; }

		[JsonPropertyName("statusDescription")]
		public string StatusDescription { get; set; }

		[JsonPropertyName("information")]
		public IDictionary<string, object> Information { get; set; }

		[JsonPropertyName("traceId")]
		public string TraceId { get; set; }

		[JsonConstructor]
		public HttpError()
		{
		}

		public HttpError(HttpStatusCode statusCode)
		{
			if (!Enum.IsDefined(typeof(HttpStatusCode), (int)statusCode))
			{
				throw new ArgumentOutOfRangeException(nameof(statusCode), $"Not a defined {nameof(HttpStatusCode)} value: {(int)statusCode}");
			}

			this.StatusCode = (int)statusCode;
			this.StatusDescription = statusCode.ToString();
		}
	}
}
