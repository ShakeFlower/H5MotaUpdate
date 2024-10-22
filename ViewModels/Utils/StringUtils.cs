using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Windows;
using System.Windows.Forms;

namespace H5MotaUpdate.ViewModels
{
    internal static class StringUtils
    {
        /// <summary>
        /// 从指定路径的.js文件中提取JObject文件
        /// </summary>
        public static JObject getValidJson(string path)
        {
            try
            {
                string jsContent = File.ReadAllText(path);
                string jsonContent;

                int start = jsContent.IndexOf("{");
                int end = jsContent.LastIndexOf("}") + 1;
                if (end != -1)
                {
                    jsonContent = jsContent.Substring(start, end - start);
                }
                else
                {
                    jsonContent = jsContent.Substring(start);
                }
                JObject jsonObject = JObject.Parse(jsonContent);
                return jsonObject;
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show("从" + path + "中读取JSON时出错:" + $"{e.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                throw;
            }
        }

        /// <summary>
        /// 对于字符串str，找到${...}内部的内容，将其中的oldWord替换为newWord
        /// </summary>
        public static string ReplaceInBetweenCurlyBraces(string str, string oldWord, string newWord)
        {
            // 定义正则表达式模式，匹配 ${...} 中的内容  
            string pattern = @"\$\{([^}]+)\}";

            // 使用 Regex.Replace 方法进行替换  
            string replacedStr = Regex.Replace(str, pattern, match =>
            {
                // 获取匹配的内容，去掉 ${ 和 }  
                string content = match.Groups[1].Value;
                // 在匹配的内容中替换 oldWord 为 newWord  
                string modifiedContent = content.Replace(oldWord, newWord);
                // 返回替换后的完整匹配字符串，包含 ${ 和 }  
                return "${" + modifiedContent + "}";
            });

            return replacedStr;
        }

        /// <summary>
        /// 输入要加减的变量名varName，增减值value，符号+=或-=或=，返回一个执行该事件的JSON
        /// </summary>
        public static JObject getAddValueJson(string varName, string value, string o)
        {
            JObject adder = new JObject();
            adder.Add(new JProperty("type", "setValue"));
            adder.Add(new JProperty("name", varName));
            if (o != "=") adder.Add(new JProperty("operator", o));
            adder.Add(new JProperty("value", value));
            return adder;
        }

        /// <summary>
        /// 输入effect语句，返回一个包含所有事件的数组
        /// </summary>
        /// 参考： events.js/doEffect
        public static JArray doEffect(string effects)
        {
            JArray newEffectsArray = new JArray();
            string[] effectArr = effects.Split([';']);
            foreach (string effect in effectArr)
            {
                string[] arr = effect.Split("+=");
                JObject currEffectJson = getAddValueJson(arr[0], arr[1], "+=");
                newEffectsArray.Add(currEffectJson);
            }
            return newEffectsArray;
        }

        public static string ReplaceOldNames(string input, Version version)
        {
            input = input.Replace("Jewel", "Gem");
            input = input.Replace("ratio", "core.status.thisMap.ratio");
            return input;
        }

        /// <summary>
        /// 合并两个JObject，jobject2中的键值对覆盖jobject1的键值对（不论原先是否存在），返回jobject1。
        /// </summary>
        /// <param name="targetJObject">要合并到并返回的对象。</param>  
        /// <param name="sourceJObject">提供要合并内容的对象。</param>  
        /// <returns>合并后的targetJObject。</returns>
        public static JObject MergeJObjects(JObject targetJObject, JObject sourceJObject)
        {
            foreach (JProperty prop in sourceJObject.Properties())
            {
                string key = prop.Name;
                targetJObject[key] = prop.Value;
            }
            return targetJObject;
        }

        public static JArray CreateMatrix(int width, int height)
        {
            if (width <= 0 || height <= 0)
            {
                throw new ArgumentException("Width and height must be greater than 0.");
            }

            JArray matrix = new JArray();

            for (int i = 0; i < height; i++)
            {
                JArray row = new JArray();
                for (int j = 0; j < width; j++)
                {
                    row.Add(0);
                }
                matrix.Add(row);
            }

            return matrix;
        }

        /// <summary>
        /// 读取塔的地图尺寸，默认值为13
        /// <summary>
        public static int ReadMapWidth(string filePath)
        {
            int width;
            try
            {
                string fileContent = File.ReadAllText(filePath);

                string widthPattern = @"this\._WIDTH_\s*=\s*(\d+);";

                Match widthMatch = Regex.Match(fileContent, widthPattern);

                if (widthMatch.Success)
                {
                    width = int.Parse(widthMatch.Groups[1].Value);
                }
                else
                {
                    width = 13;
                }
            }
            catch
            {
                width = 13;
            }
            return width;
        }
    }
}
