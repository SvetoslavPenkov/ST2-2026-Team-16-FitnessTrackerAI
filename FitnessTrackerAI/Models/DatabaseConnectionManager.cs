namespace FitnessTrackerAI.Models
{
    public sealed class DatabaseConnectionManager
    {
        private static readonly Lazy<DatabaseConnectionManager> _instance =
            new Lazy<DatabaseConnectionManager>(() => new DatabaseConnectionManager());

        public string ConnectionString { get; private set; } = "";

        private DatabaseConnectionManager() { }

        public static DatabaseConnectionManager Instance => _instance.Value;

        public void Configure(IConfiguration configuration)
        {
            ConnectionString = configuration.GetConnectionString("Default") ?? "";
        }
    }
}
