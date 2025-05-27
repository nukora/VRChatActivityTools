using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using MySqlConnector;
using Pomelo.EntityFrameworkCore;
using MySql.Data.MySqlClient;

namespace VRChatActivityToolsShared.Database
{
    public class DbMigrationMariaDb : DbMigrationBase
    {
        protected override DbCommand GetDbCommand(string sql, DbConnection db) =>
            new MySqlCommand(sql, (MySqlConnection)db);

        protected override DbConnection GetDbConnection() =>
            new MySqlConnection(ConnectionString);

        public DbMigrationMariaDb(string connectionString) : base(connectionString) { }
    }
}
