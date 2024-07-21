using FullText.Helpers;
using FullText.Tree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FullText.Search
{
    public class ResultItem : INotifyBase
    {

        private TreeNode _treeNode;
        private string _snippet;

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
    }
}
