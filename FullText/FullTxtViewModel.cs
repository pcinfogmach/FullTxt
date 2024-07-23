using FullText.Helpers;
using FullText.Search;
using FullText.Tree;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Text.RegularExpressions;

namespace FullText
{
    public class FullTxtViewModel : INotifyBase
    {
        public FullTxtViewModel()
        {
            SearchTerm = RecentSearchCollection[0];
            Application.Current.Exit += (s, e) => { SaveCurrentTreeStatus(); };
        }

        #region Members
        private RootTreeNode _rootNode;
        bool _isIndexingInProgress;
        private bool _isSearchInProgress;
        private string _searchButtonImage;
        private FlowDirection _textBoxFlowDirection = FlowDirection.RightToLeft;
        private string _searchTerm;
        private string _currentSearchTerm;
        ObservableCollection<string> _recentSearchCollection;
        int _recentSearchSelectedIndex = -1;
        int _selectedTabIndex = 1;
        ObservableCollection<ResultItem> _searchResults = new ObservableCollection<ResultItem>();
        int _searchResultsSelectedIndex;
        ResultItem _currentResultItem;
        Uri _previewSource;
        #endregion

        #region Properties

        public RootTreeNode RootNode
        {
            get { if (_rootNode == null) _rootNode = new TreeLoader().Load(); return _rootNode; }
            set { if (_rootNode != value) { _rootNode = value; OnPropertyChanged(nameof(RootNode)); } }
        }

        public ObservableCollection<string> RecentSearchCollection
        {
            get
            {
                if (_recentSearchCollection == null)
                {
                    _recentSearchCollection = new ObservableCollection<string>
                        (Properties.Settings.Default.RecentSearchCollection.Cast<string>());
                }
                return _recentSearchCollection;
            }
            set
            {
                if (_recentSearchCollection != value) { _recentSearchCollection = value; OnPropertyChanged(nameof(RecentSearchCollection)); }
            }
        }

        public int SelectedTabIndex
        {
            get => _selectedTabIndex;
            set { if (_selectedTabIndex != value) { _selectedTabIndex = value; OnPropertyChanged(nameof(SelectedTabIndex)); } }
        }

        public int RecentSearchSelectedIndex
        {
            get => _recentSearchSelectedIndex;
            set
            {
                if (_recentSearchSelectedIndex != value && value != -1)
                {
                    SearchTerm = RecentSearchCollection[value];
                }
            }
        }

        public string SearchButtonImage
        {
            get { if (_searchButtonImage == null) { _searchButtonImage = @"/Resources/SearchIcon.png"; } return _searchButtonImage; }
            set { if (_searchButtonImage != value) { _searchButtonImage = value; OnPropertyChanged(nameof(SearchButtonImage)); } }
        }

        public string SearchTerm
        {
            get => _searchTerm;
            set
            {
                if (_searchTerm != value)
                {
                    _searchTerm = value;
                    if (string.IsNullOrEmpty(value) || Regex.IsMatch(value, @"\p{IsHebrew}|^\W+$")) { TextBoxFlowDirection = FlowDirection.RightToLeft; }
                    else { TextBoxFlowDirection = FlowDirection.LeftToRight; }
                    OnPropertyChanged(nameof(SearchTerm));
                }
            }
        }

        public FlowDirection TextBoxFlowDirection
        {
            get => _textBoxFlowDirection;
            set { if (_textBoxFlowDirection != value) { _textBoxFlowDirection = value; OnPropertyChanged(nameof(TextBoxFlowDirection)); } }
        }

        public bool IsIndexingInProgress
        {
            get => _isIndexingInProgress;
            set { if (_isIndexingInProgress != value) { _isIndexingInProgress = value; OnPropertyChanged(nameof(IsIndexingInProgress)); } }
        }
        public bool IsSearchInProgress
        {
            get => _isSearchInProgress;
            set
            {
                if (_isSearchInProgress != value)
                {
                    _isSearchInProgress = value;
                    OnPropertyChanged(nameof(IsSearchInProgress));
                    SearchButtonImage = _isSearchInProgress ? @"/Resources/StopIcon.png" : @"/Resources/SearchIcon.png";
                }
            }
        }

        public ObservableCollection<ResultItem> SearchResults
        {
            get => _searchResults;
            set { if (_searchResults != value) { _searchResults = value; OnPropertyChanged(nameof(SearchResults)); } }
        }

        public int SearchResultsSelectedIndex
        {
            get => _searchResultsSelectedIndex;
            set
            {
                if (_searchResultsSelectedIndex != value && value != -1)
                {
                    _searchResultsSelectedIndex = value;
                    //PreviewSource = new Uri("about:blank");
                    //PreviewSource = new Uri(HtmlConverter.ConvertToHtml(SearchResults[value].TreeNode.Path));
                    CurrentResultItem = SearchResults[value];
                    OnPropertyChanged(nameof(SearchResultsSelectedIndex));
                }
            }
        }

        public ResultItem CurrentResultItem
        {
            get => _currentResultItem;
            set { if (_currentResultItem != value) { _currentResultItem = value; OnPropertyChanged(nameof(CurrentResultItem)); } }
        }

