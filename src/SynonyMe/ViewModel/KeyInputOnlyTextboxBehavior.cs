using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SynonyMe.ViewModel
{
    /// <summary>設定ウィンドウ等で、テキストボックスにキータッチのみで入力させる場合に使用する（ペースト等を許容しない）</summary>
    public class KeyInputOnlyTextboxBehavior : Behavior<TextBox>
    {
        // 直前にペーストを行ったか
        private bool _isPasted = false;

        // テキストボックスの文字列保持用変数
        private string _beforePasted = "";

        /// <summary>
        /// 対象となるUIにBehaviorがアタッチされた時の処理
        /// </summary>
        protected override void OnAttached()
        {
            base.OnAttached();
            DataObject.AddPastingHandler(AssociatedObject, OnPaste);
            // アタッチされたときに走るのだから、AssociatedObjectがnullはあり得ない
            AssociatedObject.TextChanged += OnTextChanged;
            AssociatedObject.MouseRightButtonUp += OnMouseRightButtonUp;
        }

        /// <summary>
        /// 対象となるUI要素からBehaviorがデタッチされた時の処理
        /// </summary>
        protected override void OnDetaching()
        {
            base.OnDetaching();
            DataObject.RemovePastingHandler(AssociatedObject, OnPaste);
            // デタッチする時に走るのだから、AssociatedObjectがnullはあり得ない
            AssociatedObject.TextChanged -= OnTextChanged;
            AssociatedObject.MouseRightButtonUp -= OnMouseRightButtonUp;
        }

        /// <summary>
        /// テキストが変更された時に呼ばれる処理
        /// 変更原因がペーストの場合、保持しておいた内容と入れ替える
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (e == null)
            {
                CommonLibrary.Log.Logger.Fatal("NumberOnlyTextboxBehavior", "OnTextChanged", "args is null!");
                return;
            }
            TextBox textBox = e.Source as TextBox;
            if (textBox == null)
            {
                CommonLibrary.Log.Logger.Fatal("NumberOnlyTextboxBehavior", "OnTextChanged", "sender is null!");
                return;
            }

            if (_isPasted)
            {
                CommonLibrary.Log.Logger.Debug("NumberOnlyTextboxEventArgs", "OnTextChanged", $"After pasted text:[{textBox.Text}]");
                textBox.Text = _beforePasted;
                _isPasted = false;
            }
        }

        /// <summary>
        /// ペースト時に呼ばれる処理
        /// ペースト直前に入力されていた内容を保持する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPaste(object sender, DataObjectPastingEventArgs e)
        {
            _isPasted = true;
            _beforePasted = AssociatedObject.Text;
        }

        /// <summary>
        /// マウス右ボタンクリック時の処理
        /// クリックイベントを無効化する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (e == null)
            {
                CommonLibrary.Log.Logger.Fatal("NumberOnlyTextboxEventArgs", "OnMouseRightButtonUp", "args is null!");
                return;
            }
            e.Handled = true;
        }

    }
}
