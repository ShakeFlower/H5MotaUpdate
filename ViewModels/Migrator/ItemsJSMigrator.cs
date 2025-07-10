using Newtonsoft.Json.Linq;
using System.IO;
using System.Text;
namespace H5MotaUpdate.ViewModels
{
    internal class ItemsJSMigrator
    {
        string sourcePath, destPath;
        Version version;
        readonly string FILENAME = "items.js",
            DATANAME = "items_296f5d02_12fd_4166_a7c1_b5e830c9ee3a";

        /// <summary>
        /// 请输入新旧Project文件夹的路径
        /// </summary>
        public ItemsJSMigrator(string oldProjectDirectory, string newProjectDirectory, Version ver)
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
                        Convert(ref jsonObject);
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

        void Convert(ref JObject jsonObject)
        {
            //2.7之前items.js有items,itemEffect, itemEffectTip, useItemEvent, useItemEffect, canUseItemEffect, equipCondition七个键
            //新建一个items对象，迁移其他对象中的内容
            JObject newItemDatas = (JObject)jsonObject["items"],
                itemEffect = (JObject)jsonObject["itemEffect"],
                itemEffectTip = (JObject)jsonObject["itemEffectTip"],
                useItemEvent = (JObject)jsonObject["useItemEvent"],
                useItemEffect = (JObject)jsonObject[" useItemEffect"],
                canUseItemEffect = (JObject)jsonObject["canUseItemEffect"],
                equipCondition = (JObject)jsonObject["equipCondition"];
            string[] arr = ["itemEffect", "itemEffectTip", "useItemEvent", "useItemEffect", "canUseItemEffect", "equipCondition"];
            foreach (string ele in arr)
            {
                JObject eleData = (JObject)jsonObject[ele];
                if (eleData == null || eleData.Count == 0) { continue; }
                foreach (JProperty prop in eleData.Properties())
                {
                    string key = prop.Name,
                        valueString = prop.Value.ToString();
                    valueString = StringUtils.ReplaceOldNames(valueString, version); //危险操作
                    if (newItemDatas.ContainsKey(key))
                    {
                        JObject itemData = (JObject)newItemDatas[key];
                        if (ele == "itemEffectTip")
                        {
                            valueString = "${" + valueString + "}";
                        }
                        itemData[ele] = valueString;
                    }
                }
            }
            foreach (JProperty prop in newItemDatas.Properties())
            {
                string key = prop.Name;
                JObject perData = (JObject)prop.Value;

                if (perData["cls"]?.ToString() == "keys")
                {
                    perData["cls"] = "tools";
                    perData["hideInToolbox"] = true;
                }

                if (perData["cls"]?.ToString() == "equips")
                {
                    JObject equipValue = (JObject)perData["equip"];

                    if (equipValue != null)
                    {
                        JObject valueObj = new JObject();

                        // 装备属性的名字
                        string[] keysToMove = { "atk", "def", "mdef", "hp" };

                        foreach (string keyToMove in keysToMove)
                        {
                            if (equipValue.ContainsKey(keyToMove))
                            {
                                JToken val = equipValue[keyToMove];
                                valueObj[keyToMove] = val;
                                equipValue.Remove(keyToMove);
                            }
                        }
                        if (equipValue.ContainsKey("percentage") && equipValue["percentage"].Type == JTokenType.Boolean && (bool)equipValue["percentage"])
                        {
                            equipValue["percentage"] = valueObj;
                        }
                        else equipValue["value"] = valueObj;
                        perData["equip"] = equipValue;
                    }
                }
            }
            if (newItemDatas.ContainsKey("snow"))
            {
                newItemDatas["freezeBadge"] = newItemDatas["snow"];
                newItemDatas.Remove("snow");
            }
            jsonObject = newItemDatas; //直接赋值操作需要加ref
        }
    }
}
