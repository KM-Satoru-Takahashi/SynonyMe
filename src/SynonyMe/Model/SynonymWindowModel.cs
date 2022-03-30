﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SynonyMe.ViewModel;
using System.Data.SQLite;   // DB
using System.Data;
using SynonyMe.CommonLibrary.Entity;
using SynonyMe.CommonLibrary.Log;

namespace SynonyMe.Model
{
    internal class SynonymWindowModel
    {
        #region field

        /// <summary>ViewModel</summary>
        private SynonymWindowVM _vm = null;

        private const string CLASS_NAME = "SynonymWindowModel";

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
            Logger.Info(CLASS_NAME, "CloseSyonnymWindow", "start");
            Manager.WindowManager.CloseSubWindow(CommonLibrary.SubWindowName.SynonymWindow);
        }

        /// <summary>選択した類語グループリストに紐付く類語一覧を取得する</summary>
        /// <param name="groupID">類語グループリストID</param>
        /// <returns>IDと一致する全類語</returns>
        internal SynonymWordEntity[] GetSynonymWordEntities(int groupID)
        {
            Logger.Info(CLASS_NAME, "GetSynonymWordEntites", $"start. groupID:[{groupID}]");

            SynonymWordEntity[] synonymWords = Manager.SynonymManager.GetSynonymWordEntities(groupID);
            if (synonymWords == null)
            {
                // 無登録の場合はnullなので異常とは言えないため、素直にnullを返す
                Logger.Info(CLASS_NAME, "GetSynonymWordEntites", "Registed synonym words are nothing");
                return null;
            }

            return synonymWords;
        }

        /// <summary>類語グループリストの一覧を取得する</summary>
        /// <returns>DBに登録されている全類語グループリスト</returns>
        internal SynonymGroupEntity[] GetAllSynonymGroup()
        {
            Logger.Info(CLASS_NAME, "GetAllSynonymGroup", "start");

            SynonymGroupEntity[] synonymGroups = Manager.SynonymManager.GetAllSynonymGroup();
            if (synonymGroups == null)
            {
                // 無登録の場合はnullを返す
                Logger.Info(CLASS_NAME, "GetAllSynonymGroup", "Registed synonym groups are nothing");
                return null;
            }

            return synonymGroups;
        }

        /// <summary>類語グループリスト登録</summary>
        /// <param name="groupName">新規登録するグループ名</param>
        /// <returns>正常時true, 異常時false</returns>
        internal bool RegistSynonymGroup(string groupName)
        {
            Logger.Info(CLASS_NAME, "RegistSynonymGroup", $"start. groupName:[{groupName}]");

            if (string.IsNullOrEmpty(groupName))
            {
                Logger.Error(CLASS_NAME, "RegisySynonymGroup", "groupName is null or empty!");
                return false;
            }

            return Manager.SynonymManager.RegistSynonymGroup(groupName);
        }

        /// <summary>類語登録を行う</summary>
        /// <param name="synonymWord">登録対象語句</param>
        /// <param name="groupID">登録対象のグループID</param>
        /// <returns>成功:true, 失敗:false</returns>
        internal bool RegistSynonymWord(string synonymWord, int groupID)
        {
            Logger.Info(CLASS_NAME, "ReistSynonymWord", $"start. groupID:[{groupID}]");

            if (string.IsNullOrEmpty(synonymWord))
            {
                Logger.Fatal(CLASS_NAME, "RegistSynonymWord", "synonymWord is null or empty!");
                return false;
            }

            if (groupID < CommonLibrary.Define.MIN_GROUPID)
            {
                Logger.Fatal(CLASS_NAME, "RegistSynonymWord", $"GroupID is {groupID}");
                return false;
            }

            return Manager.SynonymManager.RegistSynonymWord(synonymWord, groupID);
        }

        /// <summary>登録語句を更新する</summary>
        /// <param name="wordID">語句に割り振られているUniqueID</param>
        /// <param name="word">更新後の語句</param>
        /// <returns>true:成功, false:失敗</returns>
        internal bool UpdateSynonymWord(int wordID, string word)
        {
            Logger.Info(CLASS_NAME, "UpdateSynonymWord", $"start. wordID:[{wordID}], word:[{word}]");

            if (string.IsNullOrEmpty(word))
            {
                Logger.Fatal(CLASS_NAME, "UpdateSynonymWord", "word is null or empty!");
                return false;
            }

            if (wordID < CommonLibrary.Define.MIN_WORDID)
            {
                Logger.Fatal(CLASS_NAME, "UpdateSynonymWord", $"WordID is incorrect! wordID is [{wordID}]");
                return false;
            }

            return Manager.SynonymManager.UpdateSynonymWord(wordID, word);
        }

        /// <summary>類語グループ名を更新する</summary>
        /// <param name="groupID">対象グループID</param>
        /// <param name="groupName">更新後のグループ名</param>
        /// <returns>true:成功, false:失敗</returns>
        internal bool UpdateSynonymGroup(int groupID, string groupName)
        {
            Logger.Info(CLASS_NAME, "UpdateSynonymGroup", $"start. groupID:[{groupID}], groupName:[{groupName}]");

            if (groupID < CommonLibrary.Define.MIN_GROUPID)
            {
                Logger.Fatal(CLASS_NAME, "UpdateSynonymGroup", $"groupID is incorrect! groupID is [{groupID}]");
                return false;
            }

            if (string.IsNullOrEmpty(groupName))
            {
                Logger.Fatal(CLASS_NAME, "UpdateSynonymGroup", "groupName is null or empty!");
                return false;
            }

            return Manager.SynonymManager.UpdateSynonymGroup(groupID, groupName);
        }

        /// <summary>類語グループを更新する(更新日のみ)</summary>
        /// <param name="groupID">更新対象のグループID</param>
        /// <returns>true:成功, false:失敗</returns>
        internal bool UpdateSynonymGroup(int groupID)
        {
            Logger.Info(CLASS_NAME, "UpdateSynonymGroup", $"start. groupID[{groupID}]");

            if (groupID < CommonLibrary.Define.MIN_GROUPID)
            {
                Logger.Fatal(CLASS_NAME, "UpdateSynonymGroup", $"groupID is incorrect! groupID is [{groupID}]");
                return false;
            }

            return Manager.SynonymManager.UpdateSynonymGroup(groupID);
        }

        /// <summary>
        /// 類語グループを削除する
        /// </summary>
        /// <param name="groupID">対象のグループID</param>
        /// <returns>true:成功, false:失敗</returns>
        internal bool DeleteSynonymGroup(int groupID)
        {
            Logger.Info(CLASS_NAME, "DeleteSynonymGroup", $"start. groupID:[{groupID}]");

            if (groupID < CommonLibrary.Define.MIN_GROUPID)
            {
                Logger.Fatal(CLASS_NAME, "DeleteSynonymGroup", $"groupID is incorrect! groupID is [{groupID}]");
                return false;
            }

            return Manager.SynonymManager.DeleteSynonymGroup(groupID);
        }

        /// <summary>対象語句を削除する</summary>
        /// <param name="wordID"></param>
        /// <returns></returns>
        internal bool DeleteSynonymWord(int wordID)
        {
            Logger.Info(CLASS_NAME, "DeleteSynonymWord", $"start. wordID:{wordID}]");

            if (wordID < CommonLibrary.Define.MIN_WORDID)
            {
                Logger.Fatal(CLASS_NAME, "DeleteSynonymWord", $"wordID is incorrect!wordID is {wordID}");
                return false;
            }

            return Manager.SynonymManager.DeleteSynonymWord(wordID);
        }

        #endregion
    }
}
