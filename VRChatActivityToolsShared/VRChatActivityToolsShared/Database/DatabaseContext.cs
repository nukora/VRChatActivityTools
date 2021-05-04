using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace VRChatActivityToolsShared.Database
{
    /// <summary>
    /// データベースコンテキスト
    /// </summary>
    public class DatabaseContext : DbContext
    {
        /// <summary>
        /// データベースのバージョン
        /// </summary>
        public static int Version { get; } = 3;

        /// <summary>
        /// データベースのファイルパス
        /// </summary>
        public static string DBFilePath { get; set; } = @"VRChatActivityLog.db";

        /// <summary>
        /// ActivityLogsテーブル
        /// </summary>
        public DbSet<ActivityLog> ActivityLogs { get; set; }

        /// <summary>
        /// Informationテーブル
        /// </summary>
        public DbSet<Information> Information { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var connectionString = new SqliteConnectionStringBuilder { DataSource = DBFilePath }.ToString();
            optionsBuilder.UseSqlite(new SqliteConnection(connectionString));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ActivityLog>().HasKey(a => new { a.ID });
            modelBuilder.Entity<Information>().HasKey(a => new { a.ID });
        }
    }
}
