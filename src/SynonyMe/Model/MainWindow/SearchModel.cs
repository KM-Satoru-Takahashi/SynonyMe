using SynonyMe.CommonLibrary.Entity;
using SynonyMe.CommonLibrary.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SynonyMe.Model.MainWindow
{
    /// <summary>MainWindowModelから検索関連の処理を依頼されるModel</summary>
    internal class SearchModel
    {
        /// <summary>紐付く上位Model</summary>
        private MainWindowModel _parent;

        private const string CLASS_NAME = "SearchModel";

        /// <summary>コンストラクタ</summary>
        /// <param name="parent">MainWindowModel</param>
        public SearchModel(MainWindowModel parent)
        {
            _parent = parent;
        }

        /// <summary>検索を行います</summary>
        /// <param name="searchWord"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        internal bool Search(string searchWord, string text)
        {
            if (_parent == null)
            {
                Logger.Fatal(CLASS_NAME, "Search", "_parent is null!");
                return false;
            }

            if (_parent.ViewModel == null)
            {
                Logger.Fatal(CLASS_NAME, "Search", "ViewModel is null!");
                return false;
            }

            if (string.IsNullOrEmpty(searchWord))
            {
                Logger.Error(CLASS_NAME, "Search", "SearchWord is null or empty!");
                return false;
            }

            // 旧検索結果をクリアする
            if (_parent.ViewModel.SearchResult == null)
            {
                Logger.Error(CLASS_NAME, "Search", "SearchResult is null!");
                return false;
            }

            _parent.ViewModel.SearchResult.Clear();

            // dicのintはindex部分なので本文キャレット移動、stringは結果表示リストに使用する
            Dictionary<int, string> indexWordPairs = Searcher.GetSearcher.SearchAllWordsInText(searchWord, text, _parent.SearchResultMargin, _parent.SearchResultCount);
            if (UpdateSearchResultVisiblity(indexWordPairs) == false)
            {
                Logger.Error(CLASS_NAME, "Search", "UpdateSearchResultVisibility return false!");
                return false;
            }

            if (indexWordPairs == null || indexWordPairs.Count == 0)
            {
                // 検索結果なければ、既存の検索結果クリア＋ハイライトクリアで処理を終える
                //todo
                Logger.Info(CLASS_NAME, "Search", "No search result!");
                return true;
            }

            // 念のため昇順にソートしておく
            indexWordPairs.OrderBy(pair => pair.Key);

            SearchResultEntity[] searchResults = new SearchResultEntity[indexWordPairs.Count];
            foreach (KeyValuePair<int, string> kvp in indexWordPairs)
            {
                _parent.ViewModel.SearchResult.Add(
                    new SearchResultEntity()
                    {
                        Index = kvp.Key,
                        DisplayWord = kvp.Value
                    }
                    );
            }

            // 検索結果にハイライトをかける
            ApplyHighlightToSearchResult(searchWord);

            return true;
        }


        /// <summary>検索結果表示領域のVisibilityを更新します</summary>
        /// <returns>true:検索結果あり、false;検索結果なし</returns>
        private bool UpdateSearchResultVisiblity(Dictionary<int, string> searchResult)
        {
            if (searchResult == null)
            {
                // nullなら表示を隠す
                _parent.ViewModel.SearchResultVisibility = Visibility.Hidden;
                return false;
            }
            else if (searchResult.Count < 1)
            {
                // 検索結果がなければ、その旨を表示する
                _parent.ViewModel.NoSearchResultVisibility = Visibility.Visible;
                _parent.ViewModel.SearchResultVisibility = Visibility.Hidden;
                return false;
            }
            else
            {
                // 検索結果ありの場合、結果を表示できるようにする
                _parent.ViewModel.NoSearchResultVisibility = Visibility.Hidden;
                _parent.ViewModel.SearchResultVisibility = Visibility.Visible;
            }

            return true;
        }


        /// <summary>指定された語句にハイライトを適用します</summary>
        /// <param name="target">対象語句</param>
        /// <returns>true:成功, false:失敗</returns>
        private bool ApplyHighlightToSearchResult(string target)
        {
            if (string.IsNullOrEmpty(target))
            {
                Logger.Fatal(CLASS_NAME, "ApplyHighlightToTarget", "target is null or empty!");
                return false;
            }

            string[] targets = new string[1]
            {
                target
            };

            return ApplyHighlightToTargets(targets, CommonLibrary.ApplyHighlightKind.Search);
        }


        /// <summary>ハイライトを対象語句にそれぞれ適用します</summary>
        /// <param name="targets">対象語句</param>
        /// <returns>true:成功, false:失敗</returns>
        internal bool ApplyHighlightToTargets(string[] targets, CommonLibrary.ApplyHighlightKind kind) //todo:検索か類語検索かの判別しないとFontColorとBackGroundが分けられない
        {
            if (targets == null || targets.Any() == false)
            {
                Logger.Fatal(CLASS_NAME, "ApplyHighlightToTargets", "targets is null or empty!");
                return false;
            }

            if (_parent.HighlightManager == null)
            {
                Logger.Fatal(CLASS_NAME, "ApplyHighlightToTargets", "HighlightManager is null!");
                return false;
            }

            return _parent.HighlightManager.UpdateXshdFile(targets, kind);
        }



    }
}
