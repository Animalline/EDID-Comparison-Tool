using BidirectionalMap;
using DiffPlex;
using DiffPlex.Model;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using EDID_Comparison_Tool_For_WPF.Entities;
using EDID_Comparison_Tool_For_WPF.ConstantUtils;
using Newtonsoft.Json;
using System.Windows.Forms;
using System.Diagnostics;

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
                leftTree = TreeUtils.AddTreeNode(leftTree, path, ".txt");
                reSetV();
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
                reSetV();
                textblock.Text = path;
            }
        }

        private void comparisonButton_Click(object sender, RoutedEventArgs e)
        {
            if (leftTree.Items.Count <= 0)
            {
                System.Windows.Forms.MessageBox.Show("未选择兼容报告");
                return;
            }
            if (rightTree.Items.Count <= 0)
            {
                System.Windows.Forms.MessageBox.Show("未选择原始文件");
                return;
            }
            VariablesUtils.VariablesUtils.isCompare = true;
            TreeUtils.TreeItemMatching(leftTree, rightTree);
            this.transFolderNameColor();


            textblock.Text = "就绪";
        }

        private void leftTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue != null && TreeUtils.GetTreeViewItemLevel(e.NewValue as TreeViewItem) > 0)
            {

                BiMap<TreeViewItem, TreeViewItem> biMap = VariablesUtils.VariablesUtils.biMap;
                if (biMap.Count() > 0)
                {
                    TreeViewItem leftItem = e.NewValue as TreeViewItem;
                    //TreeViewItem rightItem = biMap.Forward[leftItem];
                    TreeViewItem rightItem = new TreeViewItem();
                    if (biMap.Forward.ContainsKey(leftItem))
                    {
                        diffView.DeletedBackground = new SolidColorBrush(Color.FromArgb(64, 216, 32, 32));
                        diffView.DeletedForeground = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
                        rightItem = biMap.Forward[leftItem];
                    }
                    else
                    {
                        diffView.NewText = "";
                        diffView.NewTextHeader = null;
                        diffView.DeletedBackground = Brushes.White;
                        diffView.DeletedForeground = Brushes.Black;
                        TreeViewItem rightSelectedItem = ((TreeViewItem)rightTree.SelectedItem);
                        if (rightSelectedItem != null)
                        {
                            rightSelectedItem.IsSelected = false;
                        }
                    }
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
                else if(VariablesUtils.VariablesUtils.isCompare == false)
                {
                    TreeViewItem leftItem = e.NewValue as TreeViewItem;
                    diffView.OldText =
                        ProcessText(
                        RemoveLInesAsKeyword(RemoveLinesAfterKeyword(
                            File.ReadAllText(leftItem.Tag + "")
                            , "Start Tag")
                        , "Reader EDID(Hex):"))
                        .TrimEnd();

                    diffView.OldTextHeader = leftItem.Header + "";
                }
            }
        }
        private void rightTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue != null && TreeUtils.GetTreeViewItemLevel(e.NewValue as TreeViewItem) > 0)
            {
                BiMap<TreeViewItem, TreeViewItem> biMap = VariablesUtils.VariablesUtils.biMap;
                if (biMap.Count() > 0)
                {
                    TreeViewItem rightItem = e.NewValue as TreeViewItem;
                    TreeViewItem leftItem = new TreeViewItem();
                    if (!Directory.Exists(rightItem.Tag + ""))
                    {
                        TreeViewItem parentItem = rightItem.Parent as TreeViewItem;
                        if (biMap.Reverse.ContainsKey(parentItem))
                        {
                            diffView.InsertedBackground = new SolidColorBrush(Color.FromArgb(64, 216, 32, 32));
                            diffView.InsertedForeground = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
                            leftItem = biMap.Reverse[parentItem];
                        }
                        else
                        {
                            diffView.OldText = "";
                            diffView.OldTextHeader = null;
                            diffView.InsertedBackground = Brushes.White;
                            diffView.InsertedForeground = Brushes.Black;
                            TreeViewItem leftSelectedItem = ((TreeViewItem)leftTree.SelectedItem);
                            if (leftSelectedItem != null)
                            {
                                leftSelectedItem.IsSelected = false;
                            }
                        }
                    }
                    else
                    {
                        if (biMap.Reverse.ContainsKey(rightItem))
                        {
                            diffView.InsertedBackground = new SolidColorBrush(Color.FromArgb(64, 216, 32, 32));
                            diffView.InsertedForeground = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
                            leftItem = biMap.Reverse[rightItem];
                        }
                        else
                        {
                            diffView.OldText = "";
                            diffView.OldTextHeader = null;
                            diffView.InsertedBackground = Brushes.White;
                            diffView.InsertedForeground = Brushes.Black;
                            TreeViewItem leftSelectedItem = ((TreeViewItem)leftTree.SelectedItem);
                            if (leftSelectedItem != null)
                            {
                                leftSelectedItem.IsSelected = false;
                            }
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
                else if(VariablesUtils.VariablesUtils.isCompare == false)
                {
                    TreeViewItem rightItem = e.NewValue as TreeViewItem;
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
                if (i < lines.Length - 1)
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
        static string RemoveLInesAsKeyword(string input, string keyword)
        {
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

            ItemCollection items2 = TreeUtils.getItem(rightTree);

            if (items1?.Count > 0)
            {
                foreach (TreeViewItem item in items1)
                {
                    collection.Add(item);
                }
            }
            if (items2?.Count > 0)
            {
                foreach (TreeViewItem item in items2)
                {
                    collection.Add(item);
                }
            }
            TreeUtils.compareTree(leftTree, rightTree);
            if (collection?.Count <= 0)
            {
                return;
            }
            for (int i = collection.Count - 1; i >= 0; i--)
            {
                if (collection[i] == null) { continue; }
                TreeViewItem item = collection[i];
                if (item != null)
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
                            if (leftLines != rightLines)
                            {
                                item.Background = Brushes.LightBlue;
                                rightItem.Background = Brushes.LightBlue;
                                collection.Remove(item);
                                collection.Remove(rightItem);
                                i--;
                                continue;
                            }
                            Differ differ = new Differ();
                            DiffResult diffresult = differ.CreateCharacterDiffs(leftText, rightText, true);
                            if (diffresult?.DiffBlocks?.Count > 0)
                            {
                                item.Background = new SolidColorBrush(Color.FromArgb(64, 216, 32, 32));
                                rightItem.Background = new SolidColorBrush(Color.FromArgb(64, 216, 32, 32));
                                collection.Remove(item);
                                collection.Remove(rightItem);
                                i--;
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
                                leftItem.Background = Brushes.LightBlue;
                                item.Background = Brushes.LightBlue;
                                collection.Remove(leftItem);
                                collection.Remove(item);
                                i--;
                                continue;
                            }
                            Differ differ = new Differ();
                            DiffResult diffresult = differ.CreateCharacterDiffs(leftText, rightText, true);
                            if (diffresult?.DiffBlocks?.Count > 0)
                            {
                                leftItem.Background = new SolidColorBrush(Color.FromArgb(64, 216, 32, 32));
                                item.Background = new SolidColorBrush(Color.FromArgb(64, 216, 32, 32));
                                collection.Remove(leftItem);
                                collection.Remove(item);
                                i--;
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

        private void cancelComparisonButton_Click(object sender, RoutedEventArgs e)
        {
            reSetV();
        }
        private void reSetV()
        {
            VariablesUtils.VariablesUtils.isCompare = false;
            VariablesUtils.VariablesUtils.biMap = new BiMap<TreeViewItem, TreeViewItem>();
        }

        private void openCompare_Click(object sender, RoutedEventArgs e)
        {
            TreeViewItem leftItem = leftTree.SelectedItem as TreeViewItem;
            TreeViewItem rightItem = rightTree.SelectedItem as TreeViewItem;
            if (leftItem != null && rightItem != null)
            {
                if(leftItem.Header!=null && !leftItem.Header.Equals("") && rightItem.Header != null && !rightItem.Header.Equals(""))
                {
                    string leftPath = leftItem.Tag + "";
                    string rightPath = rightItem.Tag + "";
                    if (rightPath != null && !Directory.Exists(rightPath))
                    {
                    }
                    else if (System.IO.Path.GetExtension(((rightItem.Items[0] as TreeViewItem).Header as string)).Equals(".dat"))
                    {
                        rightPath = rightPath + "\\" + ((rightItem.Items[0] as TreeViewItem).Header as string);
                    }
                    //调用打开beyond compare
                    Config config = null;

                    // 检查配置文件是否存在
                    if (File.Exists(ConstantUtils.ConstantUtils.configPath))
                    {
                        // 读取配置文件
                        string json = File.ReadAllText(ConstantUtils.ConstantUtils.configPath);
                        config = JsonConvert.DeserializeObject<Config>(json);
                    }
                    else
                    {
                        // 如果文件不存在，则创建一个新的配置
                        config = new Config();
                        File.WriteAllText(ConstantUtils.ConstantUtils.configPath, JsonConvert.SerializeObject(config, Formatting.Indented));
                    }
                    // 如果路径为空，则让用户选择
                    if (string.IsNullOrEmpty(config.BeyondComparePath))
                    {
                        OpenFileDialog openFileDialog = new OpenFileDialog
                        {
                            Filter = "Executable Files (*.exe)|*.exe",
                            Title = "选择 Beyond Compare"
                        };

                        if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        {
                            config.BeyondComparePath = openFileDialog.FileName;

                            // 保存新的配置到文件
                            File.WriteAllText(ConstantUtils.ConstantUtils.configPath, JsonConvert.SerializeObject(config, Formatting.Indented));
                        }
                    }
                    ProcessStartInfo processStartInfo = new ProcessStartInfo
                    {
                        FileName = config.BeyondComparePath,
                        Arguments = $"\"{leftPath}\" \"{rightPath}\"",
                        UseShellExecute = true,  // 需设置 UseShellExecute 为 true 以使用系统 shell 启动
                        CreateNoWindow = false
                    };
                    try
                    {
                        Process.Start(processStartInfo);
                    }
                    catch (Exception ex)
                    {
                        System.Windows.Forms.MessageBox.Show($"启动 Beyond Compare 时发生错误: {ex.Message}");
                    }
                }
            }
        }
    }
}
