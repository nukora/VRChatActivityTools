using System;
using VRChatActivityToolsShared.Database;

namespace VRChatActivityLogViewer
{
    /// <summary>
    /// DataGridに表示するモデル
    /// </summary>
    public class ActivityLogGridModel
    {
        /// <summary>タイムスタンプ</summary>
        public DateTime TimeStamp { get; }

        /// <summary>アクティビティの種類の名前</summary>
        public string ActivityName { get; }

        /// <summary>アクティビティの種類</summary>
        public ActivityType Type { get; }

        /// <summary>アクティビティの内容</summary>
        public string Content { get; }

        /// <summary>ワールドIDがコピーできるか</summary>
        public bool IsCopyableWorldID { get; } = false;

        /// <summary>ユーザIDがコピーできるか</summary>
        public bool IsCopyableUserID { get; } = false;

        /// <summary>ワールドID</summary>
        public string WorldID { get; }

        /// <summary>ユーザID</summary>
        public string UserID { get; }

        /// <summary>元データ</summary>
        public ActivityLog Source { get; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="activityLog"></param>
        public ActivityLogGridModel(ActivityLog activityLog)
        {
            Source = activityLog;

            Type = activityLog.ActivityType;
            TimeStamp = activityLog.Timestamp ?? default;
            if (activityLog.ActivityType == ActivityType.JoinedRoom)
            {
                ActivityName = "Join";
                Content = activityLog.WorldName;
                WorldID = activityLog.WorldID;
                IsCopyableWorldID = true;
            }
            if (activityLog.ActivityType == ActivityType.MetPlayer)
            {
                ActivityName = "Meet";
                Content = activityLog.UserName;
            }
            if (activityLog.ActivityType == ActivityType.SendInvite)
            {
                ActivityName = "Send Invite";
                Content = activityLog.WorldName;
                WorldID = activityLog.WorldID;
                IsCopyableWorldID = true;
                UserID = activityLog.UserID;
                IsCopyableUserID = true;
            }
            if (activityLog.ActivityType == ActivityType.ReceivedInvite)
            {
                ActivityName = "Received Invite";
                Content = activityLog.UserName + " -> " + activityLog.WorldName;
                WorldID = activityLog.WorldID;
                IsCopyableWorldID = true;
                UserID = activityLog.UserID;
                IsCopyableUserID = true;
            }
            if (activityLog.ActivityType == ActivityType.SendRequestInvite)
            {
                ActivityName = "Send RequestInvite";
                UserID = activityLog.UserID;
                IsCopyableUserID = true;
            }
            if (activityLog.ActivityType == ActivityType.ReceivedRequestInvite)
            {
                ActivityName = "Received RequestInvite";
                Content = activityLog.UserName;
                UserID = activityLog.UserID;
                IsCopyableUserID = true;
            }
            if (activityLog.ActivityType == ActivityType.SendFriendRequest)
            {
                ActivityName = "Send FriendRequest";
                UserID = activityLog.UserID;
                IsCopyableUserID = true;
            }
            if (activityLog.ActivityType == ActivityType.ReceivedFriendRequest)
            {
                ActivityName = "Received FriendRequest";
                Content = activityLog.UserName;
                UserID = activityLog.UserID;
                IsCopyableUserID = true;
            }
            if (activityLog.ActivityType == ActivityType.AcceptFriendRequest)
            {
                ActivityName = "Accept FriendRequest";
                Content = activityLog.UserName;
                UserID = activityLog.UserID;
                IsCopyableUserID = true;
            }
            if (activityLog.ActivityType == ActivityType.ReceivedInviteResponse)
            {
                ActivityName = "Received InviteResponse";
                Content = activityLog.UserName;
                UserID = activityLog.UserID;
                IsCopyableUserID = true;
            }
            if (activityLog.ActivityType == ActivityType.ReceivedRequestInviteResponse)
            {
                ActivityName = "Received RequestInviteResponse";
                Content = activityLog.UserName;
                UserID = activityLog.UserID;
                IsCopyableUserID = true;
            }
        }
    }
}
