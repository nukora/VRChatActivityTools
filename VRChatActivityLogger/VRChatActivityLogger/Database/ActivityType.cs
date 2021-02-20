using System;
using System.Collections.Generic;
using System.Text;

namespace VRChatActivityLogger.Database
{
    /// <summary>
    /// アクティビティの種類
    /// </summary>
    public enum ActivityType : int
    {
        /// <summary>ワールドにjoinした</summary>
        JoinedRoom = 0,

        /// <summary>プレイヤーと会った</summary>
        MetPlayer = 1,

        /// <summary>inviteを送った</summary>
        SendInvite = 2,

        /// <summary>inviteを受け取った</summary>
        ReceivedInvite = 3,

        /// <summary>reqInvを送った</summary>
        SendRequestInvite = 4,

        /// <summary>reqInvを受け取った</summary>
        ReceivedRequestInvite = 5,

        /// <summary>フレンドリクエストを送った</summary>
        SendFriendRequest = 6,

        /// <summary>フレンドリクエストを受け取った</summary>
        ReceivedFriendRequest = 7,

        /// <summary>フレンドリクエストを承認した</summary>
        AcceptFriendRequest = 8,

        /// <summary>inviteへの返信を送った</summary>
        SendInviteResponse = 9,

        /// <summary>inviteへの返信を受け取った</summary>
        ReceivedInviteResponse = 10,

        /// <summary>reqInvへの返信を送った</summary>
        SendRequestInviteResponse = 11,

        /// <summary>reqInvへの返信を受け取った</summary>
        ReceivedRequestInviteResponse = 12,
    }
}
