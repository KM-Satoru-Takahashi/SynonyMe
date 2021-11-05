﻿using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SynonyMe.CommonLibrary.Entity;
using SynonyMe.CommonLibrary.Log;

namespace SynonyMe.Model.Manager
{
    /// <summary>類語関連の処理を担うクラス</summary>
    /// <remarks>DBへのアクセスはこのクラスが引き受け、各Modelクラスが共通して使う前提とし、staticにする</remarks>
    internal static class SynonymManager
    {

        private const string CLASS_NAME = "SynonymManager";

        internal static event EventHandler UpdateSynonymEvent = delegate { };

        // TODO:SynonymModelから持ってきただけなので、全体的なリファクタリングやExceptionの妥当性の確認

        /// <summary>選択した類語グループリストに紐付く類語一覧を取得する</summary>
        /// <param name="groupID">類語グループリストID</param>
        /// <returns>IDと一致する全類語</returns>
        internal static SynonymWordEntity[] GetSynonymWordEntities(int groupID)
        {
            Logger.Info(CLASS_NAME, "GetSynonymWordEntities", $"start. groupID:[{groupID}]");

            SynonymWordEntity[] synonymWords = null;

            using (DBManager dBManager = new DBManager(CommonLibrary.Define.DB_NAME))
            {
                if (dBManager == null)
                {
                    Logger.Fatal(CLASS_NAME, "GetSynonymWordEntities", $"dBManager is null! DB_NAME:[{CommonLibrary.Define.DB_NAME}]");
                    return null;
                }

                string getTargetSynonymWord =
                    $@"SELECT * FROM {CommonLibrary.Define.DB_TABLE_SYNONYM_WORDS} WHERE GroupID == {groupID} ;";
                dBManager.GetTargetSynonymWords(getTargetSynonymWord, out synonymWords);
            }

            if (synonymWords == null)
            {
                // 無登録の場合はnullなので異常とは言えないため、素直にnullを返す
                Logger.Info(CLASS_NAME, "GetSynonymWordEntites", "synonymWords are null");
                return null;
            }

            return synonymWords;
        }

        /// <summary>類語グループリストの一覧を取得する</summary>
        /// <returns>DBに登録されている全類語グループリスト</returns>
        internal static SynonymGroupEntity[] GetAllSynonymGroup()
        {
            Logger.Info(CLASS_NAME, "GetAllSynonymGroup", "start");

            SynonymGroupEntity[] synonymGroups = null;

            using (DBManager dBManager = new DBManager(CommonLibrary.Define.DB_NAME))
            {
                if (dBManager == null)
                {
                    Logger.Fatal(CLASS_NAME, "GetAllSynonymGroup", $"dBManager is null! DB_NAME:[{CommonLibrary.Define.DB_NAME}]");
                    return null;
                }

                string GET_ALL_SYNONYMGROUP =
                    $@"SELECT * FROM {CommonLibrary.Define.DB_TABLE_SYNONYM_GROUP} ;";
                dBManager.GetTargetSynonymGroups(GET_ALL_SYNONYMGROUP, out synonymGroups);
            }

            if (synonymGroups == null)
            {
                // 無登録の場合はnullを返す
                Logger.Info(CLASS_NAME, "GetAllSynonymGroup", "Registed synonym words are nothing!");
                return null;
            }

            return synonymGroups;
        }

        /// <summary>類語グループリスト登録</summary>
        /// <param name="groupName">新規登録するグループ名</param>
        /// <returns>正常時true, 異常時false</returns>
        internal static bool RegistSynonymGroup(string groupName)
        {
            Logger.Info(CLASS_NAME, "RegistSynonymGroup", $"start. groupName:[{groupName}]");

            if (string.IsNullOrEmpty(groupName))
            {
                Logger.Error(CLASS_NAME, "RegistSynonymGroup", "groupName is null or empty!");
                return false;
            }

            using (DBManager dBManager = new DBManager(CommonLibrary.Define.DB_NAME))
            {
                if (dBManager == null)
                {
                    Logger.Fatal(CLASS_NAME, "RegistSynonymGroup", $"dBManager is null! DB_NAME:[{CommonLibrary.Define.DB_NAME}]");
                    return false;
                }

                // GroupIDはDBの方で割り振ってくれるので指定しない
                string registGroupSql =
                    $@"INSERT INTO {CommonLibrary.Define.DB_TABLE_SYNONYM_GROUP} (GroupName, GroupRegistDate, GroupUpdateDate) values ('{groupName}', '{GetTodayDate()}', '{GetTodayDate()}' ) ; ";
                dBManager.ExecuteNonQuery(registGroupSql);
            }

            // 登録したので更新イベントを発火させる
            UpdateSynonymEvent(null, null);
            return true;
        }

