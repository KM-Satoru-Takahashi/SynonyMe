using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynonyMe.CommonLibrary
{
    /// <summary>固定値やenum設定クラス</summary>
    internal static class Define
    {
        /// <summary>SynonyMeで使用するDBファイル名(含:拡張子)</summary>
        internal const string DB_NAME = "SynonymData.db";

        /// <summary>各種サブウィンドウ名</summary>
        internal enum SubWindowName
        {
            /// <summary>類語登録・編集・削除ウィンドウ</summary>
            SynonymWindow
        }
    }
}
