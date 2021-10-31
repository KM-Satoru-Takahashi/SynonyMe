using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using log4net;

namespace SynonyMe.CommonLibrary.Log
{
    /// <summary>ログ出力用のlog4net使用クラス</summary>
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
        /// <param name="log"></param>
        internal static void WriteStandardLog(string log)
        {
            if (string.IsNullOrEmpty(log))
            {
                WriteArgsErrorLog();
            }

            Log.Info(log);
        }

        /// <summary>エラーログ[Error]を出力します</summary>
        /// <param name="log"></param>
        internal static void WriteErrorLog(string log)
        {
            if (string.IsNullOrEmpty(log))
            {
                WriteArgsErrorLog();
            }

            Log.Error(log);
        }

        /// <summary>致命ログ[Fatal]を出力します</summary>
        /// <param name="log"></param>
        internal static void WriteFatalLog(string log)
        {
            if (string.IsNullOrEmpty(log))
            {
                WriteArgsErrorLog();
            }

            Log.Fatal(log);
        }

        /// <summary>デバッグログ[debug]を出力します</summary>
        /// <param name="log"></param>
        internal static void WriteDebugLog(string log)
        {
            if (string.IsNullOrEmpty(log))
            {
                WriteArgsErrorLog();
            }

            Log.Debug(log);
        }
    }
}
