using SynonyMe.CommonLibrary.Log;
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
        internal static Searcher GetSearcher
        {
            get
            {
                return _searcher;
            }
        }

        private Searcher()
        {
        }

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
    }
}
