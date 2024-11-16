using System.Diagnostics;
using System.IO;

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
                    string errMsg = folderName + "文件夹不存在，请检查";
                    MessageBox.Show(errMsg, "错误", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    ErrorLogger.LogError(errMsg, "red");
                    return false;
                }
            }
            catch
            {
                string errMsg = folderName + "文件夹不存在，请检查";
                MessageBox.Show(errMsg, "错误", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                ErrorLogger.LogError(errMsg, "red");
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
                string errMsg = $"错误：{e.Message},目标文件夹不存在" + folderName + "子文件夹，且创建失败，请检查";
                MessageBox.Show(errMsg, "错误", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                ErrorLogger.LogError(errMsg, "red");
                return false;
            }
            if (!Directory.Exists(folderDirectory))
            {
                string errMsg = "目标文件夹不存在" + folderName + "子文件夹，且创建失败，请检查";
                MessageBox.Show(errMsg, "错误", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                ErrorLogger.LogError(errMsg, "red");
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
                    ErrorLogger.LogError("迁移" + file + $"过程中出现错误：{e.Message}", "red");
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
                    ErrorLogger.LogError("迁移" + file + $"过程中出现错误：{e.Message}", "red");
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
                    ErrorLogger.LogError("迁移" + dir + $"过程中出现错误：{e.Message}", "red");
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
                    ErrorLogger.LogError("源文件夹不存在" + fileName + "子文件，请检查", "red");
                }
                else
                {
                    File.Copy(SourcePath, DestPath, true);
                }
            }
            catch (Exception e)
            {
                ErrorLogger.LogError("迁移" + fileName + $"文件出现错误：{e.Message}", "red");
            }
        }

        /// <summary>
        /// 打开readme文件
        /// </summary>
        public static void ShowHelp()
        {
            try
            {
                string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string readMePath = Path.Combine(appDirectory, "readme.txt");

                if (File.Exists(readMePath))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = readMePath,
                        UseShellExecute = true
                    });
                }
                else
                {
                    MessageBox.Show("readme.txt 不存在", "错误", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"无法打开readme.txt，发生了错误: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }
    }
}
