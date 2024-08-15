using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DiffPlex.DiffBuilder.Model;
using DiffPlex.Model;
using DiffPlex;
using Fluent;
using BidirectionalMap;

namespace EDID_Comparison_Tool_For_WPF
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow 
    {
        public MainWindow()
        {
            this.InitializeComponent();
        }
        
        private void leftButton_Click(object sender, RoutedEventArgs e)
        {
            string path = FileUtils.selectFolder("选择兼容报告");
            if (path == null)
            {
                System.Windows.Forms.MessageBox.Show("未选择兼容报告");
            }
            else
            {
                leftTree = TreeUtils.AddTreeNode(leftTree, path,".txt");
            }
        }

        private void rightButton_Click(object sender, RoutedEventArgs e)
        {
            string path = FileUtils.selectFolder("选择原始文件");
            if (path == null)
            {
                System.Windows.Forms.MessageBox.Show("选择原始文件");
            }
            else
            {
                rightTree = TreeUtils.AddTreeNode(rightTree, path, ".dat");
            }
        }

        private void comparisonButton_Click(object sender, RoutedEventArgs e)
        {
            TreeUtils.TreeItemMatching(leftTree, rightTree);
        }

        private void leftTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            BiMap<TreeViewItem, TreeViewItem> biMap = VariablesUtils.VariablesUtils.biMap;
            diffView.SetHeaderAsOldToNew();
            if(biMap.Count() > 0)
            {
                TreeViewItem leftItem = e.NewValue as TreeViewItem;
                TreeViewItem rightItem = biMap.Forward[leftItem];

                diffView.OldText =
                    ProcessText(
                    RemoveLInesAsKeyword(RemoveLinesAfterKeyword(
                        File.ReadAllText(leftItem.Tag + "")
                        , "Start Tag")
                    , "Reader EDID(Hex):"))
                    .TrimEnd();

                diffView.OldTextHeader = leftItem.Header + "";
                if (rightItem != null)
                {
                    VariablesUtils.VariablesUtils.isFirstSelect ^= false;
                    if (!VariablesUtils.VariablesUtils.isFirstSelect)
                    {
                        rightItem.IsSelected = true;
                    }
                }
            }
        }

        private void rightTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            BiMap<TreeViewItem, TreeViewItem> biMap = VariablesUtils.VariablesUtils.biMap;
            if(biMap.Count() > 0)
            {
                TreeViewItem rightItem = e.NewValue as TreeViewItem;
                TreeViewItem leftItem = new TreeViewItem();
                if (!Directory.Exists(rightItem.Tag + ""))
                {
                    TreeViewItem parentItem = rightItem.Parent as TreeViewItem;
                    leftItem = biMap.Reverse[parentItem];
                }
                else
                {
                    leftItem = biMap.Reverse[rightItem];
                }
                string rightPath = rightItem.Tag + "";
                if (rightPath != null && !Directory.Exists(rightPath))
                {
                    diffView.NewTextHeader = rightItem.Header + "";
                    diffView.NewText = ProcessText(File.ReadAllText(rightPath)).TrimEnd();
                }
                else if (System.IO.Path.GetExtension(((rightItem.Items[0] as TreeViewItem).Header as string)).Equals(".dat"))
                {
                    diffView.NewTextHeader = rightItem.Header + "";
                    diffView.NewText = ProcessText(File.ReadAllText(rightPath + "\\" + ((rightItem.Items[0] as TreeViewItem).Header as string))).TrimEnd();
                }
                if (leftItem != null)
                {
                    VariablesUtils.VariablesUtils.isFirstSelect ^= false;
                    if (!VariablesUtils.VariablesUtils.isFirstSelect)
                    {
                        leftItem.IsSelected = true;
                    }
                }
            }
        }
        private static string ProcessText(string input)
        {
            // 按行分割输入文本
            var lines = input.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            // 使用 StringBuilder 来构建输出
            StringBuilder stringBuilder = new StringBuilder();

            for (int i = 0; i < lines.Length; i++)
            {
                string currentLine = lines[i].Trim();

                // 添加当前行
                stringBuilder.AppendLine(currentLine);

                // 每隔一行插入一行
                if (i < lines.Length-1 )
                {
                    stringBuilder.AppendLine(); // 插入空行
                }
            }

            return stringBuilder.ToString();
        }
        private static string GetFirstXChars(string sb, int x)
        {
            // 将 StringBuilder 转换为字符串
            string str = sb.ToString();

            // 确保 x 不超出字符串长度
            if (x > str.Length)
            {
                x = str.Length;
            }

            return str.Substring(0, x);
        }
        static string RemoveLinesAfterKeyword(string input, string keyword)
        {
            // 将输入文本按行分割
            var lines = input.Split(new[] { '\r', '\n' }, StringSplitOptions.None);

            // 使用 StringBuilder 来构建输出
            StringBuilder stringBuilder = new StringBuilder();

            foreach (var line in lines)
            {
                // 检查当前行是否包含关键字
                if (line.Contains(keyword))
                {
                    break; // 一旦找到关键字，停止处理
                }

                // 如果没有找到关键字，添加当前行到 StringBuilder
                stringBuilder.AppendLine(line);
            }

            return stringBuilder.ToString();
        }
        static string RemoveLInesAsKeyword(string input, string keyword) {
            // 将输入文本按行分割
            var lines = input.Split(new[] { '\r', '\n' }, StringSplitOptions.None);

            // 使用 StringBuilder 来构建输出
            StringBuilder stringBuilder = new StringBuilder();

            foreach (var line in lines)
            {
                // 检查当前行是否包含关键字
                if (!line.Contains(keyword))
                {
                    // 如果没有找到关键字，添加当前行到 StringBuilder
                    stringBuilder.AppendLine(line);
                }

            }

            return stringBuilder.ToString();
        }
    }
}
