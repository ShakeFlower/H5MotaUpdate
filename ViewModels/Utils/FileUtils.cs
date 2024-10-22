using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Eventing.Reader;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using System.Xml.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Drawing;

namespace H5MotaUpdate.ViewModels
{
    internal static class FileUtils
    {
        /// <summary>
        /// 检查文件夹路径是否合法
        /// </summary>
        public static bool IsFolderPathValid(string? folderPath, string folderName)
        {
            try
            {
                if (!Directory.Exists(folderPath))
                {
                    MessageBox.Show(folderName + "文件夹不存在，请检查", "错误", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return false;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(folderName + $"文件夹不存在，请检查，错误:{e.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return false;
            }
            return true;
        }

        /// <summary>
        /// 检查指定文件夹是否存在指定的子文件夹，若不存在，创建一个
        /// </summary>
        public static bool checkFolderExist(string folderPath, string subFolderName)
        {
            string subFolderPath = Path.Combine(folderPath, subFolderName);
            if (!Directory.Exists(subFolderPath))
            {
                return tryCreateFolder(subFolderName, subFolderPath);
            }
            else { return true; }
        }

        /// <summary>
        /// 尝试在指定路径创建文件夹
        /// </summary>
        public static bool tryCreateFolder(string folderDirectory, string folderName)
        {
            try { Directory.CreateDirectory(folderDirectory); }
            catch (Exception e)
            {
                MessageBox.Show($"错误：{e.Message},目标文件夹不存在" + folderName + "子文件夹，且创建失败，请检查", "错误", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return false;
            }
            if (!Directory.Exists(folderDirectory))
            {
                MessageBox.Show("目标文件夹不存在" + folderName + "子文件夹，且创建失败，请检查", "错误", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return false;
            }
            return true;
        }

        /// <summary>
        /// 将源文件夹的所有文件拷贝至目标文件夹
        /// </summary>
        public static void CopyFolderContents(string sourceFolderPath, string destFolderPath)
        {
            string[] files = Directory.GetFiles(sourceFolderPath);
            foreach (string file in files)
            {
                try
                {
                    string targetFilePath = Path.Combine(destFolderPath, Path.GetFileName(file));
                    File.Copy(file, targetFilePath, true);
                }
                catch (Exception e)
                {
                    MessageBox.Show("迁移" + file + $"过程中出现错误：{e.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
        }

        /// <summary>
        /// 将源文件夹的所有文件（含子文件夹及其所有文件）拷贝至目标文件夹
        /// </summary>
        public static void CopyFolderContentsAndSubFolders(string sourceFolderPath, string destFolderPath)
        {
            string[] files = Directory.GetFiles(sourceFolderPath);
            foreach (string file in files)
            {
                try
                {
                    string targetFilePath = Path.Combine(destFolderPath, Path.GetFileName(file));
                    File.Copy(file, targetFilePath, true);
                }
                catch (Exception e)
                {
                    MessageBox.Show("迁移" + file + $"过程中出现错误：{e.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }

            string[] dirs = Directory.GetDirectories(sourceFolderPath);
            foreach (string dir in dirs)
            {
                try
                {
                    string targetFolderPath = Path.Combine(destFolderPath, Path.GetFileName(dir));
                    CopyFolderContentsAndSubFolders(dir, targetFolderPath);
                }
                catch (Exception e)
                {
                    MessageBox.Show("迁移" + dir + $"过程中出现错误：{e.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
        }

        /// <summary>
        /// 将源文件夹名为fileName的指定文件复制到目标文件夹
        /// </summary>
        public static void CopyFile(string sourceFolderPath, string destFolderPath, string fileName)
        {
            try
            {
                string SourcePath = sourceFolderPath;
                string DestPath = destFolderPath;

                if (!File.Exists(SourcePath))
                {
                    MessageBox.Show("源文件夹不存在" + fileName + "子文件，请检查", "错误", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
                else
                {
                    System.IO.File.Copy(SourcePath, DestPath, true);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("迁移" + fileName + $"文件出现错误：{e.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }
    }
}
