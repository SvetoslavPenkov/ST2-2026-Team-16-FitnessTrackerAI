using System.ComponentModel.DataAnnotations;

namespace FitnessTrackerAI.Models
{
    public class Workout
    {
        public int Id { get; set; }

        [Required]
        public string ExerciseName { get; set; } = "";

        [Required]
        public string MuscleGroup { get; set; } = "";

        [Range(1, 100)]
        public int Sets { get; set; }

        [Range(1, 1000)]
        public int Reps { get; set; }

        [Range(0, 1000)]
        public decimal Weight { get; set; }

        public DateTime Date { get; set; }
    }
}
