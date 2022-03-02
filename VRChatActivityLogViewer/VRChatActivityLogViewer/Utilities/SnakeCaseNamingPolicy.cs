using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace VRChatActivityLogViewer.Utilities
{
    public class SnakeCaseNamingPolicy : JsonNamingPolicy
    {
        public static SnakeCaseNamingPolicy Instance = new SnakeCaseNamingPolicy();

        private SnakeCaseNamingPolicy() { }

        public override string ConvertName(string name) =>
            Regex.Replace(name, @"([a-z])([A-Z])", @"$1_$2").ToLowerInvariant();
    }
}
