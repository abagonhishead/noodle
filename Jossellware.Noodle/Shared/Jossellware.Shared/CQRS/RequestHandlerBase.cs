namespace Jossellware.Shared.CQRS
{
	using System.Threading;
	using System.Threading.Tasks;
	using MediatR;
	using Microsoft.Extensions.Logging;

	public abstract class RequestHandlerBase<TContext, TResult> : IRequestHandler<TContext, TResult>
		where TContext : IRequest<TResult>
	{
		protected ILogger<RequestHandlerBase<TContext, TResult>> Logger { get; }

		protected RequestHandlerBase(ILogger<RequestHandlerBase<TContext, TResult>> logger)
		{
			this.Logger = logger;
		}

		public Task<TResult> Handle(TContext context, CancellationToken cancellationToken)
		{
			using (this.Logger.BeginScope<TContext>(context))
			{
				return this.HandleImplementationAsync(context, cancellationToken);
			}
		}

		protected abstract Task<TResult> HandleImplementationAsync(TContext context, CancellationToken cancellationToken);
	}

	public abstract class RequestHandlerBase<TContext> : RequestHandlerBase<TContext, Unit>
		where TContext : IRequest<Unit>
	{
		protected RequestHandlerBase(ILogger<RequestHandlerBase<TContext, Unit>> logger) 
			: base(logger)
		{
		}
	}
}
