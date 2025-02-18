using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Reflection;

namespace VRChatActivityLogViewer.VRChatApi
{
    /// <summary>
    /// VRChatAPIへのアクセスを提供するサービス
    /// </summary>
    public class VRChatApiService
    {
        /// <summary>
        /// VRCAPIのキー
        /// </summary>
        private readonly string apiKey = "JlE5Jldo5Jibnk5O5hTx6XVqsJu4WJ26";

        /// <summary>
        /// HttpClient
        /// </summary>
        private static HttpClient client = new HttpClient();

        /// <summary>
        /// JsonSerializerOptions
        /// </summary>
        private static JsonSerializerOptions jsonSerializerOptions;

        /// <summary>
        /// ユーザーエージェント文字列
        /// </summary>
        private const string UserAgent = "VRChatActivityLogViewer/{Version} nukora";

        static VRChatApiService()
        {
            jsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };

            var version = Assembly.GetExecutingAssembly().GetName().Version;
            var replacedUserAgent = UserAgent.Replace("{Version}", $"{version.Major}.{version.Minor}.{version.Build}");

            client.DefaultRequestHeaders.Add("User-Agent", replacedUserAgent);
        }

        /// <summary>
        /// ワールド情報を取得する
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<World> GetWorldAsync(string id)
        {
            var uri = $"https://api.vrchat.cloud/api/1/worlds/{id}?apiKey={apiKey}";

            var response = await client.GetAsync(uri);
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var world = JsonSerializer.Deserialize<World>(json, jsonSerializerOptions);

                return world;
            }

            return null;
        }
    }
}
