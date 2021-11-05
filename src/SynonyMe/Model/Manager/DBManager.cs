using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SQLite;   // DB
using SynonyMe.CommonLibrary.Entity;
using System.IO;
using SynonyMe.CommonLibrary.Log;

namespace SynonyMe.Model.Manager
{
    /// <summary>DB関連制御クラス</summary>
    /// <remarks>!必ず処理に明示的トランザクションを実行すべし!</remarks>
    internal class DBManager : IDisposable
    {
        /// <summary>SQLiteDBへの接続情報</summary>
        private SQLiteConnection sqLiteConnection = null;

        /// <summary>SQLiteDBでの接続と処理実行で用いるTransaction</summary>
        /// <remarks>SQLiteでは明示的にトランザクションを開始しない場合、INSERT句の開始終了で暗示的なトランザクションが行われる。
        /// 暗示的なトランザクションと明示的トランザクションが混合していることを避けるため、全て明示的トランザクションとして行うべき</remarks>
        private SQLiteTransaction sqLiteTransaction = null;

        private bool _isDisposed = false;

        private const string CLASS_NAME = "DBManager";

        // unmanaged resourceがあるため、IDisposable関連を実装する
        #region IDisposable, Dispose, Destructor

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;

            if (disposing)
            {
                // dispose managed resources
                // 現状、管理リソースで明示的に破棄するべきものはない
            }

            // DBのConnectionとTransactionを必ず閉じる・廃棄する
            Close();
        }

        ~DBManager()
        {
            Dispose(false);
        }

        #endregion

        /// <summary>DB名指定で接続するコンストラクタ</summary>
        /// <param name="DBName">拡張子コミのDB名</param>
        internal DBManager(string DBName)
        {
            Logger.Info(CLASS_NAME, "DBManager", "constructor start.");

            if (string.IsNullOrEmpty(DBName))
            {
                Logger.Fatal(CLASS_NAME, "DBManager", "DBName is null or empty!");
                return;
            }

            // DBへの接続に必要なパス関連を取得する
            string filePath = CommonLibrary.SystemUtility.GetSynonymeExeFilePath();
            if (string.IsNullOrEmpty(filePath))
            {
                Logger.Fatal(CLASS_NAME, "DBManager", "DBManager constructor:Cannot find SynonyMe.exe");
                return;
            }

            // ファイル名情報は不要なので削除する
            filePath = filePath.Replace(CommonLibrary.Define.SYNONYME_EXENAME, "");

            // DBへのファイルパスを構築する。直後の[\]はファイル名ではなく、Replaceで削除されていないので、直に連結してOK
            filePath += @"DB\SynonymData.db";

            SQLiteConnectionStringBuilder sqlDBName = new SQLiteConnectionStringBuilder { DataSource = filePath };
            if (sqlDBName == null)
            {
                Logger.Fatal(CLASS_NAME, "DBManager", "sqlDBName is null");
            }

            sqLiteConnection = new SQLiteConnection(sqlDBName.ToString());
            if (sqLiteConnection == null)
            {
                Logger.Fatal(CLASS_NAME, "DBManager", "sqLiteConnection is null");
                return;
            }

            // 直後に次のDB操作が来ることは自明なので開いておく
            sqLiteConnection.Open();
        }

        /// <summary>DB接続を閉じる</summary>
        private void Close()
        {
            Logger.Info(CLASS_NAME, "Close", "start");

            // returnするとconnectionの処理ができないので、非null判定で実施
            if (sqLiteTransaction != null)
            {
                sqLiteTransaction.Dispose();
            }

            if (sqLiteConnection == null)
            {
                Logger.Fatal(CLASS_NAME, "Close", "sqLiteConnection is null!");
                return;
            }

            sqLiteConnection.Close();
            sqLiteConnection.Dispose();
        }

