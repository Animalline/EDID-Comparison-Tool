using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.Windows.Forms;
using System.Windows;

namespace EDID_Comparison_Tool_For_WPF
{
    public class FileUtils
    {
        //选择文件夹
        public static string selectFolder(string description)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = description;
                dialog.ShowNewFolderButton = true;

                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    return dialog.SelectedPath;
                }
                else
                {
                    return null;
                }
            }
        }
    }
}
