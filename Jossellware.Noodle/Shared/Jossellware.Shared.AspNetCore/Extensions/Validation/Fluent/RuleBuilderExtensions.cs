namespace Jossellware.Shared.AspNetCore.Extensions.Validation.Fluent
{
	using System.Collections;
	using System.Linq;
	using System.Net;
	using System.Net.Sockets;
	using FluentValidation;

	public static class RuleBuilderExtensions
	{
		public static IRuleBuilderOptions<T, string> IpAddress<T>(this IRuleBuilderInitial<T, string> ruleBuilder, AddressFamily addressType = AddressFamily.InterNetwork)
		{
			return ruleBuilder
				.NotNull()
				.NotEmpty()
				.Must(x => IPAddress.TryParse(x, out var address) && address.AddressFamily == addressType);
		}

		public static IRuleBuilderOptions<T, TProperty> Required<T, TProperty>(this IRuleBuilderInitial<T, TProperty> ruleBuilder)
		{
			return ruleBuilder.NotNull().NotEmpty();
		}

		public static IRuleBuilderOptions<T, TProperty> MustWhen<T, TProperty>(this IRuleBuilderInitial<T, TProperty> ruleBuilder, Func<TProperty, bool> mustPredicate, Func<T, bool> whenPredicate, ApplyConditionTo applyConditionTo = ApplyConditionTo.AllValidators)
		{
			return ruleBuilder.Must(mustPredicate).When(whenPredicate, applyConditionTo);
		}

		public static IRuleBuilderOptions<T, TProperty> WhenNotNullOrEmpty<T, TProperty>(this IRuleBuilderOptions<T, TProperty> ruleBuilder, Func<T, TProperty> predicate)
		{
			return ruleBuilder.When((obj, ctx) =>
			{
				var value = predicate(obj);
				return RuleBuilderExtensions.ValidateNotNullOrEmptyInternal(value);
			});
		}

		private static bool ValidateNotNullOrEmptyInternal<TProperty>(TProperty? value)
		{
			if (value == null)
			{
				return false;
			}

			if (value is string stringValue)
			{
				return !string.IsNullOrWhiteSpace(stringValue);
			}

			if (value is IEnumerable enumerableValue)
			{
				return enumerableValue.Cast<object>().Any();
			}

			return object.Equals(value, default(TProperty));
		}
	}
}
