using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using VRChatActivityLogViewer.Utilities;

namespace VRChatActivityLogViewer.YoutubeApi
{
    /// <summary>
    ///  Youtube oEmbed APIへのアクセスを提供するサービス
    /// </summary>
    public class YoutubeApiService
    {
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
        private static string userAgent = "Wget/1.20.3";

        static YoutubeApiService()
        {
            jsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = SnakeCaseNamingPolicy.Instance,
                
            };

            client.DefaultRequestHeaders.Add("User-Agent", userAgent);
        }

        /// <summary>
        /// 動画のoEmbed情報を取得する
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<YoutubeEmbedResponse> GetOEmbedAsync(string id)
        {
            var uri = $"https://www.youtube.com/oembed?url=https://www.youtube.com/watch?v={id}&format=json";

            var response = await client.GetAsync(uri);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var oEmbedResponse = JsonSerializer.Deserialize<YoutubeEmbedResponse>(json, jsonSerializerOptions);

                return oEmbedResponse;
            }

            return null;
        }
    }
}