        public Uri PreviewSource
        {
            get => _previewSource;
            set
            {
                if (_previewSource != value) { _previewSource = value; OnPropertyChanged(nameof(PreviewSource)); }
            }
        }

        #endregion

        #region Commands
        public ICommand IndexedSearchCommand { get => new RelayCommand(IndexedSearch); }
        public ICommand SearchCommand { get => new RelayCommand(Search); }
        public ICommand AddNewNodeCommand { get => new RelayCommand(AddNewNode); }
        public ICommand RemoveSelectedNodeCommand { get => new RelayCommand(RemoveSelectedNode); }

        #endregion

        #region Methods

        void IndexedSearch()
        {
            _currentSearchTerm = SearchTerm;
            IsSearchInProgress = !IsSearchInProgress;

            if (SearchResults.Count > 0)
            {

            }
        }

        async void Search()
        {
            if (IsSearchInProgress) { IsSearchInProgress = false; return; }

            _currentSearchTerm = SearchTerm;
            UpdateRecentSearchCollection(SearchTerm);
            IsSearchInProgress = true;

            List<TreeNode> checkedTreeItems = RootNode.AllTreeNodes
                .Where(node => node is FileTreeNode && node.IsChecked == true).ToList();

            _searchResults.Clear();
            SearchResults = await Task.Run(() =>
            {
                return new ObservableCollection<ResultItem>
                (new LuceneSearch().Search(SearchTerm, checkedTreeItems));
            });

            if (_searchResults.Count <= 0) { MessageBox.Show("לא נמצאו תוצאות"); }
            else { SelectedTabIndex = 0; SearchResultsSelectedIndex = 0; CurrentResultItem = SearchResults[0]; }
            IsSearchInProgress = false;
        }

        public async void AddNewNode()
        {
            if (IsIndexingInProgress) { MessageBox.Show("התוכנה עסוקה בפעולות קודמות"); return; }
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                var existingNode = RootNode.Children.FirstOrDefault(child => child.Path == dialog.FileName);
                if (existingNode != null)
                {
                    var result = MessageBox.Show("תיקייה זו כבר קיימת באינדקס האם ברצונך לעדכן אותה?", "תיקייה קיימת", MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.Yes, MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);
                    if (result == MessageBoxResult.No) return;
                    existingNode.Parent.RemoveChild(existingNode);
                }

                IsIndexingInProgress = true;
                string[] files = System.IO.Directory.GetFiles(dialog.FileName, "*.*", System.IO.SearchOption.AllDirectories);
                await Task.Run(() =>
                {
                    new LuceneSearch().IndexFiles(files.ToList());
                });

                FolderTreeNode folderTreeNode = new FolderTreeNode(dialog.FileName);
                _rootNode.AddChild(folderTreeNode);
                _rootNode.AllTreeNodes.Add(folderTreeNode);
                new TreeLoader().PopulateChildren(_rootNode, folderTreeNode, new List<string>());
                OnPropertyChanged(nameof(RootNode));
                IsIndexingInProgress = false;
            }
        }

        public async void RemoveSelectedNode()
        {
            if (IsIndexingInProgress) { MessageBox.Show("התוכנה עסוקה בפעולות קודמות"); return; }
            IsIndexingInProgress = true;

            var selectedNode = _rootNode.AllTreeNodes.FirstOrDefault(node => node.IsSelected);
            if (selectedNode.Parent is RootTreeNode) selectedNode.Parent.RemoveChild(selectedNode);

            List<string> filesToRemove = selectedNode.GetAllFilePaths();
            await Task.Run(() =>
            {
                new LuceneSearch().RemoveFiles(filesToRemove);
            });

            SaveCurrentTreeStatus();
            RootNode = new TreeLoader().Load();
            IsIndexingInProgress = false;
        }

        public void UpdateRecentSearchCollection(string input)
        {
            if (_recentSearchCollection.Contains(input)) { _recentSearchCollection.Remove(input); }
            if (_recentSearchCollection.Count >= 10) { _recentSearchCollection.RemoveAt(RecentSearchCollection.Count - 1); }
            RecentSearchCollection.Insert(0, input);

            Properties.Settings.Default.RecentSearchCollection.Clear();
            Properties.Settings.Default.RecentSearchCollection.AddRange(_recentSearchCollection.ToArray());
            Properties.Settings.Default.Save();
        }

        void SaveCurrentTreeStatus()
        {
            Properties.Settings.Default.CheckedFileNodes.Clear();
            Properties.Settings.Default.CheckedFileNodes.AddRange(RootNode.AllTreeNodes
                .Where(node => node is FileTreeNode && node.IsChecked == true)
                .Select(node => node.Path)
                .ToArray());

            Properties.Settings.Default.RootFolderNodes.Clear();
            Properties.Settings.Default.RootFolderNodes.AddRange(RootNode.Children
                .Select(node => node.Path)
                .ToArray());

            Properties.Settings.Default.Save();
        }

        #endregion
    }
}
