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

        public List<SynonymSearchResultBackGroundInfo> SynonymSearchResultBackGroundInfos { get; set; }

        public string SynonymSearchFontColor { get; set; }

        public int SearchResultMargin { get; set; }

        public int SearchResultDisplayCount { get; set; }
    }

    [Serializable]
    public class SynonymSearchResultBackGroundInfo
    {
        public int Number { get; set; }

        public string Color { get; set; }
    }
}
