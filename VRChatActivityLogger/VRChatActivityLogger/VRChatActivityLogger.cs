using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Data.Sqlite;
using Newtonsoft.Json;
using static System.Environment;

namespace VRChatActivityLogger
{
    /// <summary>
    /// VRChatでの活動履歴をログから取得し、データベースに保存するクラス
    /// </summary>
    class VRChatActivityLogger
    {
        /// <summary>
        /// VRChatのログの保存場所
        /// </summary>
        public string VRChatLogFilePath { get; set; } =
            Regex.Replace(GetFolderPath(SpecialFolder.LocalApplicationData), @"\\[^\\]+$", "") + @"\LocalLow\VRChat\VRChat\";

        /// <summary>
        /// 処理を実行します。
        /// </summary>
        /// <returns></returns>
        public int Run()
        {
            var logger = Logger.GetLogger();
            try
            {
                var activityLogs = new List<ActivityLog>();
                foreach (var file in Directory.EnumerateFiles(VRChatLogFilePath, "output_log_*"))
                {
                    logger.Debug("ログを解析中 " + file);
                    activityLogs.AddRange(ParseVRChatLog(file));
                }
                activityLogs = activityLogs.OrderBy(a => a.Timestamp).ToList();

                if (!File.Exists(DatabaseContext.DBFilePath))
                {
                    logger.Info("データベースファイルが見つかりませんでした。新しく作成します。");
                    CreateDatabase();
                }

                using (var db = new DatabaseContext())
                {
                    var lastActivity = db.ActivityLogs.Find(db.ActivityLogs.Max(a => a.ID));
                    if (lastActivity != null)
                    {
                        var idBackup = lastActivity.ID;
                        lastActivity.ID = null;
                        for (int i = 0; i < activityLogs.Count; i++)
                        {
                            if (activityLogs[i].Timestamp == lastActivity.Timestamp)
                            {
                                if (activityLogs[i].Equals(lastActivity))
                                {
                                    activityLogs.RemoveRange(0, i + 1);
                                    break;
                                }
                            }
                        }
                        lastActivity.ID = idBackup;
                    }

                    using (var transaction = db.Database.BeginTransaction())
                    {
                        try
                        {
                            foreach (var activityLog in activityLogs)
                            {
                                db.Add(activityLog);
                                db.SaveChanges();
                            }
                            transaction.Commit();
                        }
                        catch (Exception)
                        {
                            transaction.Rollback();
                            throw;
                        }

                    }

                }

                logger.Info(activityLogs.Count + "件追加しました。");

            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Console.WriteLine(ex);
            }
            return 0;
        }

