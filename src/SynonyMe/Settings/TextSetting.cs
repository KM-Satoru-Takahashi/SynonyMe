using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynonyMe.Settings
{
    [Serializable]
    public class TextSetting
    {
        public bool WrappingText { get; set; }

        public bool ShowingLineCount { get; set; }

        public bool ShowingLineNumber { get; set; }

        public bool ShowingWordCount { get; set; }

        public bool ShowingNewLine { get; set; }

        public bool ShowingTab { get; set; }

        public bool ShowingSpace { get; set; }

        public string FontSize { get; set; }

        public string FontColor { get; set; }

        public string JapaneseFontName { get; set; }

        public string EnglishFontName { get; set; }
    }
}
