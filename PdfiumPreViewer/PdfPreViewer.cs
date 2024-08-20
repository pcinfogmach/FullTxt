using System.IO;
using System.Windows;
using System.Windows.Input;
using PdfiumPreViewer.Helpers;
using PdfiumViewer;
using PdfiumViewer.Core;
using PdfiumViewer.Enums;

namespace PdfiumPreViewer
{
    public class PdfPreViewer : PdfRenderer
    {
        public PdfSearchManager SearchManager;
        string lastSearchTerm; // Field to store the last search term

        public PdfPreViewer()
        {
            SearchManager = new PdfSearchManager(this);
        }

        public static readonly DependencyProperty SearchTermProperty =
            DependencyProperty.Register(
                nameof(SearchTerm),
                typeof(string),
                typeof(PdfPreViewer),
                new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty FilePathProperty =
           DependencyProperty.Register(
               nameof(FilePath),
               typeof(string),
               typeof(PdfPreViewer),
               new PropertyMetadata(string.Empty, OnFilePathChanged));

        public static readonly DependencyProperty IsHandToolsEnabledProperty =
           DependencyProperty.Register(
               nameof(IsHandToolsEnabled),
               typeof(bool),
               typeof(PdfPreViewer),
               new PropertyMetadata(false, OnIsHandToolsEnabledChanged));

        public string SearchTerm
        {
            get => (string)GetValue(SearchTermProperty);
            set => SetValue(SearchTermProperty, value);
        }
        
        public string FilePath
        {
            get => (string)GetValue(FilePathProperty);
            set => SetValue(FilePathProperty, value);
        }

        public bool IsHandToolsEnabled
        {
            get => (bool)GetValue(IsHandToolsEnabledProperty);
            set => SetValue(IsHandToolsEnabledProperty, value);
        }

        public ICommand ZoomInCommand { get => new RelayCommand(ZoomIn); }
        public ICommand ZoomOutCommand { get => new RelayCommand(ZoomOut); }
        public ICommand FitWidthCommand { get => new RelayCommand(() => ZoomMode = PdfViewerZoomMode.FitWidth); }
        public ICommand FitHeightCommand { get => new RelayCommand(() => ZoomMode = PdfViewerZoomMode.FitHeight); }
        public ICommand RotateLeftCommand { get => new RelayCommand(Counterclockwise); }
        public ICommand RotateRightCommand { get => new RelayCommand(ClockwiseRotate); }
        public ICommand SearchCommand { get => new RelayCommand(() => NewSearch()); }
        public ICommand SearchNextCommand { get => new RelayCommand(() => SearchManager.FindNext(true)); }
        public ICommand SearchPreviousCommand { get => new RelayCommand(() => SearchManager.FindNext(false)); }

        void NewSearch()
        {
            if (lastSearchTerm != SearchTerm)
            {
                SearchManager.Search(SearchTerm);
                lastSearchTerm = SearchTerm;
            }           
            try { SearchManager.FindNext(true); } catch { }
        }

        private static void OnFilePathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PdfPreViewer viewer)
            {
                viewer.UnLoad();
                viewer.OpenPdf(new FileStream((string)e.NewValue, FileMode.Open, FileAccess.Read, FileShare.Read));
            }
        }

        private static void OnIsHandToolsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PdfPreViewer viewer)
            {
                bool value = (bool)e.NewValue;
                viewer.EnableKinetic = value;
                viewer.CursorMode = value ? PdfViewerCursorMode.Pan : PdfViewerCursorMode.TextSelection;
            }
        }
    }
}
