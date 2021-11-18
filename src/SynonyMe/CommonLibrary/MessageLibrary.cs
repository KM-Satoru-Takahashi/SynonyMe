using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynonyMe.CommonLibrary
{
    /// <summary>各種文言を規定するクラス</summary>
    /// <remarks>将来的には文言ファイルとして外出しする想定だが、優先度低いのでここにまとめる</remarks>
    internal class MessageLibrary
    {
        #region button

        /// <summary>類語検索ボタン文字列</summary>
        internal const string SearchSynonymButtonText = "類語検索";

        /// <summary>検索ボタン表示文字列</summary>
        internal const string SearchButtonText = "検索";







        #endregion

        #region list header

        #region MainWindow

        internal const string MainWindowSynonymGroupName = "類語グループ名";

        internal const string MainWindowSynonymGroupLastUpdate = "最終更新日";

        internal const string MainWindowSynonymWordHeader = "類語";

        internal const string MainWindowSynonymWordRepeatCountHeader = "連続回数";

        internal const string MainWindowSynonymWordUsingCountHeader = "合計回数";

        internal const string MainWindowSynonymWordSectionHeader = "使用箇所";


        #endregion

        #endregion

        #region label

        /// <summary>ウィンドウタイトル</summary>
        internal const string MainWindowTitle = "SynonyMe";

        /// <summary>文字数表示の固定値「文字数」</summary>
        internal const string WordCountText = "文字数：";


        #endregion
    }
}
