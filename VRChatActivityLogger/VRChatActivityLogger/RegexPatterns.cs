using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace VRChatActivityLogger
{
    /// <summary>
    /// 正規表現のグループ名定義
    /// </summary>
    static public class PatternType
    {
        static public string ReceivedInvite = "ReceivedInvite";
        static public string ReceivedRequestInvite = "ReceivedRequestInvite";
        static public string SendInvite = "SendInvite";
        static public string SendRequestInvite = "SendRequestInvite";
        static public string JoinedRoom1 = "JoinedRoom1";
        static public string JoinedRoom2 = "JoinedRoom2";
        static public string MetPlayer = "MetPlayer";
        static public string SendFriendRequest = "SendFriendRequest";
        static public string ReceivedFriendRequest = "ReceivedFriendRequest";
        static public string AcceptFriendRequest = "AcceptFriendRequest";
    }

    /// <summary>
    /// 正規表現を定義するクラス
    /// </summary>
    static public class RegexPatterns
    {
        static public Regex ReceivedInviteDetail { get; }
        static public Regex ReceivedRequestInviteDetail { get; }
        static public Regex SendInviteDetail { get; }
        static public Regex SendRequestInviteDetail { get; }
        static public Regex JoinedRoom1Detail { get; }
        static public Regex JoinedRoom2Detail { get; }
        static public Regex MetPlayerDetail { get; }
        static public Regex SendFriendRequestDetail { get; set; }
        static public Regex ReceivedFriendRequestDetail { get; set; }
        static public Regex AcceptFriendRequestDetail { get; set; }
        static public Regex All { get; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        static RegexPatterns()
        {
            //ログの種類判別(個別)
            string header = @"^\d{4}\.\d{2}\.\d{2}\s\d{2}:\d{2}:\d{2}\sLog\s{8}-\s{2}";

            string receivedInvite = header + @"Received Message of type: notification content:.+""type"":""invite"".+$";
            string receivedRequestInvite = header + @"Received Message of type: notification content:.+""type"":""requestInvite"".+$";
            string sendInvite = header + @"Send notification:.+type:invite,.+$";
            string sendRequestInvite = header + @"Send notification:.+type:requestInvite.+$";
            string joinedRoom1 = header + @"\[RoomManager\] Joining w.+$";
            string joinedRoom2 = header + @"\[RoomManager\] Joining or Creating Room:.+$";
            string metPlayer = header + @"\[Player\] Initialized PlayerAPI.+$";
            string sendFriendRequest = header + @"Send notification:.+type:friendRequest.+$";
            string receivedFriendRequest = header + @"Received Message of type: notification content:.+""type"":""friendRequest"".+$";
            string acceptFriendRequest = header + @"AcceptFriendRequest.+$";

            //ログの種類判別(一括)
            string all = "";
            all += $@"(?<ReceivedInvite>{receivedInvite})|";
            all += $@"(?<ReceivedRequestInvite>{receivedRequestInvite})|";
            all += $@"(?<SendInvite>{sendInvite})|";
            all += $@"(?<SendRequestInvite>{sendRequestInvite})|";
            all += $@"(?<JoinedRoom1>{joinedRoom1})|";
            all += $@"(?<JoinedRoom2>{joinedRoom2})|";
            all += $@"(?<MetPlayer>{metPlayer})|";
            all += $@"(?<SendFriendRequest>{sendFriendRequest})|";
            all += $@"(?<ReceivedFriendRequest>{receivedFriendRequest})|";
            all += $@"(?<AcceptFriendRequest>{acceptFriendRequest})";
            All = new Regex(all, RegexOptions.Compiled);

            //ログの詳細を解析
            string detailHeader = @"^(\d{4}\.\d{2}\.\d{2}\s\d{2}:\d{2}:\d{2})\sLog\s{8}-\s{2}";

            string receivedInviteDetail = detailHeader + @"Received Message of type: notification content: ({{.+}}) received at";
            string receivedRequestInviteDetail = detailHeader + @"Received Message of type: notification content: ({{.+}}) received at";
            string sendInviteDetail = detailHeader + @".+to (.{40}) of.+worldId=(.+), worldName=(.+)}},";
            string sendRequestInviteDetail = detailHeader + @".+to (.{40}) of";
            string metPlayerDetail = detailHeader + @"\[Player\] Initialized PlayerAPI ""(.*)"" is (remote|local)$";
            string joinedRoom1Detail = detailHeader + @"\[RoomManager\] Joining (.+)$";
            string joinedRoom2Detail = detailHeader + @"\[RoomManager\] Joining or Creating Room: (.+)$";
            string sendFriendRequestDetail = detailHeader + @".+to (.{40}) of";
            string receivedFriendRequestDetail = detailHeader + @"Received Message of type: notification content: ({{.+}}) received at";
            string acceptFriendRequestDetail = detailHeader + @".+username:(.+), sender user id:(.{40}).+id: (.{40}),";

            ReceivedInviteDetail = new Regex(receivedInviteDetail, RegexOptions.Compiled);
            ReceivedRequestInviteDetail = new Regex(receivedRequestInviteDetail, RegexOptions.Compiled);
            SendInviteDetail = new Regex(sendInviteDetail, RegexOptions.Compiled);
            SendRequestInviteDetail = new Regex(sendRequestInviteDetail, RegexOptions.Compiled);
            MetPlayerDetail = new Regex(metPlayerDetail, RegexOptions.Compiled);
            JoinedRoom1Detail = new Regex(joinedRoom1Detail, RegexOptions.Compiled);
            JoinedRoom2Detail = new Regex(joinedRoom2Detail, RegexOptions.Compiled);
            SendFriendRequestDetail = new Regex(sendFriendRequestDetail, RegexOptions.Compiled);
            ReceivedFriendRequestDetail = new Regex(receivedFriendRequestDetail, RegexOptions.Compiled);
            AcceptFriendRequestDetail = new Regex(acceptFriendRequestDetail, RegexOptions.Compiled);
        }
    }
}
