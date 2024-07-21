using System;
using System.IO;
using System.IO.Pipes;
using System.Windows;
using System.Windows.Input;
using PdfiumPreViewer.Helpers;
using PdfiumViewer;
using PdfiumViewer.Core;
using PdfiumViewer.Enums;

namespace PdfiumPreViewer
{
    internal class PdfPreViewer : PdfRenderer
    {
        public PdfSearchManager SearchManager;
        string lastSearchTerm; // Field to store the last search term

        public PdfPreViewer()
        {
            SearchManager = new PdfSearchManager(this);
        }

        public static readonly DependencyProperty ImmidiateSearchTermProperty =
            DependencyProperty.Register(
                nameof(ImmidiateSearchTerm),
                typeof(string),
                typeof(PdfPreViewer),
                new PropertyMetadata(string.Empty, OnImmidiateSearchTermChanged));

        public static readonly DependencyProperty SearchTermProperty =
            DependencyProperty.Register(
                nameof(SearchTerm),
                typeof(string),
                typeof(PdfPreViewer),
                new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty FileNameProperty =
           DependencyProperty.Register(
               nameof(FileName),
               typeof(string),
               typeof(PdfPreViewer),
               new PropertyMetadata(string.Empty, OnFileNameChanged));

        public static readonly DependencyProperty IsHandToolsEnabledProperty =
           DependencyProperty.Register(
               nameof(IsHandToolsEnabled),
               typeof(bool),
               typeof(PdfPreViewer),
               new PropertyMetadata(false, OnIsHandToolsEnabledChanged));


        public string ImmidiateSearchTerm
        {
            get => (string)GetValue(ImmidiateSearchTermProperty);
            set => SetValue(ImmidiateSearchTermProperty, value);
        }

        public string SearchTerm
        {
            get => (string)GetValue(SearchTermProperty);
            set => SetValue(SearchTermProperty, value);
        }
        
        public string FileName
        {
            get => (string)GetValue(FileNameProperty);
            set => SetValue(FileNameProperty, value);
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


        private static void OnImmidiateSearchTermChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PdfPreViewer viewer)  
            {
                viewer.SearchManager.Search((string)e.NewValue);
                try { viewer.SearchManager.FindNext(true); } catch { }
            }
        }

        void NewSearch()
        {
            if (lastSearchTerm != SearchTerm)
            {
                SearchManager.Search(SearchTerm);
                lastSearchTerm = SearchTerm;
            }           
            try { SearchManager.FindNext(true); } catch { }
        }

        private static void OnFileNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PdfPreViewer viewer)
            {
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
