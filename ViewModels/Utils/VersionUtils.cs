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

namespace H5MotaUpdate.ViewModels
{
    internal static class VersionUtils
    {
        public static bool IsValidVersion(string? version)
        {
            if (String.IsNullOrEmpty(version)) return false;
            string[] segments = version.Split('.');
            foreach (string segment in segments)
            {
                if (!int.TryParse(segment, out int part)) return false;
            }
            return true;
        }

        public static string GetVersion(string? folderPath)
        {
            if (folderPath == null) return "文件夹路径不合法";

            string filePath = Path.Combine(folderPath, "main.js");
            string version;
            try
            {
                if (!File.Exists(filePath)) return "给定文件夹未找到文件main.js";

                string fileContent = File.ReadAllText(filePath);

                Regex versionRegex = new Regex(@"this\.version\s*=\s*['""](\d+(\.\d+)+)['""];");
                Match match = versionRegex.Match(fileContent);

                if (!match.Success) return "文件 main.js中未找到版本号！";
                version = match.Groups[1].Value;
                if (!IsValidVersion(version)) return "文件 main.js中未找到格式合法的版本号！";
            }
            catch (Exception ex)
            {
                return "读取版本号失败，原因: " + ex.Message;
            }
            return version;
        }
    }
}
