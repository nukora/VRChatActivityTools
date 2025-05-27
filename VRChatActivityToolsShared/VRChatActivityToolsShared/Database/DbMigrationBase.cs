using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace VRChatActivityToolsShared.Database
{
    /// <summary>
    /// DBのマイグレーションを行うクラスです。
    /// </summary>
    public abstract class DbMigrationBase : IDbMigration
    {
        public string ConnectionString { get; protected set; }

        public DbMigrationBase(string connectionString)
        {
            ConnectionString = connectionString;
        }

        /// <summary>
        /// DBへの接続オブジェクトを求める
        /// </summary>
        /// <returns></returns>
        protected abstract DbConnection GetDbConnection();

        /// <summary>
        /// SQL応答を得るオブジェクトを求める
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        protected abstract DbCommand GetDbCommand(string sql, DbConnection db);

        public void CreateDbAndTables(ActivityContextBase context)
        {
            context.Database.EnsureCreated();
            context.Add(new Information { Version = ActivityContextBase.Version });
            context.SaveChanges();
        }

        public int GetCurrentVersion(ActivityContextBase context)
        {
            if (!context.HasTable("Information"))
            { return 1; }
            return context.Information.Select(a => a.Version).First();
        }

        /// <summary>
        /// データベースを更新します。
        /// </summary>
        public void UpgradeDatabase(ActivityContextBase context)
        {
            var currentVersion = GetCurrentVersion(context);

            if (currentVersion < 2)
            {
                UpgradeDatabaseVersion2();
            }

            if (currentVersion < 3)
            {
                UpgradeDatabaseVersion3(context);
            }
        }

        /// <summary>
        /// データベースをver2へ更新します。
        /// </summary>
        private void UpgradeDatabaseVersion2()
        {
            using var db = GetDbConnection();

            db.Open();

            #region var sql = "...";
            var sql =
@"
CREATE TABLE ""Information"" (
    ""ID"" INTEGER NOT NULL CONSTRAINT ""PK_Information"" PRIMARY KEY AUTOINCREMENT,
    ""Version"" INTEGER NOT NULL
);

INSERT INTO ""Information"" (
    ""Version""
)
VALUES (
    @Version
);

ALTER TABLE
    ""ActivityLogs""
ADD COLUMN
    ""Message"" TEXT NULL;

ALTER TABLE
    ""ActivityLogs""
ADD COLUMN
    ""Url"" TEXT NULL;
";
            #endregion

            using var command = GetDbCommand(sql, db);

            command.Parameters.Add(new SqliteParameter("@Version", 2));

            command.ExecuteNonQuery();
        }

        /// <summary>
        /// データベースをver3へ更新します。
        /// </summary>
        private void UpgradeDatabaseVersion3(ActivityContextBase context)
        {
            foreach (var inf in context.Information)
            { inf.Version = 3; }
            context.SaveChanges();
        }
    }
}
