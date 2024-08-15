using BidirectionalMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace EDID_Comparison_Tool_For_WPF.VariablesUtils
{
    public static class VariablesUtils
    {
        public static BiMap<TreeViewItem,TreeViewItem> biMap = new BiMap<TreeViewItem,TreeViewItem>();

        public static bool isFirstSelect = false;
    }
}
