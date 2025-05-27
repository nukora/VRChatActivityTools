using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pomelo.EntityFrameworkCore.MySql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using System.Data;

namespace VRChatActivityToolsShared.Database
{
    public class ActivityContextMariaDb : ActivityContextBase
    {
        public ActivityContextMariaDb(DbContextOptions<ActivityContextBase> options) : base(options) { }

        public override bool HasTable(string tableName)
        {
            var con = Database.GetDbConnection();
            var info = con.GetSchema("Tables");
            return info.AsEnumerable().Any(a => a["TABLE_NAME"].ToString() == tableName);
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
