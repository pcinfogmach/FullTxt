using DocumentFormat.OpenXml.Spreadsheet;
using FullText.Search;
using FullText.Tree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using static System.ComponentModel.Design.ObjectSelectorEditor;

namespace FullText
{
    /// <summary>
    /// Interaction logic for FullTxtView.xaml
    /// </summary>
    public partial class FullTxtView : UserControl
    {
        public FullTxtView()
        {
            InitializeComponent();

            Loaded += (s, e) => { SearchTextBox.Focus(); };
        }

        private void SearchTextBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            var txtControl = sender as TextBox;
            txtControl.Dispatcher.BeginInvoke(new Action(() =>
            {
                txtControl.SelectAll();
            }));
        }

        private void ShowNextResult_Click(object sender, RoutedEventArgs e)
        {
            viewModel.SearchResultsSelectedIndex--;
        }

        private void ShowPreviousResult_Click(object sender, RoutedEventArgs e)
        {
            viewModel.SearchResultsSelectedIndex++;
        }

        private void TreeViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is TreeViewItem treeViewItem && 
                treeViewItem.DataContext is FileTreeNode treeNode)
                System.Diagnostics.Process.Start(treeNode.Path);

        }

        private void TreeViewItem_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is TreeViewItem treeViewItem && 
                treeViewItem.DataContext is FileTreeNode treeNode)
                System.Diagnostics.Process.Start(System.IO.Path.GetDirectoryName(treeNode.Path));
        }

        private void ListViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListViewItem listViewItem &&
                listViewItem.DataContext is ResultItem resultItem)
                System.Diagnostics.Process.Start(resultItem.TreeNode.Path);
        }

        private void ListViewItem_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListViewItem listViewItem &&
                listViewItem.DataContext is ResultItem resultItem)
                System.Diagnostics.Process.Start(System.IO.Path.GetDirectoryName(resultItem.TreeNode.Path));
        }
    }
}
