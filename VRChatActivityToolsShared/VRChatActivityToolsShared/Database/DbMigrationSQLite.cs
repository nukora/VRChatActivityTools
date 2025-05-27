using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace VRChatActivityToolsShared.Database
{
    /// <summary>
    /// DBのマイグレーションを行うクラスです。
    /// </summary>
    public class DbMigrationSQLite : DbMigrationBase
    {
        public DbMigrationSQLite(string connectionString) : base(connectionString) { }

        protected override DbConnection GetDbConnection() =>
            new SqliteConnection(ConnectionString);

        protected override DbCommand GetDbCommand(string sql, DbConnection db) =>
            new SqliteCommand(sql, (SqliteConnection)db);
    }
}
