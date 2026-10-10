using FitnessTrackerAI.Models;
using Microsoft.AspNetCore.Mvc;

namespace FitnessTrackerAI.Controllers
{
    public class WorkoutController : Controller
    {
        // Временен списък за демо (вместо истинска БД засега)
        private static List<Workout> _workouts = new List<Workout>();

        // 1. SELECT WITH FILTER: Показва списъка и може да филтрира по мускулна група
        [HttpGet]
        public IActionResult Index(string muscleGroupFilter)
        {
            // Използваме нашия Singleton тук за демонстрация
            var singletonTest = DatabaseConnectionManager.Instance;
            ViewBag.Status = "Singleton работи! Връзка: " + singletonTest.ConnectionString;

            var workouts = _workouts.AsQueryable();

            // Ако е въведен филтър, филтрираме списъка
            if (!string.IsNullOrEmpty(muscleGroupFilter))
            {
                workouts = workouts.Where(w => w.MuscleGroup.Contains(muscleGroupFilter, StringComparison.OrdinalIgnoreCase));
                ViewBag.CurrentFilter = muscleGroupFilter;
            }

            return View(workouts.ToList());
        }

        // 2. INSERT (GET): Отваря формата за добавяне
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // 3. INSERT (POST): Записва данните, изпратени от формата
        [HttpPost]
        public IActionResult Create(Workout workout)
        {
            if (ModelState.IsValid)
            {
                workout.Id = _workouts.Count + 1;
                workout.Date = DateTime.Now;
                _workouts.Add(workout); // Добавяне в списъка (Insert)

                return RedirectToAction(nameof(Index));
            }
            return View(workout);
        }
    }
    
}
