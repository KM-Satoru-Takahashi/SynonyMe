using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SQLite;   // DB


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
            if (string.IsNullOrEmpty(DBName))
            {
                // DB名は内部的に保持しているので、異常はthrowするべき
                throw new ArgumentException("DBName is null or empty");
            }

            SQLiteConnectionStringBuilder sqlDBName = new SQLiteConnectionStringBuilder { DataSource = DBName };
            if (sqlDBName == null)
            {
                throw new NullReferenceException("sqlDBName is null");
            }

            sqLiteConnection = new SQLiteConnection(sqlDBName.ToString());
            if (sqLiteConnection == null)
            {
                throw new ArgumentException("sqLiteConnection is null");
            }

            // 直後に次のDB操作が来ることは自明なので開いておく
            sqLiteConnection.Open();
        }

        /// <summary>DB接続を閉じる</summary>
        private void Close()
        {
            // returnするとconnectionの処理ができないので、非null判定で実施
            if (sqLiteTransaction != null)
            {
                sqLiteTransaction.Dispose();
            }

            if (sqLiteConnection == null)
            {
                return;
            }

            sqLiteConnection.Close();
            sqLiteConnection.Dispose();
        }

        /// <summary>トランザクションの開始</summary>
        /// <returns>true:正常, false:異常</returns>
        private bool BeginTransaction()
        {
            if (sqLiteConnection == null)
            {
                return false;
            }

            sqLiteTransaction = sqLiteConnection.BeginTransaction();
            if (sqLiteTransaction == null)
            {
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
        internal bool GetTargetSynonymGroups(string sql, out CommonLibrary.SynonymGroupEntity[] synonymGroups)
        {
            synonymGroups = null;
            if (string.IsNullOrEmpty(sql))
            {
                return false;
            }

            using (SQLiteDataReader reader = ExecuteQuery(sql))
            {
                try
                {
                    if (reader == null)
                    {
                        // 実行結果が必ずあるはずなのに、何もないのは異常だろう
                        return false;
                    }

                    List<CommonLibrary.SynonymGroupEntity> synonymGroupList = new List<CommonLibrary.SynonymGroupEntity>();

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

                        CommonLibrary.SynonymGroupEntity synonymGroupEntity = new CommonLibrary.SynonymGroupEntity
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
                    throw new Exception(e.Message);
                }
            }
        }

        /// <summary>クエリによりSynonymWordsEntityを取得する</summary>
        /// <param name="sql"></param>
        /// <param name="synonymWords"></param>
        /// <returns></returns>
        internal bool GetTargetSynonymWords(string sql, out CommonLibrary.SynonymWordEntity[] synonymWords)
        {
            synonymWords = null;

            if (string.IsNullOrEmpty(sql))
            {
                return false;
            }

            using (SQLiteDataReader reader = ExecuteQuery(sql))
            {
                if (reader == null)
                {
                    // 実行結果が必ずあるはずなのに、何もないのは異常だろう
                    return false;
                }

                List<CommonLibrary.SynonymWordEntity> synonymWordList = new List<CommonLibrary.SynonymWordEntity>();
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

                        CommonLibrary.SynonymWordEntity synonymWordEntity = new CommonLibrary.SynonymWordEntity
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
                    throw new Exception(e.Message);
                }
            }
        }

        /// <summary>レコードを取得するSQLの実行処理を行う</summary>
        /// <param name="sql">実行したいSQL文</param>
        /// <returns>結果を保持したReaderインスタンス★呼び出し元で適切に破棄する！</returns>
        private SQLiteDataReader ExecuteQuery(string sql)
        {
            if (string.IsNullOrEmpty(sql))
            {
                return null;
            }

            if (sqLiteConnection == null || sqLiteConnection.State != ConnectionState.Open)
            {
                return null;
            }

            using (SQLiteCommand command = sqLiteConnection.CreateCommand())
            {
                if (command == null)
                {
                    return null;
                }

                command.CommandText = sql;
                try
                {
                    return command.ExecuteReader();
                }
                catch (Exception e)
                {
                    throw new Exception(e.Message);
                }
                // 呼び出し元でCloseしているので、ExecuteQueryではfinally句でCloseしなくて良い
            }
        }


        /// <summary>Query実行結果を必要としないSQLを実行する</summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        internal bool ExecuteNonQuery(string sql)
        {
            if (string.IsNullOrEmpty(sql))
            {
                return false;
            }

            if (sqLiteConnection == null || sqLiteConnection.State != ConnectionState.Open)
            {
                return false;
            }

            using (SQLiteCommand command = sqLiteConnection.CreateCommand())
            {
                if (command == null)
                {
                    return false;
                }

                command.CommandText = sql;
                try
                {
                    if (BeginTransaction() == false)
                    {
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
                        Rollback();
                        return false;
                    }
                }
                catch (Exception e)
                {
                    throw new Exception(e.Message);
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
                throw new NullReferenceException("Commit sqLiteTransaction is null");
            }

            sqLiteTransaction.Commit();
        }

        /// <summary>クエリ失敗時にロールバックする</summary>
        private void Rollback()
        {
            if (sqLiteTransaction == null)
            {
                throw new NullReferenceException("Rollback sqLiteTransaction is null");
            }

            sqLiteTransaction.Rollback();
        }
    }
}
