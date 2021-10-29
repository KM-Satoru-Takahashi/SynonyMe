using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Rendering;

namespace SynonyMe.AvalonEdit.Highlight
{
    /// <summary>
    /// Xml Syntax Highlighting Definitionのカラーに関する既定クラス
    /// </summary>
    /// <remarks>HighlightingBrushのコンストラクタがprotectedなので、ラップクラスを用意して用いる</remarks>
    class XshdBrush : HighlightingBrush
    {
        #region field

        private SolidColorBrush _brush = null;

        #endregion

        #region HighlightningBrush

        /// <summary>
        /// Xshd書き込み時にColorタグの色指定用Attributeに書き込まれる値を取得します
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Brush GetBrush(ITextRunConstructionContext context)
        {
            return _brush;
        }

        #endregion

        #region method

        public XshdBrush(SolidColorBrush fontBrush)
        {
            _brush = fontBrush;
        }

        public XshdBrush(Color color) : this(new SolidColorBrush(color))
        {
        }

        public override string ToString()
        {
            if (_brush == null)
            {
                throw new NullReferenceException("XshdBrush ToString _brush is null");
            }

            return _brush.ToString();
        }

        #endregion
    }
}
