using System.IO;
using System.Text.RegularExpressions;

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

        /// <summary>
        /// 从给定路径的旧塔文件夹中读取版本号
        /// </summary>
        /// <param name="folderPath"></param>
        /// <returns></returns>
        public static string GetVersion(string? folderPath)
        {
            if (folderPath == null) return "文件夹路径不合法";

            string filePath = Path.Combine(folderPath, "main.js");

            if (!File.Exists(filePath)) return "给定文件夹未找到文件main.js";

            string fileContent;
            try
            {
                fileContent = File.ReadAllText(filePath);
            }
            catch (Exception ex)
            {
                return "读取文件内容失败，原因: " + ex.Message;
            }

            string version = null;

            // 优先匹配 this.__VERSION__ = "...";
            Regex specialVersionRegex = new Regex(
                @"this\s*\.\s*__VERSION__\s*=\s*['""](\d+(\.\d+)+)['""]\s*;?");
            Match specialMatch = specialVersionRegex.Match(fileContent);
            if (specialMatch.Success)
            {
                version = specialMatch.Groups[1].Value;
                if (IsValidVersion(version))
                    return version;
            }

            // 回退到原来的方式：this.version = "x.x.x"
            Regex normalVersionRegex = new Regex(@"this\.version\s*=\s*['""](\d+(\.\d+)+)['""];?");
            Match normalVersionMatch = normalVersionRegex.Match(fileContent);

            if (normalVersionMatch.Success)
            {
                version = normalVersionMatch.Groups[1].Value;
                if (IsValidVersion(version))
                    return version;
            }

            return "文件 main.js中未找到格式合法的版本号！";
        }
    }
}
