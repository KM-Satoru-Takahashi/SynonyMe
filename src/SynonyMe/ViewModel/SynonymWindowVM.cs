﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using SynonyMe.Model;
using System.Collections.ObjectModel;
using System.Windows;
using SynonyMe.CommonLibrary.Entity;
using SynonyMe.CommonLibrary.Log;

namespace SynonyMe.ViewModel
{
    public class SynonymWindowVM : ViewModelBase
    {
        #region field

        /// <summary>model</summary>
        private SynonymWindowModel _model = null;

        /// <summary>表示中の類語グループリスト</summary>
        private ObservableCollection<SynonymGroupEntity> _displaySynonymGroup = null;

        /// <summary>類語グループリストから選択した類語グループ</summary>
        private ObservableCollection<SynonymWordEntity> _displaySynonymWords = null;

        private const string CLASS_NAME = "SynonymWindowVM";

        #endregion

        #region property

        /// <summary>タイトルテキスト</summary>
        public string SynonymWindowTitle { get; } = "Test";

        /// <summary>類語グループリスト、類語一覧リストヘッダの「更新日」テキスト</summary>
        public string HeaderUpdateDate { get; } = "更新日";

        /// <summary>類語グループリスト、類語一覧リストヘッダの「追加日」テキスト</summary>
        public string HeaderRegistDate { get; } = "追加日";

        /// <summary>類語グループリストヘッダの「類語グループ名」テキスト</summary>
        public string HeaderGroupName { get; } = "類語グループ名";

        /// <summary>類語リストヘッダの「登録語」テキスト</summary>
        public string HeaderSynonymWord { get; } = "登録語";

        /// <summary>類語グループリスト登録コマンド</summary>
        public ICommand Command_RegistSynonymGroup { get; private set; } = null;

        /// <summary>類語グループリスト編集コマンド</summary>
        public ICommand Command_EditSynonymGroup { get; private set; } = null;

        /// <summary>類語グループリスト削除コマンド</summary>
        public ICommand Command_DeleteSynonymGroup { get; private set; } = null;

        /// <summary>類語一覧リスト登録コマンド</summary>
        public ICommand Command_RegistSynonymWord { get; private set; } = null;

        /// <summary>類語一覧リスト編集コマンド</summary>
        public ICommand Command_EditSynonymWord { get; private set; } = null;

        /// <summary>類語一覧リスト削除コマンド</summary>
        public ICommand Command_DeleteSynonymWord { get; private set; } = null;

        /// <summary>閉じるボタン押下時コマンド</summary>
        public ICommand Command_Close { get; private set; } = null;

        /// <summary>類語グループリストからグループ選択時の実行コマンド</summary>
        /// <remarks>類語一覧リストに類語を表示させるために用いる</remarks>
        public ICommand Command_SelectSynonymGroup { get; private set; } = null;

        /// <summary>類語一覧リストから類語選択時の実行コマンド</summary>
        public ICommand Command_SelectSynonymWord { get; private set; } = null;

        /// <summary>類語グループ入力テキスト</summary>
        public string InputGroupWord { get; set; } = null;

        /// <summary>類語入力テキスト</summary>
        public string InputSynonymWord { get; set; } = null;

        /// <summary>選択中の類語グループリスト</summary>
        public SynonymGroupEntity SelectedGroup { get; set; } = null;

        /// <summary>選択中の類語</summary>
        public SynonymWordEntity SelectedWord { get; set; } = null;

        /// <summary>類語グループリスト一覧オブジェクト</summary>
        public ObservableCollection<SynonymGroupEntity> DisplaySynonymGroups
        {
            get
            {
                return _displaySynonymGroup;
            }
            set
            {
                _displaySynonymGroup = value;
                OnPropertyChanged("DisplaySynonymGroups");
            }
        }

