using System;
using System.Collections.Generic;
using System.Text;

namespace VRChatActivityLogViewer.YoutubeApi
{
    /// <summary>
    /// YoutubeのoEmbedレスポンス
    /// </summary>
    public class YoutubeEmbedResponse
    {
        public string Title { get; set; }
        public string AuthorName { get; set; }
        public string AuthorUrl { get; set; }
        public string Type { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
        public string Version { get; set; }
        public string ProviderName { get; set; }
        public string ProviderUrl { get; set; }
        public int ThumbnailHeight { get; set; }
        public int ThumbnailWidth { get; set; }
        public string ThumbnailUrl { get; set; }
        public string Html { get; set; }
    }
}
