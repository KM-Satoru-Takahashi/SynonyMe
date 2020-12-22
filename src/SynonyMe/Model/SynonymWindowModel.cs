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
            Manager.DBManager dBManager = new Manager.DBManager(CommonLibrary.Define.DB_NAME);
            if (dBManager == null)
            {
                return null;
            }

            string getTargetSynonymWord =
                $@"SELECT * FROM {CommonLibrary.Define.DB_TABLE_SYNONYM_WORDS} WHERE GroupID == {groupID} ;";

            CommonLibrary.SynonymWordEntity[] synonymWords = null;
            dBManager.GetTargetSynonymWords(getTargetSynonymWord, out synonymWords);
            if (synonymWords == null)
            {
                // 無登録の場合はnullなので異常とは言えないため、素直にnullを返す
                return null;
            }

            return synonymWords;
        }

        /// <summary>類語グループリストの一覧を取得する</summary>
        /// <returns>DBに登録されている全類語グループリスト</returns>
        internal CommonLibrary.SynonymGroupEntity[] GetAllSynonymGroup()
        {
            Manager.DBManager dBManager = new Manager.DBManager(CommonLibrary.Define.DB_NAME);
            if (dBManager == null)
            {
                return null;
            }

            CommonLibrary.SynonymGroupEntity[] synonymGroups = null;
            string GET_ALL_SYNONYMGROUP =
                $@"SELECT * FROM {CommonLibrary.Define.DB_TABLE_SYNONYM_GROUP} ;";

            dBManager.GetTargetSynonymGroups(GET_ALL_SYNONYMGROUP, out synonymGroups);
            if (synonymGroups == null)
            {
                // 無登録の場合はnullを返す
                return null;
            }

            return synonymGroups;
        }

        /// <summary>類語グループリスト登録</summary>
        /// <param name="groupName">新規登録するグループ名</param>
        /// <returns>正常時true, 異常時false</returns>
        internal bool RegistSynonymGroup(string groupName)
        {
            if (string.IsNullOrEmpty(groupName))
            {
                return false;
            }

            Manager.DBManager dBManager = new Manager.DBManager(CommonLibrary.Define.DB_NAME);
            if (dBManager == null)
            {
                return false;
            }

            string registDate = GetTodayDate();
            string updateDate = GetTodayDate();

            // GroupIDはDBの方で割り振ってくれるので指定しない
            string registGroupSql = 
                $@"INSERT INTO {CommonLibrary.Define.DB_TABLE_SYNONYM_GROUP} (GroupName, GroupRegistDate, GroupUpdateDate) values ('{groupName}', '{registDate}', '{updateDate}' ) ; ";

            dBManager.ExecuteNonQuery(registGroupSql);

            return true;
        }

        /// <summary>類語登録を行う</summary>
        /// <param name="synonymWord">登録対象語句</param>
        /// <param name="groupID">登録対象のグループID</param>
        /// <returns>成功:true, 失敗:false</returns>
        internal bool RegistSynonymWord(string synonymWord, int groupID)
        {
            if(string.IsNullOrEmpty(synonymWord))
            {
                return false;
            }

            if(groupID < CommonLibrary.Define.MIN_GROUPID)
            {
                throw new SQLiteException($"GroupID is {groupID}");
            }

            string registDate = GetTodayDate();
            string updateDate = GetTodayDate();

            // WordIDはDBの方で割り振ってくれるので指定しない
            string registWordSql =
                $@"INSERT INTO {CommonLibrary.Define.DB_TABLE_SYNONYM_WORDS} (GroupID, Word, RegistDate, UpdateDate) values ('{groupID}', '{synonymWord}', '{registDate}', '{updateDate}' ) ; ";

            Manager.DBManager dBManager = new Manager.DBManager(CommonLibrary.Define.DB_NAME);
            if (dBManager == null)
            {
                return false;
            }

            dBManager.ExecuteNonQuery(registWordSql);

            return true;
        }

        /// <summary>現在日時をyyyy/MM/dd形式で取得する</summary>
        /// <returns></returns>
        private string GetTodayDate()
        {
            return DateTime.Now.ToString("yyyy/MM/dd");
        }

        #endregion
    }
}
