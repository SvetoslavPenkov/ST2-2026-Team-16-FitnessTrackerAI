namespace FitnessTrackerAI.Models
{
    public sealed class WorkoutRepository
    {
        private static readonly Lazy<WorkoutRepository> _instance =
            new Lazy<WorkoutRepository>(() => new WorkoutRepository());

        private readonly List<Workout> _workouts = new List<Workout>();
        private readonly object _lock = new object();
        private int _nextId = 1;

        private WorkoutRepository() { }

        public static WorkoutRepository Instance => _instance.Value;

        public List<Workout> GetAll()
        {
            lock (_lock)
            {
                return _workouts.Select(Clone).ToList();
            }
        }

        public Workout? GetById(int id)
        {
            lock (_lock)
            {
                var found = _workouts.FirstOrDefault(w => w.Id == id);
                return found == null ? null : Clone(found);
            }
        }

        public void Add(Workout workout)
        {
            lock (_lock)
            {
                workout.Id = _nextId++;
                _workouts.Add(Clone(workout));
            }
        }

        public bool Update(Workout workout)
        {
            lock (_lock)
            {
                var existing = _workouts.FirstOrDefault(w => w.Id == workout.Id);
                if (existing == null) return false;

                existing.ExerciseName = workout.ExerciseName;
                existing.MuscleGroup = workout.MuscleGroup;
                existing.Sets = workout.Sets;
                existing.Reps = workout.Reps;
                existing.Weight = workout.Weight;
                return true;
            }
        }

        public bool Delete(int id)
        {
            lock (_lock)
            {
                return _workouts.RemoveAll(w => w.Id == id) > 0;
            }
        }

        private static Workout Clone(Workout w) => new Workout
        {
            Id = w.Id,
            ExerciseName = w.ExerciseName,
            MuscleGroup = w.MuscleGroup,
            Sets = w.Sets,
            Reps = w.Reps,
            Weight = w.Weight,
            Date = w.Date
        };
    }
}