        /// <summary>類語リスト一覧オブジェクト</summary>
        public ObservableCollection<SynonymWordEntity> DisplaySynonymWords
        {
            get
            {
                return _displaySynonymWords;
            }
            set
            {
                _displaySynonymWords = value;
                OnPropertyChanged("DisplaySynonymWords");
            }
        }

        #endregion

        #region method

        /// <summary>コンストラクタ</summary>
        public SynonymWindowVM()
        {
#if false // 初回生成時のみの処理をコメントアウト->通常は実行済みなので気にしない、将来削除予定
            // SynonymData.dbの初回生成時のみ実行　Gitコミット時はCreateとテスト用データのInsertが済んでいるのでコメントアウト
            // テスト用 DBの生成
            var sqlName = new SQLiteConnectionStringBuilder { DataSource = "SynonymData.db" };

            using (var con = new SQLiteConnection(sqlName.ToString()))
            {
                con.Open();

                using (var cmd = new SQLiteCommand(con))
                {
                    // SynonymGroup
                    cmd.CommandText = "CREATE TABLE IF NOT EXISTS SynonymGroup(" +
                        "GroupID INTEGER PRIMARY KEY UNIQUE NOT NULL," +
                        "GroupName TEXT NOT NULL," +
                        "GroupRegistDate TEXT NOT NULL, " +
                        "GroupUpdateDate TEXT NOT NULL )";
                    cmd.ExecuteNonQuery();

                    // 初回だけ
                    string dataAddSql = "INSERT INTO SynonymGroup (GroupName, GroupRegistDate, GroupUpdateDate) values ('testGroup1', '2020/12/14', '2020/12/15') ; ";
                    dataAddSql += "INSERT INTO SynonymGroup (GroupName, GroupRegistDate, GroupUpdateDate) values ('testGroup2', '2020/12/14', '2020/12/16') ; ";
                    dataAddSql += "INSERT INTO SynonymGroup (GroupName, GroupRegistDate, GroupUpdateDate) values ('testGroup3', '2020/12/25', '2020/12/15') ; ";

                    cmd.CommandText = dataAddSql;
                    cmd.ExecuteNonQuery();

                    // SynonymWord
                    cmd.CommandText = "CREATE TABLE IF NOT EXISTS SynonymWords( " +
                        "WordID INTEGER PRIMARY KEY UNIQUE NOT NULL," +
                        "GroupID INTEGER, " +
                        "Word TEXT NOT NULL, " +
                        "RegistDate TEXT NOT NULL, " +
                        "UpdateDate TEXT NOT NULL, " +
                        "FOREIGN KEY (GroupID) REFERENCES SynonymGroup (GroupID) " +
                        ")";

                    cmd.ExecuteNonQuery();

                    string dataAddSql2 = "INSERT INTO SynonymWords (GroupID, Word, RegistDate, UpdateDate) values ('1', 'test001-1', '2020/12/21', '2020/12/22');";
                    dataAddSql2 += "INSERT INTO SynonymWords (GroupID, Word, RegistDate, UpdateDate) values ('1', 'test001-2', '2020/12/23', '2020/12/26');";
                    dataAddSql2 += "INSERT INTO SynonymWords (GroupID, Word, RegistDate, UpdateDate) values ('2', 'test002-1', '2020/12/23', '2020/12/27');";

                    cmd.CommandText = dataAddSql2;
                    cmd.ExecuteNonQuery();
                }
            }

#endif
            #region コマンド初期化

            Command_SelectSynonymGroup = new CommandBase(ExecuteSelectSynonymGroup, null);
            Command_RegistSynonymGroup = new CommandBase(ExecuteRegistSynonymGroup, null);
            Command_EditSynonymGroup = new CommandBase(ExecuteEditSynonymGroup, null);
            Command_DeleteSynonymGroup = new CommandBase(ExecuteDeleteSynonymGroup, null);
            Command_SelectSynonymWord = new CommandBase(ExecuteSelectSynonymWord, null);
            Command_RegistSynonymWord = new CommandBase(ExecuteRegistSynonymWord, null);
            Command_EditSynonymWord = new CommandBase(ExecuteEditSynonymWord, null);
            Command_DeleteSynonymWord = new CommandBase(ExecuteDeleteSynonymWord, null);
            Command_Close = new CommandBase(ExecuteClose, null);

            #endregion

            _model = new SynonymWindowModel(this);

            // DBから必要な情報を取得する
            UpdateDisplaySynonymGroups();
        }

