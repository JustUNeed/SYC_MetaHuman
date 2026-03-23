
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;

namespace SYC_AssetSystem.Models
{
    public class FolderTreeNode : INotifyPropertyChanged
    {
        public string Name { get; set; } = string.Empty;
        public string FullPath { get; set; } = string.Empty;
        public bool IsDirectory { get; set; }
        public ObservableCollection<FolderTreeNode> Children { get; set; } = new();

        private bool _isExpanded = false;
        public bool IsExpanded 
        { 
            get => _isExpanded;
            set 
            {
                if (_isExpanded != value)
                {
                    _isExpanded = value;
                    OnPropertyChanged();

                    // 如果展开且没有子节点，则加载子节点
                    if (_isExpanded && Children.Count == 0)
                    {
                        LoadChildren();
                    }
                }
            }
        }

        public static FolderTreeNode LoadDirectoryTree(string rootPath)
        {
            var rootNode = new FolderTreeNode
            {
                Name = Path.GetFileName(rootPath),
                FullPath = rootPath,
                IsDirectory = true,
                IsExpanded = true
            };

            try
            {
                // 加载子目录
                var directories = Directory.GetDirectories(rootPath);
                foreach (var directory in directories)
                {
                    var dirNode = new FolderTreeNode
                    {
                        Name = Path.GetFileName(directory),
                        FullPath = directory,
                        IsDirectory = true
                    };
                    rootNode.Children.Add(dirNode);
                }

                // 加载文件
                var files = Directory.GetFiles(rootPath);
                foreach (var file in files)
                {
                    var fileNode = new FolderTreeNode
                    {
                        Name = Path.GetFileName(file),
                        FullPath = file,
                        IsDirectory = false
                    };
                    rootNode.Children.Add(fileNode);
                }
            }
            catch (UnauthorizedAccessException)
            {
                // 忽略无访问权限的文件夹
            }

            return rootNode;
        }

        public void LoadChildren()
        {
            if (!IsDirectory || Children.Count > 0)
                return;

            try
            {
                // 加载子目录
                var directories = Directory.GetDirectories(FullPath);
                foreach (var directory in directories)
                {
                    var dirNode = new FolderTreeNode
                    {
                        Name = Path.GetFileName(directory),
                        FullPath = directory,
                        IsDirectory = true
                    };
                    Children.Add(dirNode);
                }

                // 加载文件
                var files = Directory.GetFiles(FullPath);
                foreach (var file in files)
                {
                    var fileNode = new FolderTreeNode
                    {
                        Name = Path.GetFileName(file),
                        FullPath = file,
                        IsDirectory = false
                    };
                    Children.Add(fileNode);
                }
            }
            catch (UnauthorizedAccessException)
            {
                // 忽略无访问权限的文件夹
            }
        }

        public event PropertyChangedEventHandler PropertyChanged = null; 

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
