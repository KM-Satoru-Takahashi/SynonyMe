using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynonyMe.CommonLibrary
{
    /// <summary>C#のSystemまわりのメソッドで必要なものを実装し、提供するクラス</summary>
    internal static class SystemUtility
    {
        /// <summary>
        /// 起動中exe(SynonyMe.exe)の絶対パスを取得します
        /// </summary>
        /// <returns>取得できなかった場合はnull</returns>
        internal static string GetSynonymeExeFilePath()
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            if(assembly == null)
            {
                // error log
                return null;
            }

            string synonymeExePath = System.Reflection.Assembly.GetExecutingAssembly().Location;

            if (string.IsNullOrEmpty(synonymeExePath))
            {
                // error log
                return null;
            }

            return synonymeExePath;
        }

    }
}
