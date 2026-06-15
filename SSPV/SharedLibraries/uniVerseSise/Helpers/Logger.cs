namespace uniVerseSise.Helpers
{
    public class Logger : Singleton<Logger>
    {
        #region Propiedades

        private NLog.Logger logger;

        #endregion

        #region Singleton

        private Logger()
        {
            logger = NLog.LogManager.GetCurrentClassLogger();
        }

        #endregion

        #region Metodos

        public void LogDebug(object message)
        {
            logger.Log(NLog.LogLevel.Debug, message);
        }

        public void LogError(object message)
        {
            logger.Log(NLog.LogLevel.Error, message);
        }

        public void LogFatal(object message)
        {
            logger.Log(NLog.LogLevel.Fatal, message);
        }

        public void LogInfo(object message)
        {
            logger.Log(NLog.LogLevel.Info, message);
        }

        public void LogTrace(object message)
        {
            logger.Log(NLog.LogLevel.Trace, message);
        }

        public void LogWarning(object message)
        {
            logger.Log(NLog.LogLevel.Warn, message);
        }

        public void Log(NLog.LogLevel level, object message)
        {
            logger.Log(level, message);
        }

        #endregion
    }
}
