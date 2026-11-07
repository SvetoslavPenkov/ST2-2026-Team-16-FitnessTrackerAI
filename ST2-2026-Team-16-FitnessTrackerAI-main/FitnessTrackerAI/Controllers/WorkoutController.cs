using Microsoft.AspNetCore.Mvc;
using FitnessTrackerAI.Models;

namespace FitnessTrackerAI.Controllers
{
    public class WorkoutController : Controller
    {
        //Управлява заявките за тренировките — извличане на данни, записване на нови тренировки и генериране на изгледи за AI съвети.
        private readonly WorkoutRepository _repository;
        private readonly GeminiClientManager _aiManager;

        public WorkoutController(GeminiClientManager aiManager)
        {
            _repository = new WorkoutRepository();
            _aiManager = aiManager;
        }

        // 1. SELECT WITH FILTER (Седмица 1)
        public IActionResult Index(string? categoryFilter, string? searchKeyword)
        {
            ViewBag.CurrentCategory = categoryFilter;
            ViewBag.CurrentSearch = searchKeyword;
            var workouts = _repository.GetAll(categoryFilter, searchKeyword);
            return View(workouts);
        }

        // 2. CREATE (INSERT - Седмица 1)
        public IActionResult Create() => View(new Workout());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Workout workout)
        {
            if (ModelState.IsValid)
            {
                _repository.Add(workout);
                return RedirectToAction(nameof(Index));
            }
            return View(workout);
        }

        // 3. EDIT (UPDATE - Седмица 2)
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
            if (ModelState.IsValid)
            {
                _repository.Update(workout);
                return RedirectToAction(nameof(Index));
            }
            return View(workout);
        }

        // 4. DELETE (Седмица 2)
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

        // 5. ADVICE (ИИ Асистент: Външен Gemini API / Локален Ollama / Мултимодалност - Седмици 3 и 4)
        [HttpGet]
        public IActionResult Advice() => View();

        [HttpPost]
        public async Task<IActionResult> Advice(string prompt, string provider, IFormFile? imageFile)
        {
            if (string.IsNullOrWhiteSpace(prompt) && imageFile == null)
            {
                ViewBag.Error = "Моля, въведете въпрос или прикачете снимка.";
                return View();
            }

            string result;
            if (imageFile != null && imageFile.Length > 0)
            {
                using var ms = new MemoryStream();
                await imageFile.CopyToAsync(ms);
                result = await _aiManager.AnalyzeImageAsync(
                    string.IsNullOrWhiteSpace(prompt) ? "Анализирай тази фитнес снимка/храна:" : prompt,
                    ms.ToArray(),
                    imageFile.ContentType);
            }
            else if (provider == "local")
            {
                result = await _aiManager.AskLocalModelAsync(prompt);
            }
            else
            {
                result = await _aiManager.AskGeminiAsync(prompt);
            }

            ViewBag.Prompt = prompt;
            ViewBag.Provider = provider;
            ViewBag.Response = result;

            return View();
        }
    }
}