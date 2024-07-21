using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FullText.Tree
{
    internal class TreeLoader
    {
        public RootTreeNode Load()
        {
            List<string> checkedFileNodes = Properties.Settings.Default.CheckedFileNodes.Cast<string>().ToList();
            List<string> directories = Properties.Settings.Default.RootFolderNodes.Cast<string>().ToList();
            RootTreeNode rootNode = new RootTreeNode(null);
            foreach (var directory in directories)
            {
                if (Directory.Exists(directory))
                {
                    FolderTreeNode folderTreeNode = new FolderTreeNode(directory);
                    rootNode.AddChild(folderTreeNode);
                    rootNode.AllTreeNodes.Add(folderTreeNode);
                    PopulateChildren(rootNode, folderTreeNode, checkedFileNodes);
                }
            }
            return rootNode;
        }

        public void PopulateChildren(RootTreeNode rootNode, FolderTreeNode parentnode, List<string> checkedFileNodes)
        {
            string[] directories = Directory.GetDirectories(parentnode.Path);
            foreach (var directory in directories)
            {
                FolderTreeNode folderTreeNode = new FolderTreeNode(directory);
                parentnode.AddChild(folderTreeNode);
                rootNode.AllTreeNodes.Add(folderTreeNode);
                PopulateChildren(rootNode, folderTreeNode, checkedFileNodes);
            }

            string[] files = Directory.GetFiles(parentnode.Path);
            foreach (var file in files)
            {
                FileTreeNode fileTreeNode = new FileTreeNode(file);
                if (checkedFileNodes.Contains(file)) { fileTreeNode.IsChecked = true; }
                parentnode.AddChild(fileTreeNode);
                rootNode.AllTreeNodes.Add(fileTreeNode);
            }
        }
    }
}
