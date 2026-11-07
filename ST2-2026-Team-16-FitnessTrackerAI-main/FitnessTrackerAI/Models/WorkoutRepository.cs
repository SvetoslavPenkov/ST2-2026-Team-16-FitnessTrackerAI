using Microsoft.Data.Sqlite;

namespace FitnessTrackerAI.Models
{
    // [DESIGN PATTERN: Repository]
    // Управлява всички CRUD операции (Insert, Select с филтър, Update, Delete) към SQLite базата данни.
    public class WorkoutRepository
    {
        private readonly string _connectionString;

        public WorkoutRepository()
        {
            _connectionString = DatabaseConnectionManager.Instance.ConnectionString;
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Workouts (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Title TEXT NOT NULL,
                    Category TEXT NOT NULL,
                    DurationMinutes INTEGER NOT NULL,
                    CaloriesBurned INTEGER NOT NULL,
                    Date DATETIME NOT NULL,
                    Notes TEXT
                );";
            command.ExecuteNonQuery();
        }

        // SELECT WITH FILTER
        public IEnumerable<Workout> GetAll(string? categoryFilter, string? searchKeyword)
        {
            var workouts = new List<Workout>();
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var query = "SELECT * FROM Workouts WHERE 1=1";
            if (!string.IsNullOrWhiteSpace(categoryFilter))
                query += " AND Category = @Category";
            if (!string.IsNullOrWhiteSpace(searchKeyword))
                query += " AND (Title LIKE @Search OR Notes LIKE @Search)";

            query += " ORDER BY Date DESC";

            using var command = new SqliteCommand(query, connection);
            if (!string.IsNullOrWhiteSpace(categoryFilter))
                command.Parameters.AddWithValue("@Category", categoryFilter);
            if (!string.IsNullOrWhiteSpace(searchKeyword))
                command.Parameters.AddWithValue("@Search", $"%{searchKeyword}%");

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                workouts.Add(new Workout
                {
                    Id = reader.GetInt32(0),
                    Title = reader.GetString(1),
                    Category = reader.GetString(2),
                    DurationMinutes = reader.GetInt32(3),
                    CaloriesBurned = reader.GetInt32(4),
                    Date = reader.GetDateTime(5),
                    Notes = reader.IsDBNull(6) ? null : reader.GetString(6)
                });
            }
            return workouts;
        }

        public Workout? GetById(int id)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            using var command = new SqliteCommand("SELECT * FROM Workouts WHERE Id = @Id", connection);
            command.Parameters.AddWithValue("@Id", id);
            using var reader = command.ExecuteReader();

            if (reader.Read())
            {
                return new Workout
                {
                    Id = reader.GetInt32(0),
                    Title = reader.GetString(1),
                    Category = reader.GetString(2),
                    DurationMinutes = reader.GetInt32(3),
                    CaloriesBurned = reader.GetInt32(4),
                    Date = reader.GetDateTime(5),
                    Notes = reader.IsDBNull(6) ? null : reader.GetString(6)
                };
            }
            return null;
        }

        // INSERT
        public void Add(Workout workout)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            using var command = new SqliteCommand(@"
                INSERT INTO Workouts (Title, Category, DurationMinutes, CaloriesBurned, Date, Notes)
                VALUES (@Title, @Category, @DurationMinutes, @CaloriesBurned, @Date, @Notes)", connection);

            command.Parameters.AddWithValue("@Title", workout.Title);
            command.Parameters.AddWithValue("@Category", workout.Category);
            command.Parameters.AddWithValue("@DurationMinutes", workout.DurationMinutes);
            command.Parameters.AddWithValue("@CaloriesBurned", workout.CaloriesBurned);
            command.Parameters.AddWithValue("@Date", workout.Date);
            command.Parameters.AddWithValue("@Notes", workout.Notes ?? (object)DBNull.Value);
            command.ExecuteNonQuery();
        }

        // UPDATE
        public void Update(Workout workout)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            using var command = new SqliteCommand(@"
                UPDATE Workouts 
                SET Title = @Title, Category = @Category, DurationMinutes = @DurationMinutes, 
                    CaloriesBurned = @CaloriesBurned, Date = @Date, Notes = @Notes
                WHERE Id = @Id", connection);

            command.Parameters.AddWithValue("@Id", workout.Id);
            command.Parameters.AddWithValue("@Title", workout.Title);
            command.Parameters.AddWithValue("@Category", workout.Category);
            command.Parameters.AddWithValue("@DurationMinutes", workout.DurationMinutes);
            command.Parameters.AddWithValue("@CaloriesBurned", workout.CaloriesBurned);
            command.Parameters.AddWithValue("@Date", workout.Date);
            command.Parameters.AddWithValue("@Notes", workout.Notes ?? (object)DBNull.Value);
            command.ExecuteNonQuery();
        }

        // DELETE
        public void Delete(int id)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            using var command = new SqliteCommand("DELETE FROM Workouts WHERE Id = @Id", connection);
            command.Parameters.AddWithValue("@Id", id);
            command.ExecuteNonQuery();
        }
    }
}