using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RMS.Shared.Constants;
using System.Text;

namespace RMS.Services.AI.Service
{
    public class GeminiApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public GeminiApiClient(string apiKey)
        {
            _httpClient = new HttpClient();
            _apiKey = apiKey;
        }

        public async Task<string> GenerateContentAsync(string prompt, string model = Constant.GEMINI_MODEL_NAME_PRO)
        {
            string url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={_apiKey}";

            // Cấu hình request để ép kiểu trả về JSON (nếu model hỗ trợ) 
            // và thêm prompt
            var request = new
            {
                contents = new[] {
                    new { parts = new[] { new { text = prompt } } }
                },
                // Mẹo: Thêm generationConfig để gợi ý model trả về JSON
                generationConfig = new
                {
                    responseMimeType = "application/json"
                }
            };

            string jsonRequest = JsonConvert.SerializeObject(request);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, content);
            string jsonResponse = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                // Log ra lỗi thực tế từ Google để debug
                throw new Exception($"Gemini API Error: {jsonResponse}");
            }

            dynamic geminiResponse = JsonConvert.DeserializeObject(jsonResponse);

            // Lấy text thô từ phản hồi
            string text = geminiResponse?.candidates?[0]?.content?.parts?[0]?.text;

            if (string.IsNullOrEmpty(text)) return null;

            // --- ĐOẠN CODE SỬA LỖI ---
            // Chỉ trích xuất phần nằm trong dấu { và }
            return ExtractJson(text);
        }

        // Hàm helper để làm sạch chuỗi JSON
        private string ExtractJson(string text)
        {
            try
            {
                int startIndex = text.IndexOf('{');
                int endIndex = text.LastIndexOf('}');

                if (startIndex >= 0 && endIndex > startIndex)
                {
                    // Cắt đúng đoạn JSON valid
                    return text.Substring(startIndex, endIndex - startIndex + 1);
                }

                // Nếu không tìm thấy dấu ngoặc, trả về nguyên gốc (để throw error ở tầng trên nếu cần)
                return text;
            }
            catch
            {
                return text;
            }
        }

        /// <summary>
        /// Check ảnh SIÊU GẮT (Ultra-Strict).
        /// Chặn tất cả: 18+, Bikini, Đồ lót, Gợi cảm.
        /// </summary>
        public async Task<bool> CheckImageSafetyAsync(byte[] imageBytes, string mimeType = "image/jpeg")
        {
            
            string model = Constant.GEMINI_MODEL_NAME_PRO;
            string url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={_apiKey}";

            string base64Image = Convert.ToBase64String(imageBytes);

            // 1. CẤU HÌNH: BLOCK_NONE 
            // Để Google không tự chặn, trả về toàn bộ thông tin để ta tự xử lý.
            var safetySettings = new[]
            {
                new { category = "HARM_CATEGORY_SEXUALLY_EXPLICIT", threshold = "BLOCK_NONE" },
                new { category = "HARM_CATEGORY_HATE_SPEECH",       threshold = "BLOCK_NONE" },
                new { category = "HARM_CATEGORY_HARASSMENT",        threshold = "BLOCK_NONE" },
                new { category = "HARM_CATEGORY_DANGEROUS_CONTENT", threshold = "BLOCK_NONE" }
            };

            var requestPayload = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new object[]
                        {
                            // 2. PROMPT CHECK KÉP:
                            // Yêu cầu AI đóng vai người kiểm duyệt khắt khe và trả về JSON.
                            new { text = @"
                                You are a strict content moderator. Analyze this image for:
                                1. Sexually explicit content.
                                2. Nudity.
                                3. Suggestive clothing (bikini, lingerie, underwear, revealing clothes).
                                4. Suggestive poses.
                                
                                If the image contains ANY of these, it is UNSAFE.
                                Return a JSON object strictly like this:
                                { ""is_safe"": boolean, ""reason"": ""string"" }
                                "
                            },
                            new
                            {
                                inline_data = new
                                {
                                    mime_type = mimeType,
                                    data = base64Image
                                }
                            }
                        }
                    }
                },
                safetySettings,
                // Gợi ý cho Model trả về đúng định dạng JSON
                generationConfig = new { responseMimeType = "application/json" }
            };

            try
            {
                string jsonRequest = JsonConvert.SerializeObject(requestPayload);
                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(url, content);
                string jsonResponse = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Gemini API Error: {jsonResponse}");
                    return false; // Lỗi API -> Coi như không an toàn cho chắc
                }

                // Gọi hàm phân tích kỹ
                return AnalyzeSafetyResponse(jsonResponse);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return false;
            }
        }

        private bool AnalyzeSafetyResponse(string jsonResponse)
        {
            try
            {
                var jObject = JObject.Parse(jsonResponse);

                // --- BƯỚC 1: CHECK METADATA (Lớp bảo vệ thứ nhất) ---
                // Kiểm tra các chỉ số Google chấm điểm
                var candidates = jObject["candidates"];
                if (candidates != null && candidates.Any())
                {
                    var safetyRatings = candidates[0]["safetyRatings"];
                    if (safetyRatings != null)
                    {
                        foreach (var rating in safetyRatings)
                        {
                            string category = rating["category"]?.ToString();
                            string probability = rating["probability"]?.ToString();

                            // Nếu là danh mục 18+
                            if (category == "HARM_CATEGORY_SEXUALLY_EXPLICIT")
                            {
                                // CHẶN GẮT: Chặn cả LOW, MEDIUM, HIGH.
                                // Chỉ tha cho NEGLIGIBLE (Không đáng kể - ví dụ Icon X, phong cảnh).
                                if (probability == "LOW" || probability == "MEDIUM" || probability == "HIGH")
                                {
                                    Console.WriteLine($"[Blocked by Metadata] Category: {category}, Probability: {probability}");
                                    return false;
                                }
                            }
                        }
                    }

                    // --- BƯỚC 2: CHECK TEXT RESPONSE (Lớp bảo vệ thứ hai) ---
                    // Đọc nội dung JSON mà AI trả về ("is_safe": false...)
                    string text = candidates[0]?["content"]?["parts"]?[0]?["text"]?.ToString();
                    if (!string.IsNullOrEmpty(text))
                    {
                        // Làm sạch JSON (đề phòng AI thêm markdown ```json)
                        string cleanJson = ExtractJson(text);
                        var result = JsonConvert.DeserializeObject<dynamic>(cleanJson);

                        bool aiVerdictSafe = (bool)(result?.is_safe ?? false);
                        string reason = (string)result?.reason;

                        if (!aiVerdictSafe)
                        {
                            Console.WriteLine($"[Blocked by AI Verdict] Reason: {reason}");
                            return false;
                        }
                    }
                }

                // Kiểm tra PromptFeedback (nếu ảnh quá ghê bị chặn ngay từ đầu)
                var promptFeedback = jObject["promptFeedback"];
                if (promptFeedback != null)
                {
                    var blockReason = promptFeedback["blockReason"]?.ToString();
                    if (!string.IsNullOrEmpty(blockReason) && blockReason != "BLOCK_REASON_UNSPECIFIED")
                    {
                        return false;
                    }
                }

                return true; // Vượt qua tất cả các ải -> An toàn
            }
            catch
            {
                // Có lỗi khi parse -> Return false (Thà giết nhầm còn hơn bỏ sót)
                return false;
            }
        }

    }
}