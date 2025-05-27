using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MySql.Data.MySqlClient;
using Pomelo.EntityFrameworkCore.MySql.Storage;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VRChatActivityToolsShared.Database
{
    public class DbOperatorFactory
    {
        /// <summary>
        /// バージョン移行オブジェクト
        /// </summary>
        public IDbMigration DbMigration { get; private set; }

        /// <summary>
        /// 接続設定オブジェクト
        /// </summary>
        private readonly DbConfig _config;

        public DbOperatorFactory(DbConfig config)
        {
            _config = config;

            switch (_config.DbKind)
            {
                case DbKind.SQLite:
                    var c = ActivityContextSQLite.ParseConnectionString(config.ConnectionString);
                    DbMigration = new DbMigrationSQLite(c);
                    break;
                case DbKind.MariaDB:
                    DbMigration = new DbMigrationMariaDb(config.ConnectionString);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// コンテキストを返す
        /// </summary>
        /// <returns></returns>
        public ActivityContextBase GetDbContext()
        {
            ActivityContextBase context;
            var connectionString = _config.ConnectionString;
            var builder = new DbContextOptionsBuilder<ActivityContextBase>();
            switch (_config.DbKind)
            {
                case DbKind.SQLite:
                    builder.UseSqlite(connectionString);
                    context = new ActivityContextSQLite(builder.Options);
                    break;
                case DbKind.MariaDB:
                    builder.UseMySql(connectionString,
                        options =>
                        {
                            options.ServerVersion(ServerVersion.AutoDetect(connectionString));
                            options.CharSetBehavior(CharSetBehavior.AppendToAllColumns);
                            options.CharSet(CharSet.Utf8Mb4);
                        });
                    context = new ActivityContextMariaDb(builder.Options);
                    break;
                default:
                    throw new NotSupportedException();
            }

            return context;
        }
    }
}
