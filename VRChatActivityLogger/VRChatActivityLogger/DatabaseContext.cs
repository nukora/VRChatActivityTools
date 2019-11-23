using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace VRChatActivityLogger
{
    /// <summary>
    /// データベースコンテキスト
    /// </summary>
    class DatabaseContext : DbContext
    {
        /// <summary>
        /// データベースのファイルパス
        /// </summary>
        public static string DBFilePath { get; set; } = @"VRChatActivityLog.db";

        /// <summary>
        /// ActivityLogsテーブル
        /// </summary>
        public DbSet<ActivityLog> ActivityLogs { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var connectionString = new SqliteConnectionStringBuilder { DataSource = DBFilePath }.ToString();
            optionsBuilder.UseSqlite(new SqliteConnection(connectionString));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ActivityLog>().HasKey(a => new { a.ID });
        }
    }
}
