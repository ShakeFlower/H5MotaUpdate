using Newtonsoft.Json.Linq;
using System.IO;
using System.Text;

namespace H5MotaUpdate.ViewModels
{
    internal class DataJSMigrator
    {
        string sourcePath, destPath;
        Version version;
        readonly string FILENAME = "data.js",
            DATANAME = "data_a1e2fb4a_e986_4524_b0da_9b7ba7c0874d";
        public DataJSMigrator(string oldProjectDirectory, string newProjectDirectory, Version ver)
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
                        Convert(jsonObject);
                    }
                    StringBuilder newJsContent = new StringBuilder();
                    newJsContent.Append("var " + DATANAME + " = ");
                    newJsContent.Append(jsonObject.ToString());
                    File.WriteAllText(destPath, newJsContent.ToString());
                }
                MessageBox.Show("迁移project/" + FILENAME + "文件完成。");
            }
            catch (Exception e)
            {
                MessageBox.Show("迁移project/" + FILENAME + $"过程中出现错误: {e.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        void MigrateDirect()
        {
            FileUtils.CopyFile(sourcePath, destPath, FILENAME);
        }

        void Convert(JObject jsonObject)
        {

            JObject mainData = (JObject)jsonObject["main"],
                firstData = (JObject)jsonObject["firstData"],
                valuesData = (JObject)jsonObject["values"],
                flagsData = (JObject)jsonObject["flags"];

            JArray tilesetArr = (JArray)mainData["tilesets"];
            if (tilesetArr != null && tilesetArr.Count > 0) // 2.4.2(?)之前不存在main.tilesets
            {
                foreach (JToken tileset in tilesetArr)
                {
                    string tilesetStr = tileset.ToString();
                    if (tilesetStr.Contains('.'))
                    {
                        tilesetStr += ".png";
                    }
                }
            }

            #region
            // 转换难度列表levelChoose的格式
            JArray levelChooseArr = (JArray)mainData["levelChoose"];
            JArray newlevelChooseArr = new JArray();

            if (flagsData["startDirectly"] != null && flagsData["startDirectly"].Value<bool>() == true)
            {
                // startDirectly为true时直接开始，无难度选项
            }
            else
            {
                for (int i = 0; i < levelChooseArr.Count; i++)
                {
                    JArray level = (JArray)levelChooseArr[i];
                    JObject newLevel = new JObject();
                    newLevel.Add("title", level[0]);
                    newLevel.Add("name", level[1]);
                    newLevel.Add("hard", i);
                    newLevel.Add("color", new JArray(64, 25, 85, 1));
                    newLevel.Add("action", new JArray());
                    newlevelChooseArr.Add(newLevel);
                }
            }
            mainData["levelChoose"] = newlevelChooseArr;
            #endregion

            // 增加main.fonts
            mainData.Add("fonts", new JArray());

            // 合并main.styles
            JObject newStyles = new JObject(
                new JProperty("startBackground", "project/images/bg.jpg"),
                new JProperty("startVerticalBackground", "project/images/bg.jpg"), // 竖屏标题界面背景图
                new JProperty("startLogoStyle", "color: black"),
                new JProperty("startButtonsStyle", "background-color: #32369F; opacity: 0.85; color: #FFFFFF; border: #FFFFFF 2px solid; caret-color: #FFD700;"),
                new JProperty("statusLeftBackground", "url(project/materials/ground.png) repeat"),
                new JProperty("statusTopBackground", "url(project/materials/ground.png) repeat"),
                new JProperty("toolsBackground", "url(project/materials/ground.png) repeat"),
                new JProperty("borderColor", new JArray(204, 204, 204, 1)),
                new JProperty("statusBarColor", new JArray(255, 255, 255, 1)),
                new JProperty("floorChangingStyle", "background-color: black; color: white"),
                new JProperty("font", "Verdana")
                );
            string[] styleKeys = ["startLogoStyle", "startButtonsStyle", "statusLeftBackground", "statusTopBackground", "toolsBackground", "font"];
            foreach (string key in styleKeys)
            {
                if (mainData.ContainsKey(key))
                {
                    newStyles[key] = mainData[key];
                }
            }
            foreach (JProperty prop in newStyles.Properties())
            {
                string key = prop.Name;
                if (mainData.ContainsKey(key))
                {
                    mainData.Remove(key);
                }
            }
            mainData["styles"] = newStyles;

            mainData.Remove("floorChangingBackground");
            mainData.Remove("floorChangingTextColor");

            JArray imageArr = (JArray)mainData["images"];
            if (!imageArr.Contains("hero.png")) imageArr.Add("hero.png"); //?

            JObject heroData = (JObject)firstData["hero"],
                heroItemData = (JObject)heroData["items"];
            heroData["image"] = "hero.png";
            heroData["followers"] = new JArray();
            if (heroData["exp"] == null)
            {
                heroData["exp"] = heroData["experience"];
            }
            if (heroData["equipment"] == null)
            {
                heroData["equipment"] = new JArray(); // 预防某些超级老样板没有hero.equipment}
            }
            heroData.Remove("experience");

            JObject heroToolsData = (JObject)heroItemData["tools"];
            JObject heroKeysData = (JObject)heroItemData["keys"];
            if (heroKeysData != null)
            {
                if (heroToolsData == null)
                {
                    heroToolsData = new JObject();
                }
                StringUtils.MergeJObjects(heroToolsData, heroKeysData);
            }
            heroItemData.Remove("keys");


            #region
            // 转换全局商店shop的格式
            JArray shopArr = (JArray)firstData["shops"];
            for (int i = 0; i < shopArr.Count; i++)
            {
                JObject shop = (JObject)shopArr[i];

                if (shop["item"] != null && shop["item"].Value<bool>() == true) // 道具商店
                {
                    shop["use"] = "money";
                }
                else if (shop.ContainsKey("commonEvent")) // 公共事件商店
                {

                }
                else //普通商店
                {
                    JArray choiceArr = (JArray)shop["choices"];
                    string use = shop["use"].ToString(),
                        shopNeed = shop["need"].ToString(),
                        shopText = shop["text"].ToString(),
                        shopId = shop["id"].ToString(),
                        flagName_Time = "flag:" + shopId + "_times", // 新设的购买次数变量flag:xxx
                        flagName_Price = "flag:" + shopId + "_price"; // 新设的价格变量flag:xxx
                    string priceStr = shopNeed.Replace("times", flagName_Time); //用新变量名取代times之后的商店价格字符串
                    shop["text"] = StringUtils.ReplaceInBetweenCurlyBraces(shopText, "times", flagName_Time);
                    shop["text"] = StringUtils.ReplaceInBetweenCurlyBraces(shopText, "need", flagName_Price);
                    shop["disablePreview"] = false;

                    if (use == "experience")
                    {
                        use = "exp";
                    }

                    for (int j = 0; j < choiceArr.Count; j++)
                    {
                        JObject choice = (JObject)choiceArr[j];
                        string requirement;
                        string? choiceNeed = null;
                        if (choice.ContainsKey("need"))
                        {
                            choiceNeed = choice["need"].ToString().Replace("times", flagName_Time);
                            requirement = "status:" + use + ">=" + choiceNeed;
                        }
                        else
                        {
                            requirement = "status:" + use + ">=" + priceStr;
                        }
                        choice["need"] = requirement; // 单个选项的使用条件

                        JArray newAction = new JArray();
                        JObject setPrice = StringUtils.getAddValueJson(flagName_Price, choiceNeed != null ? choiceNeed : priceStr, "="),
                            deductMoney = StringUtils.getAddValueJson("status:" + use, flagName_Price, "-="),
                            addTime = StringUtils.getAddValueJson(flagName_Time, "1", "+=");
                        newAction.Add(setPrice);
                        newAction.Add(deductMoney);
                        newAction.Add(addTime);

                        string oldEffect = choice["effect"].ToString();
                        var newEffectJArray = StringUtils.doEffect(oldEffect);
                        newAction.Merge(newEffectJArray);
                        choice["action"] = newAction; //单个选项使用时执行的事件
                    }
                }
            }
            #endregion

            if (valuesData["statusCanvasRowsOnMobile"] == null)
            {
                valuesData["statusCanvasRowsOnMobile"] = flagsData["statusCanvasRowsOnMobile"];
            }
            flagsData.Remove("statusCanvasRowsOnMobile");

            if (valuesData["redGem"] == null)
            {
                valuesData["redGem"] = valuesData["redJewel"];
            }
            valuesData.Remove("redJewel");

            if (valuesData["blueGem"] == null)
            {
                valuesData["blueGem"] = valuesData["blueJewel"];
            }
            valuesData.Remove("blueJewel");

            if (valuesData["greenGem"] == null)
            {
                valuesData["greenGem"] = valuesData["greenJewel"];
            }
            valuesData.Remove("greenJewel");

            valuesData.Remove("moveSpeed");
            valuesData.Remove("floorChangeTime");

            string[] statusList = [
                "enableHP",
                "enableAtk",
                "enableDef",
                "enableFloor",
                "enableName",
                "enableLv",
                "enableHPMax",
                "enableMana",
                "enableMDef",
                "enableMoney",
                "enableExp",
                "enableLevelUp",
                "levelUpLeftMode",
                "enableKeys",
                "enableGreenKey",
                "enablePZF",
                "enableDebuff",
                "enableSkill",
            ];

            if (flagsData["statusBarItems"] == null)
            {
                JArray statusBarItemsArr = new JArray();
                foreach (string status in statusList)
                {
                    if (flagsData.ContainsKey(status) && flagsData[status].Value<bool>() == true)
                    {
                        statusBarItemsArr.Add(status);
                    }
                    else if (new string[] { "enableHP", "enableAtk", "enableDef" }.Contains(status))
                    {// hp,atk,def为2.7前默认显示的变量，必须显示
                        statusBarItemsArr.Add(status);
                    }
                    else if (status == "enableExp" && flagsData["enableExperience"].Value<bool>() == true)
                    {// experience更新为exp
                        statusBarItemsArr.Add(status);
                    }
                }
                flagsData["statusBarItems"] = statusBarItemsArr;
            }
            foreach (string status in statusList)
            {
                flagsData.Remove(status);
            }
            flagsData.Remove("pickaxeFourDirections");
            flagsData.Remove("bombFourDirections");
            flagsData.Remove("snowFourDirections");
            flagsData.Remove("bigKeyIsBox");
            flagsData.Remove("equipment");
            flagsData.Remove("iconInEquipbox");
            flagsData.Remove("hatredDecrease");
            flagsData.Remove("betweenAttackCeil");
            flagsData.Remove("startDirectly");
            flagsData.Remove("enableDisabledShop");
            flagsData.Remove("checkConsole");
        }
    }
}
