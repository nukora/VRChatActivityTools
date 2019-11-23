using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace VRChatActivityLogViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ObservableCollection<ActivityLogGridModel> ActivityLogGridModelCollection = new ObservableCollection<ActivityLogGridModel>();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            DisableProcessingMode();

            fromDatePicker.SelectedDate = DateTime.Today.AddDays(-1);
            untilDatePicker.SelectedDate = DateTime.Today;
            ActivityLogGrid.ItemsSource = ActivityLogGridModelCollection;
        }

        /// <summary>
        /// Searchボタンクリック時のイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void searchButton_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                EnableProcessingMode();

                var parameter = new ActivityLogSearchParameter
                {
                    IsJoinedRoom = joinCheckBox.IsChecked ?? false,
                    IsMetPlayer = meetCheckBox.IsChecked ?? false,
                    IsSendInvite = sendInvCheckBox.IsChecked ?? false,
                    IsSendRequestInvite = sendReqInvCheckBox.IsChecked ?? false,
                    IsReceivedInvite = recvInvCheckBox.IsChecked ?? false,
                    IsReceivedRequestInvite = recvReqInvCheckBox.IsChecked ?? false,
                    IsSendFriendRequest = sendFriendReqCheckBox.IsChecked ?? false,
                    IsReceivedFriendRequest = recvFriendReqCheckBox.IsChecked ?? false,
                    IsAcceptFriendRequest = acptFriendReqCheckBox.IsChecked ?? false,
                    FromDateTime = fromDatePicker.SelectedDate,
                    UntilDateTime = untilDatePicker.SelectedDate?.AddDays(1),
                };
                var activityLogs = await VRChatActivityLogModel.SearchActivityLogs(parameter);

                ActivityLogGridModelCollection.Clear();
                foreach (var activityLog in activityLogs)
                {
                    ActivityLogGridModelCollection.Add(new ActivityLogGridModel(activityLog));
                }

                DisableProcessingMode();

            }
            catch (Exception)
            {
                MessageBox.Show("エラーが発生しました。プログラムを終了します。", "VRChatActivityLogViewer");
                Application.Current.Shutdown();
            }

        }

        /// <summary>
        /// Loggerボタンクリック時のイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void loggerButton_Click(object sender, RoutedEventArgs e)
        {

            try
            {

                EnableProcessingMode();

                await Task.Run(() =>
                {
                    var process = Process.Start("VRChatActivityLogger.exe");
                    process.WaitForExit();
                });

            }
            catch (System.ComponentModel.Win32Exception)
            {
                MessageBox.Show("VRChatActivityLogger.exeが見つかりませんでした。", "VRChatActivityLogViewer");
            }
            catch (Exception)
            {
                MessageBox.Show("エラーが発生しました。プログラムを終了します。", "VRChatActivityLogViewer");
                Application.Current.Shutdown();
            }
            finally
            {

                DisableProcessingMode();

            }

        }

        /// <summary>
        /// WorldIDコピーボタンクリック時のイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CopyWorldIDButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                if (button.Tag is ActivityLogGridModel tag)
                {
                    Clipboard.SetText(tag.WorldID ?? "");
                }
            }
        }

        /// <summary>
        /// UserIDコピーボタンクリック時のイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CopyUserIDButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                if (button.Tag is ActivityLogGridModel tag)
                {
                    Clipboard.SetText(tag.UserID ?? "");
                }
            }
        }

        /// <summary>
        /// Joinボタンクリック時のイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void JoinButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                if (button.Tag is ActivityLogGridModel tag)
                {
                    var uri = "vrchat://launch?id=" + tag.WorldID;
                    uri = uri.Replace("&", "^&");
                    Process.Start(new ProcessStartInfo("cmd", $"/c start {uri}") { CreateNoWindow = true });
                }
            }
        }

        /// <summary>
        /// 処理中モードにする
        /// </summary>
        private void EnableProcessingMode()
        {
            taskbarInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Indeterminate;
            Mouse.OverrideCursor = Cursors.Wait;
            loggerButton.IsEnabled = false;
            searchButton.IsEnabled = false;
        }

        /// <summary>
        /// 処理中モードを解除する
        /// </summary>
        private void DisableProcessingMode()
        {
            taskbarInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.None;
            Mouse.OverrideCursor = null;
            loggerButton.IsEnabled = true;

            if (File.Exists(DatabaseContext.DBFilePath))
            {
                searchButton.IsEnabled = true;
            }
            else
            {
                searchButton.IsEnabled = false;
            }
        }
    }
}
