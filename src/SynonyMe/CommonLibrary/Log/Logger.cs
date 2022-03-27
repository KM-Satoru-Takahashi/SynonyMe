
using log4net;

namespace SynonyMe.CommonLibrary.Log
{    /// <summary>ログ出力用のlog4net使用クラス</summary>
    internal static class Logger
    {
        /// <summary>ログ出力機構(Logプロパティから取得して使用してください)</summary>
        private static ILog _log = null;
        /// <summary>ログ出力機構</summary>
        private static ILog Log
        {
            get
            {
                if (_log == null)
                {
                    _log = LogManager.GetLogger("SynonyMeLog");
                }
                return _log;
            }
        }

        /// <summary>各種ログ出力時、引数がnullだった場合のエラーログを出力します</summary>
        private static void WriteArgsErrorLog()
        {
            const int beforeMethod = 1;
            System.Diagnostics.StackFrame callerFrame = new System.Diagnostics.StackFrame(beforeMethod);
            System.Reflection.MethodBase callerMethod = callerFrame.GetMethod();
            string log = "WriteLogArgs is null! : " + callerMethod.Name;
            Log.Error(log);
        }

        /// <summary>標準ログ[info]を出力します</summary>
        internal static void Info(string className, string methodName, string message)
        {
            if (string.IsNullOrEmpty(message) || string.IsNullOrEmpty(className) || string.IsNullOrEmpty(methodName))
            {
                WriteArgsErrorLog();
            }

            Log.Info($"ClassName:[{className}], MethodName:[{methodName}], {message}");
        }

        /// <summary>警告ログ[Warn]を出力します</summary>
        /// <param name="className"></param>
        /// <param name="methodName"></param>
        /// <param name="message"></param>
        internal static void Warn(string className, string methodName, string message)
        {
            if (string.IsNullOrEmpty(message) || string.IsNullOrEmpty(className) || string.IsNullOrEmpty(methodName))
            {
                WriteArgsErrorLog();
            }

            Log.Warn($"ClassName:[{className}], MethodName:[{methodName}], {message}");
        }

        /// <summary>エラーログ[Error]を出力します</summary>
        internal static void Error(string className, string methodName, string message)
        {
            if (string.IsNullOrEmpty(message) || string.IsNullOrEmpty(className) || string.IsNullOrEmpty(methodName))
            {
                WriteArgsErrorLog();
            }

            Log.Error($"ClassName:[{className}], MethodName:[{methodName}], {message}");
        }

        /// <summary>致命ログ[Fatal]を出力します</summary>
        internal static void Fatal(string className, string methodName, string message)
        {
            if (string.IsNullOrEmpty(message) || string.IsNullOrEmpty(className) || string.IsNullOrEmpty(methodName))
            {
                WriteArgsErrorLog();
            }

            Log.Fatal($"ClassName:[{className}], MethodName:[{methodName}], {message}");
        }

        /// <summary>デバッグログ[debug]を出力します</summary>
        internal static void Debug(string className, string methodName, string message)
        {
            if (string.IsNullOrEmpty(message) || string.IsNullOrEmpty(className) || string.IsNullOrEmpty(methodName))
            {
                WriteArgsErrorLog();
            }

            Log.Debug($"ClassName:[{className}], MethodName:[{methodName}], {message}");
        }
    }
}
