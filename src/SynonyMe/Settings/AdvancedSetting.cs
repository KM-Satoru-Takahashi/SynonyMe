using SynonyMe.CommonLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynonyMe.Settings
{
    public class AdvancedSetting
    {
        public LogLevel LogLevel { get; set; }

        public bool SpeedUpSearch { get; set; }

        public int LogRetentionDays { get; set; }

        public int SynonymSearchResultDisplayCount { get; set; }

        public List<string> TargetFileExtensionList { get; set; }

        public AdvancedSetting()
        {


        }
    }
}
