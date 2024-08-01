using FullText.Helpers;
using FullText.Search;
using FullText.Tree;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Text.RegularExpressions;
using Lucene.Net.Util;
using com.sun.source.util;

namespace FullText
{
    public class FullTxtViewModel : INotifyBase
    {
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
        private Visibility _highLightTextBlockVisibility;
        private bool _isHighLightTextBlockVisible = true;
        int _searchResultsSelectedIndex;
        ResultItem _currentResultItem;
        Uri _previewSource;

        LuceneIndexer indexer = new LuceneIndexer();
        List<string> foldersToIndex = new List<string>();
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

        public short DistanceBetweenSearchWords
        {
            get => Properties.Settings.Default.DistanceBetweenSearchWords;
            set 
            {
                Properties.Settings.Default.DistanceBetweenSearchWords = value; 
                Properties.Settings.Default.Save();
                OnPropertyChanged(nameof(DistanceBetweenSearchWords));
            } 
        }

        public bool IsHighLightTextBlockVisibile
        {
            get => _isHighLightTextBlockVisible;
            set
            {
                if (_isHighLightTextBlockVisible != value)
                {
                    _isHighLightTextBlockVisible = value;
                    HighLightTextBlockVisibility = value ? Visibility.Visible : Visibility.Collapsed;
                    OnPropertyChanged(nameof(IsHighLightTextBlockVisibile));
                }
            }
        }

        public Visibility HighLightTextBlockVisibility
        {
            get => Visibility.Collapsed;
            set
            {
                if (_highLightTextBlockVisibility != value)
                {
                    _highLightTextBlockVisibility = value;
                    OnPropertyChanged(nameof(HighLightTextBlockVisibility));
                }
            }
        }

        #endregion

        #region Commands
        public ICommand IndexedSearchCommand { get => new RelayCommand(IndexedSearch); }
        public ICommand LocalSearchCommand { get => new RelayCommand(LocalSearch); }
        public ICommand AddNewNodeCommand { get => new RelayCommand(AddNewFolderToIndex); }
        public ICommand RemoveSelectedNodeCommand { get => new RelayCommand(RemoveSelectedNode); }

        #endregion

        #region Methods

        public FullTxtViewModel()
        {
            SearchTerm = RecentSearchCollection[0];
            Application.Current.Exit += (s, e) => { SaveCurrentTreeStatus(); };
            Properties.Settings.Default.PropertyChanged += Default_PropertyChanged;
        }

        private void Default_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Properties.Settings.Default.NodeToChange))
            {
                new TreeLoader().UpdateTree(RootNode, Properties.Settings.Default.NodeToChange);
            }
        }

        async void LocalSearch()
        {
            SelectedTabIndex = 0;
            string queryText = SearchTerm;
            CommonOpenFileDialog dialog = new CommonOpenFileDialog { IsFolderPicker = true };

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                StartSearch();

                var filePaths = System.IO.Directory.GetFiles(dialog.FileName, "*.*", System.IO.SearchOption.AllDirectories);

                var tasks = filePaths.Select(async filePath =>
                {
                    if (!IsSearchInProgress) return;
                    SearchResults.AddRange(await Task.Run(() =>
                    {
                        return new InMemoryLuceneSearch().Search(filePath, queryText);
                    }));
                });

                await Task.WhenAll(tasks);

                EndSearch();
            }
        }

        async void IndexedSearch()
        {
            if (IsSearchInProgress) { IsSearchInProgress = false; return; }
            if (RootNode.Children.Count == 0) 
            {
                var result = HebrewMessageBox.YesNoMessageBox("לא נמצאו קבצים באינדקס האם ברצונך לערוך חיפוש חד פעמי ללא אינדקס?");
                if (result == MessageBoxResult.Yes) { LocalSearch(); }
                return; 
            }
            
            StartSearch();

            List<TreeNode> checkedTreeItems = RootNode.AllTreeNodes
                .Where(node => node is FileTreeNode && node.IsChecked == true).ToList();
            SearchResults = await Task.Run(() =>
            {
                return new ObservableCollection<ResultItem>
                (new LuceneSearch().Search(SearchTerm, checkedTreeItems));
            });

            if (_searchResults.Count > 0) { SelectedTabIndex = 0; SearchResultsSelectedIndex = 0; CurrentResultItem = SearchResults[0]; }
            EndSearch();
        }

        void StartSearch()
        {
            _currentSearchTerm = SearchTerm;
            UpdateRecentSearchCollection(SearchTerm);
            IsSearchInProgress = true;
            _searchResults.Clear();
        }

        void EndSearch()
        {
            if (_searchResults.Count <= 0) { MessageBox.Show("לא נמצאו תוצאות"); }
            IsSearchInProgress = false;
        }

        public async void AddNewFolderToIndex()
        {
            IsIndexingInProgress = true;
            await Task.Run(async () => { await indexer.NewIndexingTask(); });
            IsIndexingInProgress = false;
        }

        public async void RemoveSelectedNode()
        {
            if (IsIndexingInProgress) { MessageBox.Show("התוכנה עסוקה בפעולות קודמות"); return; }

            var selectedNode = _rootNode.AllTreeNodes.FirstOrDefault(node => node.IsSelected);
            if (selectedNode != null && selectedNode.Parent is RootTreeNode)
            {
                List<string> filesToRemove = selectedNode.GetAllFilePaths();
                await Task.Run(() =>
                {
                    new LuceneIndexer().RemoveFiles(filesToRemove);
                });
                selectedNode.Parent.RemoveChild(selectedNode);
            }
            else
            {
                HebrewMessageBox.InformationMessageBox("אין אפשרות למחוק פריט זה");
            }
            //SaveCurrentTreeStatus();
            //RootNode = new TreeLoader().Load();
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
