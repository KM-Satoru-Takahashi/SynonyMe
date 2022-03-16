using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynonyMe.Model.Manager.Events
{
    internal class SettingChangedEventArgs : EventArgs
    {
        //todo:最終的には各設定クラスにSettingBase的なのを継承させ、それをイベント引数にしたい
        //現状、objectで保持する。将来的に改修するため、特に弾いたりはしない

        //重複しないようにConcurrentDicとかにした方が良いかもtodo
        /// <summary>変更対象の設定情報を保持している、スレッドセーフな設定種別(キー)と設定情報(値)のペア</summary>
        private readonly Dictionary<Type, object> _changedSettings = new Dictionary<Type, object>();

        public SettingChangedEventArgs(Dictionary<Type, object> targetSettings)
        {
            if (targetSettings == null || targetSettings.Any() == false)
            {
                //todo:error log
                return;
            }

            foreach (KeyValuePair<Type, object> pair in targetSettings)
            {
                if (pair.Value == null)
                {
                    //todo:error log
                    continue;
                }

                _changedSettings.Add(pair.Key, pair.Value);
            }
        }

        /// <summary>通知対象となる設定情報を保持します</summary>
        /// <param name="type">対象種別</param>
        /// <param name="setting">対象ファイル情報</param>
        public SettingChangedEventArgs(Type type, object setting)
        {
            _changedSettings.Add(type, setting);
        }

        internal bool AddChangedSetting(Type type, object setting)
        {
            if (setting == null)
            {
                //todo:log
                return false;
            }

            _changedSettings.Add(type, setting);
            return true;
        }

        internal object GetTargetSetting(Type targetType)
        {
            object target = null;
            if (_changedSettings.TryGetValue(targetType, out target) == false)
            {
                //error log
                return null;
            }

            return target;
        }

    }
}
