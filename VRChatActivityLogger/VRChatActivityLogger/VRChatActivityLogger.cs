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
        /// ロガー
        /// </summary>
        private NLog.Logger logger = Logger.GetLogger();

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
                ClearErrorInfoFile();

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
                WriteErrorInfoFile();
                return -1;
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
            string rawData = "";
            var activityLogs = new List<ActivityLog>();
            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var sr = new StreamReader(fs, Encoding.GetEncoding("UTF-8")))
            {
                processingFilePath = filePath;
                processingLineNumber = 0;

                while ((rawData = sr.ReadLine()) != null)
                {
                    processingLineNumber++;

                    if (rawData.Length > 25 && rawData.Substring(20, 5) == "Error")
                    {
                        continue;
                    }

                    Match match = RegexPatterns.All.Match(rawData);

                    if (!match.Success)
                    {
                        continue;
                    }

                    processingLine = match.Value;

                    if (match.Groups[PatternType.ReceivedInvite].Value.Length != 0)
                    {
                        var m = RegexPatterns.ReceivedInviteDetail.Match(match.ToString());
                        var jsonRawData = m.Groups[2].Value.Replace("{{", "{").Replace("}}", "}");
                        var numCurlyBracketBegin = jsonRawData.Count(c => c == '{');
                        var numCurlyBracketEnd = jsonRawData.Count(c => c == '}');
                        if (numCurlyBracketBegin > numCurlyBracketEnd)
                        {
                            jsonRawData += new string('}', numCurlyBracketBegin - numCurlyBracketEnd);
                        }
                        dynamic content = JsonConvert.DeserializeObject(jsonRawData);
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
                    }
                    else if (match.Groups[PatternType.ReceivedRequestInvite].Value.Length != 0)
                    {
                        var m = RegexPatterns.ReceivedRequestInviteDetail.Match(match.ToString());
                        var jsonRawData = m.Groups[2].Value.Replace("{{", "{").Replace("}}", "}");
                        dynamic content = JsonConvert.DeserializeObject(jsonRawData);
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
                    }
                    else if (match.Groups[PatternType.SendInvite].Value.Length != 0)
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
                    }
                    else if (match.Groups[PatternType.SendRequestInvite].Value.Length != 0)
                    {
                        var m = RegexPatterns.SendRequestInviteDetail.Match(match.ToString());
                        activityLogs.Add(new ActivityLog
                        {
                            ActivityType = ActivityType.SendRequestInvite,
                            Timestamp = DateTime.Parse(m.Groups[1].Value),
                            UserID = m.Groups[2].Value,
                        });
                    }
                    else if (match.Groups[PatternType.JoinedRoom1].Value.Length != 0)
                    {
                        var m = RegexPatterns.JoinedRoom1Detail.Match(match.ToString());
                        activityLogs.Add(new ActivityLog
                        {
                            ActivityType = ActivityType.JoinedRoom,
                            Timestamp = DateTime.Parse(m.Groups[1].Value),
                            WorldID = m.Groups[3].Value,
                        });
                    }
                    else if (match.Groups[PatternType.JoinedRoom2].Value.Length != 0)
                    {
                        var m = RegexPatterns.JoinedRoom2Detail.Match(match.ToString());
                        if (activityLogs.Any() && activityLogs[activityLogs.Count - 1].ActivityType == ActivityType.JoinedRoom)
                        {
                            activityLogs[activityLogs.Count - 1].WorldName = m.Groups[3].Value;
                        }
                        else
                        {
                            activityLogs.Add(new ActivityLog
                            {
                                ActivityType = ActivityType.JoinedRoom,
                                Timestamp = DateTime.Parse(m.Groups[1].Value),
                                WorldName = m.Groups[3].Value,
                            });
                        }
                    }
                    else if (match.Groups[PatternType.MetPlayer].Value.Length != 0)
                    {
                        var m = RegexPatterns.MetPlayerDetail.Match(match.ToString());
                        activityLogs.Add(new ActivityLog
                        {
                            ActivityType = ActivityType.MetPlayer,
                            Timestamp = DateTime.Parse(m.Groups[1].Value),
                            UserName = m.Groups[3].Value,
                        });
                    }
                    else if (match.Groups[PatternType.SendFriendRequest].Value.Length != 0)
                    {
                        var m = RegexPatterns.SendFriendRequestDetail.Match(match.ToString());
                        activityLogs.Add(new ActivityLog
                        {
                            ActivityType = ActivityType.SendFriendRequest,
                            Timestamp = DateTime.Parse(m.Groups[1].Value),
                            UserID = m.Groups[2].Value,
                        });
                    }
                    else if (match.Groups[PatternType.ReceivedFriendRequest].Value.Length != 0)
                    {
                        var m = RegexPatterns.ReceivedFriendRequestDetail.Match(match.ToString());
                        var jsonRawData = m.Groups[2].Value.Replace("{{", "{").Replace("}}", "}");
                        dynamic content = JsonConvert.DeserializeObject(jsonRawData);
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
                    }
                    else if (match.Groups[PatternType.AcceptFriendRequest].Value.Length != 0)
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
                    }
                    else
                    {
                        continue;
                    }

                    processingLine = string.Empty;
                }

                processingLineNumber = 0;
                processingFilePath = string.Empty;
            }

            return activityLogs;
        }

        /// <summary>
        /// データベースを新規作成します。
        /// </summary>
        /// <returns></returns>
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

        private string processingFilePath = string.Empty;

        private int processingLineNumber = 0;

        private string processingLine = string.Empty;

        private readonly string errorFilePath = "./Logs/VRChatActivityLogger/errorfile.txt";

        /// <summary>
        /// エラーファイルをクリアします。
        /// </summary>
        private void ClearErrorInfoFile()
        {
            if (File.Exists(errorFilePath))
            {
                File.Delete(errorFilePath);
            }
        }

        /// <summary>
        /// エラーファイルを書き出します。
        /// </summary>
        private void WriteErrorInfoFile()
        {
            if (!string.IsNullOrEmpty(processingFilePath))
            {
                var body =
                    Path.GetFullPath(processingFilePath) + Environment.NewLine +
                    processingLineNumber + Environment.NewLine +
                    processingLine + Environment.NewLine;
                File.WriteAllText(errorFilePath, body);

                logger.Error($"{processingFilePath}#{processingLineNumber}");
                logger.Error($"{processingLine}");
            }
        }
    }
}
