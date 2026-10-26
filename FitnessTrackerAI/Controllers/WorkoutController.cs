using FitnessTrackerAI.Models;
using Microsoft.AspNetCore.Mvc;

namespace FitnessTrackerAI.Controllers
{
    public class WorkoutController : Controller
    {
        private readonly WorkoutRepository _repository = WorkoutRepository.Instance;

        [HttpGet]
        public IActionResult Index(string muscleGroupFilter)
        {
            var singletonTest = DatabaseConnectionManager.Instance;

            var workouts = _repository.GetAll().AsQueryable();

            if (!string.IsNullOrEmpty(muscleGroupFilter))
            {
                workouts = workouts.Where(w => w.MuscleGroup.Contains(muscleGroupFilter, StringComparison.OrdinalIgnoreCase));
                ViewBag.CurrentFilter = muscleGroupFilter;
            }

            return View(workouts.ToList());
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Workout workout)
        {
            if (ModelState.IsValid)
            {
                workout.Date = DateTime.Now;
                _repository.Add(workout);

                return RedirectToAction(nameof(Index));
            }
            return View(workout);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var workout = _repository.GetById(id);
            if (workout == null) return NotFound();
            return View(workout);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Workout workout)
        {
            if (!ModelState.IsValid) return View(workout);

            if (!_repository.Update(workout)) return NotFound();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Delete(int id)
        {
            var workout = _repository.GetById(id);
            if (workout == null) return NotFound();
            return View(workout);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            _repository.Delete(id);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Advice()
        {
            ViewBag.Advice = await GeminiClientManager.Instance.GetWorkoutAdviceAsync(_repository.GetAll());
            return View();
        }
    }
}
