using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace VRChatActivityToolsShared.Database
{
    public class ActivityLog : IEquatable<ActivityLog>
    {
        /// <summary>アクティビティで一意のID</summary>
        public int? ID { get; set; }
        /// <summary>アクティビティの種類</summary>
        public ActivityType ActivityType { get; set; }
        /// <summary>そのアクティビティを行った時刻</summary>
        public DateTime? Timestamp { get; set; }
        /// <summary>関連する通知ID</summary>
        public string NotificationID { get; set; }
        /// <summary>関連するユーザID</summary>
        public string UserID { get; set; }
        /// <summary>関連するユーザ名</summary>
        public string UserName { get; set; }
        /// <summary>関連するワールドID</summary>
        public string WorldID { get; set; }
        /// <summary>関連するワールド名</summary>
        public string WorldName { get; set; }
        /// <summary>関連するメッセージ</summary>
        public string Message { get; set; }
        /// <summary>関連するURL</summary>
        public string Url { get; set; }

        public bool Equals(ActivityLog other)
        {
            return other is ActivityLog log &&
                EqualityComparer<int?>.Default.Equals(ID, log.ID) &&
                ActivityType == log.ActivityType &&
                EqualityComparer<DateTime?>.Default.Equals(Timestamp, log.Timestamp) &&
                NotificationID == log.NotificationID &&
                UserID == log.UserID &&
                UserName == log.UserName &&
                WorldID == log.WorldID &&
                WorldName == log.WorldName &&
                Message == log.Message &&
                Url == log.Url;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ActivityLog);
        }

        public override int GetHashCode()
        {
            HashCode hash = new HashCode();

            hash.Add(ID);
            hash.Add(ActivityType);
            hash.Add(Timestamp);
            hash.Add(NotificationID);
            hash.Add(UserID);
            hash.Add(UserName);
            hash.Add(WorldID);
            hash.Add(WorldName);
            hash.Add(Message);
            hash.Add(Url);

            return hash.ToHashCode();
        }

        public static bool operator ==(ActivityLog l, ActivityLog r)
        {
            if (l is null && r is null)
            {
                return true;
            }

            return Equals(l, r);
        }

        public static bool operator !=(ActivityLog l, ActivityLog r) => !(l == r);
    }
}
