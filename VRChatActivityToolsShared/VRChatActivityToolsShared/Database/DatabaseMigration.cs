using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Text;

namespace VRChatActivityToolsShared.Database
{
    /// <summary>
    /// DBのマイグレーションを行うクラスです。
    /// </summary>
    public static class DatabaseMigration
    {
        /// <summary>
        /// データベースを新規作成します。
        /// </summary>
        /// <returns></returns>
        public static void CreateDatabase()
        {
            using var db = new SqliteConnection($"Filename={DatabaseContext.DBFilePath}");

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

CREATE TABLE ""ActivityLogs"" (
    ""ID"" INTEGER NOT NULL CONSTRAINT ""PK_ActivityLogs"" PRIMARY KEY AUTOINCREMENT,
    ""ActivityType"" INTEGER NOT NULL,
    ""Timestamp"" TEXT NULL,
    ""NotificationID"" TEXT NULL,
    ""UserID"" TEXT NULL,
    ""UserName"" TEXT NULL,
    ""WorldID"" TEXT NULL,
    ""WorldName"" TEXT NULL,
    ""Message"" TEXT NULL,
    ""Url"" TEXT NULL
);
";
            #endregion

            using var command = new SqliteCommand(sql, db);

            command.Parameters.Add(new SqliteParameter("@Version", DatabaseContext.Version));

            command.ExecuteNonQuery();
        }

        /// <summary>
        /// 現在のデータベースがどのバージョンで作成されたかを取得します。
        /// </summary>
        /// <returns></returns>
        public static int GetCurrentVersion()
        {
            using (var db = new SqliteConnection($"Filename={DatabaseContext.DBFilePath}"))
            {
                db.Open();

                #region var existsTableSql = "...";
                var existsTableSql =
@"
SELECT
    COUNT(*)
FROM
    sqlite_master
WHERE
    TYPE = 'table' AND
    name = 'Information';
";
                #endregion

                using (var command = new SqliteCommand(existsTableSql, db))
                {
                    using var reader = command.ExecuteReader();
                    reader.Read();

                    var hasTable = 0 < reader.GetInt32(0);

                    if (!hasTable)
                    {
                        return 1;
                    }
                }

                #region var versionSql = "...";
                var versionSql =
@"
SELECT
    Version
FROM
    Information;
";
                #endregion

                using (var command = new SqliteCommand(versionSql, db))
                {
                    using var reader = command.ExecuteReader();
                    reader.Read();

                    var version = reader.GetInt32(0);

                    return version;
                }
            }
        }

        /// <summary>
        /// データベースを更新します。
        /// </summary>
        public static void UpgradeDatabase()
        {
            var currentVersion = GetCurrentVersion();

            if (currentVersion < 2)
            {
                UpgradeDatabaseVersion2();
            }
        }

        /// <summary>
        /// データベースをver2へ更新します。
        /// </summary>
        private static void UpgradeDatabaseVersion2()
        {
            using var db = new SqliteConnection($"Filename={DatabaseContext.DBFilePath}");

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

            using var command = new SqliteCommand(sql, db);

            command.Parameters.Add(new SqliteParameter("@Version", 2));

            command.ExecuteNonQuery();
        }
    }
}
