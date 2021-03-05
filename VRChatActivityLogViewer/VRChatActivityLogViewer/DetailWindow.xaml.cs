using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
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

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="activityLog"></param>
        public DetailWindow(ActivityLog activityLog)
        {
            InitializeComponent();

            vrchatApiService = new VRChatApiService();
            webService = new WebService();

            this.activityLog = activityLog;

            ActivityTypeText.Text = ActivityTypeToString(activityLog.ActivityType);
            TimestampText.Text = activityLog.Timestamp?.ToString("yyyy/MM/dd HH:mm:ss");

            if (activityLog.ActivityType == ActivityType.JoinedRoom)
            {
                HeaderGrid.Background = new SolidColorBrush(Colors.Plum);
                WorldNameText.Text = activityLog.WorldName;
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
                if (activityLog.ActivityType == ActivityType.JoinedRoom)
                {
                    var id = activityLog.WorldID?.Split(':')[0];

                    if (!string.IsNullOrWhiteSpace(id))
                    {
                        var world = await vrchatApiService.GetWorldAsync(id);

                        if (world != null)
                        {
                            using (var stream = await webService.GetStreamAsync(world.ImageUrl))
                            {
                                var bitmap = new BitmapImage();

                                bitmap.BeginInit();
                                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                                bitmap.CreateOptions = BitmapCreateOptions.None;
                                bitmap.StreamSource = stream;
                                bitmap.EndInit();
                                bitmap.Freeze();

                                ImageContent.Source = bitmap;
                            }

                            WorldAuthorText.Text = $"by {world.AuthorName}";
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
            _ => "Unknown Activity",
        };
    }
}
