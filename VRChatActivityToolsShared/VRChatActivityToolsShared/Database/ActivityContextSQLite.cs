using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Linq;

namespace VRChatActivityToolsShared.Database
{
    /// <summary>
    /// データベースコンテキスト
    /// </summary>
    public class ActivityContextSQLite : ActivityContextBase
    {
        /// <summary>
        /// データベースのファイルパス
        /// </summary>
        public static string DBFilePath { get; } = @"VRChatActivityLog.db";

        public static string ParseConnectionString(string con)
        {
            if (string.IsNullOrEmpty(con))
            {
                return new SqliteConnectionStringBuilder { DataSource = DBFilePath }.ToString();
            }
            return con;
        }

        public ActivityContextSQLite(DbContextOptions<ActivityContextBase> options) : base(options) { }

        public override bool HasTable(string tableName)
        {
            using var db = Database.GetDbConnection();
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
            " + $"name = '{tableName}';";
            #endregion
            using var command = new SqliteCommand(existsTableSql, (SqliteConnection)db);
            using var reader = command.ExecuteReader();
            if (!reader.Read())
            {
                return false;
            }

            return 0 < reader.GetInt32(0);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ActivityLog>(e =>
            {
                e.ToTable("ActivityLogs")
                 .HasKey(a => a.ID)
                 .HasName("PK_ActivityLogs");

                e.Property("ID")
                 .ValueGeneratedOnAdd()
                 .IsRequired();

                e.Property("ActivityType")
                 .IsRequired();

                e.Property("Timestamp")
                 .HasColumnType("datetime");

                e.Property("NotificationID")
                 .HasColumnType("text");

                e.Property("UserID")
                 .HasColumnType("text");

                e.Property("UserName")
                 .HasColumnType("text");

                e.Property("WorldID")
                 .HasColumnType("text");

                e.Property("WorldName")
                 .HasColumnType("text");

                e.Property("Message")
                 .HasColumnType("text");

                e.Property("Url")
                 .HasColumnType("text");
            });

            modelBuilder.Entity<Information>(e =>
            {
                e.ToTable("Information")
                 .HasKey(a => a.ID)
                 .HasName("PK_Information");

                e.Property(a => a.ID)
                 .ValueGeneratedOnAdd()
                 .IsRequired();
            });
        }
    }
}