        /// <summary>閉じるボタン押下時処理</summary>
        /// <param name="parameter"></param>
        private void ExecuteClose(object parameter)
        {
            if (_model == null)
            {
                Logger.Fatal(CLASS_NAME, "ExecuteClose", "_model is null");
                return;
            }

            _model.CloseSynonymWindow();
        }

        /// <summary>類語グループリスト登録コマンド</summary>
        /// <param name="parameter"></param>
        private void ExecuteRegistSynonymGroup(object parameter)
        {
            if (string.IsNullOrEmpty(InputGroupWord))
            {
                Logger.Error(CLASS_NAME, "ExecuteRegistSynonymGroup", "InputGroupWord is null or empty!");
                return;
            }

            if (_model == null)
            {
                Logger.Fatal(CLASS_NAME, "ExecuteRegistSynonymGroup", "_model is null");
                return;
            }

            if (_model.RegistSynonymGroup(InputGroupWord) == false)
            {
                Logger.Error(CLASS_NAME, "ExecuteRegistSynonymGroup", "RegistSynonymGroup return false");
                return;
            }

            // 登録が正常に行われたなら、表示中の類語グループリストを更新する
            UpdateDisplaySynonymGroups();
        }

        /// <summary>類語グループリスト編集コマンド</summary>
        /// <param name="parameter">編集中の行[SynonymGroupEntity]</param>
        private void ExecuteEditSynonymGroup(object parameter)
        {
            if (parameter == null)
            {
                Logger.Fatal(CLASS_NAME, "ExecuteEditSynonymGroup", "parameter is null!");
                return;
            }

            SynonymGroupEntity targetGroup = parameter as SynonymGroupEntity;
            if (targetGroup == null)
            {
                Logger.Fatal(CLASS_NAME, "ExecuteEditSynonymGroup", "targetGroup is null!");
                return;
            }

            if (_model == null)
            {
                Logger.Fatal(CLASS_NAME, "ExecuteEditSynonymGroup", "_model is null!");
                return;
            }

            if (_model.UpdateSynonymGroup(targetGroup.GroupID, targetGroup.GroupName) == false)
            {
                Logger.Error(CLASS_NAME, "ExecuteEditSynonymGroup", "UpdateSynonymGroup return false");
                return;
            }

            UpdateDisplayGroupsAndWords(targetGroup.GroupID);
        }

        /// <summary>選択類語グループ削除コマンド</summary>
        /// <param name="parameter"></param>
        private void ExecuteDeleteSynonymGroup(object parameter)
        {
            if (SelectedGroup == null)
            {
                Logger.Fatal(CLASS_NAME, "ExecuteDeleteSynonymGroup", "SelectedGroup is null");
                return;
            }

            if (_model == null)
            {
                Logger.Fatal(CLASS_NAME, "ExecuteDeleteSynonymGroup", "_model is null");
                return;
            }

            if (_model.DeleteSynonymGroup(SelectedGroup.GroupID) == false)
            {
                Logger.Error(CLASS_NAME, "ExecuteDeleteSynonymGroup", "DeleteSynonymGroup return false");
                return;
            }

            // 画面表示の更新
            // 類語リストは一旦空にする
            UpdateDisplaySynonymGroups();
            DisplaySynonymWords = null;
        }

