using Microsoft.Toolkit.Win32.UI.Controls.Interop.WinRT;
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
using VRChatActivityToolsShared.Database;

namespace VRChatActivityLogViewer
{
    /// <summary>
    /// DetailWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class DetailWindow : Window
    {
        private ActivityLog activityLog;
        private VRChatApiService vrchatApiService;
        private WebService webService;
        private World world;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="activityLog"></param>
        public DetailWindow(ActivityLog activityLog)
        {
            InitializeComponent();

            this.activityLog = activityLog;

            vrchatApiService = new VRChatApiService();
            webService = new WebService();

            InitializeView();
        }

        /// <summary>
        /// 表示する内容を初期化します
        /// </summary>
        private void InitializeView()
        {
            // ボタンの有効/無効
            JoinButton.Visibility = activityLog.WorldID != null ? Visibility.Visible : Visibility.Collapsed;
            CopyWorldIdButton.Visibility = activityLog.WorldID != null ? Visibility.Visible : Visibility.Collapsed;
            CopyUserIdButton.Visibility = activityLog.UserID != null ? Visibility.Visible : Visibility.Collapsed;
            CopyUrlButton.Visibility = activityLog.ActivityType == ActivityType.PlayedVideo && activityLog.Url != null ? Visibility.Visible : Visibility.Collapsed;

            // アクティビティタイプとタイムスタンプ
            ActivityTypeText.Text = ActivityTypeToString(activityLog.ActivityType);
            TimestampText.Text = activityLog.Timestamp?.ToString("yyyy/MM/dd HH:mm:ss");

            // アクティビティタイプによってヘッダの表示内容を変更
            InitializeHeaderView();

            // Videoの場合
            if (activityLog.ActivityType == ActivityType.PlayedVideo)
            {
                // Youtubeのみ特別扱いして埋め込みプレイヤーを表示する
                var youtubeUrlRegex = @"^https?://(www\.)?youtube\.com/watch\?v=([^&]+).*$|^https?://youtu\.be/(.*)$";
                var match = Regex.Match(activityLog.Url, youtubeUrlRegex);

                if (match.Success)
                {
                    var id = match.Groups[2].Success ? match.Groups[2].Value : match.Groups[3].Success ? match.Groups[3].Value : string.Empty;
                    var url = @$"https://www.youtube.com/embed/{id}";
                    VideoGrid.Visibility = Visibility.Visible;
                    VideoWebBrowser.Source = new Uri(url);
                }
            }

            // Video以外の場合は共通の処理
            if (activityLog.ActivityType != ActivityType.PlayedVideo)
            {
                // ワールド名がある場合
                if (activityLog.WorldName != null)
                {
                    WorldNameText.Text = activityLog.WorldName;
                    ChangeWorldInfoButton.Visibility = Visibility.Visible;
                    UnknownContentsGrid.Visibility = Visibility.Hidden;
                    WorldInfoGrid.Visibility = Visibility.Visible;
                }

                // メッセージかURLがある場合
                if (activityLog.Message != null || activityLog.Url != null)
                {
                    MessageText.Text = activityLog.Message;
                    ChangeMessageInfoButton.Visibility = Visibility.Visible;

                    if (activityLog.WorldName == null)
                    {
                        UnknownContentsGrid.Visibility = Visibility.Hidden;
                        MessageInfoGrid.Visibility = Visibility.Visible;
                    }
                }
            }
        }

        /// <summary>
        /// ウィンドウが閉じる時のイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DetailWindow_Closed(object sender, EventArgs e)
        {
            VideoWebBrowser.Close();
        }

        /// <summary>
        /// ヘッダを初期化します
        /// </summary>
        private void InitializeHeaderView()
        {
            if (activityLog.ActivityType == ActivityType.JoinedRoom)
            {
                HeaderGrid.Background = new SolidColorBrush(Colors.Plum);
            }

            if (activityLog.ActivityType == ActivityType.ReceivedInvite ||
                activityLog.ActivityType == ActivityType.ReceivedInviteResponse ||
                activityLog.ActivityType == ActivityType.ReceivedRequestInvite ||
                activityLog.ActivityType == ActivityType.ReceivedRequestInviteResponse)
            {
                HeaderGrid.Background = new SolidColorBrush(Colors.LightBlue);
                FromUserName.Text = $"from {activityLog.UserName}";
            }

            if (activityLog.ActivityType == ActivityType.SendInvite ||
                activityLog.ActivityType == ActivityType.SendRequestInvite)
            {
                HeaderGrid.Background = new SolidColorBrush(Colors.SkyBlue);
            }

            if (activityLog.ActivityType == ActivityType.SendFriendRequest)
            {
                HeaderGrid.Background = new SolidColorBrush(Colors.LightGreen);
            }

            if (activityLog.ActivityType == ActivityType.ReceivedFriendRequest ||
                activityLog.ActivityType == ActivityType.AcceptFriendRequest)
            {
                HeaderGrid.Background = new SolidColorBrush(Colors.LightGreen);
                FromUserName.Text = $"from {activityLog.UserName}";
            }

            if (activityLog.ActivityType == ActivityType.PlayedVideo)
            {
                HeaderGrid.Background = new SolidColorBrush(Colors.Pink);
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

                if (activityLog.WorldID != null)
                {
                    if (world != null)
                    {
                        WorldImageContent.Source = await CreateBitmapImageFromUri(world.ThumbnailImageUrl);
                        WorldImageContent.Visibility = Visibility.Visible;

                        WorldAuthorText.Text = $"by {world.AuthorName}";
                    }
                }

                if (activityLog.Url != null && activityLog.ActivityType != ActivityType.PlayedVideo)
                {
                    MessageImageContent.Source = await CreateBitmapImageFromUri(activityLog.Url);
                    MessageImageContent.Visibility = Visibility.Visible;
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

            var id = activityLog?.WorldID?.Split(':')[0];
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
            _ => "Unknown Activity",
        };

        /// <summary>
        /// Joinボタンクリック時のイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void JoinButton_Click(object sender, RoutedEventArgs e)
        {
            if(activityLog == null || activityLog.WorldID == null)
            {
                return;
            }

            var uri = "vrchat://launch?id=" + activityLog.WorldID;
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
            if (activityLog == null || activityLog.WorldID == null)
            {
                return;
            }

            Clipboard.SetText(activityLog.WorldID ?? "");
        }

        /// <summary>
        /// Copy User IDボタンクリック時のイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CopyUserIdButton_Click(object sender, RoutedEventArgs e)
        {
            if (activityLog == null || activityLog.UserID == null)
            {
                return;
            }

            Clipboard.SetText(activityLog.UserID ?? "");
        }

        /// <summary>
        /// Copy Urlボタンクリック時のイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CopyUrlButton_Click(object sender, RoutedEventArgs e)
        {
            if (activityLog == null || activityLog.Url == null)
            {
                return;
            }

            Clipboard.SetText(activityLog.Url ?? "");
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
            var id = activityLog?.WorldID?.Split(':')[0];
            var uri = $"https://vrchat.com/home/world/{id}";

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
                var s = activityLog.Url.Split('/');
                var fileName = s[s.Length - 1];
                var dialog = new SaveFileDialog();
                dialog.FileName = fileName;
                dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);

                if (dialog.ShowDialog() ?? false)
                {
                    await webService.DownloadFile(activityLog.Url, dialog.FileName);
                }
            }
            catch (Exception)
            {
                MessageBox.Show("ファイルの保存に失敗しました", "VRChatActivityLogViewer", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 埋め込みプレイヤーが新しいウィンドウをリクエストした時のイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void VideoWebBrowser_NewWindowRequested(object sender, WebViewControlNewWindowRequestedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("cmd", $"/c start {e.Uri}") { CreateNoWindow = true });
            e.Handled = true;
        }
    }
}
