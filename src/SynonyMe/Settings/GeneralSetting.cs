using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynonyMe.Settings
{
    [Serializable]
    public class GeneralSetting
    {
        public bool WrappingText { get; set; }

        public bool ShowingLineCount { get; set; }

        public bool ShowingNumberOfLines { get; set; }

        public bool ShowingWordCount { get; set; }

        public bool ShowingNewLine { get; set; }

        public bool ShowingTab { get; set; }

        public bool ShowingSpace { get; set; }

        public int FontSize { get; set; }

        public string FontColor { get; set; }

        public string MainFontName { get; set; }

        public string SubFontName { get; set; }
    }
}
