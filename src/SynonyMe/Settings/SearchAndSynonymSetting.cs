using SynonyMe.CommonLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynonyMe.Settings
{
    [Serializable]
    public class SearchAndSynonymSetting
    {
        public string SearchResultBackGroundColor { get; set; }

        public string SearchResultFontColor { get; set; }

        public FontColorKind SearchResultFontColorKind { get; set; }

        //todo:将来はリスト化して持たせることも想定するが、
        //現状xamlも繰り返し配置ではなく都度要素を配置しているので、
        //冗長で正直な書き方にしておく
        public string SynonymSearchResultColor1 { get; set; }
        public string SynonymSearchResultColor2 { get; set; }
        public string SynonymSearchResultColor3 { get; set; }
        public string SynonymSearchResultColor4 { get; set; }
        public string SynonymSearchResultColor5 { get; set; }
        public string SynonymSearchResultColor6 { get; set; }
        public string SynonymSearchResultColor7 { get; set; }
        public string SynonymSearchResultColor8 { get; set; }
        public string SynonymSearchResultColor9 { get; set; }
        public string SynonymSearchResultColor10 { get; set; }

        public string SynonymSearchFontColor { get; set; }

        public FontColorKind SynonymSearchFontColorKind { get; set; }

        public int SearchResultMargin { get; set; }

        public int SearchResultDisplayCount { get; set; }
    }

}
