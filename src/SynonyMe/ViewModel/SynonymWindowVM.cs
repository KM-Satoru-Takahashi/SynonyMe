using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;   // DB
using System.Windows.Input;
using SynonyMe.Model;

namespace SynonyMe.ViewModel
{
    public class SynonymWindowVM : ViewModelBase
    {
        #region field

        /// <summary>model</summary>
        private SynonymWindowModel _model = null;

        #endregion

        #region property

        /// <summary>タイトルテキスト</summary>
        public string SynonymWindowTitle { get; } = "Test";

        /// <summary>類語グループリスト、類語一覧リストの「更新日：」テキスト</summary>
        public string UpdateDate { get; } = "更新日：";

        /// <summary>類語グループリスト、類語一覧リストの「追加日：」テキスト</summary>
        public string RegistDate { get; } = "追加日：";

        /// <summary>類語グループリスト登録コマンド</summary>
        public ICommand Command_RegistSynonymGroup { get; protected set; } = null;

        /// <summary>類語グループリスト編集コマンド</summary>
        public ICommand Command_EditSynonymGroup { get; protected set; } = null;

        /// <summary>類語グループリスト削除コマンド</summary>
        public ICommand Command_DeleteSynonymGroup { get; protected set; } = null;

        /// <summary>類語一覧リスト登録コマンド</summary>
        public ICommand Command_RegistSynonymItem { get; protected set; } = null;

        /// <summary>類語一覧リスト編集コマンド</summary>
        public ICommand Command_EditSynonymItem { get; protected set; } = null;

        /// <summary>類語一覧リスト削除コマンド</summary>
        public ICommand Command_DeleteSynonymItem { get; protected set; } = null;

        /// <summary>閉じるボタン押下時コマンド</summary>
        public ICommand Command_Close { get; protected set; } = null;

        /// <summary>類語グループ入力テキスト</summary>
        public string InputGroupWord { get; set; } = null;


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
                        "GroupID INTERGER PRIMARY KEY UNIQUE AUTOINCREMENT," +    // NOT NULLをつけるとIDをnullでの自動発番ができなくなる
                        "GroupName TEXT NOT NULL," +
                        "GroupRegistDate TEXT NOT NULL, " +
                        "GroupUpdateDate TEXT NOT NULL )";
                    cmd.ExecuteNonQuery();

                    /* // 初回だけ
                    string dataAddSql = "INSERT INTO SynonymGroup (GroupID, GroupName, GroupRegistDate, GroupUpdateDate) values ('1','testGroup1', '20201214', '20201215') ; ";
                    dataAddSql += "INSERT INTO SynonymGroup (GroupID, GroupName, GroupRegistDate, GroupUpdateDate) values ('2','testGroup2', '20201214', '20201215') ; ";
                    dataAddSql += "INSERT INTO SynonymGroup (GroupID, GroupName, GroupRegistDate, GroupUpdateDate) values ('3','testGroup3', '20201214', '20201215') ; ";

                    cmd.CommandText = dataAddSql;
                    cmd.ExecuteNonQuery();
                    */

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

            Command_RegistSynonymGroup = new CommandBase(ExecuteRegistSynonymGroup, null);
            Command_EditSynonymGroup = new CommandBase(ExecuteEditSynonymGroup, null);
            Command_DeleteSynonymGroup = new CommandBase(ExecuteDeleteSynonymGroup, null);
            Command_RegistSynonymItem = new CommandBase(ExecuteRegistSynonymItem, null);
            Command_EditSynonymItem = new CommandBase(ExecuteEditSynonymItem, null);
            Command_DeleteSynonymItem = new CommandBase(ExecuteDeleteSynonymItem, null);
            Command_Close = new CommandBase(ExecuteClose, null);

            #endregion

            _model = new SynonymWindowModel(this);
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
        private void ExecuteRegistSynonymItem(object parameter)
        {

        }

        /// <summary>類語編集コマンド</summary>
        /// <param name="parameter"></param>
        private void ExecuteEditSynonymItem(object parameter)
        {

        }

        /// <summary>類語削除コマンド</summary>
        /// <param name="parameter"></param>
        private void ExecuteDeleteSynonymItem(object parameter)
        {

        }
        #endregion

    }
}
