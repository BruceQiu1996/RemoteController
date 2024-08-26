using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace RemoteController.Client.Helpers
{
    public class HttpRequest
    {
        private readonly HttpClient _httpClient;
        public static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions();

        public HttpRequest()
        {
            _httpClient = new HttpClient();
            _jsonSerializerOptions.Encoder = JavaScriptEncoder.Create(new TextEncoderSettings(UnicodeRanges.All));
            _jsonSerializerOptions.ReadCommentHandling = JsonCommentHandling.Skip;
            _jsonSerializerOptions.PropertyNameCaseInsensitive = true;
            _jsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
            _jsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            BuildHttpClient(_httpClient);
        }

        public void SetToken(string token)
        {
            if (!string.IsNullOrEmpty(token))
            {
                if (_httpClient.DefaultRequestHeaders.Contains("Authorization"))
                {
                    _httpClient.DefaultRequestHeaders.Remove("Authorization");
                }

                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
            }
        }

        public async Task<HttpResponseMessage> PostAsync(string url, dynamic body)
        {
            HttpResponseMessage resp = null;
            if (body == null)
            {
                resp = await _httpClient.PostAsync(url, null);
            }
            else
            {
                var content = new StringContent(JsonSerializer.Serialize(body, _jsonSerializerOptions));
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                resp = await _httpClient.PostAsync(url, content);
            }
            if (resp.IsSuccessStatusCode)
            {
                return resp;
            }
            else if (resp.StatusCode == HttpStatusCode.Unauthorized)
            {
                ExcuteWhileUnauthorized?.Invoke();
                return default;
            }
            else if (resp.StatusCode == HttpStatusCode.BadRequest)
            {
                var message = await resp.Content.ReadAsStringAsync();
                ExcuteWhileBadRequest?.Invoke(message);
            }
            else if (resp.StatusCode == HttpStatusCode.InternalServerError)
            {
                var message = await resp.Content.ReadAsStringAsync();
                ExcuteWhileInternalServerError?.Invoke(message);
            }

            return default;
        }

        public async Task<HttpResponseMessage> GetAsync(string url)
        {
            var resp = await _httpClient.GetAsync(url).ConfigureAwait(false);
            if (resp.IsSuccessStatusCode)
            {
                return resp;
            }
            else if (resp.StatusCode == HttpStatusCode.Unauthorized)
            {
                ExcuteWhileUnauthorized?.Invoke();
                return default;
            }
            else if (resp.StatusCode == HttpStatusCode.BadRequest)
            {
                var message = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                ExcuteWhileBadRequest?.Invoke(message);
            }
            else if (resp.StatusCode == HttpStatusCode.InternalServerError)
            {
                var message = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                ExcuteWhileInternalServerError?.Invoke(message);
            }

            return default;
        }

        public Func<Task<bool>> TryRefreshToken;//当服务端返回401的时候，尝试利用refreshtoken重新获取accesstoken以及refreshtoken
        public event Action ExcuteWhileUnauthorized; //401
        public event Action<string> ExcuteWhileBadRequest;//400
        public event Action<string> ExcuteWhileInternalServerError;//500

        public void BuildHttpClient(HttpClient httpClient)
        {
            httpClient.BaseAddress = new Uri("http://127.0.0.1:5590");
            httpClient.Timeout = TimeSpan.FromSeconds(18);
            httpClient.DefaultRequestVersion = HttpVersion.Version10;
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }
    }
}
