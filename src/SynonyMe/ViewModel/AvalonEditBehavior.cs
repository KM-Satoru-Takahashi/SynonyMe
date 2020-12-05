using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.AvalonEdit;
using System.Windows;

namespace SynonyMe.ViewModel
{
    /// <summary>MainWindowのテキスト編集箇所で使用するビヘイビア(AvalonEdit用)</summary>
    internal sealed class AvalonEditBehavior : Behavior<TextEditor>
    {
        /// <summary>AvalonEditor用のプロパティ宣言</summary>
        public static readonly DependencyProperty TextEditorProperty =
            DependencyProperty.Register(
                "AvalonTextEditor",
                typeof(string),
                typeof(AvalonEditBehavior),
                new FrameworkPropertyMetadata(default(string), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, PropertyChangedCallback)
                );

        /// <summary>AvalonEdit用プロパティ</summary>
        public string AvalonTextEditor
        {
            get { return (string)GetValue(TextEditorProperty); }
            set { SetValue(TextEditorProperty, value); }
        }

        /// <summary>表示テキスト変更時に(入力・削除)動作するビヘイビア</summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void AssociatedObjectOnTextChanged(object sender, EventArgs eventArgs)
        {
            TextEditor textEditor = sender as TextEditor;
            if (textEditor != null)
            {
                if (textEditor.Document != null)
                {
                    AvalonTextEditor = textEditor.Document.Text;
                }
            }
        }

        private static void PropertyChangedCallback(
            DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            if (dependencyObject == null || dependencyPropertyChangedEventArgs == null)
            {
                return;
            }

            AvalonEditBehavior behavior = dependencyObject as AvalonEditBehavior;
            if (behavior != null && behavior.AssociatedObject != null)
            {
                TextEditor textEditor = behavior.AssociatedObject as TextEditor;
                if (textEditor != null && textEditor.Document != null)
                {
                    int tempCaretOffset = textEditor.CaretOffset;
                    textEditor.Document.Text = dependencyPropertyChangedEventArgs.NewValue.ToString();

                    if (textEditor.CaretOffset >= tempCaretOffset)
                    {
                        textEditor.CaretOffset = tempCaretOffset;
                    }
                }
            }
        }

        #region Behavior override

        protected override void OnAttached()
        {
            base.OnAttached();
            if (AssociatedObject != null)
            {
                AssociatedObject.TextChanged += AssociatedObjectOnTextChanged;
            }
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            if (AssociatedObject != null)
            {
                AssociatedObject.TextChanged -= AssociatedObjectOnTextChanged;
            }
        }

        #endregion

    }
}
