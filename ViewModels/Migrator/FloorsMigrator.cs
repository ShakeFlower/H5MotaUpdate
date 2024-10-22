using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Eventing.Reader;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Xml.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace H5MotaUpdate.ViewModels
{
    internal class FloorsMigrator
    {
        string sourcePath, destPath;
        Version version;
        readonly string FILENAME = "floors";
        string?[] mapsIndexArray;
        int mapWidth = 13;

        /// <summary>
        /// 请输入新旧Project文件夹的路径
        /// </summary>
        public FloorsMigrator(string oldProjectDirectory, string newProjectDirectory, Version ver, int width)
        {
            sourcePath = System.IO.Path.Combine(oldProjectDirectory, FILENAME);
            destPath = System.IO.Path.Combine(newProjectDirectory, FILENAME);
            this.version = ver;
            this.mapWidth = width;
        }

        public void Migrate(string?[] mapsIndexArray)
        {
            try
            {
                if (version.CompareTo(new Version(2, 7)) >= 0)
                {
                    MigrateDirect();
                }
                else
                {
                    this.mapsIndexArray = mapsIndexArray;
                    MigrateFloors();
                }
                MessageBox.Show("迁移project/" + FILENAME + "文件夹完成。");
            }
            catch (Exception e)
            {
                MessageBox.Show("迁移project/" + FILENAME + $"文件夹过程中出现错误: {e.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        void MigrateDirect()
        {
            FileUtils.CopyFolderContents(sourcePath, destPath);
        }

        void MigrateFloors()
        {
            string[] files = Directory.GetFiles(sourcePath);
            foreach (string file in files)
            {
                string sourceFilePath = System.IO.Path.Combine(sourcePath, System.IO.Path.GetFileName(file)),
                    destFilePath = System.IO.Path.Combine(destPath, System.IO.Path.GetFileName(file));
                MigrateOneFloor(sourceFilePath, destFilePath);
            }
        }

        void MigrateOneFloor(string sourceFilePath, string destFilePath)
        {
            // 每一层try catch一次，一层出错不影响其它层继续复制
            try
            {
                string floorName = System.IO.Path.GetFileNameWithoutExtension(sourceFilePath);
                JObject jsonObject = StringUtils.getValidJson(sourceFilePath);
                if (version.CompareTo(new Version(2, 7)) <= 0)
                {
                    Convert(jsonObject);
                }
                StringBuilder newJsContent = new StringBuilder();
                newJsContent.Append("main.floors." + floorName + " = ");
                newJsContent.Append(jsonObject.ToString());
                File.WriteAllText(destFilePath, newJsContent.ToString());
            }
            catch (Exception e)
            {
                MessageBox.Show("迁移楼层文件" + System.IO.Path.GetFileName(sourceFilePath) + $"时出现错误: {e.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        void Convert(JObject jsonObject)
        {
            jsonObject["canFlyFrom"] = jsonObject["canFlyTo"];
            jsonObject["ratio"] = jsonObject["item_ratio"];
            jsonObject.Remove("item_ratio");
            jsonObject["width"] = mapWidth;
            jsonObject["height"] = mapWidth;
            jsonObject["autoEvent"] = new JObject();

            #region
            // 楼层贴图：老版本为一字符串或数组，字符串会被自动转为[0,0,str]
            // 其中t[0],t[1]分别为x,y,t[2]为贴图名字 剩下的不知道干嘛的
            JToken oldImages = jsonObject["images"];
            string imageName = null;
            if (oldImages is JArray oldImagesArr)
            {
                if (oldImagesArr.Count >= 3)
                {
                    imageName = oldImagesArr[2].ToString();
                }
            }
            else
            {
                imageName = oldImages.ToString();
            }
            if (imageName == null)
            {
                jsonObject["images"] = new JArray();
            }
            JObject newImages = new JObject(new JProperty("name", imageName),
                new JProperty("canvas", "bg"),
                new JProperty("sx", 0),
                new JProperty("sy", 0),
                new JProperty("w", 416),
                new JProperty("h", 416)
                );
            jsonObject["images"] = new JArray(newImages);
            #endregion

            #region
            JArray mapMatrix = (JArray)jsonObject["map"];
            JArray zeroBgMatrix = StringUtils.CreateMatrix(mapWidth, mapWidth);
            for (int i = 0; i < mapMatrix.Count; i++)
            {
                JArray lineArr = (JArray)mapMatrix[i];
                for (int j = 0; j < lineArr.Count; j++)
                {
                    int onePoint = lineArr[j].Value<int>();
                    if (onePoint >= 81 && onePoint <= 86 && mapsIndexArray != null)
                    { // 将terrains门替换为animates门
                        lineArr[j] = mapsIndexArray[onePoint - 81];
                    }
                    if (onePoint == 167 && version.CompareTo(new Version(2, 6)) < 0)
                    {
                        // 2.6以后，滑冰转移到背景层
                        JArray bgMatrix = (JArray)jsonObject["bgmap"].DeepClone();
                        if (bgMatrix == null || bgMatrix.Count == 0)
                        {
                            jsonObject["bgmap"] = zeroBgMatrix.DeepClone();
                        }
                        jsonObject["bgmap"][i][j] = 167;
                        mapMatrix[i][j] = 0;
                    }
                }
            }
            #endregion
        }
    }
}