        /// <summary>トランザクションの開始</summary>
        /// <returns>true:正常, false:異常</returns>
        private bool BeginTransaction()
        {
            Logger.Info(CLASS_NAME, "BeginTrasaction", "start");

            if (sqLiteConnection == null)
            {
                Logger.Fatal(CLASS_NAME, "BeginTrasaction", "sqLiteConnection is null!");
                return false;
            }

            sqLiteTransaction = sqLiteConnection.BeginTransaction();
            if (sqLiteTransaction == null)
            {
                Logger.Fatal(CLASS_NAME, "BeginTrasaction", "sqLiteTransaction is null!");
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>クエリによりSynonymGroupEntityを取得する</summary>
        /// <param name="sql">実行対象のSQL文</param>
        /// <param name="synonymGroups">取得したSynonymGroupEntity配列</param>
        /// <returns>成功時:true, 失敗時:false</returns>
        internal bool GetTargetSynonymGroups(string sql, out SynonymGroupEntity[] synonymGroups)
        {
            Logger.Info(CLASS_NAME, "GetTargetSynonymGroups", $"start, SQL:[{sql}]");

            synonymGroups = null;
            if (string.IsNullOrEmpty(sql))
            {
                Logger.Error(CLASS_NAME, "GetTargetSynonymGroups", "sql is null or empty");
                return false;
            }

            using (SQLiteDataReader reader = ExecuteQuery(sql))
            {
                try
                {
                    if (reader == null)
                    {
                        // 実行結果が必ずあるはずなのに、何もないのは異常だろう
                        Logger.Error(CLASS_NAME, "GetTargetSynonymGroups", "reader result is null");
                        return false;
                    }

                    List<SynonymGroupEntity> synonymGroupList = new List<SynonymGroupEntity>();

                    // SQLの実行結果をここで格納する
                    while (reader.Read())
                    {
                        // SQLiteのInteger型はInt64->C#ではlong型に値する
                        // 現状、上限値をintの最大値としてID付加時に処理するため、int型に無条件でキャストしてよい
                        int groupID = -1;
                        long? nullableGroupID = reader["GroupID"] as long?;
                        if (nullableGroupID != null)
                        {
                            groupID = (int)nullableGroupID;
                        }
                        string groupName = reader["GroupName"] as string;
                        string groupRegistDate = reader["GroupRegistDate"] as string;
                        string groupUpdateDate = reader["GroupUpdateDate"] as string;

                        SynonymGroupEntity synonymGroupEntity = new SynonymGroupEntity
                        {
                            GroupID = groupID,
                            GroupName = groupName,
                            GroupRegistDate = groupRegistDate,
                            GroupUpdateDate = groupUpdateDate
                        };
                        synonymGroupList.Add(synonymGroupEntity);
                    }

                    if (synonymGroupList != null)
                    {
                        synonymGroups = synonymGroupList.ToArray();
                    }

                    return true;
                }
                catch (Exception e)
                {
                    Logger.Fatal(CLASS_NAME, "GetTargetSynonymGroups", e.Message);
                    return false;
                }
            }
        }

        /// <summary>クエリによりSynonymWordsEntityを取得する</summary>
        /// <param name="sql"></param>
        /// <param name="synonymWords"></param>
        /// <returns></returns>
        internal bool GetTargetSynonymWords(string sql, out SynonymWordEntity[] synonymWords)
        {
            Logger.Info(CLASS_NAME, "GetTargetSynonymWrods", $"start, SQL[{sql}]");

            synonymWords = null;

            if (string.IsNullOrEmpty(sql))
            {
                Logger.Fatal(CLASS_NAME, "GetTargetSynonymWrods", "sql is null or empty!");
                return false;
            }

            using (SQLiteDataReader reader = ExecuteQuery(sql))
            {
                if (reader == null)
                {
                    // 実行結果が必ずあるはずなのに、何もないのは異常だろう
                    Logger.Error(CLASS_NAME, "GetTargetSynonymWrods", "reader result is null");
                    return false;
                }

                List<SynonymWordEntity> synonymWordList = new List<SynonymWordEntity>();
                try
                {
                    // SQLの実行結果をここで格納する
                    while (reader.Read())
                    {
                        // SQLiteのInteger型はInt64->C#ではlong型に値する
                        // 現状、上限値をintの最大値としてID付加時に処理するため、int型に無条件でキャストしてよい
                        int wordID = -1;
                        long? nullableWordID = reader["WordID"] as long?;
                        if (nullableWordID != null)
                        {
                            wordID = (int)nullableWordID;
                        }

                        int groupID = -1;
                        long? nullableGroupID = reader["GroupID"] as long?;
                        if (nullableGroupID != null)
                        {
                            groupID = (int)nullableGroupID;
                        }

                        string word = reader["Word"] as string;
                        string wordRegistDate = reader["RegistDate"] as string;
                        string wordUpdateDate = reader["UpdateDate"] as string;

                        SynonymWordEntity synonymWordEntity = new SynonymWordEntity
                        {
                            WordID = wordID,
                            GroupID = groupID,
                            Word = word,
                            RegistDate = wordRegistDate,
                            UpdateDate = wordUpdateDate
                        };
                        synonymWordList.Add(synonymWordEntity);
                    }

                    if (synonymWordList != null)
                    {
                        synonymWords = synonymWordList.ToArray();
                    }

                    return true;
                }
                catch (Exception e)
                {
                    Logger.Fatal(CLASS_NAME, "GetTargetSynonymWords", e.Message);
                    return false;
                }
            }
        }

        /// <summary>レコードを取得するSQLの実行処理を行う</summary>
        /// <param name="sql">実行したいSQL文</param>
        /// <returns>結果を保持したReaderインスタンス★呼び出し元で適切に破棄する！</returns>
        private SQLiteDataReader ExecuteQuery(string sql)
        {
            Logger.Info(CLASS_NAME, "ExecuteQuery", $"start, SQL:[{sql}]");

            if (string.IsNullOrEmpty(sql))
            {
                Logger.Error(CLASS_NAME, "ExecuteQuery", "sql is null or empty");
                return null;
            }

            if (sqLiteConnection == null || sqLiteConnection.State != ConnectionState.Open)
            {
                string conStatus = sqLiteConnection == null ? "null" : sqLiteConnection.State.ToString();
                Logger.Fatal(CLASS_NAME, "ExecuteQuery", $"sqLiteConnection is incorrect! status:[{conStatus}]");
                return null;
            }

            using (SQLiteCommand command = sqLiteConnection.CreateCommand())
            {
                if (command == null)
                {
                    Logger.Error(CLASS_NAME, "ExecuteQuery", "command is null!");
                    return null;
                }

                command.CommandText = sql;
                try
                {
                    return command.ExecuteReader();
                }
                catch (Exception e)
                {
                    Logger.Fatal(CLASS_NAME, "ExecuteQuery", e.Message);
                    return null;
                }
                // 呼び出し元でCloseしているので、ExecuteQueryではfinally句でCloseしなくて良い
            }
        }


        /// <summary>Query実行結果を必要としないSQLを実行する</summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        internal bool ExecuteNonQuery(string sql)
        {
            Logger.Info(CLASS_NAME, "ExecuteNonQuery", $"start, SQL[{sql}]");

            if (string.IsNullOrEmpty(sql))
            {
                Logger.Error(CLASS_NAME, "ExecuteNonQuery", "sql is incorrect!");
                return false;
            }

            if (sqLiteConnection == null || sqLiteConnection.State != ConnectionState.Open)
            {
                string conStatus = sqLiteConnection == null ? "null" : sqLiteConnection.State.ToString();
                Logger.Fatal(CLASS_NAME, "ExecuteNonQuery", $"sqLiteConnection is incorrect! status:[{conStatus}]");
                return false;
            }

            using (SQLiteCommand command = sqLiteConnection.CreateCommand())
            {
                if (command == null)
                {
                    Logger.Fatal(CLASS_NAME, "ExecuteNonQuery", "command is null");
                    return false;
                }

                command.CommandText = sql;
                try
                {
                    if (BeginTransaction() == false)
                    {
                        Logger.Fatal(CLASS_NAME, "ExecuteNonQuery", "BeginTransaction Failed");
                        return false;
                    }

                    int result = command.ExecuteNonQuery();

                    if (IsSuccessed(result))
                    {
                        Commit();
                        return true;
                    }
                    else
                    {
                        Logger.Error(CLASS_NAME, "ExecuteNonQuery", "Execute query failed. Rollback start.");
                        Rollback();
                        return false;
                    }
                }
                catch (Exception e)
                {
                    Logger.Fatal(CLASS_NAME, "ExecuteNonQuery", e.Message);
                    return false;
                }
            }
        }

        /// <summary>ExecuteNonQuery実行時の成否判定</summary>
        /// <param name="result">ExecuteNonQueryの戻り値</param>
        /// <returns>true:成功, false:失敗</returns>
        private bool IsSuccessed(int result)
        {
            if (result < CommonLibrary.Define.EXECUTE_NONQUERY_SUCCESSED)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>クエリ実行成功時にコミットを行う</summary>
        private void Commit()
        {
            if (sqLiteTransaction == null)
            {
                Logger.Fatal(CLASS_NAME, "Commit", "sqLiteTransaction is null");
                return;
            }

            sqLiteTransaction.Commit();
        }

        /// <summary>クエリ失敗時にロールバックする</summary>
        private void Rollback()
        {
            if (sqLiteTransaction == null)
            {
                Logger.Fatal(CLASS_NAME, "Rollback", "sqLiteTransaction is null");
                return;
            }

            sqLiteTransaction.Rollback();
        }
    }
}
