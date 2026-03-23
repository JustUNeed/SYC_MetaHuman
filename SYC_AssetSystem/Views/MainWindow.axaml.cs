using Avalonia.Controls;
using Avalonia.Interactivity;
using SYC_AssetSystem.Models;

namespace SYC_AssetSystem.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OnTreeViewItemExpanded(object sender, RoutedEventArgs e)
        {
            if (sender is TreeViewItem treeViewItem && treeViewItem.DataContext is FolderTreeNode node)
            {
                if (node.IsExpanded && node.Children.Count == 0)
                {
                    node.LoadChildren();
                }
            }
        }
    }
}