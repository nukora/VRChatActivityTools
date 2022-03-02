using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using VRChatActivityLogViewer.VRChatApi;
using VRChatActivityLogViewer.YoutubeApi;
using VRChatActivityToolsShared.Database;

namespace VRChatActivityLogViewer
{
    /// <summary>
    /// DetailWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class DetailWindow : Window
    {
        public ActivityLog ActivityLog { get; }

        private VRChatApiService vrchatApiService;
        private YoutubeApiService youtubeApiService;
        private WebService webService;
        private World world;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="activityLog"></param>
        public DetailWindow(ActivityLog activityLog)
        {
            this.ActivityLog = activityLog;

            vrchatApiService = new VRChatApiService();
            youtubeApiService = new YoutubeApiService();
            webService = new WebService();

            InitializeComponent();

            InitializeView();
        }

        /// <summary>
        /// 表示する内容を初期化します
        /// </summary>
        private void InitializeView()
        {
            // ボタンの有効/無効
            JoinButton.Visibility = ActivityLog.WorldID != null ? Visibility.Visible : Visibility.Collapsed;
            CopyWorldIdButton.Visibility = ActivityLog.WorldID != null ? Visibility.Visible : Visibility.Collapsed;
            CopyWorldNameButton.Visibility = ActivityLog.WorldID != null ? Visibility.Visible : Visibility.Collapsed;
            CopyUserIdButton.Visibility = ActivityLog.UserID != null ? Visibility.Visible : Visibility.Collapsed;
            CopyUrlButton.Visibility = ActivityLog.ActivityType == ActivityType.PlayedVideo && ActivityLog.Url != null ? Visibility.Visible : Visibility.Collapsed;

            // アクティビティタイプとタイムスタンプ
            ActivityTypeText.Text = ActivityTypeToString(ActivityLog.ActivityType);
            TimestampText.Text = ActivityLog.Timestamp?.ToString("yyyy/MM/dd HH:mm:ss");

            // アクティビティタイプによってヘッダの表示内容を変更
            InitializeHeaderView();

            // Video以外の場合は共通の処理
            if (ActivityLog.ActivityType != ActivityType.PlayedVideo)
            {
                // ワールド名がある場合
                if (ActivityLog.WorldName != null)
                {
                    WorldNameText.Text = ActivityLog.WorldName;
                    ChangeWorldInfoButton.Visibility = Visibility.Visible;
                    UnknownContentsGrid.Visibility = Visibility.Hidden;
                    WorldInfoGrid.Visibility = Visibility.Visible;
                }

                // メッセージかURLがある場合
                if (ActivityLog.Message != null || ActivityLog.Url != null)
                {
                    MessageText.Text = ActivityLog.Message;
                    ChangeMessageInfoButton.Visibility = Visibility.Visible;

                    if (ActivityLog.WorldName == null)
                    {
                        UnknownContentsGrid.Visibility = Visibility.Hidden;
                        MessageInfoGrid.Visibility = Visibility.Visible;
                    }
                }
            }
        }

        /// <summary>
        /// ヘッダを初期化します
        /// </summary>
        private void InitializeHeaderView()
        {
            if (ActivityLog.ActivityType == ActivityType.ReceivedInvite ||
                ActivityLog.ActivityType == ActivityType.ReceivedInviteResponse ||
                ActivityLog.ActivityType == ActivityType.ReceivedRequestInvite ||
                ActivityLog.ActivityType == ActivityType.ReceivedRequestInviteResponse ||
                ActivityLog.ActivityType == ActivityType.ReceivedFriendRequest ||
                ActivityLog.ActivityType == ActivityType.AcceptFriendRequest ||
                ActivityLog.ActivityType == ActivityType.AcceptInvite ||
                ActivityLog.ActivityType == ActivityType.AcceptRequestInvite)
            {
                FromUserName.Text = $"from {ActivityLog.UserName}";
            }
        }

        /// <summary>
        /// 詳細画面が読み込まれた後に呼び出されるイベントです
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void DetailWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await GetWorldInformation();

                if (ActivityLog.WorldID != null)
                {
                    if (world != null)
                    {
                        WorldImageContent.Source = await CreateBitmapImageFromUri(world.ThumbnailImageUrl);
                        WorldImageContent.Visibility = Visibility.Visible;

                        WorldAuthorText.Text = $"by {world.AuthorName}";
                    }
                }

                if (ActivityLog.Url != null && ActivityLog.ActivityType != ActivityType.PlayedVideo)
                {
                    MessageImageContent.Source = await CreateBitmapImageFromUri(ActivityLog.Url);
                    MessageImageContent.Visibility = Visibility.Visible;
                }

                if (ActivityLog.ActivityType == ActivityType.PlayedVideo)
                {
                    // Youtubeのみ特別扱いしてサムネイルを表示する
                    var youtubeUrlRegex = @"^https?://(www\.)?youtube\.com/watch\?v=([^&]+).*$|^https?://youtu\.be/([^\?]+).*$";
                    var match = Regex.Match(ActivityLog.Url, youtubeUrlRegex);

                    if (match.Success)
                    {
                        var id = match.Groups[2].Success ? match.Groups[2].Value : match.Groups[3].Success ? match.Groups[3].Value : string.Empty;
                        var oEmbed = await youtubeApiService.GetOEmbedAsync(id);

                        if (oEmbed != null)
                        {
                            UnknownContentsGrid.Visibility = Visibility.Hidden;
                            VideoGrid.Visibility = Visibility.Visible;

                            VideoTitleText.Text = oEmbed.Title;
                            VideoAuthorText.Text = $"{oEmbed.AuthorName}";
                            VideoImageContent.Source = await CreateBitmapImageFromUri(oEmbed.ThumbnailUrl);
                            VideoImageContent.Visibility = Visibility.Visible;
                        }
                    }
                }
            }
            catch (Exception ex) when (ex is InvalidOperationException || ex is HttpRequestException)
            {
                // 外部から情報を取得する要素はおまけのため、エラーを無視する
            }
            catch (Exception)
            {
                MessageBox.Show("エラーが発生しました。プログラムを終了します。", "VRChatActivityLogViewer", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }
        }

