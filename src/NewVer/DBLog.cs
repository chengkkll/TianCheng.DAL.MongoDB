using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace TianCheng.DAL
{
    /// <summary>
    /// 日志操作
    /// </summary>
    public class DBLog
    {
        static private ILogger _Logger = null;

        static private LogLevel DebugLevel = LogLevel.Information;
        static private LogLevel ConsoleLevel = LogLevel.Warning;
        static private LogLevel FileLevel = LogLevel.Warning;
        static private string FileFormat = "Logs/TianCheng.DBOperation-{Date}.txt";
        static private string CategoryName = "MongoDB_Operation";
        /// <summary>
        /// 初始化日志
        /// </summary>
        static private void InitLogger()
        {
            ILoggerFactory loggerFactory = new LoggerFactory();

            loggerFactory.AddConsole(ConsoleLevel);         // 在控制台输出
            loggerFactory.AddDebug(DebugLevel);             // 在VS输出窗口输出
            loggerFactory.AddFile(FileFormat, FileLevel);   // 在文件中输出

            _Logger = loggerFactory.CreateLogger(CategoryName);
        }

        /// <summary>
        /// 更新日志的配置信息
        /// </summary>
        /// <param name="settingContent"></param>
        static public void UpdateLogSetting(string settingContent)
        {
            // 获取配置信息
            var debug = JObject.Parse(settingContent)["Logging"]?["Debug"]?["LogLevel"]?[CategoryName]?.Value<string>();
            DebugLevel = GetLevel(debug, LogLevel.Debug);
            var console = JObject.Parse(settingContent)["Logging"]?["Console"]?["LogLevel"]?[CategoryName]?.Value<string>();
            ConsoleLevel = GetLevel(console, LogLevel.Information);
            var file = JObject.Parse(settingContent)["Logging"]?["File"]?["LogLevel"]?[CategoryName]?.Value<string>();
            FileLevel = GetLevel(file, LogLevel.Warning);

            var fileFormat = JObject.Parse(settingContent)["Logging"]?["File"]?["FileNameFormat"]?[CategoryName]?.Value<string>();
            if(String.IsNullOrWhiteSpace(fileFormat))
            {
                FileFormat = "Logs/TianCheng.DBOperation-{Date}.txt";
            }
            else
            {
                FileFormat = fileFormat;
            }
            // 重置日志输出结果
            InitLogger();
        }

        /// <summary>
        /// 根据字符串转换成日志级别
        /// </summary>
        /// <param name="val"></param>
        /// <param name="defLevel"></param>
        /// <returns></returns>
        static private LogLevel GetLevel(string val, LogLevel defLevel = LogLevel.Warning)
        {
            if(String.IsNullOrWhiteSpace(val))
            {
                return defLevel;
            }
            val = val.ToLower();
            if (val.Length > 3)
            {
                val = val.Substring(0, 3);
            }

            switch (val)
            {
                case "tra": { return LogLevel.Trace; }
                case "deb": { return LogLevel.Debug; }
                case "inf": { return LogLevel.Information; }
                case "war": { return LogLevel.Warning; }
                case "err": { return LogLevel.Error; }
                case "cri": { return LogLevel.Critical; }
                default: { return defLevel; }
            }
        }

        /// <summary>
        /// 日志操作
        /// </summary>
        static public ILogger Logger
        {
            get
            {
                if (_Logger == null)
                {
                    InitLogger();
                }
                return _Logger;
            }
        }
    }
}
