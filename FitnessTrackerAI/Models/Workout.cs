namespace FitnessTrackerAI.Models
{
    public class Workout
    {
        public int Id { get; set; }
        public string ExerciseName { get; set; }    
        public string MuscleGroup { get; set; }   
        public int Sets { get; set; }      
        public int Reps { get; set; }          
        public decimal Weight { get; set; }     
        public DateTime Date { get; set; }
    }
}