        /// <summary>類語登録コマンド</summary>
        /// <param name="parameter"></param>
        private void ExecuteRegistSynonymWord(object parameter)
        {
            if (_model == null)
            {
                throw new NullReferenceException("ExecuteRegisySynonymWord _model is null");
            }

            if (string.IsNullOrEmpty(InputSynonymWord))
            {
                return;
            }

            if (SelectedGroup == null)
            {
                return;
            }

            _model.RegistSynonymWord(InputSynonymWord, SelectedGroup.GroupID);

            // 所属する類語グループの更新日を更新する
            _model.UpdateSynonymGroup(SelectedGroup.GroupID);

            UpdateDisplayGroupsAndWords(SelectedGroup.GroupID);
        }

        /// <summary>類語編集コマンド</summary>
        /// <param name="parameter">RoutedEventArgs:類語リストでのテキストボックス入力値</param>
        private void ExecuteEditSynonymWord(object parameter)
        {
            SynonymWordEntity targetEntity = (SynonymWordEntity)parameter;
            if (targetEntity == null)
            {
                Logger.Error(CLASS_NAME, "ExecuteEditSynonymWord", "targetEntity is null!");
                return;
            }

            if (_model == null)
            {
                Logger.Fatal(CLASS_NAME, "ExecuteEditSynonymWord", "_model is null!");
                return;
            }

            _model.UpdateSynonymWord(targetEntity.WordID, targetEntity.Word);

            // 所属する類語グループの更新日を更新する
            _model.UpdateSynonymGroup(targetEntity.GroupID);

            // 画面表示の更新 ここで実行してしまうと、類語グループをクリックしたときに1クリックで遷移できなくなる(OnPropertyChangedで画面側の更新が走るため)
            // UpdateDisplayGroupsAndWords(targetEntity.GroupID);
        }


        /// <summary>類語削除コマンド</summary>
        /// <param name="parameter"></param>
        private void ExecuteDeleteSynonymWord(object parameter)
        {
            if (SelectedWord == null)
            {
                Logger.Error(CLASS_NAME, "ExecuteDeleteSynonymWord", "SelectedWord is null!");
                return;
            }

            if (_model == null)
            {
                Logger.Fatal(CLASS_NAME, "ExecuteDeleteSynonymWord", "_model is null");
                return;
            }

            _model.DeleteSynonymWord(SelectedWord.WordID);

            // 所属する類語グループの更新
            _model.UpdateSynonymGroup(SelectedWord.GroupID);

            // 画面表示の更新
            UpdateDisplayGroupsAndWords(SelectedWord.GroupID);
        }

        /// <summary>類語グループ選択時実行コマンド</summary>
        /// <param name="parameter">選択した類語グループ</param>
        private void ExecuteSelectSynonymGroup(object parameter)
        {
            if (parameter == null)
            {
                Logger.Error(CLASS_NAME, "ExecuteSelectSynonymGroup", "parameter is null!");
                return;
            }

            SynonymGroupEntity selectedGroup = ConvertParameterToSynonymGroupEntity(parameter);
            if (selectedGroup == null)
            {
                Logger.Error(CLASS_NAME, "ExecuteSelectSynonymGroup", "selectedGroup is null!");
                return;
            }

            SelectedGroup = selectedGroup;
            UpdateDisplaySynonymWords(selectedGroup.GroupID);
        }

        /// <summary>類語一覧リスト選択時実行コマンド</summary>
        /// <param name="parameter"></param>
        private void ExecuteSelectSynonymWord(object parameter)
        {
            if (parameter == null)
            {
                Logger.Error(CLASS_NAME, "ExecuteSelectSynonymWord", "parameter is null!");
                return;
            }

            SynonymWordEntity selectedWord = ConvertParameterToSynonymWordEntity(parameter);
            if (selectedWord == null)
            {
                Logger.Error(CLASS_NAME, "ExecuteSelectSynonymWord", "selectedWord is null!");
                return;
            }

            SelectedWord = selectedWord;
        }

