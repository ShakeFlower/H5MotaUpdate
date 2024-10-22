using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Eventing.Reader;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using System.Xml.Linq;
using Microsoft.VisualBasic.Devices;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace H5MotaUpdate.ViewModels
{
    internal class ServerTableMigrator
    {
        string sourcePath, destPath;
        Version version;
        readonly string FILENAME = "_server/table";
        public ServerTableMigrator(string oldRootDirectory, string newRootDirectory, Version ver)
        {
            sourcePath = System.IO.Path.Combine(oldRootDirectory, FILENAME);
            destPath = System.IO.Path.Combine(newRootDirectory, FILENAME);
            this.version = ver;
        }

        public void Migrate()
        {
            try
            {
                MigrateDirect();
                MessageBox.Show("迁移" + FILENAME + "文件夹完成。");
            }
            catch (Exception e)
            {
                MessageBox.Show("迁移" + FILENAME + $"过程中出现错误: {e.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        void MigrateDirect()
        {
            FileUtils.CopyFolderContentsAndSubFolders(sourcePath, destPath);
        }
    }
}
