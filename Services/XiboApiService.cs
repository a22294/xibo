using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Xibo_CMVV.Models;

namespace Xibo_CMVV.Services
{
    public class XiboApiService
    {

        private readonly HttpClient _client;
        private string _accessToken;

        private const string clientId = "9aea48df0ac3d7cac1d5d77aaab262ea9e0af7a3";
        private const string clientSecret = "c6d2a161044d1c019b5b30ed20fb19ab5853796dd9267fc7ab666b30209a0a0a5f9bc6d1151278c9f9db42d168b53833248012dcb94518976f7f9582918845611a44835eab8ee70e882164fb0747a49e43ee348103d38d3f4eec2fec18eb0e9e73d9b1def282454be747602b08954073ffe1e93c614bc8bfdbbecaecc2dd4a";
        private const string tokenUrl = "http://192.168.59.131/api/authorize/access_token";
        private const string baseApiUrl = "http://192.168.59.131/api";

        public XiboApiService()
        {
            _client = new HttpClient();
        }

        public async Task<bool> AuthenticateAsync()
        {
            var request = new HttpRequestMessage(HttpMethod.Post, tokenUrl)
            {
                Content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("grant_type",    "client_credentials"),
                    new KeyValuePair<string, string>("client_id",     clientId),
                    new KeyValuePair<string, string>("client_secret", clientSecret)
                })
            };

            try
            {
                var response = await _client.SendAsync(request);
                var body = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine("⚠️  ERRO NA AUTENTICAÇÃO");
                    Console.WriteLine($"Status Code: {response.StatusCode} ({(int)response.StatusCode})");
                    Console.WriteLine($"Resposta: {body}");

                    if ((int)response.StatusCode == 404)
                        Console.WriteLine("❌ Verifica o valor da variável 'tokenUrl'. O endpoint pode estar errado.");
                    else if ((int)response.StatusCode == 400)
                        Console.WriteLine("❌ Verifica se o 'grant_type' está correto e se o clientId/clientSecret estão certos.");
                    else if ((int)response.StatusCode == 401)
                        Console.WriteLine("❌ Credenciais inválidas ou aplicação mal configurada no Xibo CMS.");

                    return false;
                }

