using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace EtherCAT_Studio
{
    public class OllamaClient
    {
        private readonly HttpClient _client;
        private readonly string _baseUrl;
        private const string DefaultModel = "gemma3:1b";

        public OllamaClient(string baseUrl = "http://localhost:11434")
        {
            _baseUrl = NormalizeBaseUrl(baseUrl);
            _client = new HttpClient();
        }

        private static string NormalizeBaseUrl(string baseUrl)
        {
            var url = (baseUrl ?? string.Empty).Trim();
            if (url.EndsWith("/"))
            {
                url = url.TrimEnd('/');
            }

            if (url.EndsWith("/api", StringComparison.OrdinalIgnoreCase))
            {
                url = url.Substring(0, url.Length - 4);
            }

            return string.IsNullOrWhiteSpace(url) ? "http://localhost:11434" : url;
        }

        public async Task<string> GenerateAsync(string prompt)
        {
            try
            {
                var request = new
                {
                    model = DefaultModel,
                    prompt = prompt,
                    stream = false
                };

                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                var response = await _client.PostAsync($"{_baseUrl}/api/generate", content);
                response.EnsureSuccessStatusCode();

                var responseText = await response.Content.ReadAsStringAsync();
                var doc = JsonDocument.Parse(responseText);
                return doc.RootElement.GetProperty("response").GetString() ?? "";
            }
            catch (Exception ex)
            {
                throw new Exception($"Ollama API 호출 실패: {ex.Message}");
            }
        }
    }

    public class SequenceJsonGenerator
    {
        private readonly OllamaClient _ollama;

        public SequenceJsonGenerator(string? baseUrl = null)
        {
            _ollama = new OllamaClient(baseUrl ?? "http://localhost:11434");
        }

        public async Task<string> GenerateSequenceJsonAsync(string naturalLanguagePrompt)
        {
            // Ollama에 전송할 시스템 프롬프트
            var systemPrompt = @"
당신은 EtherCAT 서보 모터 제어 시스템을 위한 JSON 시퀀스 생성 전문가입니다.
사용자의 자연어 지시를 받아 정확하게 다음 JSON 포맷으로 생성하세요:

JSON 포맷 (반드시 이 형식을 따라야 함):
{
  ""sequence_name"": ""User Description"",
  ""steps"": [
    {
      ""id"": ""step_0"",
      ""type"": ""START"",
      ""params"": {}
    },
    {
      ""id"": ""step_1"",
      ""type"": ""MOTION"",
      ""params"": {
        ""axis"": ""X"",
        ""pos"": 1000,
        ""speed"": 500
      }
    },
    {
      ""id"": ""step_2"",
      ""type"": ""WAIT"",
      ""params"": {
        ""delay_ms"": 2000
      }
    },
    {
      ""id"": ""step_3"",
      ""type"": ""END"",
      ""params"": {}
    }
  ]
}

템플릿 타입과 필수 params:
- START: {} (시작점)
- END: {} (종료점)
- MOTION: {axis: ""X/Y/Z"", pos: number, speed: number}
- WAIT: {delay_ms: number (밀리초)}
- LINEAR_MOVE: {target_x: number, target_y: number, target_z: number, speed: number}
- REL_MOVE: {axis: ""X/Y/Z"", distance: number, speed: number}
- CIRCULAR_MOVE: {center_x: number, center_y: number, end_x: number, end_y: number, direction: ""CW/CCW""}
- COUNTER: {name: ""counter_name"", initial: number, target: number, increment: number}
- GOTO: {to_step: ""step_id"", description: ""reason""}
- SET_DO: {port: number, value: 0/1}
- CHECK_DI: {port: number, expected: 0/1}

사용자 입력: ";

            var fullPrompt = systemPrompt + naturalLanguagePrompt + @"

필수 규칙:
1. 항상 START로 시작, END로 끝남
2. 각 단계마다 id를 ""step_0"", ""step_1"" ... 형식으로 정확히 지정
3. 모든 params 필드에 올바른 값 입력 (문자열은 ""큰따옴표"" 사용)
4. 숫자 값은 쌍따옴표 없이 입력 (예: 1000, 500.5)
5. 유효한 JSON만 반환 (주석 없음)

JSON만 반환:";

            try
            {
                var response = await _ollama.GenerateAsync(fullPrompt);
                
                // JSON 추출
                int jsonStart = response.IndexOf("{");
                int jsonEnd = response.LastIndexOf("}");
                
                if (jsonStart >= 0 && jsonEnd > jsonStart)
                {
                    string jsonStr = response.Substring(jsonStart, jsonEnd - jsonStart + 1);
                    
                    // 유효성 검사
                    var doc = JsonDocument.Parse(jsonStr);
                    return jsonStr;
                }
                else
                {
                    throw new Exception("생성된 응답에서 JSON을 찾을 수 없습니다.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"시퀀스 생성 실패: {ex.Message}");
            }
        }
    }
}
