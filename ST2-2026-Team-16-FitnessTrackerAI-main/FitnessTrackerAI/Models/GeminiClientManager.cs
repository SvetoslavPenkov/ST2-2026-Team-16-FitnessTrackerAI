using System.Text;
using System.Text.Json;

namespace FitnessTrackerAI.Models
{
    /// [DESIGN PATTERN 3: Factory / Strategy Pattern]
    /// Управлява комуникацията с външни (Gemini 3.8) и локални (Ollama) ИИ модели с поддръжка на повторни опити (Retry) при претоварване (503).
    public class GeminiClientManager
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private const string GeminiModel = "gemini-3.8-flash";

        public GeminiClientManager(IConfiguration config, HttpClient httpClient)
        {
            _httpClient = httpClient;
            _apiKey = config["Gemini:ApiKey"] ?? "";
        }

        public async Task<string> AskGeminiAsync(string prompt)
        {
            if (string.IsNullOrWhiteSpace(_apiKey) || _apiKey.Contains("YOUR_GEMINI"))
            {
                return "Грешка: Липсва валиден Gemini API ключ! Добавете го в appsettings.json.";
            }

            int maxRetries = 3;
            int delayMs = 2000;

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    var url = $"https://generativelanguage.googleapis.com/v1beta/models/{GeminiModel}:generateContent?key={_apiKey}";
                    var requestBody = new
                    {
                        contents = new[] { new { parts = new[] { new { text = prompt } } } }
                    };

                    var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
                    var response = await _httpClient.PostAsync(url, content);
                    var json = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        using var doc = JsonDocument.Parse(json);
                        return doc.RootElement.GetProperty("candidates")[0].GetProperty("content").GetProperty("parts")[0].GetProperty("text").GetString() ?? "Празен отговор.";
                    }

                    // При грешка 503 (претоварване), изчакваме и правим повторен опит
                    if ((int)response.StatusCode == 503 && attempt < maxRetries)
                    {
                        await Task.Delay(delayMs * attempt);
                        continue;
                    }

                    if ((int)response.StatusCode == 503)
                    {
                        return "Сървърът на Google в момента е временно претоварен (503 Service Unavailable). Моля, опитайте отново след няколко секунди.";
                    }

                    if ((int)response.StatusCode == 429)
                    {
                        return $"Грешка: Достигнат е лимитът на заявките (Quota Exceeded) за модела {GeminiModel}. Опитайте отново след малко.";
                    }

                    return $"Грешка от Gemini API ({response.StatusCode}): {json}";
                }
                catch (Exception ex)
                {
                    if (attempt == maxRetries)
                        return $"Изключение при връзката с Gemini ({GeminiModel}): {ex.Message}";
                }
            }

            return "Възникна временна грешка при връзката с Gemini.";
        }

        public async Task<string> AskLocalModelAsync(string prompt)
        {
            try
            {
                var requestBody = new { model = "llama3", prompt = prompt, stream = false };
                var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("http://localhost:11434/api/generate", content);

                if (!response.IsSuccessStatusCode) return "Локалният модел Ollama не отговаря.";

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                return doc.RootElement.GetProperty("response").GetString() ?? "";
            }
            catch
            {
                return "Грешка при връзката с локалния модел (Ollama). Уверете се, че работи на localhost:11434.";
            }
        }

        public async Task<string> AnalyzeImageAsync(string prompt, byte[] imageBytes, string mimeType)
        {
            if (string.IsNullOrWhiteSpace(_apiKey) || _apiKey.Contains("YOUR_GEMINI"))
            {
                return "Грешка: Липсва валиден Gemini API ключ! Добавете го в appsettings.json.";
            }

            int maxRetries = 3;
            int delayMs = 2000;

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    var url = $"https://generativelanguage.googleapis.com/v1beta/models/{GeminiModel}:generateContent?key={_apiKey}";
                    var base64Image = Convert.ToBase64String(imageBytes);

                    var requestBody = new
                    {
                        contents = new[]
                        {
                            new
                            {
                                parts = new object[]
                                {
                                    new { text = prompt },
                                    new { inline_data = new { mime_type = mimeType, data = base64Image } }
                                }
                            }
                        }
                    };

                    var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
                    var response = await _httpClient.PostAsync(url, content);
                    var json = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        using var doc = JsonDocument.Parse(json);
                        return doc.RootElement.GetProperty("candidates")[0].GetProperty("content").GetProperty("parts")[0].GetProperty("text").GetString() ?? "Празен отговор.";
                    }

                    if ((int)response.StatusCode == 503 && attempt < maxRetries)
                    {
                        await Task.Delay(delayMs * attempt);
                        continue;
                    }

                    if ((int)response.StatusCode == 503)
                    {
                        return "Сървърът на Google в момента е временно претоварен (503 Service Unavailable). Моля, опитайте отново след няколко секунди.";
                    }

                    if ((int)response.StatusCode == 429)
                    {
                        return $"Грешка: Достигнат е лимитът на заявките (Quota Exceeded) за модела {GeminiModel}. Опитайте отново след малко.";
                    }

                    return $"Грешка от Gemini API ({response.StatusCode}): {json}";
                }
                catch (Exception ex)
                {
                    if (attempt == maxRetries)
                        return $"Изключение при мултимодалния анализ: {ex.Message}";
                }
            }

            return "Възникна временна грешка при връзката с Gemini.";
        }
    }
}