                var json = JsonDocument.Parse(body);
                _accessToken = json.RootElement.GetProperty("access_token").GetString() ?? throw new InvalidOperationException();
                Console.WriteLine("✅ Autenticação com sucesso. Access Token obtido.");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Exceção durante a autenticação: " + ex.Message);
                return false;
            }
        }

        public async Task<List<XiboUser>> GetUsersAsync()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"{baseApiUrl}/user");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

            var response = await _client.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<List<XiboUser>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<XiboUser>();
        }

        public async Task<XiboUser?> CreateUserAsync(XiboUser newUser)
        {
            if (string.IsNullOrWhiteSpace(newUser.userName) ||
                string.IsNullOrWhiteSpace(newUser.password) ||
                newUser.password.Length < 8 ||
                string.IsNullOrWhiteSpace(newUser.email))
            {
                Console.WriteLine("❌ Dados inválidos: username, email ou password (mín. 8 chars) em falta.");
                return null;
            }

            var payload = new Dictionary<string, string>
            {
                { "user",       newUser.userName },
                { "password",   newUser.password },
                { "email",      newUser.email },
                { "usertypeId", "2" }, // admin=2, user=1
                { "groupId",    "3" }
            };

            var request = new HttpRequestMessage(HttpMethod.Post, $"{baseApiUrl}/user")
            {
                Content = new FormUrlEncodedContent(payload)
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

            var response = await _client.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();

            Console.WriteLine("🔸 Payload enviado:");
            foreach (var pair in payload)
                Console.WriteLine($"{pair.Key} = {pair.Value}");
            Console.WriteLine("📨 Resposta completa:");
            Console.WriteLine(body);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"❌ Erro: {response.StatusCode} ({(int)response.StatusCode})");
                return null;
            }

            Console.WriteLine("✅ Utilizador criado com sucesso.");
            return JsonSerializer.Deserialize<XiboUser>(body, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }

        public async Task<bool> UpdateUserAsync(XiboUser user)
        {
            var payload = new Dictionary<string, string>
            {
                { "userName",      user.userName },
                { "email",         user.email },
                { "userTypeId",    "1" },
                { "homePageId",    "1" },
                { "libraryQuota",  "100" },
                { "password",      user.password ?? "" },
                { "groupId",       "6" },
                { "newUserWizard", "0" },
                { "hideNavigation","0" }
            };

            var request = new HttpRequestMessage(HttpMethod.Put, $"{baseApiUrl}/user/{user.userId}")
            {
                Content = new FormUrlEncodedContent(payload)
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

            var resp = await _client.SendAsync(request);
            var body = await resp.Content.ReadAsStringAsync();

            if (!resp.IsSuccessStatusCode)
            {
                Console.WriteLine($"❌ ERRO ao atualizar utilizador: {resp.StatusCode} ({(int)resp.StatusCode})");
                Console.WriteLine("Resposta completa:");
                Console.WriteLine(body);
                return false;
            }

            Console.WriteLine("✅ Utilizador atualizado com sucesso.");
            return true;
        }

        public async Task<bool> DeleteUserAsync(int userId)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, $"{baseApiUrl}/user/{userId}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

            var response = await _client.SendAsync(request);
            return response.IsSuccessStatusCode;
        }

        public async Task<List<XiboDisplay>> GetDisplaysAsync()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"{baseApiUrl}/display");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

            var response = await _client.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();

            var items = JsonSerializer.Deserialize<JsonElement>(json);
            var displays = new List<XiboDisplay>();

            foreach (var item in items.EnumerateArray())
            {
                displays.Add(new XiboDisplay
                {
                    displayId = item.GetProperty("displayId").GetInt32(),
                    display = item.GetProperty("display").GetString(),
                    description = item.TryGetProperty("description", out var d) ? d.GetString() : "",
                    isActive = item.TryGetProperty("isActive", out var a) && a.GetInt32() == 1,
                    loggedIn = item.TryGetProperty("loggedIn", out var l) && l.GetInt32() == 1
                });
            }

            return displays;
        }

        public async Task<bool> UploadMediaAsync(string name, string filepath, string? description = null)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"{baseApiUrl}/library");
            var content = new MultipartFormDataContent();

            content.Add(new StringContent(name), "name");
            if (!string.IsNullOrWhiteSpace(description))
                content.Add(new StringContent(description), "description");

            content.Add(new StringContent("media"), "type");
            var fileContent = new ByteArrayContent(File.ReadAllBytes(filepath));
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            content.Add(fileContent, "files", Path.GetFileName(filepath));

            request.Content = content;
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

            var response = await _client.SendAsync(request);
            return response.IsSuccessStatusCode;
        }

        public async Task<List<XiboLayout>> GetLayoutsAsync()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"{baseApiUrl}/layout");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

            var response = await _client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
                return new List<XiboLayout>();

            var json = await response.Content.ReadAsStringAsync();
            var items = JsonSerializer.Deserialize<JsonElement>(json);

            var layouts = new List<XiboLayout>();
            foreach (var item in items.EnumerateArray())
            {
                layouts.Add(new XiboLayout
                {
                    layoutId = item.GetProperty("layoutId").GetInt32(),
                    layout = item.GetProperty("layout").GetString(),
                    description = item.TryGetProperty("description", out var d) ? d.GetString() : "",
                    status = item.TryGetProperty("status", out var s) ? s.GetString() : ""
                });
            }

            return layouts;
        }

        public async Task<XiboLayout?> GetLayoutByIdAsync(int layoutId)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"{baseApiUrl}/layout/{layoutId}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

            var response = await _client.SendAsync(request);
            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<XiboLayout>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }

        public async Task<XiboLayout?> CreateLayoutAsync(XiboLayout layout)
        {
            var json = JsonSerializer.Serialize(layout);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, $"{baseApiUrl}/layout")
            {
                Content = content
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

            var response = await _client.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();

            return response.IsSuccessStatusCode
                ? JsonSerializer.Deserialize<XiboLayout>(body)
                : null;
        }

        public async Task<XiboLayout?> UpdateLayoutAsync(int layoutId, XiboLayout layout)
        {
            var json = JsonSerializer.Serialize(layout);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Put, $"{baseApiUrl}/layout/{layoutId}")
            {
                Content = content
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

            var response = await _client.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();

            return response.IsSuccessStatusCode
                ? JsonSerializer.Deserialize<XiboLayout>(body)
                : null;
        }

        public async Task<bool> DeleteLayoutAsync(int layoutId)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, $"{baseApiUrl}/layout/{layoutId}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

            var response = await _client.SendAsync(request);
            return response.IsSuccessStatusCode;
        }

        public async Task<List<JsonElement>> GetLibraryAsync()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"{baseApiUrl}/library");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

            var response = await _client.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<List<JsonElement>>(json) ?? new List<JsonElement>();
        }

        public async Task<bool> DeleteMediaAsync(int mediaId)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, $"{baseApiUrl}/library/{mediaId}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

            var response = await _client.SendAsync(request);
            return response.IsSuccessStatusCode;
        }


        public async Task<bool> UploadImageToLayoutAsync(int layoutId, string imagePath)
        {
            var multipartContent = new MultipartFormDataContent
            {
                { new StringContent("image"), "type" },
                { new StringContent(layoutId.ToString()), "layoutId" },
                { new ByteArrayContent(File.ReadAllBytes(imagePath)), "media", Path.GetFileName(imagePath) }
            };

            var request = new HttpRequestMessage(HttpMethod.Post, $"{baseApiUrl}/layout/{layoutId}/region/1/playlist")
            {
                Content = multipartContent
            };

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

            try
            {
                var response = await _client.SendAsync(request);
                var responseBody = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"📨 Resposta UploadImageToLayout: {responseBody}");

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"❌ Erro no upload: {response.StatusCode}");
                }

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Exceção ao enviar imagem: {ex.Message}");
                return false;
            }
        }


        // Agendamentos
        public async Task<List<Agendamento>> GetAgendamentosAsync()
        {
            if (string.IsNullOrEmpty(_accessToken))
                throw new InvalidOperationException("Token inválido.");

            string from = DateTime.UtcNow.AddDays(-7).ToString("yyyy-MM-dd");
            string to = DateTime.UtcNow.ToString("yyyy-MM-dd");
            var url = $"{baseApiUrl}/schedule/events?from={from}&to={to}";

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

            var response = await _client.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();

            Console.WriteLine($"📨 GET {url} → {response.StatusCode}");
            Console.WriteLine(json);

            response.EnsureSuccessStatusCode();
            var wrapper = JsonSerializer.Deserialize<ScheduleResponse>(json);
            return wrapper?.Data ?? new List<Agendamento>();
        }

        public async Task<bool> AssignLayoutToDisplayAsync(int displayId, int layoutId)
        {
            var payload = new Dictionary<string, string>
    {
        { "layoutId", layoutId.ToString() },
        { "displayId", displayId.ToString() }
    };

            var request = new HttpRequestMessage(HttpMethod.Post, $"{baseApiUrl}/display/{displayId}/layout/{layoutId}")
            {
                Content = new FormUrlEncodedContent(payload)
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

            var response = await _client.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"📨 Resposta AssignLayout: {body}");

            return response.IsSuccessStatusCode;
        }
        public async Task<bool> CriarAgendamentoAsync(Agendamento novo)
        {
            if (string.IsNullOrEmpty(_accessToken))
                throw new InvalidOperationException("Token inválido.");

            var payload = new
            {
                eventTypeId = novo.EventTypeId,
                eventName = novo.EventName,
                fromDt = novo.FromDt,    // corrigido para usar FromDt
                toDt = novo.ToDt,      // corrigido para usar ToDt
                isPriority = novo.IsPriority ? 1 : 0,
                displayGroupIds = novo.DisplayGroupIds,
                layoutId = novo.LayoutId,
                campaignId = novo.CampaignId,
                commandId = novo.CommandId,
                displayOrder = novo.DisplayOrder
            };

            var jsonContent = JsonSerializer.Serialize(payload);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, $"{baseApiUrl}/schedule")
            {
                Content = content
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

            var response = await _client.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();

            Console.WriteLine("📨 Resposta completa:");
            Console.WriteLine(responseBody);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"❌ Erro ao criar agendamento: {response.StatusCode} ({(int)response.StatusCode})");
                return false;
            }

            Console.WriteLine("✅ Agendamento criado com sucesso.");
            return true;
        }
    }
}
