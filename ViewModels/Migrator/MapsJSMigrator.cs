using Newtonsoft.Json.Linq;
using System.IO;
using System.Text;

namespace H5MotaUpdate.ViewModels
{
    internal class MapsJSMigrator
    {
        string sourcePath, destPath;
        Version version;
        readonly string FILENAME = "maps.js",
            DATANAME = "maps_90f36752_8815_4be8_b32b_d7fad1d0542e";
        public string?[] mapsIndexArray;

        /// <summary>
        /// 请输入新旧Project文件夹的路径
        /// </summary>
        public MapsJSMigrator(string oldProjectDirectory, string newProjectDirectory, Version ver)
        {
            sourcePath = System.IO.Path.Combine(oldProjectDirectory, FILENAME);
            destPath = System.IO.Path.Combine(newProjectDirectory, FILENAME);
            this.version = ver;
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
                        Convert_before2_7(jsonObject);
                    }
                    StringBuilder newJsContent = new StringBuilder();
                    newJsContent.Append("var " + DATANAME + " = \n");
                    newJsContent.Append(jsonObject.ToString());
                    File.WriteAllText(destPath, newJsContent.ToString());
                }
                ErrorLogger.LogError("迁移project/" + FILENAME + "文件完成。");
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

        void Convert_before2_7(JObject jsonObject)
        {
            // 下面列出一些样板图块名字的更改，以备后续需求。为适配旧有事件和脚本，暂时沿用原来的名字。
            // blueShop-left -> blueShopLeft,
            // blueShop-right -> blueShopRight,
            // pinkShop-left -> pinkShopLeft
            // pinkShop-right -> pinkShopRight
            // snow -> freezeBadge
            Dictionary<string, JObject> dictionary = new Dictionary<string, JObject>{
                { "blueShop-left", new JObject { { "cls", "terrains" } } },
                { "blueShop-right", new JObject { { "cls", "terrains" } } },
                { "pinkShop-left", new JObject { { "cls", "terrains" } } },
                { "pinkShop-right", new JObject { { "cls", "terrains" } } },
                { "lavaNet", new JObject { { "cls", "animates" }, { "canPass", true }, { "trigger", "null" }, { "script", "(function () {\n\t// 血网的伤害效果移动到 checkBlock 中处理\n\n\t// 如果要做一次性血网，可直接注释掉下面这句话：\n\t// core.removeBlock(core.getHeroLoc('x'), core.getHeroLoc('y'));\n})();" }, { "name", "血网" } } },
                { "poisonNet", new JObject { { "cls", "animates" }, { "canPass", true }, { "trigger", "null" }, { "script", "(function () {\n\t// 直接插入公共事件进行毒处理\n\tif (!core.hasItem('amulet')) {\n\t\tcore.insertAction({ \"type\": \"insert\", \"name\": \"毒衰咒处理\", \"args\": [0] });\n\t}\n\n\t// 如果要做一次性毒网，可直接注释掉下面这句话：\n\t// core.removeBlock(core.getHeroLoc('x'), core.getHeroLoc('y'));\n})()" }, { "name", "毒网" } } },
                { "weakNet", new JObject { { "cls", "animates" }, { "canPass", true }, { "trigger", "null" }, { "script", "(function () {\n\t// 直接插入公共事件进行衰处理\n\tif (!core.hasItem('amulet')) {\n\t\tcore.insertAction({ \"type\": \"insert\", \"name\": \"毒衰咒处理\", \"args\": [1] });\n\t}\n\n\t// 如果要做一次性衰网，可直接注释掉下面这句话：\n\t// core.removeBlock(core.getHeroLoc('x'), core.getHeroLoc('y'));\n})()" }, { "name", "衰网" } } },
                { "curseNet", new JObject { { "cls", "animates" }, { "canPass", true }, { "trigger", "null" }, { "script", "(function () {\n\t// 直接插入公共事件进行咒处理\n\tif (!core.hasItem('amulet')) {\n\t\tcore.insertAction({ \"type\": \"insert\", \"name\": \"毒衰咒处理\", \"args\": [2] });\n\t}\n\n\t// 如果要做一次性咒网，可直接注释掉下面这句话：\n\t// core.removeBlock(core.getHeroLoc('x'), core.getHeroLoc('y'));\n})()" }, { "name", "咒网" } } },
                { "snow", new JObject { { "cls", "items" } } },
                { "arrowUp", new JObject { { "cls", "terrains" }, { "canPass", true }, { "cannotOut", new JArray { "left", "right", "down" } }, { "cannotIn", new JArray { "up" } } } },
                { "arrowDown", new JObject { { "cls", "terrains" }, { "canPass", true }, { "cannotOut", new JArray { "left", "right", "up" } }, { "cannotIn", new JArray { "down" } } } },
                { "arrowLeft", new JObject { { "cls", "terrains" }, { "canPass", true }, { "cannotOut", new JArray { "up", "down", "right" } }, { "cannotIn", new JArray { "left" } } } },
                { "arrowRight", new JObject { { "cls", "terrains" }, { "canPass", true }, { "cannotOut", new JArray { "up", "down", "left" } }, { "cannotIn", new JArray { "right" } } } },
                { "light", new JObject { { "cls", "terrains" }, { "trigger", "null" }, { "canPass", true }, { "script", "(function () {\n\tcore.setBlock(core.getNumberById('darkLight'), core.getHeroLoc('x'), core.getHeroLoc('y'));\n})();" } } },
            };

            // 原先不存在的新图块信息
            Dictionary<string, JObject> newIconsDictionary = new Dictionary<string, JObject> {
                { "yellowDoor", new JObject { { "cls", "animates" }, { "trigger", "openDoor" }, { "animate", 1 }, { "doorInfo", new JObject { { "time", 160 }, { "openSound", "door.mp3" }, { "closeSound", "door.mp3" }, { "keys", new JObject { { "yellowKey", 1 } } } } }, { "name", "黄门" } } },
                { "blueDoor", new JObject { { "cls", "animates" }, { "trigger", "openDoor" }, { "animate", 1 }, { "doorInfo", new JObject { { "time", 160 }, { "openSound", "door.mp3" }, { "closeSound", "door.mp3" }, { "keys", new JObject { { "blueKey", 1 } } } } }, { "name", "蓝门" } } },
                { "redDoor", new JObject { { "cls", "animates" }, { "trigger", "openDoor" }, { "animate", 1 }, { "doorInfo", new JObject { { "time", 160 }, { "openSound", "door.mp3" }, { "closeSound", "door.mp3" }, { "keys", new JObject { { "redKey", 1 } } } } }, { "name", "红门" } } },
                { "greenDoor", new JObject { { "cls", "animates" }, { "trigger", "openDoor" }, { "animate", 1 }, { "doorInfo", new JObject { { "time", 160 }, { "openSound", "door.mp3" }, { "closeSound", "door.mp3" }, { "keys", new JObject { { "greenKey", 1 } } } } }, { "name", "绿门" } } },
                { "specialDoor", new JObject { { "cls", "animates" }, { "trigger", "openDoor" }, { "animate", 1 }, { "doorInfo", new JObject { { "time", 160 }, { "openSound", "door.mp3" }, { "closeSound", "door.mp3" }, { "keys", new JObject { { "specialKey", 1 } } } } }, { "name", "机关门" } } },
                { "steelDoor", new JObject { { "cls", "animates" }, { "trigger", "openDoor" }, { "animate", 1 }, { "doorInfo", new JObject { { "time", 160 }, { "openSound", "door.mp3" }, { "closeSound", "door.mp3" }, { "keys", new JObject { { "steelKey", 1 } } } } }, { "name", "铁门" } } },
                { "yellowWallDoor", new JObject { { "cls", "animates" }, { "canBreak", true }, { "animate", 1 }, { "doorInfo", new JObject { { "time", 160 }, { "openSound", "door.mp3" }, { "closeSound", "door.mp3" }, { "keys", new JObject() } } } } },
                { "whiteWallDoor", new JObject { { "cls", "animates" }, { "canBreak", true }, { "animate", 1 }, { "doorInfo", new JObject { { "time", 160 }, { "openSound", "door.mp3" }, { "closeSound", "door.mp3" }, { "keys", new JObject() } } } } },
                { "blueWallDoor", new JObject { { "cls", "animates" }, { "canBreak", true }, { "animate", 1 }, { "doorInfo", new JObject { { "time", 160 }, { "openSound", "door.mp3" }, { "closeSound", "door.mp3" }, { "keys", new JObject() } } } } },
                { "ground", new JObject { { "cls", "terrains" } } },
                { "grass", new JObject { { "cls", "terrains" } } },
                { "grass2", new JObject { { "cls", "terrains" } } },
                { "snowGround", new JObject { { "cls", "terrains" } } },
                { "ground2", new JObject { { "cls", "terrains" } } },
                { "ground3", new JObject { { "cls", "terrains" } } },
                { "ground4", new JObject { { "cls", "terrains" } } },
                { "sand", new JObject { { "cls", "terrains" } } },
                { "ground5", new JObject { { "cls", "terrains" } } },
                { "yellowWall2", new JObject { { "cls", "terrains" } } },
                { "whiteWall2", new JObject { { "cls", "terrains" } } },
                { "blueWall2", new JObject { { "cls", "terrains" } } },
                { "blockWall", new JObject { { "cls", "terrains" } } },
                { "grayWall", new JObject { { "cls", "terrains" } } },
                { "white", new JObject { { "cls", "terrains" } } },
                { "ground6", new JObject { { "cls", "terrains" } } },
                { "soil", new JObject { { "cls", "terrains" } } },
                { "ground7", new JObject { { "cls", "terrains" } } },
                { "ground8", new JObject { { "cls", "terrains" } } }
            };

            // 生成一个新图块序号的数组
            mapsIndexArray = new string?[newIconsDictionary.Count];

            // 从0-10000寻找可分配给新图块的序号
            for (int i = 1, c = 0; i < 10000 && c < newIconsDictionary.Count; i++)
            {
                if (i == 17) continue; //不知道为什么，但是编辑器里17号被占用了
                string str_i = i.ToString();
                if (!jsonObject.ContainsKey(str_i))
                {
                    mapsIndexArray[c++] = str_i;
                }
                if (i == 9999)
                {
                    ErrorLogger.LogError("警告！Maps.js中存在过多元素！可能引起一些未知问题。", "red");
                }
            }

            {
                int index = 0;
                foreach (KeyValuePair<string, JObject> perdata in newIconsDictionary)
                {
                    string key = perdata.Key;
                    JObject valuePairs = perdata.Value;
                    string? iconsNumber = mapsIndexArray[index++];
                    if (String.IsNullOrEmpty(iconsNumber)) { break; }
                    JObject iconsValue = perdata.Value;
                    iconsValue = StringUtils.MergeJObjects(iconsValue, new JObject(new JProperty("id", key)));
                    jsonObject[iconsNumber] = perdata.Value;
                }
            }

            foreach (JProperty prop in jsonObject.Properties())
            {
                if (prop.Value is JObject propObj)
                {
                    JToken noPass = propObj["noPass"];

                    if (noPass != null && noPass.Value<bool>())
                    {
                        propObj["canPass"] = false;
                    }
                    else if (noPass != null && !noPass.Value<bool>())
                    {
                        propObj["canPass"] = true;
                    }

                    // 移除 noPass 属性
                    propObj.Remove("noPass");

                    string iconId = propObj["id"]?.ToString();
                    if (!string.IsNullOrEmpty(iconId) && dictionary.ContainsKey(iconId))
                    {
                        prop.Value = StringUtils.MergeJObjects(propObj, dictionary[iconId]);
                    }
                    else
                    {
                        prop.Value = propObj;
                    }
                }
            }

            for (int i = 81; i <= 86; i++)
            {  // 删除terrains的几个门的索引，避免影响animates门的匹配
                jsonObject.Remove(i.ToString());
            }
        }
    }
}
