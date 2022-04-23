namespace Jossellware.Shared.AspNetCore.Extensions
{
    public static class ConfigurationExtensions
    {
        public static T? BindSection<T>(this IConfiguration? configuration, string sectionKey, T? instance = null)
            where T : class, new()
        {
            var section = configuration?.GetSection(sectionKey);
            if (section != null)
            {
                instance = instance ?? new T();
                section.Bind(instance);
                return instance;
            }

            return default(T);
        }

        public static bool GetBoolValue(this IConfiguration? configuration, string sectionKey)
        {
            return configuration.GetValue<bool>(sectionKey, false);
        }
    }
}
