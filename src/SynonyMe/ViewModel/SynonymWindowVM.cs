using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;   // DB

namespace SynonyMe.ViewModel
{
    public class SynonymWindowVM : ViewModelBase
    {
        #region field

        #endregion

        #region property

        public string SynonymWindowTitle { get; } = "Test";

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
        }

#endregion

    }
}
