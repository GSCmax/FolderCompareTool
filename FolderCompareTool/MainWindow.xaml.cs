﻿using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace FolderCompareTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        FileInfo[]? files1;
        FileInfo[]? files2;
        List<Tuple<FileInfo, FileInfo>>? matchedPairs;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Folder1SelBT_Click(object sender, RoutedEventArgs e)
        {
            OpenFolderDialog openFolderDialog = new();
            if (openFolderDialog.ShowDialog() == true)
                Folder1TB.Text = openFolderDialog.FolderName;
        }

        private void Folder2SelBT_Click(object sender, RoutedEventArgs e)
        {
            OpenFolderDialog openFolderDialog = new();
            if (openFolderDialog.ShowDialog() == true)
                Folder2TB.Text = openFolderDialog.FolderName;
        }

        private void CompareBT_Click(object sender, RoutedEventArgs e)
        {
            FileLB.Items.Clear();

            try
            {
                files1 = new DirectoryInfo(Folder1TB.Text).GetFiles("*.*", SearchOption.TopDirectoryOnly);
                files2 = new DirectoryInfo(Folder2TB.Text).GetFiles("*.*", SearchOption.TopDirectoryOnly);
            }
            catch
            {
                MessageBox.Show("文件夹路径错误", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (GlobalDataHelper.appConfig!.FileNameTolerance == 0)
            {
                matchedPairs = new(files1.Join(
                    files2,
                    f1 => f1.Name,
                    f2 => f2.Name,
                    (f1, f2) => new Tuple<FileInfo, FileInfo>(f1, f2)
                ));
            }
            else
            {
                matchedPairs = [];

                Parallel.ForEach(files1, file1 =>
                {
                    foreach (FileInfo file2 in files2)
                    {
                        string name1 = Path.GetFileName(file1.Name);
                        string name2 = Path.GetFileName(file2.Name);

                        // 提前过滤长度差异过大的文件名
                        if (Math.Abs(name1.Length - name2.Length) > GlobalDataHelper.appConfig!.FileNameTolerance) continue;

                        int distance = LevenshteinDistance(name1, name2);
                        if (distance <= GlobalDataHelper.appConfig!.FileNameTolerance)
                        {
                            lock (matchedPairs)
                            {
                                matchedPairs.Add(Tuple.Create(file1, file2));
                            }
                        }
                    }
                });
            }

            if (matchedPairs.Count() != 0)
            {
                if (GlobalDataHelper.appConfig!.CompareMode == 1)
                {
                    foreach (var file in matchedPairs)
                    {
                        // 提前判断文件大小，若不一致则哈希值必不相等
                        if (file.Item1.Length == file.Item2.Length)
                        {
                            if (GetFileMD5(file.Item1.FullName) == GetFileMD5(file.Item2.FullName))
                            {
                                FileLB.Items.Add(new SameNameFile
                                {
                                    FileName1 = file.Item1.Name,
                                    FileName2 = file.Item2.Name,
                                    FilePath1 = file.Item1.FullName,
                                    FilePath2 = file.Item2.FullName,
                                    FileSize1 = GlobalDataHelper.FormatFileSize(file.Item1.Length),
                                    FileSize2 = GlobalDataHelper.FormatFileSize(file.Item2.Length),
                                });
                            }
                        }
                    }
                }
                else
                {
                    foreach (var file in matchedPairs)
                    {
                        FileLB.Items.Add(new SameNameFile
                        {
                            FileName1 = file.Item1.Name,
                            FileName2 = file.Item2.Name,
                            FilePath1 = file.Item1.FullName,
                            FilePath2 = file.Item2.FullName,
                            FileSize1 = GlobalDataHelper.FormatFileSize(file.Item1.Length),
                            FileSize2 = GlobalDataHelper.FormatFileSize(file.Item2.Length),
                        });
                    }
                }
            }
        }

        private void FileLBItem_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SameNameFile snf = ((sender as ListBoxItem)!.Content as SameNameFile)!;
            Process.Start("explorer.exe", $"/select, \"{snf.FilePath1}\"");
            Process.Start("explorer.exe", $"/select, \"{snf.FilePath2}\"");
        }

        /// <summary>
        /// 计算文件的SHA256哈希值
        /// </summary>
        /// <param name="fullName"></param>
        /// <returns></returns>
        private string GetFileHash(string fullName)
        {
            using (FileStream fileStream = new(fullName, FileMode.Open))
            {
                using (SHA256 sha256 = SHA256.Create())
                {
                    byte[] hash = sha256.ComputeHash(fileStream);
                    StringBuilder stringBuilder = new();
                    for (int i = 0; i < hash.Length; i++)
                    {
                        stringBuilder.Append(hash[i].ToString("X2"));
                    }
                    return stringBuilder.ToString();
                }
            }
        }

        /// <summary>
        /// 计算文件的MD5哈希值
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private string GetFileMD5(string filePath)
        {
            using (FileStream fileStream = new(filePath, FileMode.Open))
            {
                using (MD5 md5 = MD5.Create())
                {
                    byte[] hash = md5.ComputeHash(fileStream);
                    return BitConverter.ToString(hash).Replace("-", "");
                }
            }
        }

        /// <summary>
        /// 计算两个字符串的Levenshtein距离
        /// </summary>
        /// <param name="s1"></param>
        /// <param name="s2"></param>
        /// <returns></returns>
        private int LevenshteinDistance(string s1, string s2)
        {
            if (s1 == s2) return 0;

            int m = s1.Length;
            int n = s2.Length;
            int[,] matrix = new int[m + 1, n + 1];

            for (int i = 0; i <= m; i++) matrix[i, 0] = i;
            for (int j = 0; j <= n; j++) matrix[0, j] = j;

            for (int i = 1; i <= m; i++)
            {
                for (int j = 1; j <= n; j++)
                {
                    int cost = (s1[i - 1] == s2[j - 1]) ? 0 : 1;
                    matrix[i, j] = Math.Min(Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1), matrix[i - 1, j - 1] + cost);
                }
            }

            return matrix[m, n];
        }
    }
}