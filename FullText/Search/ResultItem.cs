using FullText.Helpers;
using FullText.Tree;
using NPOI.OpenXmlFormats.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FullText.Search
{
    public class ResultItem : INotifyBase
    {
        private TreeNode _treeNode;
        private string _snippet;
        string _markedTerm;
        int _resultNumber;

        public TreeNode TreeNode
        {
            get => _treeNode;
            set { if (_treeNode != value) { _treeNode = value; OnPropertyChanged(nameof(TreeNode)); } }
        }

        public string Snippet
        {
            get => _snippet;
            set { if (_snippet != value) { _snippet = value.RemoveEmptyLines().Trim(); OnPropertyChanged(nameof(Snippet)); } }
        }

        public string MarkedTerm
        {
            set => _markedTerm = value;
            get
            {
                ValidateMarkedTerm();
                return _markedTerm;
            }
        }

        void ValidateMarkedTerm()
        {
            if (_markedTerm == null)
            {
                string markedText = Regex.Match(Snippet, @"<mark>(.*?)</mark>").Value;
                _markedTerm = Regex.Replace(markedText, @"</?mark>", "");
            }
        }

        public string GroupName { get; set; }
        public string Title { get; set; }
        public int ResultNumber { get; set; }
        public int TotalRelativeResults { get; set; } //number of similar results in same doc
    }
}
