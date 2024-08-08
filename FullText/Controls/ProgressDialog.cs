using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Shell;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace FullText.Controls
{
    public static class ProgressDialog
    {
        public static IProgress<double> Start(string message, int maximum, CancellationTokenSource cancellationTokenSource)
        {
            IProgress<double> reporter = null;

            // Create the UI elements
            TextBlock textBlock = new TextBlock { Text = message, TextWrapping = TextWrapping.WrapWithOverflow, Margin = new Thickness(5) };
            ProgressBar progressBar = new ProgressBar { Maximum = maximum };
            TextBlock percentageTextBlock = new TextBlock { HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(1) };
            Binding binding = new Binding("Value") { Source = progressBar, StringFormat = $"{{0:0}} \\ {maximum}" };
            percentageTextBlock.SetBinding(TextBlock.TextProperty, binding);

            Grid grid = new Grid { Margin = new Thickness(5) };
            grid.Children.Add(progressBar);
            grid.Children.Add(percentageTextBlock);

            StackPanel stackPanel = new StackPanel();
            stackPanel.Children.Add(grid);
            stackPanel.Children.Add(textBlock);

            Window window = new Window
            {
                Content = stackPanel,
                SizeToContent = SizeToContent.WidthAndHeight,
                FlowDirection = FlowDirection.RightToLeft,
                ResizeMode = ResizeMode.CanMinimize
            };

            // Set the window owner to the main window
            try { window.Owner = Application.Current.MainWindow; } catch { }

            // Handle the window closing event to cancel the task
            window.Closing += (s, e) =>
            {
                cancellationTokenSource?.Cancel();
            };

            // Create the cancel button
            Button cancelButton = new Button
            {
                Content = "בטל",
                Margin = new Thickness(5),
                HorizontalAlignment = HorizontalAlignment.Center,
                Padding = new Thickness(3, 0, 3, 0),
            };

            cancelButton.Click += (s, e) =>
            {
                window.Close();
            };

            stackPanel.Children.Add(cancelButton);

            // Create taskbar item info for progress indication
            TaskbarItemInfo taskbarItemInfo = new TaskbarItemInfo
            {
                ProgressState = TaskbarItemProgressState.Normal
            };
            window.TaskbarItemInfo = taskbarItemInfo;

            // Define the progress reporter
            reporter = new Progress<double>(incrementValue =>
            {
                try
                {
                    Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        if (incrementValue == -1)
                        {
                            taskbarItemInfo.ProgressState = TaskbarItemProgressState.None;
                            window.Close();
                        }
                        else
                        {
                            progressBar.Value += incrementValue;
                            taskbarItemInfo.ProgressValue = progressBar.Value / progressBar.Maximum;
                        }
                    }), DispatcherPriority.Background);
                }
                catch { }
            });

            // Show the progress dialog window
            window.Show();

            return reporter;
        }
    }
}
