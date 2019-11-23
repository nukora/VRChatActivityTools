using System;
using System.Collections.Generic;
using System.Text;

namespace VRChatActivityLogger
{
    public class ActivityLog
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

        public override bool Equals(object obj)
        {
            return obj is ActivityLog log &&
                    EqualityComparer<int?>.Default.Equals(ID, log.ID) &&
                     ActivityType == log.ActivityType &&
                    EqualityComparer<DateTime?>.Default.Equals(Timestamp, log.Timestamp) &&
                     NotificationID == log.NotificationID &&
                     UserID == log.UserID &&
                     UserName == log.UserName &&
                     WorldID == log.WorldID &&
                     WorldName == log.WorldName;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ID, ActivityType, Timestamp, NotificationID, UserID, UserName, WorldID, WorldName);
        }
    }
}
