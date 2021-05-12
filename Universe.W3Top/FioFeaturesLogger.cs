using System;
using Microsoft.Extensions.Logging;
using Universe.FioStream;

namespace Universe.W3Top
{
    class FioFeaturesLogger : IPicoLogger
    {
        private readonly ILoggerFactory LoggerFactory;
        private Lazy<ILogger> _Logger;
        private ILogger Logger => _Logger.Value;

        private FioFeaturesLogger()
        {
            _Logger = new Lazy<ILogger>(() => LoggerFactory.CreateLogger("fio-features"));
        }

        public FioFeaturesLogger(ILoggerFactory loggerFactory) : this()
        {
            LoggerFactory = loggerFactory;
        }

        public void LogInfo(string message)
        {
            Logger.LogInformation(message);
        }

        public void LogWarning(string message)
        {
            Logger.LogWarning(message);
        }
    }
}