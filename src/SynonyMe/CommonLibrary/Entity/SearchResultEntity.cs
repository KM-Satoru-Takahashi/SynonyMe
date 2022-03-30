using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynonyMe.CommonLibrary.Entity
{
    /// <summary>語句検索結果1つに対して1つ対応するEntity</summary>
    public class SearchResultEntity
    {
        public int Index { get; set; }

        public string DisplayWord { get; set; }
    }
}
