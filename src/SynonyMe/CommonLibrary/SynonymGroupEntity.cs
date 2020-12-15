using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynonyMe.CommonLibrary
{
    /// <summary>DBのSynonymGroupテーブルに対応したクラス</summary>
    public class SynonymGroupEntity
    {
        public int GroupID { get; set; }

        public string GroupName { get; set; }

        public string GroupRegistDate { get; set; }

        public string GroupUpdateDate { get; set; }
    }
}