        /// <summary>類語登録を行う</summary>
        /// <param name="synonymWord">登録対象語句</param>
        /// <param name="groupID">登録対象のグループID</param>
        /// <returns>成功:true, 失敗:false</returns>
        internal static bool RegistSynonymWord(string synonymWord, int groupID)
        {
            Logger.Info(CLASS_NAME, "RegistSynonymWord", $"start. synonymWord:[{synonymWord}], groupID:[{groupID}]");

            if (string.IsNullOrEmpty(synonymWord))
            {
                Logger.Error(CLASS_NAME, "RegistSynonymWord", "synonymWord is null or empty!");
                return false;
            }

            if (groupID < CommonLibrary.Define.MIN_GROUPID)
            {
                Logger.Fatal(CLASS_NAME, "RegistSynonymWord", $"GroupID is {groupID}");
                return false;
            }

            using (DBManager dBManager = new DBManager(CommonLibrary.Define.DB_NAME))
            {
                if (dBManager == null)
                {
                    Logger.Fatal(CLASS_NAME, "RegistSynonymWord", $"dBManager is null! DB_NAME:[{CommonLibrary.Define.DB_NAME}]");
                    return false;
                }

                // WordIDはDBの方で割り振ってくれるので指定しない
                string registWordSql =
                    $@"INSERT INTO {CommonLibrary.Define.DB_TABLE_SYNONYM_WORDS} (GroupID, Word, RegistDate, UpdateDate) values ('{groupID}', '{synonymWord}', '{GetTodayDate()}', '{GetTodayDate()}' ) ; ";
                dBManager.ExecuteNonQuery(registWordSql);
            }

            UpdateSynonymEvent(null, null);
            return true;
        }

        /// <summary>登録語句を更新する</summary>
        /// <param name="wordID">語句に割り振られているUniqueID</param>
        /// <param name="word">更新後の語句</param>
        /// <returns></returns>
        internal static bool UpdateSynonymWord(int wordID, string word)
        {
            Logger.Info(CLASS_NAME, "UpdateSynonymWord", $"start. wordID:[{wordID}], word:[{word}]");

            if (string.IsNullOrEmpty(word))
            {
                Logger.Error(CLASS_NAME, "UpdateSynonymWord", "word is null or empty!");
                return false;
            }

            if (wordID < CommonLibrary.Define.MIN_WORDID)
            {
                Logger.Fatal(CLASS_NAME, "UpdateSynonymWord", $"wordID is {wordID}");
                return false;
            }

            using (DBManager dBManager = new DBManager(CommonLibrary.Define.DB_NAME))
            {
                if (dBManager == null)
                {
                    Logger.Fatal(CLASS_NAME, "UpdateSynonymWord", $"dBManager is null! DB_NAME:[{CommonLibrary.Define.DB_NAME}]");
                    return false;
                }

                string updateWordSql =
                    $@"UPDATE {CommonLibrary.Define.DB_TABLE_SYNONYM_WORDS} SET Word = '{word}', UpdateDate = '{GetTodayDate()}' WHERE WordID == {wordID} ; ";
                dBManager.ExecuteNonQuery(updateWordSql);
            }

            UpdateSynonymEvent(null, null);
            return true;
        }

        /// <summary>類語グループ名を更新する</summary>
        /// <param name="groupID">対象グループID</param>
        /// <param name="groupName">更新後のグループ名</param>
        /// <returns>true:成功, false:失敗</returns>
        internal static bool UpdateSynonymGroup(int groupID, string groupName)
        {
            Logger.Info(CLASS_NAME, "UpdateSynonymGroup", $"start. groupID:[{groupID}], groupName:[{groupName}]");

            if (groupID < CommonLibrary.Define.MIN_GROUPID)
            {
                Logger.Fatal(CLASS_NAME, "UpdateSynonymGroup", $"groupID is {groupID}");
                return false;
            }

            if (string.IsNullOrEmpty(groupName))
            {
                Logger.Error(CLASS_NAME, "UpdateSynonymGroup", "groupName is null or empty!");
                return false;
            }

            string updateGroupSql =
                $@"UPDATE {CommonLibrary.Define.DB_TABLE_SYNONYM_GROUP} SET GroupName = '{groupName}', GroupUpdateDate = '{GetTodayDate()}' WHERE GroupID == {groupID} ; ";

            UpdateSynonymEvent(null, null);
            return UpdateSynonymGroup(updateGroupSql);
        }