        /// <summary>
        /// VRChatのログを解析します。
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private List<ActivityLog> ParseVRChatLog(string filePath)
        {
            var logger = Logger.GetLogger();
            string rawData = "";
            var activityLogs = new List<ActivityLog>();
            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var sr = new StreamReader(fs, Encoding.GetEncoding("UTF-8")))
            {
                while ((rawData = sr.ReadLine()) != null)
                {
                    if (rawData.Length > 25 && rawData.Substring(20, 5) == "Error")
                    {
                        continue;
                    }
                    Match match = RegexPatterns.All.Match(rawData);
                    if (match.Groups[PatternType.ReceivedInvite].Value.Length != 0)
                    {
                        var m = RegexPatterns.ReceivedInviteDetail.Match(match.ToString());
                        var jsonRawDate = m.Groups[2].Value.Replace("{{", "{").Replace("}}", "}");
                        dynamic content = JsonConvert.DeserializeObject(jsonRawDate);
                        var activityLog = new ActivityLog
                        {
                            ActivityType = ActivityType.ReceivedInvite,
                            Timestamp = DateTime.Parse(m.Groups[1].Value),
                            NotificationID = content.id,
                            UserID = content.senderUserId,
                            UserName = content.senderUsername,
                            WorldID = content.details.worldId,
                            WorldName = content.details.worldName,
                        };
                        if (!activityLogs.Where(a => a.NotificationID == activityLog.NotificationID).Any())
                        {
                            activityLogs.Add(activityLog);
                        }
                        continue;
                    }
                    if (match.Groups[PatternType.ReceivedRequestInvite].Value.Length != 0)
                    {
                        var m = RegexPatterns.ReceivedRequestInviteDetail.Match(match.ToString());
                        var jsonRawDate = m.Groups[2].Value.Replace("{{", "{").Replace("}}", "}");
                        dynamic content = JsonConvert.DeserializeObject(jsonRawDate);
                        var activityLog = new ActivityLog
                        {
                            ActivityType = ActivityType.ReceivedRequestInvite,
                            Timestamp = DateTime.Parse(m.Groups[1].Value),
                            NotificationID = content.id,
                            UserID = content.senderUserId,
                            UserName = content.senderUsername,
                        };
                        if (!activityLogs.Where(a => a.NotificationID == activityLog.NotificationID).Any())
                        {
                            activityLogs.Add(activityLog);
                        }
                        continue;
                    }
                    if (match.Groups[PatternType.SendInvite].Value.Length != 0)
                    {
                        var m = RegexPatterns.SendInviteDetail.Match(match.ToString());
                        activityLogs.Add(new ActivityLog
                        {
                            ActivityType = ActivityType.SendInvite,
                            Timestamp = DateTime.Parse(m.Groups[1].Value),
                            UserID = m.Groups[2].Value,
                            WorldID = m.Groups[3].Value,
                            WorldName = m.Groups[4].Value,
                        });
                        continue;
                    }
                    if (match.Groups[PatternType.SendRequestInvite].Value.Length != 0)
                    {
                        var m = RegexPatterns.SendRequestInviteDetail.Match(match.ToString());
                        activityLogs.Add(new ActivityLog
                        {
                            ActivityType = ActivityType.SendRequestInvite,
                            Timestamp = DateTime.Parse(m.Groups[1].Value),
                            UserID = m.Groups[2].Value,
                        });
                        continue;
                    }
                    if (match.Groups[PatternType.JoinedRoom1].Value.Length != 0)
                    {
                        var m = RegexPatterns.JoinedRoom1Detail.Match(match.ToString());
                        activityLogs.Add(new ActivityLog
                        {
                            ActivityType = ActivityType.JoinedRoom,
                            Timestamp = DateTime.Parse(m.Groups[1].Value),
                            WorldID = m.Groups[2].Value,
                        });
                        continue;
                    }
                    if (match.Groups[PatternType.JoinedRoom2].Value.Length != 0)
                    {
                        var m = RegexPatterns.JoinedRoom2Detail.Match(match.ToString());
                        if (activityLogs.Any() && activityLogs[activityLogs.Count - 1].ActivityType == ActivityType.JoinedRoom)
                        {
                            activityLogs[activityLogs.Count - 1].WorldName = m.Groups[2].Value;
                        }
                        else
                        {
                            activityLogs.Add(new ActivityLog
                            {
                                ActivityType = ActivityType.JoinedRoom,
                                Timestamp = DateTime.Parse(m.Groups[1].Value),
                                WorldName = m.Groups[2].Value,
                            });
                        }
                        continue;
                    }
                    if (match.Groups[PatternType.MetPlayer].Value.Length != 0)
                    {
                        var m = RegexPatterns.MetPlayerDetail.Match(match.ToString());
                        activityLogs.Add(new ActivityLog
                        {
                            ActivityType = ActivityType.MetPlayer,
                            Timestamp = DateTime.Parse(m.Groups[1].Value),
                            UserName = m.Groups[2].Value,
                        });
                        continue;
                    }
                    if (match.Groups[PatternType.SendFriendRequest].Value.Length != 0)
                    {
                        var m = RegexPatterns.SendFriendRequestDetail.Match(match.ToString());
                        activityLogs.Add(new ActivityLog
                        {
                            ActivityType = ActivityType.SendFriendRequest,
                            Timestamp = DateTime.Parse(m.Groups[1].Value),
                            UserID = m.Groups[2].Value,
                        });
                        continue;
                    }
                    if (match.Groups[PatternType.ReceivedFriendRequest].Value.Length != 0)
                    {
                        var m = RegexPatterns.ReceivedFriendRequestDetail.Match(match.ToString());
                        var jsonRawDate = m.Groups[2].Value.Replace("{{", "{").Replace("}}", "}");
                        dynamic content = JsonConvert.DeserializeObject(jsonRawDate);
                        var activityLog = new ActivityLog
                        {
                            ActivityType = ActivityType.ReceivedFriendRequest,
                            Timestamp = DateTime.Parse(m.Groups[1].Value),
                            NotificationID = content.id,
                            UserID = content.senderUserId,
                            UserName = content.senderUsername,
                        };
                        if (!activityLogs.Where(a => a.NotificationID == activityLog.NotificationID).Any())
                        {
                            activityLogs.Add(activityLog);
                        }
                        continue;
                    }
                    if (match.Groups[PatternType.AcceptFriendRequest].Value.Length != 0)
                    {
                        var m = RegexPatterns.AcceptFriendRequestDetail.Match(match.ToString());
                        activityLogs.Add(new ActivityLog
                        {
                            ActivityType = ActivityType.AcceptFriendRequest,
                            Timestamp = DateTime.Parse(m.Groups[1].Value),
                            UserName = m.Groups[2].Value,
                            UserID = m.Groups[3].Value,
                            NotificationID = m.Groups[4].Value,
                        });
                        continue;
                    }
                }
            }

            return activityLogs;
        }

        private bool CreateDatabase()
        {
            using (SqliteConnection db = new SqliteConnection($"Filename={DatabaseContext.DBFilePath}"))
            {
                db.Open();

                string tableCommand = @"

CREATE TABLE ""ActivityLogs"" (
    ""ID"" INTEGER NOT NULL CONSTRAINT ""PK_ActivityLogs"" PRIMARY KEY AUTOINCREMENT,
    ""ActivityType"" INTEGER NOT NULL,
    ""Timestamp"" TEXT NULL,
    ""NotificationID"" TEXT NULL,
    ""UserID"" TEXT NULL,
    ""UserName"" TEXT NULL,
    ""WorldID"" TEXT NULL,
    ""WorldName"" TEXT NULL
);
                ";

                SqliteCommand createTable = new SqliteCommand(tableCommand, db);

                createTable.ExecuteReader();
            }
            return true;
        }
    }
}
