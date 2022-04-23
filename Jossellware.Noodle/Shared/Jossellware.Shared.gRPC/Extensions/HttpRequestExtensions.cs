namespace Jossellware.Shared.gRPC.Extensions
{
	using Jossellware.Shared.gRPC.Helpers;
	using Microsoft.AspNetCore.Http;

	public static class HttpRequestExtensions
	{
		public static bool IsGrpcRequest(this HttpRequest request)
		{
			return ProtocolHelper.IsGrpcRequest(request);
		}
	}
}
