using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace VRChatActivityLogViewer
{
    /// <summary>
    /// LoggerErrorDialog.xaml の相互作用ロジック
    /// </summary>
    public partial class LoggerErrorDialog : Window
    {
        private string errorFilePath;

        public LoggerErrorDialog(string errorFilePath)
        {
            InitializeComponent();
            this.errorFilePath = errorFilePath;
            filePathLink.Text = errorFilePath;
        }

        private void filePathLink_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("EXPLORER.EXE", @$"/select,""{errorFilePath}""");
        }

        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
