namespace FitnessTrackerAI.Models
{
    public sealed class DatabaseConnectionManager
    {
        private static DatabaseConnectionManager _instance = null;
        private static readonly object _lock = new object();

        public string ConnectionString { get; private set; }

        // Частен конструктор - никой извън този клас не може да го извика директно
        private DatabaseConnectionManager()
        {
            ConnectionString = "Server=localhost;Database=FitnessDB;";
        }

        // Единственият начин да вземеш инстанцията
        public static DatabaseConnectionManager Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new DatabaseConnectionManager();
                    }
                    return _instance;
                }
            }
        }
    }
}
