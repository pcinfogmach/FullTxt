using Ookii.Dialogs.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TextSeeker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void StartProcess_Click(object sender, RoutedEventArgs e)
        {
            var progressDialog = new ProgressDialog
            {
                WindowTitle = "Progress",
                Text = "Processing...",
                Description = "Please wait while the task is running.",
                ShowTimeRemaining = true,
                ShowCancelButton = true
            };

            progressDialog.DoWork += (s, args) =>
            {
                for (int i = 0; i <= 100; i++)
                {
                    if (progressDialog.CancellationPending)
                    {
                        args.Cancel = true;
                        break;
                    }

                    Thread.Sleep(50); // Simulate work
                    progressDialog.ReportProgress(i);
                }


                if (progressDialog.CancellationPending)
                {
                    MessageBox.Show("Operation canceled.");
                }
                else
                {
                    MessageBox.Show("Operation completed.");
                }
            };

            await Task.Run(() => progressDialog.ShowDialog());

        }
    }
}
