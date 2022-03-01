using System;
using System.Collections.Generic;

namespace VRChatActivityLogger
{
    class Program
    {
        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        private static extern bool AllocConsole();

        /// <summary>
        /// エントリポイント
        /// </summary>
        /// <param name="rawArgs"></param>
        static int Main(string[] rawArgs)
        {

            var args = new Argument(rawArgs)
            {
                NamedParameters = new Dictionary<string, string> {
                    { "console", "true" },
                }
            };
            if (args.NamedParameters["console"].ToLower() != "false")
            {
                AllocConsole();
            }

            var logger = Logger.GetLogger();
            logger.Info("VRChatActivityLoggerを実行します。");

            var app = new VRChatActivityLogger();
            app.VRChatLogFilePath = "./Test/";
            var returnCode = app.Run();

            logger.Info("VRChatActivityLoggerを終了します。");

            return returnCode;
        }
    }
}