        /// <summary>
        /// ワールドの追加情報をVRCAPIで取得します
        /// </summary>
        /// <returns></returns>
        private async Task GetWorldInformation()
        {
            if (world != null)
            {
                return;
            }

            var id = ActivityLog?.WorldID?.Split(':')[0];
            if (!string.IsNullOrWhiteSpace(id))
            {
                world = await vrchatApiService.GetWorldAsync(id);
            }
        }

        /// <summary>
        /// ネットワークから画像を取得し、BitmapImageを作成します
        /// </summary>
        /// <returns></returns>
        private async Task<BitmapImage> CreateBitmapImageFromUri(string uri)
        {
            using (var stream = await webService.GetStreamAsync(uri))
            {
                var bitmap = new BitmapImage();

                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.CreateOptions = BitmapCreateOptions.None;
                bitmap.StreamSource = stream;
                bitmap.EndInit();
                bitmap.Freeze();

                return bitmap;
            }
        }

        /// <summary>
        /// アクティビティタイプからヘッダに表示する文字列を取得します
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private string ActivityTypeToString(ActivityType type) => type switch
        {
            ActivityType.JoinedRoom => "Join",
            ActivityType.MetPlayer => "Meet",
            ActivityType.SendInvite => "Send Invite",
            ActivityType.ReceivedInvite => "Received Invite",
            ActivityType.SendRequestInvite => "Send RequestInvite",
            ActivityType.ReceivedRequestInvite => "Received RequestInvite",
            ActivityType.SendFriendRequest => "Send FriendRequest",
            ActivityType.ReceivedFriendRequest => "Received FriendRequest",
            ActivityType.AcceptFriendRequest => "Accept FriendRequest",
            ActivityType.SendInviteResponse => "Send Invite Response",
            ActivityType.ReceivedInviteResponse => "Received Invite Response",
            ActivityType.SendRequestInviteResponse => "Send RequestInvite Response",
            ActivityType.ReceivedRequestInviteResponse => "Received RequestInvite Response",
            ActivityType.PlayedVideo => "Video",
            ActivityType.AcceptInvite => "Accept Invite",
            ActivityType.AcceptRequestInvite => "Accept RequestInvite",
            _ => "Unknown Activity",
        };

