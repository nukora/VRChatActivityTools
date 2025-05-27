using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace VRChatActivityLogger
{
    class Program
    {
        // 遅延ロードなのでWindows以外でもコールしない限り例外にはならない模様
        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        private static extern bool AllocConsole();

        /// <summary>
        /// エントリポイント
        /// </summary>
        /// <param name="rawArgs"></param>
        static int Main(string[] rawArgs)
        {
            var logger = Logger.GetLogger();
            var args = new Argument(rawArgs)
            {
                NamedParameters = new Dictionary<string, string>
                {
                    { "console", "true" },
                }
            };
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && args.NamedParameters["console"].ToLower() != "false")
            {
                AllocConsole();
            }

            logger.Info("VRChatActivityLoggerを実行します。");

            var env = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? string.Empty;
            var config = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
                                                   .AddJsonFile("appsettings.json")
                                                   .AddJsonFile($"appsettings.{env}.json", true)
                                                   .Build();
            var app = new VRChatActivityLogger(new VRChatActivityToolsShared.Database.DbConfig(config));

            // VRCログファイルの検索先指定があればそちらを使う
            var vrc = config.GetSection("VRChat");
            if (vrc != null && !string.IsNullOrEmpty(vrc["LogFileDir"]))
            {
                app.VRChatLogFilePath = vrc["LogFileDir"];
            }
            var returnCode = app.Run();

            logger.Info("VRChatActivityLoggerを終了します。");

            return returnCode;
        }
    }
}
