using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SynonyMe.ViewModel;
using System.Data.SQLite;   // DB
using System.Data;

namespace SynonyMe.Model
{
    internal class SynonymWindowModel
    {
        #region field

        /// <summary>ViewModel</summary>
        private SynonymWindowVM _vm = null;

        #endregion

        #region property

        #endregion

        #region method

        /// <summary>コンストラクタ</summary>
        /// <param name="vm"></param>
        internal SynonymWindowModel(SynonymWindowVM vm)
        {
            _vm = vm;
        }

        /// <summary>類語ウィンドウを閉じる処理</summary>
        internal void CloseSynonymWindow()
        {
            WindowManager.CloseSubWindow(CommonLibrary.Define.SubWindowName.SynonymWindow);
        }

        /// <summary>選択した類語グループリストに紐付く類語一覧を取得する</summary>
        /// <param name="groupID">類語グループリストID</param>
        /// <returns>IDと一致する全類語</returns>
        internal CommonLibrary.SynonymWordEntity[] GetSynonymWordEntities(int groupID)
        {
            // todo : DB access

            // mock
            CommonLibrary.SynonymWordEntity test1 = new CommonLibrary.SynonymWordEntity
            {
                RegistDate = "20201218",
                GroupID = 1,
                UpdateDate = "20201219",
                WordID = 1,
                Word = "groupID:" + groupID + "Test1"
            };

            CommonLibrary.SynonymWordEntity test2 = new CommonLibrary.SynonymWordEntity
            {
                RegistDate = "20201218",
                GroupID = 1,
                UpdateDate = "20201219",
                WordID = 1,
                Word = "groupID:" + groupID + "Test2"
            };

            CommonLibrary.SynonymWordEntity[] testArray = new CommonLibrary.SynonymWordEntity[]
            {
                test1,test2
            };

            return testArray;
        }

        /// <summary>類語グループリストの一覧を取得する</summary>
        /// <returns>DBに登録されている全類語グループリスト</returns>
        internal List<CommonLibrary.SynonymGroupEntity> GetAllSynonymGroup()
        {
            Manager.DBManager dBManager = new Manager.DBManager(CommonLibrary.Define.DB_NAME);
            if(dBManager == null)
            {
                return null;
            }

            CommonLibrary.SynonymGroupEntity[] synonymGroupArray = null;
            dBManager.ExecuteQuery("SELECT * FROM SynonymGroup;", out synonymGroupArray);
            if(synonymGroupArray==null)
            {
                // 無登録の場合は空リストを返せば良い？
                // もしnullでないならnewしなくていいので、要調査
                return new List<CommonLibrary.SynonymGroupEntity>();
            }

            return synonymGroupArray.ToList();
        }

        /// <summary>類語グループリスト登録</summary>
        /// <param name="groupName">新規登録するグループ名</param>
        /// <returns>正常時true, 異常時false</returns>
        internal bool RegistSynonymGroup(string groupName)
        {
            if(string.IsNullOrEmpty(groupName))
            {
                return false;
            }

            Manager.DBManager dBManager = new Manager.DBManager(CommonLibrary.Define.DB_NAME);
            if(dBManager==null)
            {
                return false;
            }

            string registDate = GetTodayDate();
            string updateDate = GetTodayDate();

            // GroupIDはDBの方で割り振ってくれるので指定しない
            string registGroupSql = string.Format("INSERT INTO SynonymGroup (GroupName, GroupRegistDate, GroupUpdateDate) values ('{0}', '{1}', '{2}' ) ; ", groupName, registDate, updateDate);

            dBManager.ExecuteNonQuery(registGroupSql);

            return true;
        }


        private string GetTodayDate()
        {
            return DateTime.Now.ToString("yyyy/MM/dd");
        }

        #endregion
    }
}
