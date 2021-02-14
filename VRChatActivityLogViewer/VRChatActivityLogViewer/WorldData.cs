using System.Runtime.Serialization;

namespace VRChatActivityLogViewer
{
    [System.Runtime.Serialization.DataContract]
    class WorldData
    {
        /// <summary>ワールドに一意のID</summary>
        [DataMember]
        public string id;

        /// <summary>ワールドの名前</summary>
        [DataMember]
        public string name;

        /// <summary>ワールドの説明/summary>
        [DataMember]
        public string description;

        /// <summary>ワールドの作者</summary>
        [DataMember]
        public string authorName;

        /// <summary>ワールドの画像を示すURL</summary>
        [DataMember]
        public string imageUrl;

        /// <summary>ワールドのサムネイル画像を示すURL</summary>
        [DataMember]
        public string thumbnailImageUrl;

    }
}
