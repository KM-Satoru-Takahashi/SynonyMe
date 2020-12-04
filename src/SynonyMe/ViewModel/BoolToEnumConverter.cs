using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace SynonyMe.ViewModel
{
    /// <summary>bool値とenumを相互に変換します</summary>
    public class BoolToEnumConverter : IValueConverter
    {
        /// <summary>enum値をラジオボタンのbool値に変換します</summary>
        /// <param name="value">ラジオボタンに設定されたenum値</param>
        /// <param name="targetType"></param>
        /// <param name="parameter">enum型の文字列</param>
        /// <param name="info"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo info)
        {
            if (parameter == null)
            {
                //todo:log
                return DependencyProperty.UnsetValue;
            }

            // xamlから文字列でenum値が渡されてくる
            string parameterString = parameter.ToString(); // stringにasキャストするとnullになってしまう
            if (string.IsNullOrEmpty(parameterString))
            {
                //todo:log
                return DependencyProperty.UnsetValue;
            }

            // enum定義されていなければ未定義値とする
            if (!Enum.IsDefined(value.GetType(), value))
            {
                //todo:log
                return DependencyProperty.UnsetValue;
            }

            // IsDefinedで確認してはいるが、念のためtry-catchする
            object parameterValue = null;
            try
            {
                parameterValue = Enum.Parse(value.GetType(), parameterString);
            }
            catch
            {
                //error log
                return DependencyProperty.UnsetValue;
            }

            // enumとパラメタが等しい→チェック状態と判断する
            return parameterValue != null && parameterValue.Equals(value);
        }

        /// <summary>ラジオボタンのbool状態をenumに変換します</summary>
        /// <param name="value">ラジオボタンのチェック状態</param>
        /// <param name="targetType"></param>
        /// <param name="parameter">ラジオボタン定義のenum値</param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null)
            {
                //todo:error
                return DependencyProperty.UnsetValue;
            }

            string parameterString = parameter.ToString();
            if (string.IsNullOrEmpty(parameterString))
            {
                return DependencyProperty.UnsetValue;
            }

            if (true.Equals(value))
            {
                try
                {
                    return Enum.Parse(targetType, parameterString);
                }
                catch
                {
                    //todo:log
                    return DependencyProperty.UnsetValue;
                }
            }
            else
            {
                return DependencyProperty.UnsetValue;
            }
        }
    }
}
