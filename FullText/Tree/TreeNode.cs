using FullText.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FullText.Tree
{
    public class RootTreeNode : TreeNode
    {
        public List<TreeNode> AllTreeNodes = new List<TreeNode>();
        public RootTreeNode(string path) : base(path) {  }
    }
    public class FolderTreeNode : TreeNode { public FolderTreeNode(string path) : base(path) { } }
    public class FileTreeNode : TreeNode { public FileTreeNode(string path) : base(path) { } }
    public class TreeNode : INotifyBase
    {
        private string _name;
        private string _path;
        private string _directory;
        private DateTime _dateLastModified;
        private TreeNode _parent;
        private ObservableCollection<TreeNode> _children = new ObservableCollection<TreeNode>();
        private bool _isSelected;
        private bool? _isChecked = false;
        public float searchScore;

        public TreeNode(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) { return; }
            _path = path;
            _directory = System.IO.Path.GetDirectoryName(path);
            _name = System.IO.Path.GetFileName(path);
        }

        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        public string Path
        {
            get => _path;
            set
            {
                if (_path != value)
                {
                    _path = value;
                    OnPropertyChanged(nameof(Path));
                }
            }
        }

        public string Directory
        {
            get => _directory;
            set
            {
                if (_directory != value)
                {
                    _directory = value;
                    OnPropertyChanged(nameof(Directory));
                }
            }
        }

        public DateTime DateLastModified
        {
            get => _dateLastModified;
            set
            {
                if (_dateLastModified != value)
                {
                    _dateLastModified = value;
                    OnPropertyChanged(nameof(DateLastModified));
                }
            }
        }
        public TreeNode Parent
        {
            get => _parent;
            set
            {
                if (_parent != value)
                {
                    _parent = value;
                    OnPropertyChanged(nameof(Parent));
                }
            }
        }

        public ObservableCollection<TreeNode> Children
        {
            get => _children;
            set
            {
                if (_children != value)
                {
                    _children = value;
                    OnPropertyChanged(nameof(Children));
                }
            }
        }

        public bool? IsChecked
        {
            get { return _isChecked; }
            set { this.SetIsChecked(value, true, true); }
        }

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                }
            }
        }

        public void AddChild(TreeNode child)
        {
            child.Parent = this;
            Children.Add(child);
        }

        public void RemoveChild(TreeNode child)
        {
            if (_children.Remove(child))
            {
                child.Parent = null;
                OnPropertyChanged(nameof(Children));
            }
        }

        void SetIsChecked(bool? value, bool updateChildren, bool updateParent)
        {
            if (value == _isChecked)
                return;

            _isChecked = value;

            if (updateChildren && _isChecked.HasValue)
            {
                foreach (var child in Children)
                {
                    child.SetIsChecked(_isChecked, true, false);
                }
            }

            if (updateParent && _parent != null)
                _parent.VerifyCheckState();

            this.OnPropertyChanged("IsChecked");
        }

        void VerifyCheckState()
        {
            bool? state = null;
            for (int i = 0; i < this.Children.Count; ++i)
            {
                bool? current = this.Children[i].IsChecked;
                if (i == 0)
                {
                    state = current;
                }
                else if (state != current)
                {
                    state = null;
                    break;
                }
            }
            this.SetIsChecked(state, false, true);
        }

        public List<string> GetAllFilePaths()
        {
            List<string> files = new List<string>();
            if (this is FolderTreeNode)
            {
                files.AddRange(System.IO.Directory.GetFiles(Path, "*.*", System.IO.SearchOption.AllDirectories));
            }
            else if (this is FileTreeNode)
            {
                files.Add(this.Path);
            }
            return files;
        }

        public TreeNode HardCopy()
        {
            if (this is FileTreeNode)
            {
                FileTreeNode treeNode = new FileTreeNode(Path);
                foreach (var node in Children)
                {
                    treeNode.AddChild(node.HardCopy());
                }
                return treeNode;
            }
            else if (this is FolderTreeNode)
            {
                FolderTreeNode treeNode = new FolderTreeNode(Path);
                foreach (var node in Children)
                {
                    treeNode.AddChild(node.HardCopy());
                }
                return treeNode;
            }
            else
            {
                TreeNode treeNode = new TreeNode(Path);
                foreach (var node in Children)
                {
                    treeNode.AddChild(node.HardCopy());
                }
                return treeNode;
            }
        }
    }
}
