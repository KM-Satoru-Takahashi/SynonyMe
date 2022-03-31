using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace SynonyMe.ViewModel
{
    /// <summary>ExpanderとGridSplitterを併用するための添付プロパティ定義</summary>
    /// <remarks>添付プロパティを使用しないと、GridSplitterで広げた部分がExpanderを閉じても残り、残念な見た目になる</remarks>
    public static class ExpanderGridSplitterProperty
    {
        /// <summary>GridSplitterによる長さ変更に対応するExpander指定</summary>
        public enum GridSnapMode
        {
            /// <summary>何もしない</summary>
            None,
            /// <summary>閉じたときに自動で戻す</summary>
            Auto,
            /// <summary>閉じても伸ばした状態の長さを保持</summary>
            Explicit
        }

        /// <summary>BoolToVisibleコンバータ</summary>
        private static BooleanToVisibilityConverter BoolToVisibleConverter { get; } = new BooleanToVisibilityConverter();

        private static bool _isFirst = true;

        #region DependencyProperties

        /// <summary>ExpanderのGridSnapMode取得</summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static GridSnapMode GetGridSnap(Expander obj)
        {
            if (obj == null)
            {
                return GridSnapMode.None;
            }

            return (GridSnapMode)obj.GetValue(GridSnapProperty);
        }

        /// <summary>ExpanderへGridSnapModeの設定</summary>
        /// <param name="obj"></param>
        /// <param name="mode"></param>
        public static void SetGridSnap(Expander obj, GridSnapMode mode)
        {
            if (obj == null)
            {
                return;
            }

            obj.SetValue(GridSnapProperty, mode);
        }

        /// <summary>ExpanderでGridSplitterをうまく使うための添付プロパティ</summary>
        public static readonly DependencyProperty GridSnapProperty =
            DependencyProperty.RegisterAttached(
                "GridSnap",
                typeof(GridSnapMode),
                typeof(ExpanderGridSplitterProperty),
                new PropertyMetadata(
                    GridSnapMode.None,
                    (d, e) =>
                    {
                        GridSnapMode mode = GridSnapMode.None;
                        if (e.NewValue is GridSnapMode)
                        {
                            mode = (GridSnapMode)e.NewValue;
                        }
                        else
                        {
                            return;
                        }

                        Expander expander = null;
                        if (d is Expander)
                        {
                            expander = (Expander)d;
                        }
                        else
                        {
                            return;
                        }

                        expander.Dispatcher.BeginInvoke((Action)(async () =>
                        {
                            // コントロール取得のため、少し待たせて処理を開始
                            await Task.Delay(500);

                            // 横/縦で処理を分ける
                            switch (expander.ExpandDirection)
                            {
                                // 縦
                                case ExpandDirection.Down:
                                case ExpandDirection.Up:
                                    expander.AttachMode_Vertical(mode);
                                    break;
                                case ExpandDirection.Right:
                                case ExpandDirection.Left:
                                    expander.AttachMode_Horizontal(mode);
                                    break;
                                default:
                                    break;
                            }
                        }));
                    }));


        /// <summary>GridSplitter取得</summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static GridSplitter GetTargetGridSplitter(DependencyObject obj)
        {
            if (obj == null)
            {
                return null;
            }

            return (GridSplitter)obj.GetValue(TargetGridSplitterProperty);
        }

        /// <summary>GridSplitter設定</summary>
        /// <param name="obj"></param>
        /// <param name="splitter"></param>
        public static void SetTargetGridSplitter(DependencyObject obj, GridSplitter splitter)
        {
            if (obj == null)
            {
                return;
            }

            obj.SetValue(TargetGridSplitterProperty, splitter);
        }

        /// <summary>GridSplitter指定用の添付プロパティ定義</summary>
        /// <remarks>GridSplitterが複数ある場合はx:Nameで設定したGridSplitterをここに登録すること</remarks>
        public static readonly DependencyProperty TargetGridSplitterProperty =
            DependencyProperty.RegisterAttached(
                "TargetGridSplitter",
                typeof(GridSplitter),
                typeof(ExpanderGridSplitterProperty),
                new PropertyMetadata(null)
                );

        /// <summary>前回開いた際のExpanderの長さを保持する</summary>
        /// <param name="expander"></param>
        /// <returns></returns>
        private static GridLength GetLastGridLength(Expander expander)
        {
            if (expander == null)
            {
                return new GridLength();
            }

            return (GridLength)expander.GetValue(LastGridLengthProperty);
        }

        /// <summary>次回開いた際のExpanderサイズを記憶しておく</summary>
        /// <param name="expander"></param>
        /// <param name="rowDefinition"></param>
        private static void SetLastGridLength(Expander expander, GridLength gridLength)
        {
            if (expander == null)
            {
                return;
            }
            expander.SetValue(LastGridLengthProperty, gridLength);
        }

        /// <summary>
        /// 初めてExpanderを開く際の横幅を指定する
        /// </summary>
        /// <param name="expander">対象Expander</param>
        /// <remarks>現状横開きExpanderにしか対応していないので要注意</remarks>
        private static void SetFirstGridLength(Expander expander)
        {
            if (expander == null)
            {
                return;
            }

            View.MainWindow mw = Model.Manager.WindowManager.GetMainWindow();
            if (mw == null)
            {
                return;
            }

            // メインウィンドウの1/4を与えれば良い想定だが、将来的に変更しても良い
            double subAreaLength = mw.ActualWidth / 4;
            expander.SetValue(LastGridLengthProperty, new GridLength(subAreaLength));
            _isFirst = false;
        }

        /// <summary>直前のGridサイズ(Expanderサイズ)を記憶しておくための添付プロパティ定義</summary>
        private static readonly DependencyProperty LastGridLengthProperty =
            DependencyProperty.RegisterAttached(
                "LastGridLength",
                typeof(GridLength),
                typeof(ExpanderGridSplitterProperty),
                new PropertyMetadata(GridLength.Auto)
                );

        #endregion

        #region Vertical

        /// <summary>垂直方向時のExpanderの内部処理拡張メソッド</summary>
        /// <param name="expander"></param>
        /// <param name="mode"></param>
        private static void AttachMode_Vertical(this Expander expander, GridSnapMode mode)
        {
            if (expander == null)
            {
                return;
            }
            // イベント解除
            expander.Expanded -= Expanded_Vertical;
            expander.Collapsed -= Collapsed_Vertical;

            Grid targetGrid = expander.FindAncestor<Grid>();
            if (targetGrid == null)
            {
                return;
            }

            // 対象となるGridの情報を取得していく
            int gridRow = Grid.GetRow(expander);
            RowDefinition targetRowDefinition = targetGrid.RowDefinitions[gridRow];

            // プロパティ内に情報をセット
            SetTargetRowDefinition(expander, targetRowDefinition);
            SetLastGridLength(expander, targetRowDefinition.Height);

            // GridSplitterを取得
            GridSplitter gridSplitter = null;
            if (mode == GridSnapMode.Auto)
            {
                gridSplitter = targetGrid.FindDescendant<GridSplitter>();
            }
            else
            {
                gridSplitter = GetTargetGridSplitter(expander);
            }

            if (mode == GridSnapMode.Explicit && gridSplitter == null)
            {
                // GridSplitterの移動をそのままにするのにGridSplitterがnullはあり得ない
                throw new ArgumentException("Mode [explicit] requires [TargetGridSplitter]");
            }
            else if (gridSplitter == null)
            {
                return;
            }

            // Binding実行
            gridSplitter.SetBinding(UIElement.VisibilityProperty, new Binding(nameof(Expander.IsExpanded))
            {
                Mode = BindingMode.OneWay,
                Converter = BoolToVisibleConverter,
                Source = expander
            });

            expander.Expanded += Expanded_Vertical;
            expander.Collapsed += Collapsed_Vertical;
        }

        /// <summary>Expanderが閉じたときのイベント</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void Collapsed_Vertical(object sender, RoutedEventArgs e)
        {
            Expander expander = sender as Expander;
            if (expander == null)
            {
                return;
            }

            RowDefinition rowDefinition = GetTargetRowDefinition(expander);
            SetLastGridLength(expander, rowDefinition.Height);
            rowDefinition.Height = GridLength.Auto;
        }

        /// <summary>Expanderが開いたときのイベント</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void Expanded_Vertical(object sender, RoutedEventArgs e)
        {
            Expander expander = sender as Expander;
            if (expander == null)
            {
                return;
            }

            RowDefinition rowDefinition = GetTargetRowDefinition(expander);
            rowDefinition.Height = GetLastGridLength(expander);
        }

        /// <summary>対象のRowDefinition取得</summary>
        /// <param name="expander"></param>
        /// <returns></returns>
        private static RowDefinition GetTargetRowDefinition(Expander expander)
        {
            if (expander == null)
            {
                return null;
            }

            return (RowDefinition)expander.GetValue(TargetRowDefinitionProperty);
        }

        /// <summary>対象のRowDefinition設定</summary>
        /// <param name="expander"></param>
        /// <param name="rowDefinition"></param>
        private static void SetTargetRowDefinition(Expander expander, RowDefinition rowDefinition)
        {
            if (expander == null)
            {
                return;
            }

            expander.SetValue(TargetRowDefinitionProperty, rowDefinition);
        }


        private static readonly DependencyProperty TargetRowDefinitionProperty =
            DependencyProperty.RegisterAttached(
                "TargetRowDefinition",
                typeof(RowDefinition),
                typeof(ExpanderGridSplitterProperty),
                new PropertyMetadata(null)
                    );

        #endregion

        #region Horizontal

        private static void AttachMode_Horizontal(this Expander expander, GridSnapMode mode)
        {
            if (expander == null)
            {
                return;
            }

            expander.Expanded -= Expanded_Horizontal;
            expander.Collapsed -= Collapsed_Horizontal;

            Grid targetGrid = expander.FindAncestor<Grid>();
            if (targetGrid == null)
            {
                return;
            }

            if (mode == GridSnapMode.None)
            {
                return;
            }

            int gridColumn = Grid.GetColumn(expander);
            ColumnDefinition columnDefinition = targetGrid.ColumnDefinitions[gridColumn];
            SetTargetColumnDefinition(expander, columnDefinition);

            // 初回にExpanderを開く際、開かれたGridのActualWidthをとれずに検索テキストボックスの幅が微小になってしまう
            // そのため、exeを立ち上げて最初にExpanderを開く際のみMainWindowの比で強引に値を入れてやる
            // Expanderを開く前はExpander側の幅を調節できないため、数値がおかしくなる心配はないと想定される
            if (_isFirst)
            {
                SetFirstGridLength(expander);
            }
            else
            {
                SetLastGridLength(expander, columnDefinition.Width);
            }

            GridSplitter gridSplitter = null;
            if (mode == GridSnapMode.Auto)
            {
                gridSplitter = targetGrid.FindDescendant<GridSplitter>();
            }
            else
            {
                gridSplitter = GetTargetGridSplitter(expander);
            }

            if (mode == GridSnapMode.Explicit && gridSplitter == null)
            {
                throw new ArgumentException("[Explicit] requires [TargetGridSplitter]");
            }
            else if(gridSplitter == null)
            {
                return;
            }

            gridSplitter.SetBinding(UIElement.VisibilityProperty, new Binding(nameof(Expander.IsExpanded))
            {
                Mode = BindingMode.OneWay,
                Converter = BoolToVisibleConverter,
                Source = expander,
            });

            expander.Expanded += Expanded_Horizontal;
            expander.Collapsed += Collapsed_Horizontal;
        }

        private static void Collapsed_Horizontal(object sender, RoutedEventArgs e)
        {
            Expander expander = sender as Expander;
            if (expander == null)
            {
                return;
            }

            ColumnDefinition columnDefinition = GetTargetColumnDefinition(expander);
            SetLastGridLength(expander, columnDefinition.Width);
            columnDefinition.Width = GridLength.Auto;
        }

        private static void Expanded_Horizontal(object sender, RoutedEventArgs e)
        {
            Expander expander = sender as Expander;
            if (expander == null)
            {
                return;
            }

            ColumnDefinition columnDefinition = GetTargetColumnDefinition(expander);
            if (columnDefinition == null)
            {
                return;
            }

            columnDefinition.Width = GetLastGridLength(expander);
        }

        private static ColumnDefinition GetTargetColumnDefinition(Expander expander)
        {
            if (expander == null)
            {
                return null;
            }

            return (ColumnDefinition)expander.GetValue(TargetColumnDefinitionProperty);
        }

        private static void SetTargetColumnDefinition(Expander expander, ColumnDefinition columnDefinition)
        {
            if (expander == null)
            {
                return;
            }

            expander.SetValue(TargetColumnDefinitionProperty, columnDefinition);
        }

        private static readonly DependencyProperty TargetColumnDefinitionProperty =
            DependencyProperty.RegisterAttached(
                "TargetColumnDefinition",
                typeof(ColumnDefinition),
                typeof(ExpanderGridSplitterProperty),
                new PropertyMetadata(null)
                );

        #endregion

        #region Common

        /// <summary>親要素を取得する拡張メソッド</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dependencyObject"></param>
        /// <returns></returns>
        public static T FindAncestor<T>(this DependencyObject dependencyObject)
            where T : DependencyObject
        {
            while (dependencyObject != null)
            {
                T target = null;
                if (dependencyObject is T)
                {
                    target = (T)dependencyObject;
                    return target;
                }

                dependencyObject = VisualTreeHelper.GetParent(dependencyObject);
            }
            return null;
        }

        /// <summary>子要素を取得する拡張メソッド</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dependencyObject"></param>
        /// <returns></returns>
        public static T FindDescendant<T>(this DependencyObject dependencyObject)
            where T : DependencyObject
        {
            if (dependencyObject == null)
            {
                return null;
            }

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(dependencyObject); ++i)
            {
                DependencyObject child = VisualTreeHelper.GetChild(dependencyObject, i);

                T result = child as T;
                if (result == null)
                {
                    result = FindDescendant<T>(child);
                }

                if (result != null)
                {
                    return result;
                }
            }
            return null;
        }

        #endregion

    }
}
