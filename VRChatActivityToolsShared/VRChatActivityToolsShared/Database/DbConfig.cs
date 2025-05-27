using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace VRChatActivityToolsShared.Database
{
    /// <summary>
    /// DB接続設定クラス
    /// </summary>
    public class DbConfig
    {
        /// <summary>
        /// 接続先DBの種類
        /// </summary>
        public DbKind DbKind { get; } = DbKind.SQLite;

        /// <summary>
        /// 接続文字列
        /// </summary>
        /// <remarks>
        /// DB種類によって記入内容が異なるため注意
        /// </remarks>
        public string ConnectionString { get; }

        public DbConfig() { }

        public DbConfig(DbKind kind, string connectionString) : base()
        {
            DbKind = kind;
            ConnectionString = connectionString;
        }

        /// <summary>
        /// configファイルから値を取り込む
        /// </summary>
        /// <param name="config"></param>
        public DbConfig(IConfiguration config) : base()
        {
            DbKind = Enum.Parse<DbKind>(config["DbKind"]);
            ConnectionString = config.GetConnectionString("VRChatActivityLog");
        }
    }
}
