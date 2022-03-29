using SynonyMe.CommonLibrary.Log;
using SynonyMe.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynonyMe.Model
{
    /// <summary>検索関連の処理を行うクラス</summary>
    internal sealed class Searcher
    {
        private const string CLASS_NAME = "Searcher";

        private static Searcher _searcher = new Searcher();

        /// <summary>検索処理実現クラス</summary>
        internal static Searcher GetSearcher
        {
            get
            {
                return _searcher;
            }
        }

        /// <summary>シングルトン担保</summary>
        private Searcher()
        { }

        /// <summary>
        /// 検索結果の全Indexを取得する
        /// </summary>
        /// <param name="searchWord">検索語句</param>
        /// <param name="targetText">対象文章</param>
        /// <returns></returns>
        internal int[] GetAllSearchResultIndex(string searchWord, string targetText, int maxResultCount)
        {
            if (string.IsNullOrEmpty(searchWord) || string.IsNullOrEmpty(targetText))
            {
                Logger.Fatal(CLASS_NAME, "GetAllSearchResultIndex", "args is null or empty!");
                return null;
            }

            // 文書中で該当するインデックスを一旦入れておくリストを用意
            List<int> searchResultIndexList = new List<int>();

            // 1箇所目をまず探す
            int foundIndex = targetText.IndexOf(searchWord);
            if (foundIndex < 0)
            {
                // 検索したが何もない場合はエラーではないので空の配列を戻すようにする
                return new int[0];
            }

            // 他の箇所を繰り返し探していく
            int resultCount = 1;
            while (0 <= foundIndex) // 該当がなくなると検索結果インデックスは-1が戻ってくる
            {
                // 検索結果は規定値まで
                if (maxResultCount < resultCount)
                {
                    break;
                }

                // 最初に[前回の検索結果インデックス]をリストに追加しておく
                // 1箇所目も登録される
                searchResultIndexList.Add(foundIndex);

                // 次の検索位置は「前の検索位置」に「検索対象の語句の長さ」を足した地点
                int nextIndex = foundIndex + searchWord.Length;
                if (nextIndex < targetText.Length)
                {
                    foundIndex = targetText.IndexOf(searchWord, nextIndex);
                }
                else
                {
                    // 文章の長さを超えるなら、検索しない
                    break;
                }

                ++resultCount;
            }

            return searchResultIndexList.ToArray();

        }

        /// <summary>検索処理を実施する</summary>
        /// <param name="searchWord">検索語句</param>
        /// <param name="targetText">検索対象の文章</param>
        /// <param name="margin">検索結果として、対象語句の前後何文字を含めるか</param>
        /// <returns>文章内の検索対象index, margin含めた検索結果のdictionary</returns>
        internal Dictionary<int, string> SearchAllWordsInText(string searchWord, string targetText, int margin, int maxResultCount)
        {
            Logger.Info(CLASS_NAME, "SearchAllWordsInText", $"start. searchWord:[{searchWord}], margin:[{margin}]");

            // check args
            if (string.IsNullOrEmpty(searchWord))
            {
                Logger.Fatal(CLASS_NAME, "SearchAllWordsInText", "searchWord is null or empty!");
                return null;
            }
            else if (string.IsNullOrEmpty(targetText))
            {
                Logger.Fatal(CLASS_NAME, "SearchAllWordsInText", "targetText is null or empty!");
                return null;
            }
            else if (margin < 0 /*最大値は現状未定、最小値も設定ファイルや定数で外出しする予定だが、現状ハードコーティングとする*/)
            {
                Logger.Fatal(CLASS_NAME, "SearchAllWordsInText", $"margin is incorrect! value:[{margin}]");
                return null;
            }

            // 検索対象語句のインデックスを取得する
            int[] searchResultIndexArray = Searcher.GetSearcher.GetAllSearchResultIndex(searchWord, targetText, maxResultCount);
            if (searchResultIndexArray == null)
            {
                // nullは異常な場合
                Logger.Fatal(CLASS_NAME, "SearchAllWordsInText", "searchResultIndexArray is null!");
                return null;
            }
            else if (searchResultIndexArray.Any() == false)
            {
                // 検索したが結果が無い場合はEmptyを返す
                Logger.Info(CLASS_NAME, "SearchAllWordsInText", "No search result.");
                return new Dictionary<int, string>();
            }
            int searchResultCount = searchResultIndexArray.Count();

            string[] searchResultWordArray = GetAllSearchResultWords(searchResultIndexArray, searchWord, targetText, margin);
            if (searchResultWordArray == null)
            {
                Logger.Fatal(CLASS_NAME, "SearchAllWordsInText", "searchResultWordsArray is null!");
                return null;
            }
            else if (searchResultWordArray.Any() == false)
            {
                // 検索したが結果が無い場合はEmptyを返す
                Logger.Error(CLASS_NAME, "SearchAllWordsInText", "searchResultWordsArray is empty!");
                return new Dictionary<int, string>();
            }

            // 最終的にDictionaryで返せばよくない？
            Dictionary<int/*index*/, string/*result*/> searchResultIndexWordPairs = new Dictionary<int, string>();
            for (int i = 0; i < searchResultCount; ++i)
            {
                searchResultIndexWordPairs.Add(searchResultIndexArray[i], searchResultWordArray[i]);
            }

            return searchResultIndexWordPairs;
        }

        /// <summary>先頭から順に対象語句を検索し、マージンを考慮した全検索結果を取得する</summary>
        /// <param name="allIndexArray"></param>
        /// <param name="searchWord"></param>
        /// <param name="targetText"></param>
        /// <returns></returns>
        private string[] GetAllSearchResultWords(int[] allIndexArray, string searchWord, string targetText, int margin)
        {
            #region check args

            if (allIndexArray == null || allIndexArray.Any() == false)
            {
                return null;
            }
            else if (string.IsNullOrEmpty(searchWord) || string.IsNullOrEmpty(targetText))
            {
                return null;
            }
            else if (margin < 0)
            {
                return null;
            }

            #endregion

            // 実際にテキストから、Viewに表示対象となる語句領域を切り取っていく
            // インデックス分だけ必ずあるはず
            int searchResultCount = allIndexArray.Count();
            string[] searchResultWordArray = new string[searchResultCount];
            for (int targetIndex = 0; targetIndex < searchResultCount; ++targetIndex)
            {
                // 手前側マージン
                int frontMargin = allIndexArray[targetIndex] - margin;
                // 後ろ側マージン→インデックス＋検索対象語句＋マージン
                int behindMargin = allIndexArray[targetIndex] + searchWord.Length + margin;

                // 後ろのマージンがなくても、最後の検索とは限らないので、foreachは続けること
                // 例：「あああああああ」で「あ」だけを検索した場合
                if (frontMargin < 0 && targetText.Length < behindMargin + 1) // LengthとIndexを比較するのでIndexに+1しておく
                {
                    // 手前に規定値分のマージンがなく、後ろにも規定値分のマージンがない場合
                    // 「文字列の最初～文字列の最後」までを切り取る→検索対象の文字列をそのまま入れ込む
                    searchResultWordArray[targetIndex] = targetText;
                }
                else if (frontMargin < 0)
                {
                    // 手前に規定値分のマージンがなく、後ろには規定値分のマージンがある場合
                    // 「文字列の最初～インデックス＋検索対象語句＋後ろのマージン」だけ切り取る
                    searchResultWordArray[targetIndex] = targetText.Substring(0, searchWord.Length + margin); // substringの第2引数は切り取る文字数
                }
                else if (targetText.Length < behindMargin + 1)
                {
                    // 手前に規定値分のマージンがあり、後ろには規定値分のマージンがない場合
                    // 「手前のマージン～文字列の最後」までを切り取る
                    searchResultWordArray[targetIndex] = targetText.Substring(frontMargin);

                }
                else
                {
                    // 手前に規定値分のマージンがあり、後ろにも規定値分のマージンがある場合
                    // 「手前のマージン～インデックス＋検索対象語句＋後ろのマージン」だけ切り取る
                    // marginを2倍しておかないと手前のmargin分しか切り取れない
                    searchResultWordArray[targetIndex] = targetText.Substring(frontMargin, searchWord.Length + 2 * margin);
                }
            }

            return searchResultWordArray;
        }

        /// <summary>渡された全類語を対象の文章内から検索する</summary>
        /// <param name="targetSynonymWords">検索対象の全類語</param>
        /// <param name="targetText">検索先の文章</param>
        /// <returns>正常時：検索結果、異常時：null</returns>
        internal List<MainWindowVM.DisplaySynonymSearchResult> GetAllSynonymSearchResult(MainWindowVM.DisplaySynonymWord[] targetSynonymWords, string targetText, int margin, int maxResultCount)
        {
            // 結果返却用のListを用意
            List<MainWindowVM.DisplaySynonymSearchResult> synonymSearchResults
                = new List<MainWindowVM.DisplaySynonymSearchResult>();

            foreach (MainWindowVM.DisplaySynonymWord target in targetSynonymWords)
            {
                if (target == null)
                {
                    Logger.Error(CLASS_NAME, "GetAllSynonymSearchResult", "target is null!");
                    continue;
                }

                int[] allIndexinText = Searcher.GetSearcher.GetAllSearchResultIndex(target.SynonymWord, targetText, maxResultCount);
                if (allIndexinText == null)
                {
                    Logger.Fatal(CLASS_NAME, "GetAllSynonymSearchResult", "allIndexInText is null or empty!");
                    return null;
                }
                else if (allIndexinText.Any() == false)
                {
                    //todo:log
                    continue;
                }

                // todo:marginはハードコーディングになっているので、設定ファイルに外だしなどする
                string[] allResultinText = GetAllSearchResultWords(allIndexinText, target.SynonymWord, targetText, margin);
                if (allResultinText == null || allResultinText.Any() == false)
                {
                    Logger.Fatal(CLASS_NAME, "GetAllSynonymSearchResult", "allResultinText is null or empty!");
                    return null;
                }

                // alIndexとallResultは先頭から順に対応しているはずなので、それをペアにしてDicに入れ込んでいく
                // 個数が異なったら何かがおかしい
                if (allIndexinText.Count() != allResultinText.Count())
                {
                    Logger.Fatal(CLASS_NAME, "GetAllSynonymSearchResult", $"index and result is incorrect. index:[{allIndexinText.Count()}], result[{allResultinText.Count()}]");
                    return null;
                }

                for (int i = 0; i < allResultinText.Count(); ++i)
                {
                    synonymSearchResults.Add(
                        new MainWindowVM.DisplaySynonymSearchResult()
                        {
                            SynonymWord = target.SynonymWord,
                            UsingSection = allResultinText[i],
                            Index = allIndexinText[i]
                        }
                        );
                }
            }
            return synonymSearchResults;
        }

        /// <summary>類語検索処理を実施する</summary>
        /// <param name="groupId">選択中の類語グループID</param>
        /// <param name="targetText">対象（表示中）テキスト</param>
        /// <returns>結果配列</returns>
        internal MainWindowVM.DisplaySynonymSearchResult[] SynonymSearch(MainWindowVM.DisplaySynonymWord[] targetSynonyms, string targetText, int margin, int maxResultcount)
        {
            #region check args

            if (targetSynonyms == null || targetSynonyms.Any() == false)
            {
                Logger.Fatal(CLASS_NAME, "SynonymSearch", "targetSynonyms are null");
                return null;
            }

            if (string.IsNullOrEmpty(targetText))
            {
                Logger.Fatal(CLASS_NAME, "SynonymSearch", "targetText is null");
                return null;
            }

            #endregion

            Logger.Info(CLASS_NAME, "SynonymSearch", $"start. targetSynonyms count is {targetSynonyms.Count()}");

            // 類語の全検索結果を取得
            List<MainWindowVM.DisplaySynonymSearchResult> unsortedSynonymSearchResults
                = GetAllSynonymSearchResult(targetSynonyms, targetText, margin, maxResultcount);

            // Index順にSortして配列化する。昇順であることを保証したいが、動作が不安定になる場合は
            // 将来的にSortedDictionaryとOrderdDictionaryの使用を考える
            MainWindowVM.DisplaySynonymSearchResult[] sortedSynonymSearchResultArray
                = unsortedSynonymSearchResults.OrderBy(result => result.Index).ToArray();

            // 参照型を値渡しして、resultのRepeatCountとUsingCountを取得してreturnする
            // 参照型の参照渡しをするとインスタンスごと書き換えられるリスクがあるので許容しない
            AdjustRepeatCountAndUsingCount(sortedSynonymSearchResultArray);
            return sortedSynonymSearchResultArray;
        }

        /// <summary>渡された類語検索結果に基づいて、内部のRepeatCountとUsingCountを計算する</summary>
        /// <param name="sortedSynonymSearchResult">indexで昇順ソート済みの類語検索結果配列</param>
        /// <remarks>引数の連続した2要素を参照するため、ソート済みでないと結果がおかしくなる</remarks>
        private void AdjustRepeatCountAndUsingCount(MainWindowVM.DisplaySynonymSearchResult[] sortedSynonymSearchResult)
        {
            // 初回はRepeatCountもUsingCountも0確定のため、繰り返しのindexは1から開始する
            for (int index = 1; index < sortedSynonymSearchResult.Count(); ++index)
            {
                // アクセス負荷軽減のため、一旦ローカルに取り出す
                MainWindowVM.DisplaySynonymSearchResult indexEntity = sortedSynonymSearchResult[index];

                // 直前に存在しているか否か（RepeatCount）
                MainWindowVM.DisplaySynonymSearchResult preIndexEntity = sortedSynonymSearchResult[index - 1];
                if (indexEntity.SynonymWord == preIndexEntity.SynonymWord)
                {
                    // 直前にヒットしていた結果の類語が、現在の類語と同じであれば、繰り返し回数を+1する
                    indexEntity.RepeatCount = preIndexEntity.RepeatCount;
                    ++indexEntity.RepeatCount;
                }
                else
                {
                    // 直前にヒットしていた結果の類語が、現在の類語と異なるなら、繰り返し回数を0とする
                    indexEntity.RepeatCount = 0;
                }

                // これまでに何回ヒットしているか（UsingCount）
                // これまでに何個同じSynonymWordが存在しているかと同義になる
                int usingCount = sortedSynonymSearchResult.Count(
                    entity => entity != null &&                             // nullチェック
                              entity.Index < indexEntity.Index &&           // 現在の要素以前
                              entity.SynonymWord == indexEntity.SynonymWord // 現在の類語と同じである
                    );
                indexEntity.UsingCount = usingCount;
            }

            return;
        }


    }
}
