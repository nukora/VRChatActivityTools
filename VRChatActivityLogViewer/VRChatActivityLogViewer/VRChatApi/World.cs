using System;
using System.Collections.Generic;
using System.Text;

namespace VRChatActivityLogViewer.VRChatApi
{
    /// <summary>
    /// VRChatAPI World object
    /// </summary>
    public class World
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Featured { get; set; }
        public string AuthorID { get; set; }
        public string AuthorName { get; set; }
        public int Capacity { get; set; }
        public string[] Tags { get; set; }
        public string ReleaseStatus { get; set; }
        public string ImageUrl { get; set; }
        public string ThumbnailImageUrl { get; set; }
        public int Version { get; set; }
        public string Organization { get; set; }
        public string PreviewYoutubeId { get; set; }
        public int Favorites { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime PublicationDate { get; set; }
        public DateTime LabsPublicationDate { get; set; }
        public int Visits { get; set; }
        public int Popularity { get; set; }
        public int Heat { get; set; }
        public int PublicOccupants { get; set; }
        public int PrivateOccupants { get; set; }
        public int Occupants { get; set; }
    }
}
