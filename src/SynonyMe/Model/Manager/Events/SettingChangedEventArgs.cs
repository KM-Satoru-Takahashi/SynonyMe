using System;
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
        public object ChangedSetting { get; }

        public SettingChangedEventArgs(object setting)
        {
            ChangedSetting = setting;
        }
    }
}
