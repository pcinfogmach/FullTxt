using System;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Data;
using System.Windows.Shell;

namespace FullText.Controls
{
    public static class ProgressDialog
    {
        public static IProgress<double> Start(string message, int maximum)
        {
            IProgress<double> reporter = null;
            Application.Current.Dispatcher.Invoke(() => {

                TextBlock textBlock = new TextBlock { Text = message, TextWrapping = TextWrapping.WrapWithOverflow, Margin = new Thickness(5), };
                ProgressBar progressBar = new ProgressBar { Maximum = maximum};
                TextBlock percentageTextBlock = new TextBlock { HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center , Margin = new Thickness(1) };
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
                    Owner = Application.Current.MainWindow,
                    ResizeMode = ResizeMode.CanMinimize
                };

                TaskbarItemInfo taskbarItemInfo = new TaskbarItemInfo();
                //Application.Current.MainWindow.TaskbarItemInfo = taskbarItemInfo; 
                window.TaskbarItemInfo = taskbarItemInfo;
                taskbarItemInfo.ProgressState = TaskbarItemProgressState.Normal;

                reporter = new Progress<double>(OnProgressChanged);
                void OnProgressChanged(double incrementValue)
                {
                    if (incrementValue == -1) { taskbarItemInfo.ProgressState = TaskbarItemProgressState.None; window.Close(); }
                    //else if (progressBar.Value >= progressBar.Maximum) { window.Close(); taskbarItemInfo.ProgressState = TaskbarItemProgressState.None; }
                    else 
                    {
                        progressBar.Value += incrementValue;
                        taskbarItemInfo.ProgressValue = (double)progressBar.Value / progressBar.Maximum; 
                    }
                }

                window.Show();
            });

            return reporter;
        }
    }
}
