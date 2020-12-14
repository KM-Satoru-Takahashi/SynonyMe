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
        SynonymWindowModel _model = null;

        #endregion

        #region property

        /// <summary>タイトルテキスト</summary>
        public string SynonymWindowTitle { get; } = "Test";

        /// <summary>類語グループリスト、類語一覧リストの「更新日：」テキスト</summary>
        public string UpdateDate { get; } = "更新日：";

        /// <summary>類語グループリスト、類語一覧リストの「追加日：」テキスト</summary>
        public string RegistDate { get; } = "追加日：";

        /// <summary>類語グループリスト登録コマンド</summary>
        public ICommand Command_SynonymGroupRegist { get; protected set; } = null;

        /// <summary>類語グループリスト編集コマンド</summary>
        public ICommand Command_SynonymGroupEdit { get; protected set; } = null;

        /// <summary>類語グループリスト削除コマンド</summary>
        public ICommand Command_SynonymGroupDelete { get; protected set; } = null;

        /// <summary>類語一覧リスト登録コマンド</summary>
        public ICommand Command_SynonymItemRegist { get; protected set; } = null;

        /// <summary>類語一覧リスト編集コマンド</summary>
        public ICommand Command_SynonymItemEdit { get; protected set; } = null;

        /// <summary>類語一覧リスト削除コマンド</summary>
        public ICommand Command_SynonymItemDelete { get; protected set; } = null;

        /// <summary>閉じるボタン押下時コマンド</summary>
        public ICommand Command_Close { get; protected set; } = null;

        #endregion

        #region method

        /// <summary>コンストラクタ</summary>
        public SynonymWindowVM()
        {
#if false // 初回生成時のみの処理をコメントアウト->通常は実行済みなので気にしない、将来削除予定
            /* SynonymData.dbの初回生成時のみ実行　Gitコミット時はCreateとテスト用データのInsertが済んでいるのでコメントアウト
            // テスト用 DBの生成
            var sqlName = new SQLiteConnectionStringBuilder { DataSource = "SynonymData.db" };

            using(var con = new SQLiteConnection(sqlName.ToString()))
            {
                con.Open();

                using(var cmd = new SQLiteCommand(con))
                {
                    cmd.CommandText = "CREATE TABLE IF NOT EXISTS SynonymData("
                        + "synonymGroup TEXT NOT NULL,"
                        +"synonymWord TEXT NOT NULL)";
                    cmd.ExecuteNonQuery();

                    string dataAddSql = "INSERT INTO SynonymData (synonymGroup, synonymWord) values ('testGroup1','testWord11') ; ";
                    dataAddSql += "INSERT INTO SynonymData(synonymGroup, synonymWord) values ('testGroup1', 'testWord12') ; ";
                    dataAddSql += "INSERT INTO SynonymData(synonymGroup, synonymWord) values ('testGroup1', 'testWord13') ; ";
                    dataAddSql += "INSERT INTO SynonymData(synonymGroup, synonymWord) values ('testGroup2', 'testWord21') ; ";
                    dataAddSql += "INSERT INTO SynonymData(synonymGroup, synonymWord) values ('testGroup2', 'testWord22') ; ";

                    cmd.CommandText = dataAddSql;
                    cmd.ExecuteNonQuery();
                }
            }
            */
#endif

            #region コマンド初期化

            Command_Close = new CommandBase(ExecuteClose, null);

            #endregion

            _model = new SynonymWindowModel(this);
        }


        private void ExecuteClose(object parameter)
        {
            if(_model == null)
            {
                throw new NullReferenceException("SynonymWindowModel is null");
            }

            _model.CloseSynonymWindow();   
        }

        #endregion

    }
}
