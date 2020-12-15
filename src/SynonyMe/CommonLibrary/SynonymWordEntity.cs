using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynonyMe.CommonLibrary
{
    /// <summary>DBのSynonymWordsテーブルに対応したクラス</summary>
    public class SynonymWordEntity
    {
        public int WordID { get; set; }

        public int SynonymID { get; set; }

        public string Word { get; set; }

        public string RegistDate { get; set; }

        public string UpdateDate { get; set; }
    }
}
