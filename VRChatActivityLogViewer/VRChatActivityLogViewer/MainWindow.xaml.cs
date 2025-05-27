using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VRChatActivityToolsShared.Database;
using Microsoft.Extensions.Configuration;

namespace VRChatActivityLogViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ObservableCollection<ActivityLogGridModel> ActivityLogGridModelCollection = new ObservableCollection<ActivityLogGridModel>();

        private readonly string errorFilePath = "./Logs/VRChatActivityLogger/errorfile.txt";

        private readonly IConfigurationRoot _configuration;

        private readonly DbOperatorFactory _dbOperatorFactory;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            var env = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? string.Empty;
            _configuration = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
                                                       .AddJsonFile("appsettings.json")
                                                       .AddJsonFile($"appsettings.{env}.json", true)
                                                       .Build();
            _dbOperatorFactory = new DbOperatorFactory(new DbConfig(_configuration));
        }

        /// <summary>
        /// ウィンドウロード時の処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DisableProcessingMode();

            periodComboBox.SelectedIndex = 0;

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
            await ExecuteSearch();
        }

        /// <summary>
        /// 検索処理の実行
        /// </summary>
        /// <returns></returns>
        private async Task ExecuteSearch()
        {
            try
            {
                // 処理中モード開始
                EnableProcessingMode();

                // DBが古い場合はアップグレードする
                using (var context = _dbOperatorFactory.GetDbContext())
                {
                    if (_dbOperatorFactory.DbMigration.GetCurrentVersion(context) < ActivityContextBase.Version)
                    {
                        _dbOperatorFactory.DbMigration.UpgradeDatabase(context);
                    }
                }

                // 検索期間の計算
                DateTime? fromDate = default;
                DateTime? untilDate = default;

                if (periodComboBox.SelectedItem is ComboBoxItem periodItem)
                {
                    if (periodItem != null)
                    {
                        var period = (SearchPeriod)periodItem.Tag;

                        if (period == SearchPeriod.Recent)
                        {
                            fromDate = DateTime.Today.AddDays(-1);
                            untilDate = DateTime.Today;
                        }
                        else if (period == SearchPeriod.OneWeek)
                        {
                            fromDate = DateTime.Today.AddDays(-7);
                            untilDate = DateTime.Today;
                        }
                        else if (period == SearchPeriod.OneMonth)
                        {
                            fromDate = DateTime.Today.AddDays(-30);
                            untilDate = DateTime.Today;
                        }
                        else if (period == SearchPeriod.OneYear)
                        {
                            fromDate = DateTime.Today.AddDays(-365);
                            untilDate = DateTime.Today;
                        }
                        else if (period == SearchPeriod.All)
                        {
                            fromDate = DateTime.Parse("1970/01/01 00:00:00");
                            untilDate = DateTime.Parse("3000/12/31 23:59:59");
                        }
                        else if (period == SearchPeriod.Custom)
                        {
                            fromDate = fromDatePicker.SelectedDate;
                            untilDate = untilDatePicker.SelectedDate;
                        }
                    }
                }

                // ログの検索
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
                    FromDateTime = fromDate,
                    UntilDateTime = untilDate?.AddDays(1),
                    IsReceivedInviteResponse = recvInvResCheckBox.IsChecked ?? false,
                    IsReceivedRequestInviteResponse = recvReqInvResCheckBox.IsChecked ?? false,
                    IsPlayedVideo = videoCheckBox.IsChecked ?? false,
                    IsAcceptInvite = acptInvCheckBox.IsChecked ?? false,
                    IsAcceptRequestInvite = acptReqInvCheckBox.IsChecked ?? false,
                };
                var activityLogs = await VRChatActivityLogModel.SearchActivityLogs(parameter, _dbOperatorFactory);

                // 選択アイテムの保存
                var selectedItem = ActivityLogGrid.SelectedItem as ActivityLogGridModel;

                // グリッド作成
                ActivityLogGridModelCollection.Clear();

                var keywords = keywordBox.Text.Split(' ').Where(s => s != string.Empty).ToArray();

                if (smartSearchCheckBox.IsChecked)
                {
                    // スマート検索
                    var tmpList = new List<ActivityLogGridModel>();

                    for (var i = 0; i < activityLogs.Count; i++)
                    {
                        var gridModel = new ActivityLogGridModel(activityLogs[i]);

                        if (keywords.Any())
                        {
                            var contained = keywords.All(k => gridModel.Content?.Contains(k, StringComparison.CurrentCultureIgnoreCase) ?? false);

                            if (!contained)
                            {
                                continue;
                            }
                        }

                        if (gridModel.Type != ActivityType.JoinedRoom)
                        {
                            ActivityLog relatedJoin = null;

                            for (var j = i; 0 <= j; j--)
                            {
                                if (activityLogs[j].ActivityType == ActivityType.JoinedRoom)
                                {
                                    relatedJoin = activityLogs[j];

                                    break;
                                }
                            }

                            ActivityLog latestJoin = null;

                            for (var j = tmpList.Count - 1; 0 <= j; j--)
                            {
                                if (tmpList[j].Type == ActivityType.JoinedRoom)
                                {
                                    latestJoin = tmpList[j].Source;

                                    break;
                                }
                            }

                            if (relatedJoin != null && relatedJoin != latestJoin)
                            {
                                tmpList.Add(new ActivityLogGridModel(relatedJoin));
                            }
                        }

                        tmpList.Add(gridModel);
                    }

                    foreach (var gridModel in tmpList.OrderByDescending(a => a.TimeStamp))
                    {
                        ActivityLogGridModelCollection.Add(gridModel);
                    }
                }
                else
                {
                    // 従来の検索
                    foreach (var activityLog in activityLogs.OrderByDescending(a => a.Timestamp))
                    {
                        var gridModel = new ActivityLogGridModel(activityLog);

                        if (keywords.Any())
                        {
                            var contained = keywords.All(k => gridModel.Content?.Contains(k, StringComparison.CurrentCultureIgnoreCase) ?? false);

                            if (!contained)
                            {
                                continue;
                            }
                        }

                        ActivityLogGridModelCollection.Add(gridModel);
                    }
                }

                // 選択アイテムの復元
                if (selectedItem != null)
                {
                    var newSelectedItem = ActivityLogGridModelCollection.FirstOrDefault(a => a.Source.ID == selectedItem.Source.ID);

                    if (newSelectedItem != null)
                    {
                        ActivityLogGrid.SelectedItem = newSelectedItem;
                        ActivityLogGrid.ScrollIntoView(ActivityLogGrid.SelectedItem);
                    }
                }

                // 処理中モード終了
                DisableProcessingMode();
            }
            catch (Exception)
            {
                MessageBox.Show("エラーが発生しました。プログラムを終了します。", "VRChatActivityLogViewer", MessageBoxButton.OK, MessageBoxImage.Error);
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

                var success = await Task.Run(() =>
                {
                    var process = Process.Start("VRChatActivityLogger.exe");
                    process.WaitForExit();
                    return process.ExitCode == 0;
                });

                DisableProcessingMode();

                if (!success)
                {
                    if (File.Exists(errorFilePath))
                    {
                        var errorFileLines = File.ReadAllLines(errorFilePath);
                        if (0 < errorFileLines.Length)
                        {
                            var dialog = new LoggerErrorDialog(errorFileLines[0]);
                            dialog.Owner = this;
                            dialog.ShowDialog();

                            return;
                        }
                    }

                    MessageBox.Show("VRChatログの解析に失敗しました。", "VRChatActivityLogViewer", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (System.ComponentModel.Win32Exception)
            {
                MessageBox.Show("VRChatActivityLogger.exeが見つかりませんでした。", "VRChatActivityLogViewer", MessageBoxButton.OK, MessageBoxImage.Error);
                DisableProcessingMode();
            }
            catch (Exception)
            {
                MessageBox.Show("エラーが発生しました。プログラムを終了します。", "VRChatActivityLogViewer", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
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
                    Clipboard.SetDataObject(tag.WorldID ?? "");
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
                    Clipboard.SetDataObject(tag.UserID ?? "");
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
        /// Detailボタンクリック時のイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DetailButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                if (button.Tag is ActivityLogGridModel tag)
                {
                    if (!tag.IsDetailWindowEnabled)
                    {
                        return;
                    }

                    ShowDetailWindow(tag.Source);
                }
            }
        }

        /// <summary>
        /// グリッドをダブルクリックした時のイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ActivityLogGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ActivityLogGrid.SelectedItem is ActivityLogGridModel gridModel)
            {
                if (!gridModel.IsDetailWindowEnabled)
                {
                    return;
                }

                ShowDetailWindow(gridModel.Source);
            }
        }

        /// <summary>
        /// 詳細ダイアログを表示する
        /// </summary>
        /// <param name="activityLog"></param>
        private void ShowDetailWindow(ActivityLog activityLog)
        {
            var dialog = new DetailWindow(activityLog);
            dialog.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            dialog.Show();
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
            
            using (var context = _dbOperatorFactory.GetDbContext())
            {
                if (context.Database.CanConnect())
                {
                    searchButton.IsEnabled = true;
                }
                else
                {
                    searchButton.IsEnabled = false;
                }
            }

            ActivityLogGrid.Focus();
        }

        /// <summary>
        /// キーワード入力時のイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void keywordBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                await ExecuteSearch();
            }
        }

        /// <summary>
        /// Help/Aboutメニュークリック時のイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AboutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AboutDialog();
            dialog.Owner = this;
            dialog.ShowDialog();
        }

        /// <summary>
        /// File/Exitメニュークリック時のイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        /// <summary>
        /// Tools/TaskScheduler/Registerメニュークリック時のイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RegisterTaskSchedulerMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var result = MessageBox.Show("1時間毎にLoggerが実行されるようタスクスケジューラに登録しますか？", "VRChatActivityLogViewer", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if(result == MessageBoxResult.No)
                {
                    return;
                }

                var process = Process.Start(new ProcessStartInfo("add-taskschedular.bat", "/c") { RedirectStandardOutput = true, RedirectStandardError = true });
                process.WaitForExit();
                var stdo = process.StandardOutput.ReadToEnd();
                var stde = process.StandardError.ReadToEnd();

                if (string.IsNullOrWhiteSpace(stde))
                {
                    MessageBox.Show(stdo, "VRChatActivityLogViewer", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show(stde, "VRChatActivityLogViewer", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (System.ComponentModel.Win32Exception)
            {
                MessageBox.Show("add-taskschedular.batが見つかりませんでした。", "VRChatActivityLogViewer");
                DisableProcessingMode();
            }
        }

        /// <summary>
        /// Tools/TaskScheduler/Unregisterメニュークリック時のイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UnregisterTaskSchedulerMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var result = MessageBox.Show("タスクスケジューラに登録した設定を解除しますか？", "VRChatActivityLogViewer", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.No)
                {
                    return;
                }

                var process = Process.Start(new ProcessStartInfo("delete-taskschedular.bat", "/c") { RedirectStandardOutput = true, RedirectStandardError = true });
                process.WaitForExit();
                var stdo = process.StandardOutput.ReadToEnd();
                var stde = process.StandardError.ReadToEnd();

                if (string.IsNullOrWhiteSpace(stde))
                {
                    MessageBox.Show(stdo, "VRChatActivityLogViewer", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show(stde, "VRChatActivityLogViewer", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (System.ComponentModel.Win32Exception)
            {
                MessageBox.Show("delete-taskschedular.batが見つかりませんでした。", "VRChatActivityLogViewer");
                DisableProcessingMode();
            }
        }

        /// <summary>
        /// キーワードクリアボタン押下時のイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void KeywordClearButton_Click(object sender, RoutedEventArgs e)
        {
            keywordBox.Clear();
        }

        /// <summary>
        /// 検索期間コンボボックス選択時のイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void periodComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox comboBox)
            {
                if (comboBox.SelectedItem is ComboBoxItem item)
                {
                    if (item.Tag != null && (SearchPeriod)item.Tag == SearchPeriod.Custom)
                    {
                        customRange.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        customRange.Visibility = Visibility.Collapsed;
                    }
                }
            }
        }
    }
    public enum SearchPeriod
    {
        Recent,
        OneWeek,
        OneMonth,
        OneYear,
        All,
        Custom,
    }
}
