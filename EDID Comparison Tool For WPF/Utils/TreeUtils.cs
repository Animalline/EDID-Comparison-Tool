using BidirectionalMap;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using F23.StringSimilarity;
using Avalonia.Media.Imaging;
using EDID_Comparison_Tool_For_WPF.VariablesUtils;

namespace EDID_Comparison_Tool_For_WPF
{
    public class TreeUtils
    {
        //添加节点
        public static TreeView AddTreeNode(TreeView treeView,string path,string showSuffix)
        {
            
            if(path != null)
            {
                //添加前先清空节点
                treeView.Items.Clear();
                //获取文件名
                string folderName = Path.GetFileName(path);
                //创建根节点
                TreeViewItem rootItem = new TreeViewItem();
                //显示文件名称
                rootItem.Header = folderName;
                //获取上级目录地址
                DirectoryInfo parentDirectory = Directory.GetParent(path);
                //保存绝对路径
                rootItem.Tag = parentDirectory.FullName+"\\"+rootItem.Header;
                //加入到树中
                treeView.Items.Add(rootItem);
                //迭代添加节点
                SetAllTreeNodes(rootItem, path, showSuffix);
                //展开第一层
                rootItem.IsExpanded = true;
            }
            return treeView;
        }
        //迭代插入节点
        private static void SetAllTreeNodes(TreeViewItem root, string path, string showSuffix)
        {
            //获取指定目录中文件和文件夹的名称
            var dics = Directory.GetFileSystemEntries(path);
            //遍历结果集
            foreach (var dic in dics)
            {
                //创建子节点
                TreeViewItem subItem = new TreeViewItem();
                //赋值显示名称
                subItem.Header = new DirectoryInfo(dic).Name;
                //保存路径
                subItem.Tag = root.Tag+"\\"+subItem.Header;
                //获取文件后缀
                var suffix = Path.GetExtension(dic);
                //设置icon
                switch (suffix)
                {
                    case ".dat":
                        
                        break;
                    case ".txt":

                        break;
                    default:
                        break;
                }

                //获取当前层级，根节点为0级
                int level = GetTreeViewItemLevel(subItem);
                //如果是文件夹
                if (Directory.Exists(dic))
                {
                    //设置icon

                    //添加到上级根节点
                    root.Items.Add(subItem);
                    //如果有文件且层级不超过2
                    if (Directory.GetFileSystemEntries(dic).Length > 0 && level < 2)
                    {
                        //迭代
                        SetAllTreeNodes(subItem, dic, showSuffix);
                    }
                }
                //如果是要显示的后缀
                else if (showSuffix.Equals(suffix))
                {
                    root.Items.Add(subItem);
                }
            }
        }
        //获取层级
        public static int GetTreeViewItemLevel(TreeViewItem item)
        {
            int level = 0;
            //当找到根节点时，递增级别
            while (item.Parent is TreeViewItem parent)
            {
                level++;
                item = parent;
            }
            return level;
        }
        //获取rootNode下的二级目录
        public static ItemCollection getItem(TreeView treeView)
        {
            var rootItem = treeView.Items[0] as TreeViewItem;
            return rootItem.Items;
        }
        //将左右树的二级目录匹配
        public static void TreeItemMatching(TreeView leftTree, TreeView rightTree)
        {
            //获取左右树collection
            var leftTreeCollectionn = getItem(leftTree);
            var rightTreeCollection = getItem(rightTree);
            VariablesUtils.VariablesUtils.biMap = GetBestMatches(leftTreeCollectionn, rightTreeCollection);
            foreach (TreeViewItem leftItem in leftTreeCollectionn)
            {
                string leftPath = leftItem.Tag + "\\" + leftItem.Header;
                TreeViewItem rightItem = VariablesUtils.VariablesUtils.biMap.Forward[leftItem];
                string rightPath = rightItem.Tag + "\\" + rightItem.Header;
            }

        }
        //匹配最优项
        private static BiMap<TreeViewItem, TreeViewItem> GetBestMatches(ItemCollection leftTreeCollection, ItemCollection rightTreeCollection)
        {
            BiMap<TreeViewItem, TreeViewItem> results = new BiMap<TreeViewItem, TreeViewItem>();
            // Ratcliff-Obershelp算法计算相似度矩阵
            var similarityMatrix = new double[leftTreeCollection.Count, rightTreeCollection.Count];
            for (int i = 0; i < leftTreeCollection.Count; i++)
            {
                for (int j = 0; j < rightTreeCollection.Count; j++)
                {
                    var ro = new RatcliffObershelp();
                    var left = leftTreeCollection[i] as TreeViewItem;
                    string leftHeader = ((string)left.Header).Replace(Path.GetExtension((string)left.Header), "");
                    var right = rightTreeCollection[j] as TreeViewItem;
                    similarityMatrix[i, j] = ro.Similarity(leftHeader, right.Header+"");
                }
            }
            //贪心算法找到最佳匹配
            var usedInGroup2 = new bool[rightTreeCollection.Count];
            for (int i = 0; i < leftTreeCollection.Count; i++)
            {
                double maxSimilarity = double.MinValue;
                int bestMatchIndex = -1;

                for (int j = 0; j < rightTreeCollection.Count; j++)
                {
                    if (!usedInGroup2[j] && similarityMatrix[i, j] > maxSimilarity && similarityMatrix[i,j] >= ConstantUtils.ConstantUtils.SimilarityValue)
                    {
                        maxSimilarity = similarityMatrix[i, j];
                        bestMatchIndex = j;
                    }
                }

                if (bestMatchIndex != -1)
                {
                    results.Add(leftTreeCollection[i] as TreeViewItem, rightTreeCollection[bestMatchIndex] as TreeViewItem);
                    usedInGroup2[bestMatchIndex] = true;
                }
                else
                {
                    //如果没有可匹配的项，值设为null
                    results.Add(leftTreeCollection[i] as TreeViewItem, new TreeViewItem());
                }
            }

            return results;

        }
    }

}
