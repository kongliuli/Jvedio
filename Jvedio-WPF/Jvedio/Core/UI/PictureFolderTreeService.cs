using Jvedio.Entity.Data;
using SuperUtils.Framework.ORM.Wrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using static Jvedio.App;
using static Jvedio.MapperManager;

namespace Jvedio.Core.UI
{
    public sealed class PictureFolderTreeItem
    {
        public PictureFolderTreeItem(string fullPath, string name)
        {
            FullPath = fullPath;
            Name = name;
        }

        public string FullPath { get; }
        public string Name { get; }
        public List<PictureFolderTreeItem> Children { get; } = new List<PictureFolderTreeItem>();
    }

    public static class PictureFolderTreeService
    {
        public static List<PictureFolderNode> LoadNodes(long dbId)
        {
            var wrapper = new SelectWrapper<PictureFolderNode>();
            wrapper.Eq("DBId", dbId).Asc("Depth").Asc("Name");
            return pictureFolderNodeMapper.SelectList(wrapper) ?? new List<PictureFolderNode>();
        }

        public static List<PictureFolderTreeItem> BuildForest(IEnumerable<PictureFolderNode> nodes)
        {
            var list = nodes?.ToList() ?? new List<PictureFolderNode>();
            if (list.Count == 0)
                return new List<PictureFolderTreeItem>();

            var byPath = new Dictionary<string, PictureFolderTreeItem>(StringComparer.OrdinalIgnoreCase);
            foreach (PictureFolderNode node in list) {
                if (string.IsNullOrWhiteSpace(node.FullPath))
                    continue;
                string name = string.IsNullOrWhiteSpace(node.Name)
                    ? System.IO.Path.GetFileName(node.FullPath)
                    : node.Name;
                byPath[node.FullPath] = new PictureFolderTreeItem(node.FullPath, name);
            }

            var roots = new List<PictureFolderTreeItem>();
            foreach (PictureFolderNode node in list) {
                if (!byPath.TryGetValue(node.FullPath, out PictureFolderTreeItem item))
                    continue;

                string parentPath = node.ParentPath;
                if (string.IsNullOrWhiteSpace(parentPath) ||
                    !byPath.TryGetValue(parentPath, out PictureFolderTreeItem parent)) {
                    roots.Add(item);
                    continue;
                }
                parent.Children.Add(item);
            }

            SortTree(roots);
            return roots;
        }

        public static void PopulateTreeView(TreeView treeView, IEnumerable<PictureFolderTreeItem> roots)
        {
            if (treeView == null)
                return;
            treeView.Items.Clear();
            foreach (PictureFolderTreeItem root in roots ?? Enumerable.Empty<PictureFolderTreeItem>())
                treeView.Items.Add(CreateTreeViewItem(root));
        }

        private static TreeViewItem CreateTreeViewItem(PictureFolderTreeItem node)
        {
            var item = new TreeViewItem {
                Header = node.Name,
                Tag = node.FullPath,
                IsExpanded = node.Children.Count > 0 && node.Children.Count <= 8,
            };
            foreach (PictureFolderTreeItem child in node.Children)
                item.Items.Add(CreateTreeViewItem(child));
            return item;
        }

        private static void SortTree(List<PictureFolderTreeItem> nodes)
        {
            nodes.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase));
            foreach (PictureFolderTreeItem node in nodes)
                SortTree(node.Children);
        }
    }
}
