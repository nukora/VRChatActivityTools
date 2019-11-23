using NLog;
using System;
using System.Collections.Generic;
using System.Text;

namespace VRChatActivityLogger
{
    /// <summary>
    /// Loggerクラス
    /// </summary>
    static class Logger
    {
        static private bool isInitialized = false;
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Loggerを取得します。
        /// </summary>
        /// <returns></returns>
        static public NLog.Logger GetLogger()
        {
            if (!isInitialized) Initialize();
            return logger;
        }

        /// <summary>
        /// Loggerの初期化
        /// </summary>
        static private void Initialize()
        {
            var config = new NLog.Config.LoggingConfiguration();
            var fileTarget = new NLog.Targets.FileTarget()
            {
                FileName = System.Environment.CurrentDirectory + "/Logs/VRChatActivityLogger/${shortdate}.log",
                Layout = "${longdate} [${uppercase:${level}}] ${message}",
            };
            var consoleTarget = new NLog.Targets.ConsoleTarget()
            {
                Layout = "${message}",
            };
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, fileTarget);
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, consoleTarget);
            NLog.LogManager.Configuration = config;
            isInitialized = true;
        }
    }
}