        /// <summary>
        /// Joinボタンクリック時のイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void JoinButton_Click(object sender, RoutedEventArgs e)
        {
            if(ActivityLog == null || ActivityLog.WorldID == null)
            {
                return;
            }

            var uri = "vrchat://launch?id=" + ActivityLog.WorldID;
            uri = uri.Replace("&", "^&");
            Process.Start(new ProcessStartInfo("cmd", $"/c start {uri}") { CreateNoWindow = true });
        }

        /// <summary>
        /// Copy World IDボタンクリック時のイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CopyWorldIdButton_Click(object sender, RoutedEventArgs e)
        {
            if (ActivityLog == null || ActivityLog.WorldID == null)
            {
                return;
            }

            Clipboard.SetText(ActivityLog.WorldID ?? "");
        }

        /// <summary>
        /// Copy World Nameボタンクリック時のイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CopyWorldNameButton_Click(object sender, RoutedEventArgs e)
        {
            if (ActivityLog == null || ActivityLog.WorldName == null)
            {
                return;
            }

            Clipboard.SetText(ActivityLog.WorldName ?? "");
        }

        /// <summary>
        /// Copy User IDボタンクリック時のイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CopyUserIdButton_Click(object sender, RoutedEventArgs e)
        {
            if (ActivityLog == null || ActivityLog.UserID == null)
            {
                return;
            }

            Clipboard.SetText(ActivityLog.UserID ?? "");
        }

        /// <summary>
        /// Copy Urlボタンクリック時のイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CopyUrlButton_Click(object sender, RoutedEventArgs e)
        {
            if (ActivityLog == null || ActivityLog.Url == null)
            {
                return;
            }

            Clipboard.SetText(ActivityLog.Url ?? "");
        }

        /// <summary>
        /// Show Message Infoボタンクリック時のイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChangeMessageInfoButton_Click(object sender, RoutedEventArgs e)
        {
            WorldInfoGrid.Visibility = Visibility.Hidden;
            MessageInfoGrid.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Show World Infoボタンクリック時のイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChangeWorldInfoButton_Click(object sender, RoutedEventArgs e)
        {
            WorldInfoGrid.Visibility = Visibility.Visible;
            MessageInfoGrid.Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// ワールド名クリック時のイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WorldHyperlink_Click(object sender, RoutedEventArgs e)
        {
            var id = ActivityLog?.WorldID?.Split(':')[0];
            var uri = $"https://vrchat.com/home/world/{id}";

            Process.Start(new ProcessStartInfo("cmd", $"/c start {uri}") { CreateNoWindow = true });
        }

        /// <summary>
        /// ビデオタイトルクリック時のイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void VideoHyperlink_Click(object sender, RoutedEventArgs e)
        {
            var uri = $"{ActivityLog.Url}";

            Process.Start(new ProcessStartInfo("cmd", $"/c start {uri}") { CreateNoWindow = true });
        }

        /// <summary>
        /// 名前を付けて画像を保存メニュークリック時のイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void SaveMessageImageMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var s = ActivityLog.Url.Split('/');
                var fileName = s[s.Length - 1];
                var dialog = new SaveFileDialog();
                dialog.FileName = fileName;
                dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);

                if (dialog.ShowDialog() ?? false)
                {
                    await webService.DownloadFile(ActivityLog.Url, dialog.FileName);
                }
            }
            catch (Exception)
            {
                MessageBox.Show("ファイルの保存に失敗しました", "VRChatActivityLogViewer", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
