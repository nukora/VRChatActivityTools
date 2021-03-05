using System;

namespace VRChatActivityLogViewer
{
    /// <summary>
    /// アクティビティの検索条件
    /// </summary>
    class ActivityLogSearchParameter
    {
        /// <summary>ワールドにjoinした履歴を含める</summary>
        public bool IsJoinedRoom { get; set; } = true;

        /// <summary>プレイヤーと会った履歴を含める</summary>
        public bool IsMetPlayer { get; set; } = true;

        /// <summary>inviteを受け取った履歴を含める</summary>
        public bool IsReceivedInvite { get; set; } = true;

        /// <summary>reqInvを受け取った履歴を含める</summary>
        public bool IsReceivedRequestInvite { get; set; } = true;

        /// <summary>inviteを送った履歴を含める</summary>
        public bool IsSendInvite { get; set; } = true;

        /// <summary>reqInvを送った履歴を含める</summary>
        public bool IsSendRequestInvite { get; set; } = true;

        /// <summary>フレンドリクエストを送った履歴を含める</summary>
        public bool IsSendFriendRequest { get; set; } = true;

        /// <summary>フレンドリクエストを受け取った履歴を含める</summary>
        public bool IsReceivedFriendRequest { get; set; } = true;

        /// <summary>フレンドリクエストを承認した履歴を含める</summary>
        public bool IsAcceptFriendRequest { get; set; } = true;

        /// <summary>検索する期間の始まり</summary>
        public DateTime? FromDateTime { get; set; } = null;

        /// <summary>検索する期間の終わり</summary>
        public DateTime? UntilDateTime { get; set; } = null;

        /// <summary>inviteへの返信を受け取った履歴を含める</summary>
        public bool IsReceivedInviteResponse { get; set; } = true;

        /// <summary>reqInvへの返信を受け取った履歴を含める</summary>
        public bool IsReceivedRequestInviteResponse { get; set; } = true;
    }
}
