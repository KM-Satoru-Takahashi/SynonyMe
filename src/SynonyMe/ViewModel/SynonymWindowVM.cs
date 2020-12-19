using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using SynonyMe.Model;
using System.Data.SQLite;   // DB
using System.Collections.ObjectModel;

namespace SynonyMe.ViewModel
{
    public class SynonymWindowVM : ViewModelBase
    {
        #region field

        /// <summary>model</summary>
        private SynonymWindowModel _model = null;

        /// <summary>表示中の類語グループリスト</summary>
        private ObservableCollection<CommonLibrary.SynonymGroupEntity> _displaySynonymGroup = null;

        /// <summary>類語グループリストから選択した類語グループ</summary>
        private ObservableCollection<CommonLibrary.SynonymWordEntity> _displaySynonymWords = null;

        /// <summary>類語一覧リストから選択した類語</summary>
        private CommonLibrary.SynonymWordEntity _selectSynonymWord = null;

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
        public ICommand Command_RegistSynonymGroup { get; protected set; } = null;

        /// <summary>類語グループリスト編集コマンド</summary>
        public ICommand Command_EditSynonymGroup { get; protected set; } = null;

        /// <summary>類語グループリスト削除コマンド</summary>
        public ICommand Command_DeleteSynonymGroup { get; protected set; } = null;

        /// <summary>類語一覧リスト登録コマンド</summary>
        public ICommand Command_RegistSynonymWord { get; protected set; } = null;

        /// <summary>類語一覧リスト編集コマンド</summary>
        public ICommand Command_EditSynonymWord { get; protected set; } = null;

        /// <summary>類語一覧リスト削除コマンド</summary>
        public ICommand Command_DeleteSynonymWord { get; protected set; } = null;

        /// <summary>閉じるボタン押下時コマンド</summary>
        public ICommand Command_Close { get; protected set; } = null;

        /// <summary>類語グループリストからグループ選択時の実行コマンド</summary>
        /// <remarks>類語一覧リストに類語を表示させるために用いる</remarks>
        public ICommand Command_SelectSynonymGroup { get; protected set; } = null;

        /// <summary>類語一覧リストから類語選択時の実行コマンド</summary>
        public ICommand Command_SelectSynonymWord { get; protected set; } = null;

        /// <summary>類語グループ入力テキスト</summary>
        public string InputGroupWord { get; set; } = null;

        /// <summary>類語入力テキスト</summary>
        public string InputSynonymWord { get; set; } = null;

        /// <summary>類語グループリスト一覧オブジェクト</summary>
        public ObservableCollection<CommonLibrary.SynonymGroupEntity> DisplaySynonymGroups
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
        public ObservableCollection<CommonLibrary.SynonymWordEntity> DisplaySynonymWords
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
                        "GroupID INTERGER PRIMARY KEY ," +    // NOT NULLをつけるとIDをnullでの自動発番ができなくなる
                        "GroupName TEXT NOT NULL," +
                        "GroupRegistDate TEXT NOT NULL, " +
                        "GroupUpdateDate TEXT NOT NULL )";
                    cmd.ExecuteNonQuery();

                    // 初回だけ
                    string dataAddSql = "INSERT INTO SynonymGroup (GroupID, GroupName, GroupRegistDate, GroupUpdateDate) values ('1','testGroup1', '20201214', '20201215') ; ";
                    dataAddSql += "INSERT INTO SynonymGroup (GroupID, GroupName, GroupRegistDate, GroupUpdateDate) values ('2','testGroup2', '20201214', '20201215') ; ";
                    dataAddSql += "INSERT INTO SynonymGroup (GroupID, GroupName, GroupRegistDate, GroupUpdateDate) values ('3','testGroup3', '20201214', '20201215') ; ";

                    cmd.CommandText = dataAddSql;
                    cmd.ExecuteNonQuery();
                    
                    // SynonymWord
                    cmd.CommandText = "CREATE TABLE IF NOT EXISTS SynonymWords( " +
                        "WordID INTERGER PRIMARY KEY UNIQUE NOT NULL," +
                        "GroupID INTERGER, " +
                        "Word TEXT NOT NULL, " +
                        "RegistDate TEXT NOT NULL, " +
                        "UpdateDate TEXT NOT NULL, " +
                        "FOREIGN KEY (GroupID) REFERENCES SynonymGroup (GroupID) " +
                        ")";

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

            // debug
            DisplaySynonymWords = new ObservableCollection<CommonLibrary.SynonymWordEntity>();

            _model = new SynonymWindowModel(this);

            // _modelをnewした直後なのでnullチェックはさすがに省略する
            // DBから必要な情報を取得する
            DisplaySynonymGroups = new ObservableCollection<CommonLibrary.SynonymGroupEntity>(_model.GetAllSynonymGroup());
        }

        /// <summary>閉じるボタン押下時処理</summary>
        /// <param name="parameter"></param>
        private void ExecuteClose(object parameter)
        {
            if (_model == null)
            {
                throw new NullReferenceException("SynonymWindowModel is null");
            }

            _model.CloseSynonymWindow();
        }

