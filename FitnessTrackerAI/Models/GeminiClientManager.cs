using System.Text;
using System.Text.Json;

namespace FitnessTrackerAI.Models
{
    public sealed class GeminiClientManager
    {
        private static readonly Lazy<GeminiClientManager> _instance =
            new Lazy<GeminiClientManager>(() => new GeminiClientManager());

        private readonly HttpClient _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
        private string _apiKey = "";
        private string _model = "gemini-3.5-flash";
        private string _baseUrl = "https://generativelanguage.googleapis.com/v1beta";

        private GeminiClientManager() { }

        public static GeminiClientManager Instance => _instance.Value;

        public void Configure(IConfiguration configuration)
        {
            _apiKey = configuration["Gemini:ApiKey"] ?? "";
            _model = configuration["Gemini:Model"] ?? _model;
            _baseUrl = configuration["Gemini:BaseUrl"] ?? _baseUrl;
        }

        public async Task<string> GetWorkoutAdviceAsync(IEnumerable<Workout> workouts)
        {
            if (string.IsNullOrWhiteSpace(_apiKey))
                return "Липсва Gemini API ключ. Задай го с: dotnet user-secrets set \"Gemini:ApiKey\" \"<ключ>\"";

            var list = workouts.ToList();
            if (list.Count == 0)
                return "Няма тренировки за анализ. Добави поне една.";

            var sb = new StringBuilder();
            foreach (var w in list)
                sb.AppendLine($"- {w.Date:yyyy-MM-dd}: {w.ExerciseName} ({w.MuscleGroup}), {w.Sets}x{w.Reps}, {w.Weight} кг");

            var body = new
            {
                systemInstruction = new
                {
                    parts = new[] { new { text = "Ти си фитнес треньор. Анализирай списъка с тренировки и дай кратки, практични съвети на български: баланс на мускулните групи, прогресия и възстановяване. Не давай медицински съвети." } }
                },
                contents = new[]
                {
                    new { role = "user", parts = new[] { new { text = "Моите тренировки:\n" + sb } } }
                }
            };

            var request = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/models/{_model}:generateContent")
            {
                Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
            };
            request.Headers.Add("x-goog-api-key", _apiKey);

            try
            {
                using var response = await _httpClient.SendAsync(request);
                var json = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    return $"Gemini върна грешка ({(int)response.StatusCode}). Провери ключа и името на модела.";

                using var doc = JsonDocument.Parse(json);
                var text = doc.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();

                return text ?? "Gemini не върна текст.";
            }
            catch (TaskCanceledException)
            {
                return "Заявката към Gemini отне твърде дълго. Опитай пак.";
            }
            catch (Exception ex) when (ex is HttpRequestException || ex is JsonException || ex is KeyNotFoundException || ex is InvalidOperationException)
            {
                return "Неуспешна връзка или неочакван отговор от Gemini.";
            }
        }
    }
}