        /// <summary>類語グループを更新する(更新日のみ)</summary>
        /// <param name="groupID">更新対象のグループID</param>
        /// <returns>true:成功, false:失敗</returns>
        internal static bool UpdateSynonymGroup(int groupID)
        {
            Logger.Info(CLASS_NAME, "UpdateSynonymGroup", $"start. groupID:[{groupID}]");

            if (groupID < CommonLibrary.Define.MIN_GROUPID)
            {
                Logger.Fatal(CLASS_NAME, "UpdateSynonymGroup", $"groupID is {groupID}");
                return false;
            }

            string updateGroupSql =
                $@"UPDATE {CommonLibrary.Define.DB_TABLE_SYNONYM_GROUP} SET GroupUpdateDate = '{GetTodayDate()}' WHERE GroupID == {groupID} ; ";

            UpdateSynonymEvent(null, null);
            return UpdateSynonymGroup(updateGroupSql);
        }

        /// <summary>
        /// 類語グループを削除する
        /// </summary>
        /// <param name="groupID">対象のグループID</param>
        /// <returns>true:成功, false:失敗</returns>
        internal static bool DeleteSynonymGroup(int groupID)
        {
            Logger.Info(CLASS_NAME, "DeleteSynonymGroup", $"start. groupID:[{groupID}]");

            if (groupID < CommonLibrary.Define.MIN_GROUPID)
            {
                Logger.Fatal(CLASS_NAME, "DeleteSynonymGroup", $"groupID is {groupID}");
                return false;
            }

            using (DBManager dBManager = new Manager.DBManager(CommonLibrary.Define.DB_NAME))
            {
                if (dBManager == null)
                {
                    Logger.Fatal(CLASS_NAME, "DeleteSynonymGroup", $"dBManager is null! DB_NAME:[{CommonLibrary.Define.DB_NAME}]");
                    return false;
                }

                // Groupに紐付く類語を全て削除する必要がある
                string deleteWordSql =
                    $@"DELETE FROM {CommonLibrary.Define.DB_TABLE_SYNONYM_WORDS} WHERE GroupID == {groupID}";
                dBManager.ExecuteNonQuery(deleteWordSql);

                string deleteGroupSql =
                    $@"DELETE FROM {CommonLibrary.Define.DB_TABLE_SYNONYM_GROUP} WHERE GroupID == {groupID}";
                dBManager.ExecuteNonQuery(deleteGroupSql);
            }

            UpdateSynonymEvent(null, null);
            return true;
        }

        /// <summary>対象語句を削除する</summary>
        /// <param name="wordID"></param>
        /// <returns></returns>
        internal static bool DeleteSynonymWord(int wordID)
        {
            Logger.Info(CLASS_NAME, "DeleteSynonymWord", $"start. wordID:[{wordID}]");

            if (wordID < CommonLibrary.Define.MIN_WORDID)
            {
                Logger.Fatal(CLASS_NAME, "DeleteSynonymWord", $"wordID is {wordID}");
                return false;
            }

            using (DBManager dBManager = new DBManager(CommonLibrary.Define.DB_NAME))
            {
                if (dBManager == null)
                {
                    Logger.Fatal(CLASS_NAME, "DeleteSynonymWord", $"dBManager is null! DB_NAME:[{CommonLibrary.Define.DB_NAME}]");
                    return false;
                }

                string deleteWordSql =
                    $@"DELETE FROM {CommonLibrary.Define.DB_TABLE_SYNONYM_WORDS} WHERE WordID == {wordID}";
                dBManager.ExecuteNonQuery(deleteWordSql);
            }

            UpdateSynonymEvent(null, null);
            return true;
        }

        /// <summary>類語グループの更新を行う</summary>
        /// <param name="updateSql"></param>
        /// <returns></returns>
        private static bool UpdateSynonymGroup(string updateSql)
        {
            Logger.Info(CLASS_NAME, "UpdateSynonymGroup", $"start. SQL:[{updateSql}]");

            if (string.IsNullOrEmpty(updateSql))
            {
                Logger.Error(CLASS_NAME, "UpdateSynonymGroup", "updateSql is null or empty!");
                return false;
            }

            using (DBManager dBManager = new DBManager(CommonLibrary.Define.DB_NAME))
            {
                if (dBManager == null)
                {
                    Logger.Fatal(CLASS_NAME, "UpdateSynonymGroup", $"dBManager is null! DB_NAME:[{CommonLibrary.Define.DB_NAME}]");
                    return false;
                }

                dBManager.ExecuteNonQuery(updateSql);
            }

            return true;
        }

        /// <summary>現在日時をyyyy/MM/dd形式で取得する</summary>
        /// <returns></returns>
        private static string GetTodayDate()
        {
            return DateTime.Now.ToString("yyyy/MM/dd");
        }
    }
}