        /// <summary>類語グループと類語リストをともに更新する</summary>
        /// <param name="groupID"></param>
        private void UpdateDisplayGroupsAndWords(int groupID)
        {
            UpdateDisplaySynonymGroups();
            UpdateDisplaySynonymWords(groupID);
        }

        /// <summary>表示中の類語一覧リストを更新する</summary>
        /// <param name="groupID">表示対象となる類語一覧リストのグループID</param>
        private void UpdateDisplaySynonymWords(int groupID)
        {
            if (_model == null)
            {
                Logger.Fatal(CLASS_NAME, "UpdateDisplaySynonymWords", "_model is null!");
                return;
            }

            DisplaySynonymWords = new ObservableCollection<SynonymWordEntity>(_model.GetSynonymWordEntities(groupID));
        }

        /// <summary>表示中の類語グループリストを更新する</summary>
        private void UpdateDisplaySynonymGroups()
        {
            if (_model == null)
            {
                Logger.Fatal(CLASS_NAME, "UpdateDisplaySynonymGroups", "_model is null");
                return;
            }

            DisplaySynonymGroups = new ObservableCollection<SynonymGroupEntity>(_model.GetAllSynonymGroup());
        }

        /// <summary>類語グループリストの選択(SelectedItemCollection)を、SynonymGroupEntity形式に変換する</summary>
        /// <param name="parameter">選択状態の引数(型:SelectedItemCollection)</param>
        /// <returns>変換後のSynonymGroupEntity</returns>
        private SynonymGroupEntity ConvertParameterToSynonymGroupEntity(object parameter)
        {
            if (parameter == null)
            {
                Logger.Error(CLASS_NAME, "ConvertParameterToSynonymGroupEntity", "parameter is null!");
                return null;
            }

            System.Collections.IList selectedItem = (System.Collections.IList)parameter;
            if (selectedItem == null || selectedItem.Count < 1)
            {
                Logger.Error(CLASS_NAME, "ConvertParameterToSynonymGroupEntity", "selectedItem is incorrect!");
                return null;
            }

            var selectedSynonymGroup = selectedItem.Cast<SynonymGroupEntity>();
            if (selectedSynonymGroup == null || selectedSynonymGroup.Any() == false)
            {
                Logger.Error(CLASS_NAME, "ConvertParameterToSynonymGroupEntity", "selectedSynonymGroup is incorrect!");
                return null;
            }

            // 常に0番目の要素を返す(1つのみ選択することが前提なので)
            return selectedSynonymGroup.FirstOrDefault();
        }

        /// <summary>類語リストの選択(SelectedItemCollection)をSynonymWordEntity形式に変換する</summary>
        /// <param name="parameter">選択状態の引数(型:SelectedItemCollection)</param>
        /// <returns>変換後のSynonymWordEntity</returns>
        private SynonymWordEntity ConvertParameterToSynonymWordEntity(object parameter)
        {
            if (parameter == null)
            {
                Logger.Error(CLASS_NAME, "ConvertParameterToSynonymWordEntity", "parameter is null!");
                return null;
            }

            System.Collections.IList selectedItem = (System.Collections.IList)parameter;
            if (selectedItem == null || selectedItem.Count < 1)
            {
                Logger.Error(CLASS_NAME, "ConvertParameterToSynonymWordEntity", "selectedItem is incorrect!");
                return null;
            }

            var selectedSynonymWord = selectedItem.Cast<SynonymWordEntity>();
            if (selectedSynonymWord == null || selectedSynonymWord.Any() == false)
            {
                Logger.Error(CLASS_NAME, "ConvertParameterToSynonymWordEntity", "selectedWord is incorrect!");
                return null;
            }

            // 常に0番目の要素を返す(1つのみ選択することが前提なので)
            return selectedSynonymWord.FirstOrDefault();
        }


        #endregion

    }
}
