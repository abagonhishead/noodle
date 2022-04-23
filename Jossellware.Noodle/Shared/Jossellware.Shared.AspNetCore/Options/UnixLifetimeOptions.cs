namespace Jossellware.Shared.AspNetCore.Options
{
    public class UnixLifetimeOptions
    {
        public bool UseSystemdSocket { get; set; }

        public string ManagedSocketPath { get; set; }
    }
}
