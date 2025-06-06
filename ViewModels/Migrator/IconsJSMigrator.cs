﻿using Newtonsoft.Json.Linq;
using System.IO;
using System.Text;


namespace H5MotaUpdate.ViewModels
{
    internal class IconsJSMigrator
    {
        string sourcePath, destPath;
        Version version;
        readonly string FILENAME = "icons.js",
            DATANAME = "icons_4665ee12_3a1f_44a4_bea3_0fccba634dc1";

        /// <summary>
        /// 请输入新旧Project文件夹的路径
        /// </summary>
        public IconsJSMigrator(string oldProjectDirectory, string newProjectDirectory, Version ver)
        {
            sourcePath = System.IO.Path.Combine(oldProjectDirectory, FILENAME);
            destPath = System.IO.Path.Combine(newProjectDirectory, FILENAME);
            version = ver;
        }

        public void Migrate()
        {
            try
            {
                if (version.CompareTo(new Version(2, 7)) >= 0)
                {
                    MigrateDirect();
                }
                else
                {
                    JObject jsonObject = StringUtils.getValidJson(sourcePath);
                    if (version.CompareTo(new Version(2, 7)) < 0)
                    {
                        ConvertIconsJS_before2_7(jsonObject);
                    }
                    StringBuilder newJsContent = new StringBuilder();
                    newJsContent.Append("var " + DATANAME + " = \n");
                    newJsContent.Append(jsonObject.ToString());
                    File.WriteAllText(destPath, newJsContent.ToString());
                }
                ErrorLogger.LogError("迁移project/" + FILENAME + "文件完成");
            }
            catch (Exception e)
            {
                ErrorLogger.LogError("迁移project/" + FILENAME + $"过程中出现错误: {e.Message}", "red");
            }
        }

        void MigrateDirect()
        {

            FileUtils.CopyFile(sourcePath, destPath, FILENAME);
        }

        void ConvertIconsJS_before2_7(JObject jsonObject)
        {
            // 暂时将错就错沿用旧名字，有问题再说
            /*
            JObject terrains = (JObject)jsonObject["terrains"];
            terrains.Remove("yellowWall");
            terrains.Remove("blueWall");
            terrains.Remove("whiteWall");
            terrains.Remove("yellowDoor");
            terrains.Remove("blueDoor");
            terrains.Remove("redDoor");
            terrains.Remove("greenDoor");
            terrains.Remove("specialDoor");
            terrains.Remove("steelDoor");

            if (terrains["blueShopLeft"] == null)
            {
                terrains["blueShopLeft"] = terrains["blueShop-left"];
            }
            terrains.Remove("blueShop-right");
            if (terrains["blueShopRight"] == null)
            {
                terrains["blueShopRight"] = terrains["blueShop-right"];
            }
            terrains.Remove("blueShop-right");
            if (terrains["pinkShopLeft"] == null)
            {
                terrains["pinkShopLeft"] = terrains["pinkShop-left"];
            }
            terrains.Remove("pinkShop-left");
            if (terrains["pinkShopRight"] == null)
            {
                terrains["pinkShopRight"] = terrains["pinkShop-right"];
            }
            terrains.Remove("pinkShop-left");
            JObject items = (JObject)jsonObject["items"];
            if (items["snow"] != null)
            {
                items["freezeBadge"] = items["snow"];
            }
            items.Remove("snow");
            */
        }
    }
}
