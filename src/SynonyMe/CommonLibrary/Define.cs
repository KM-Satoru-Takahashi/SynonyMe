using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynonyMe.CommonLibrary
{

    /// <summary>AvalonEditで表示する背景色定義</summary>
    /// <remarks>BackGroundとするとHighlightManagerで紛らわしい
    /// 将来は詳細に定義することも想定するが、現状は白黒の区別のみつける</remarks>
    internal enum WallPaperColor
    {
        Black = 0,
        White = 1
    }

    /// <summary>固定値やenum設定クラス</summary>
    internal static class Define
    {
        /// <summary>SynonyMeで使用するDBファイル名(含:拡張子)</summary>
        internal const string DB_NAME = "SynonymData.db";

        /// <summary>類語グループリストのDBテーブル名</summary>
        internal const string DB_TABLE_SYNONYM_GROUP = "SynonymGroup";

        /// <summary>類語リストのDBテーブル名</summary>
        internal const string DB_TABLE_SYNONYM_WORDS = "SynonymWords";

        /// <summary>グループIDの最小値</summary>
        internal const int MIN_GROUPID = 1;

        /// <summary>類語IDの最小値</summary>
        internal const int MIN_WORDID = 1;

        /// <summary>データ登録・更新・削除実行時の成功閾値</summary>
        internal const int EXECUTE_NONQUERY_SUCCESSED = 1;

        /// <summary>テキストハイライト用の設定ファイル名</summary>
        internal const string XSHD_FILENAME = "work.xshd";

        /// <summary>このexe名</summary>
        internal const string SYNONYME_EXENAME = "SynonyMe.exe";

        /// <summary>各種サブウィンドウ名</summary>
        internal enum SubWindowName
        {
            /// <summary>類語登録・編集・削除ウィンドウ</summary>
            SynonymWindow,

            /// <summary>設定ウィンドウ</summary>
            SettingWindow
        }

        /// <summary>GetWindowLongで使用するAppのインスタンスのハンドル値</summary>
        internal const int GWL_STYLE = -16;

        /// <summary>タイトルバーにコントロールがあるWindow</summary>
        internal const int WS_SYSMENU = 0x80000;
    }
}
