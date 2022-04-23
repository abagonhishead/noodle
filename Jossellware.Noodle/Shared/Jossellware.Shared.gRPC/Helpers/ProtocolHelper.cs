namespace Jossellware.Shared.gRPC.Helpers
{
	using System;
	using Microsoft.AspNetCore.Http;

	public static class ProtocolHelper
	{
		public static bool IsGrpcRequest(HttpRequest request)
		{
			return !string.IsNullOrWhiteSpace(request?.ContentType) &&
				(request.ContentType.StartsWith(ProtocolConstants.ContentType.Grpc, StringComparison.Ordinal) ||
				string.Equals(request.ContentType, ProtocolConstants.ContentType.Grpc, StringComparison.Ordinal) ||
				string.Equals(request.ContentType, ProtocolConstants.ContentType.GrpcWeb, StringComparison.Ordinal) ||
				string.Equals(request.ContentType, ProtocolConstants.ContentType.GrpcWebText, StringComparison.Ordinal));
		}
	}
}
