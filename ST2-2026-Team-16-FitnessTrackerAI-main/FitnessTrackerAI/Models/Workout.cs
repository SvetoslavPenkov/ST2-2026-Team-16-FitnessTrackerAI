using System.ComponentModel.DataAnnotations;

namespace FitnessTrackerAI.Models
{
    public class Workout
    {
        //Модел за дадена тренировка (дата, времетраене, тип тренировка, списък с упражнения).
        public int Id { get; set; }

        [Required(ErrorMessage = "Заглавието е задължително.")]
        [Display(Name = "Тренировка")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Категорията е задължителна.")]
        [Display(Name = "Категория")]
        public string Category { get; set; } = string.Empty;

        [Range(1, 600, ErrorMessage = "Времетраенето трябва да е между 1 и 600 минути.")]
        [Display(Name = "Времетраене (мин)")]
        public int DurationMinutes { get; set; }

        [Range(0, 10000, ErrorMessage = "Изгорените калории трябва да са положително число.")]
        [Display(Name = "Изгорени калории")]
        public int CaloriesBurned { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Дата")]
        public DateTime Date { get; set; } = DateTime.Now;

        [Display(Name = "Бележки")]
        public string? Notes { get; set; }
    }
}