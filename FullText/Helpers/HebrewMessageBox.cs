using System.Reflection;
using System.Windows;

namespace FullText.Helpers
{
    public static class HebrewMessageBox
    {
        static string AppName { get {return Assembly.GetExecutingAssembly().GetName().Name; } }

        public static MessageBoxResult YesNoMessageBox(string message, string caption)
        {
            return MessageBox.Show(message, caption,
                 MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes,
                    MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);
        }

        public static MessageBoxResult YesNoMessageBox(string message)
        {
            return MessageBox.Show(message, AppName,
                 MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes,
                    MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);
        }

        public static MessageBoxResult InformationMessageBox(string message, string caption)
        {
           return MessageBox.Show(message, caption,
                 MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK,
                    MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);
        }

        public static MessageBoxResult InformationMessageBox(string message)
        {
            return MessageBox.Show(message, AppName,
                  MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK,
                     MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);
        }
    }
}
