using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SYC_AssetSystem.Models;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SYC_AssetSystem.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        private string _searchText = string.Empty;
        private string _searchPath = Environment.CurrentDirectory;
        private ObservableCollection<string> _searchResults = new();
        private bool _isSearching = false;
        private FolderTreeNode _rootNode;
        private const string DefaultAssetPath = @"R:\R_20260111_SYCMetaHuman\Data\Ass";

        public string Greeting { get; } = "Welcome to SYC Asset System!";

        public FolderTreeNode RootNode
        {
            get => _rootNode;
            set => SetProperty(ref _rootNode, value);
        }

        public string SearchText
        {
            get => _searchText;
            set => SetProperty(ref _searchText, value);
        }

        public string SearchPath
        {
            get => _searchPath;
            set => SetProperty(ref _searchPath, value);
        }

        public ObservableCollection<string> SearchResults
        {
            get => _searchResults;
            set => SetProperty(ref _searchResults, value);
        }

        public bool IsSearching
        {
            get => _isSearching;
            set => SetProperty(ref _isSearching, value);
        }

        public MainWindowViewModel()
        {
               // 先初始化根节点，确保不为null
            _rootNode = new FolderTreeNode
            {
                Name = "加载中...",
                FullPath = DefaultAssetPath,
                IsDirectory = true,
                IsExpanded = false
            };

            // 初始化文件夹树
            LoadFolderTree(DefaultAssetPath);
        }

        [RelayCommand]
        private async Task SearchAsync()
        {
            if (string.IsNullOrWhiteSpace(SearchText) || string.IsNullOrWhiteSpace(SearchPath))
            {
                return;
            }

            IsSearching = true;
            SearchResults.Clear();

            try
            {
                // 检查路径是否存在
                if (!Directory.Exists(SearchPath))
                {
                    SearchResults.Add($"错误：路径 \"{SearchPath}\" 不存在"); 
                    return;
                }

                // 在后台线程执行搜索
                var results = await Task.Run(() =>
                {
                    var foundFiles = new System.Collections.Generic.List<string>();
                    try
                    {
                        // 递归搜索包含搜索文本的文件
                        var files = Directory.GetFiles(SearchPath, "*.*", SearchOption.AllDirectories)
                            .Where(f => Path.GetFileName(f).IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0)
                            .Take(100); // 限制结果数量

                        foreach (var file in files)
                        {
                            foundFiles.Add(file);
                        }

                        // 搜索包含搜索文本的目录
                        var directories = Directory.GetDirectories(SearchPath, "*.*", SearchOption.AllDirectories)
                            .Where(d => Path.GetFileName(d).IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0)
                            .Take(100); // 限制结果数量

                        foreach (var directory in directories)
                        {
                            foundFiles.Add($"[目录] {directory}");
                        }
                    }
                    catch (UnauthorizedAccessException)
                    {
                        foundFiles.Add("错误：访问被拒绝，请检查权限");
                    }
                    catch (Exception ex)
                    {
                        foundFiles.Add($"错误：{ex.Message}");
                    }

                    return foundFiles;
                });

                // 更新UI
                foreach (var result in results)
                {
                    SearchResults.Add(result);
                }

                if (SearchResults.Count == 0)
                {
                    SearchResults.Add($"未找到包含 \"{SearchText}\" 的文件或目录");
                }
            }
            finally
            {
                IsSearching = false;
            }
        }

        [RelayCommand]
        private void ExpandNode(FolderTreeNode node)
        {
            if (node != null && node.IsDirectory)
            {
                node.IsExpanded = !node.IsExpanded;
                if (node.IsExpanded && node.Children.Count == 0)
                {
                    node.LoadChildren();
                }
            }
        }

        private void LoadFolderTree(string path)
        {
            try
            {
                if (Directory.Exists(path))
                {
                    RootNode = FolderTreeNode.LoadDirectoryTree(path);
                }
                else
                {
                    RootNode = new FolderTreeNode
                    {
                        Name = "文件夹不存在",
                        FullPath = path,
                        IsDirectory = true,
                        IsExpanded = true
                    };
                }
            }
            catch (Exception ex)
            {
                RootNode = new FolderTreeNode
                {
                    Name = $"加载出错: {ex.Message}",
                    FullPath = path,
                    IsDirectory = true,
                    IsExpanded = true
                };
            }
        }
    }
}