        /// <summary>類語グループリスト登録コマンド</summary>
        /// <param name="parameter"></param>
        private void ExecuteRegistSynonymGroup(object parameter)
        {
            if (string.IsNullOrEmpty(InputGroupWord))
            {
                return;
            }

            if (_model == null)
            {
                throw new NullReferenceException("ExecuteRegistSynonymGroup _model is null");
            }

            if (_model.RegistSynonymGroup(InputGroupWord) == false)
            {
                throw new Exception("ExecuteRegistSynonymGroup exception");
            }

            // 登録が正常に行われたなら、表示中の類語グループリストを更新する
            DisplaySynonymGroups = new ObservableCollection<CommonLibrary.SynonymGroupEntity>(_model.GetAllSynonymGroup());
        }

        /// <summary>類語グループリスト編集コマンド</summary>
        /// <param name="parameter"></param>
        private void ExecuteEditSynonymGroup(object parameter)
        {

        }

        private void ExecuteDeleteSynonymGroup(object parameter)
        {

        }

        /// <summary>類語登録コマンド</summary>
        /// <param name="parameter"></param>
        private void ExecuteRegistSynonymWord(object parameter)
        {
            if (string.IsNullOrEmpty(InputSynonymWord))
            {
                return;
            }
        }

        /// <summary>類語編集コマンド</summary>
        /// <param name="parameter"></param>
        private void ExecuteEditSynonymWord(object parameter)
        {
            if (_selectSynonymWord == null)
            {
                return;
            }
        }

        /// <summary>類語削除コマンド</summary>
        /// <param name="parameter"></param>
        private void ExecuteDeleteSynonymWord(object parameter)
        {

        }

        /// <summary>類語グループ選択時実行コマンド</summary>
        /// <param name="parameter">選択した類語グループ</param>
        private void ExecuteSelectSynonymGroup(object parameter)
        {
            if (parameter == null)
            {
                return;
            }

            CommonLibrary.SynonymGroupEntity selectedGroup = ConvertParameterToSynonymGroupEntity(parameter);
            if (selectedGroup == null)
            {
                return;
            }

            UpdateDisplaySynonymWords(selectedGroup.GroupID);
        }

        /// <summary>類語一覧リスト選択時実行コマンド</summary>
        /// <param name="parameter"></param>
        private void ExecuteSelectSynonymWord(object parameter)
        {
            if (parameter == null)
            {
                return;
            }

            CommonLibrary.SynonymWordEntity selectedWord = ConvertParameterToSynonymWordEntity(parameter);
            if (selectedWord == null)
            {
                return;
            }
        }

        /// <summary>表示中の類語一覧リストを更新する</summary>
        /// <param name="groupID">表示対象となる類語一覧リストのグループID</param>
        private void UpdateDisplaySynonymWords(int groupID)
        {
            CommonLibrary.SynonymWordEntity[] synonymWordsArray = _model.GetSynonymWordEntities(groupID);
            if (synonymWordsArray == null || synonymWordsArray.Any() == false)
            {
                // nullまたは空の配列の場合、表示を空にする
                DisplaySynonymWords = null;
                return;
            }

            List<CommonLibrary.SynonymWordEntity> synonymWords = synonymWordsArray.ToList();
            DisplaySynonymWords = new ObservableCollection<CommonLibrary.SynonymWordEntity>(synonymWords);
        }

        /// <summary>類語グループリストの選択(SelectedItemCollection)を、SynonymGroupEntity形式に変換する</summary>
        /// <param name="parameter">選択状態の引数(型:SelectedItemCollection)</param>
        /// <returns>変換後のSynonymGroupEntity</returns>
        private CommonLibrary.SynonymGroupEntity ConvertParameterToSynonymGroupEntity(object parameter)
        {
            if (parameter == null)
            {
                return null;
            }

            System.Collections.IList selectedItem = (System.Collections.IList)parameter;
            if (selectedItem == null || selectedItem.Count < 1)
            {
                return null;
            }

            var selectedSynonymGroup = selectedItem.Cast<CommonLibrary.SynonymGroupEntity>();
            if (selectedSynonymGroup == null || selectedSynonymGroup.Any() == false)
            {
                return null;
            }

            // 常に0番目の要素を返す(1つのみ選択することが前提なので)
            return selectedSynonymGroup.FirstOrDefault();
        }

        /// <summary>類語リストの選択(SelectedItemCollection)をSynonymWordEntity形式に変換する</summary>
        /// <param name="parameter">選択状態の引数(型:SelectedItemCollection)</param>
        /// <returns>変換後のSynonymWordEntity</returns>
        private CommonLibrary.SynonymWordEntity ConvertParameterToSynonymWordEntity(object parameter)
        {
            if (parameter == null)
            {
                return null;
            }

            System.Collections.IList selectedItem = (System.Collections.IList)parameter;
            if (selectedItem == null || selectedItem.Count < 1)
            {
                return null;
            }

            var selectedSynonymWord = selectedItem.Cast<CommonLibrary.SynonymWordEntity>();
            if (selectedSynonymWord == null || selectedSynonymWord.Any() == false)
            {
                return null;
            }

            // 常に0番目の要素を返す(1つのみ選択することが前提なので)
            return selectedSynonymWord.FirstOrDefault();
        }
        #endregion

    }
}
