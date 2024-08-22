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
using Avalonia.Media.Imaging;
using static System.Net.Mime.MediaTypeNames;
using System.Collections.ObjectModel;
using System.Collections;

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
                VariablesUtils.VariablesUtils.biMap = new BiMap<TreeViewItem, TreeViewItem>();
                textblock.Text = "未选择兼容报告";
            }
            else
            {
                leftTree = TreeUtils.AddTreeNode(leftTree, path,".txt");
                textblock.Text = path;
            }
        }

        private void rightButton_Click(object sender, RoutedEventArgs e)
        {
            string path = FileUtils.selectFolder("选择原始文件");
            if (path == null)
            {
                System.Windows.Forms.MessageBox.Show("未选择原始文件");
                VariablesUtils.VariablesUtils.biMap = new BiMap<TreeViewItem, TreeViewItem>();
                textblock.Text = "未选择原始文件";
            }
            else
            {
                rightTree = TreeUtils.AddTreeNode(rightTree, path, ".dat");
                textblock.Text = path;
            }
        }

        private void comparisonButton_Click(object sender, RoutedEventArgs e)
        {
           if(leftTree.Items.Count <= 0)
            {
                System.Windows.Forms.MessageBox.Show("未选择兼容报告");
                return;
            }
           if(rightTree.Items.Count <= 0)
            {
                System.Windows.Forms.MessageBox.Show("未选择原始文件");
                return;
            }
            TreeUtils.TreeItemMatching(leftTree, rightTree);
            this.transFolderNameColor();


            textblock.Text = "就绪";
        }

        private void leftTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue  != null  && TreeUtils.GetTreeViewItemLevel(e.NewValue as TreeViewItem) > 0)
            {

                BiMap<TreeViewItem, TreeViewItem> biMap = VariablesUtils.VariablesUtils.biMap;
                if (biMap.Count() > 0)
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
                    if (rightItem != null )
                    {
                        VariablesUtils.VariablesUtils.isFirstSelect ^= false;
                        if (!VariablesUtils.VariablesUtils.isFirstSelect)
                        {
                            rightItem.IsSelected = true;
                        }
                    }
                }
            }
        }
        private void rightTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if(e.NewValue != null && TreeUtils.GetTreeViewItemLevel(e.NewValue as TreeViewItem) > 0)
            {
                BiMap<TreeViewItem, TreeViewItem> biMap = VariablesUtils.VariablesUtils.biMap;
                if (biMap.Count() > 0)
                {
                    TreeViewItem rightItem = e.NewValue as TreeViewItem;
                    TreeViewItem leftItem = new TreeViewItem();
                    if (!Directory.Exists(rightItem.Tag + ""))
                    {
                        if (biMap.Reverse.ContainsKey(rightItem))
                        {
                            TreeViewItem parentItem = rightItem.Parent as TreeViewItem;
                            leftItem = biMap.Reverse[parentItem];
                        }
                        else
                        {
                            diffView.OldText = null;
                        }
                    }
                    else
                    {
                        if (biMap.Reverse.ContainsKey(rightItem))
                        {
                            leftItem = biMap.Reverse[rightItem];
                        }
                        else
                        {
                            diffView.OldText = null;
                        }
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
        public void transFolderNameColor()
        {
            Collection<TreeViewItem> collection = new Collection<TreeViewItem>();
            //取所有二级
            ItemCollection items1 = TreeUtils.getItem(leftTree);
            if (items1?.Count >0)
            {
                foreach (TreeViewItem item in items1)
                {
                    collection.Add(item);
                }
            }
            ItemCollection items2 = TreeUtils.getItem(rightTree);
            if (items2?.Count > 0)
            {
                foreach (TreeViewItem item in items2)
                {
                    collection.Add(item);
                }
            }
            if (collection?.Count <= 0)
            {
                return;
            }
            for (int i = collection.Count - 1; i >= 0; i--)
            {
                if (collection[i] == null) { continue; }
                TreeViewItem item = collection[i];
                if(item != null)
                {
                    //如果在双向绑定集合中出现，说明有匹配项
                    if (VariablesUtils.VariablesUtils.biMap.Forward.ContainsKey(item))
                    {
                        TreeViewItem rightItem = VariablesUtils.VariablesUtils.biMap.Forward[item];
                        string leftText = ProcessText(
                        RemoveLInesAsKeyword(RemoveLinesAfterKeyword(
                            File.ReadAllText(item.Tag + "")
                            , "Start Tag")
                        , "Reader EDID(Hex):"))
                        .TrimEnd();
                        string rightText = "";
                        if (rightItem != null)
                        {
                            string rightPath = rightItem.Tag + "";
                            if (rightPath != null && !Directory.Exists(rightPath))
                            {
                                rightText = ProcessText(File.ReadAllText(rightPath)).TrimEnd();
                            }
                            else if (System.IO.Path.GetExtension(((rightItem.Items[0] as TreeViewItem).Header as string)).Equals(".dat"))
                            {
                                rightText = ProcessText(File.ReadAllText(rightPath + "\\" + ((rightItem.Items[0] as TreeViewItem).Header as string))).TrimEnd();
                            }
                            int leftLines = CountLines(leftText);
                            int rightLines = CountLines(rightText);
                            if (leftLines != rightLines) {
                                item.Background = Brushes.LightBlue;
                                rightItem.Background = Brushes.LightBlue;
                                collection.Remove(item);
                                collection.Remove(rightItem);
                                break;
                            }
                            Differ differ = new Differ();
                            DiffResult diffresult = differ.CreateCharacterDiffs(leftText, rightText,true);
                            if(diffresult?.DiffBlocks?.Count > 0)
                            {
                                item.Background = new SolidColorBrush(Color.FromArgb(64, 216, 32, 32)); 
                                rightItem.Background = new SolidColorBrush(Color.FromArgb(64,216, 32, 32)); 
                                collection.Remove(item);
                                collection.Remove(rightItem);
                            }
                        }
                    }
                    else if (VariablesUtils.VariablesUtils.biMap.Reverse.ContainsKey(item))
                    {
                        TreeViewItem leftItem = VariablesUtils.VariablesUtils.biMap.Reverse[item];
                        string leftText = null;
                        if (leftItem != null)
                        {
                            leftText = ProcessText(
                            RemoveLInesAsKeyword(RemoveLinesAfterKeyword(
                                File.ReadAllText(leftItem.Tag + "")
                                , "Start Tag")
                            , "Reader EDID(Hex):"))
                            .TrimEnd();
                        }
                        string rightText = "";
                        if (item != null)
                        {
                            string rightPath = item.Tag + "";
                            if (rightPath != null && !Directory.Exists(rightPath))
                            {
                                rightText = ProcessText(File.ReadAllText(rightPath)).TrimEnd();
                            }
                            else if (System.IO.Path.GetExtension(((item.Items[0] as TreeViewItem).Header as string)).Equals(".dat"))
                            {
                                rightText = ProcessText(File.ReadAllText(rightPath + "\\" + ((item.Items[0] as TreeViewItem).Header as string))).TrimEnd();
                            }
                            int leftLines = CountLines(leftText);
                            int rightLines = CountLines(rightText);
                            if (leftLines != rightLines)
                            {
                                item.Background = Brushes.LightBlue;
                                item.Background = Brushes.LightBlue;
                                collection.Remove(leftItem);
                                collection.Remove(item);
                                break;
                            }
                            Differ differ = new Differ();
                            DiffResult diffresult = differ.CreateCharacterDiffs(leftText, rightText, true);
                            if (diffresult?.DiffBlocks?.Count > 0)
                            {
                                leftItem.Background = new SolidColorBrush(Color.FromArgb(64, 216, 32, 32));
                                item.Background = new SolidColorBrush(Color.FromArgb(64, 216, 32, 32));
                                collection.Remove(leftItem);
                                collection.Remove(item);
                            }
                        }
                    }
                    else
                    {
                        item.Background = Brushes.LightYellow;
                        collection.Remove(item);
                    }
                }
            }
        }
        static int CountLines(string text)
        {
            // 如果字符串为空或为 null，返回 0
            if (string.IsNullOrEmpty(text))
                return 0;

            // 使用 '\r' 和 '\n' 作为分隔符，去除空行
            string[] lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            // 返回行数
            return lines.Length;
        }
    }
}
