using Newtonsoft.Json.Linq;
using System.IO;
using System.Text;


// 功能：复制project/enemy.js文件
// 样板版本在2.9以下 按照如下逻辑改写相关数据：敌人具有光环属性:enemy.haloRange = enemy.range;同时若不具有领域enemy.range=null
// 敌人具有光环属性:enemy.haloSquare = enemy.zoneSquare;同时若不具有领域enemy.zoneSquare=null
// 敌人具有光环属性:enemy.haloAdd = enemy.Add;同时若不具有吸血enemy.add=null
// 敌人具有反击属性: enemy.counterAttack = enemy.atkValue;同时若不具有退化 enemy.atkValue = null
// 敌人具有破甲属性: enemy.breakArmor = enemy.defValue;同时若不具有退化 enemy.defValue = null
// 敌人具有领域、阻击、激光属性 分别令enemy.zone/repulse/laser 
// enemy.value = null
namespace H5MotaUpdate.ViewModels
{
    internal class EnemysJSMigrator
    {
        string sourcePath, destPath;
        Version version;
        readonly string FILENAME = "enemys.js",
            DATANAME = "enemys_fcae963b_31c9_42b4_b48c_bb48d09f3f80";

        /// <summary>
        /// 请输入新旧Project文件夹的路径
        /// </summary>
        public EnemysJSMigrator(string oldProjectDirectory, string newProjectDirectory, Version ver)
        {
            sourcePath = System.IO.Path.Combine(oldProjectDirectory, FILENAME);
            destPath = System.IO.Path.Combine(newProjectDirectory, FILENAME);
            version = ver;
        }

        public void Migrate()
        {
            try
            {
                if (version.CompareTo(new Version(2, 9)) >= 0)
                {
                    MigrateDirect();
                }
                else
                {
                    JObject jsonObject = StringUtils.getValidJson(sourcePath);
                    Convert(jsonObject);
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

        void Convert(JObject jsonObject)
        {
            if (version.CompareTo(new Version(2, 7)) < 0)
            {
                Convert_before2_7(jsonObject);
            }
            Convert_before2_9(jsonObject);
        }

        static void Convert_before2_9(JObject jsonObject)
        {
            foreach (JProperty prop in jsonObject.Properties())
            {

                JObject enemyData = (JObject)prop.Value;
                JToken enemySpecial = enemyData["special"];
                if (enemySpecial is JValue specialValue)
                {
                    transferSpecialValue(specialValue, enemyData);
                }
                else if (enemySpecial is JArray specialValueArr)
                {
                    transferSpecialArr(specialValueArr, enemyData);
                }
            }
        }

        static void Convert_before2_7(JObject jsonObject)
        {
            foreach (JProperty prop in jsonObject.Properties())
            {

                JObject enemyData = (JObject)prop.Value;
                JValue enemyExperience = (JValue)enemyData["experience"];
                if (enemyExperience != null)
                {
                    enemyData["exp"] = enemyExperience;
                }
                enemyData.Remove("experience");
            }

        }

        /// <summary>
        /// 根据单个特殊属性的JValue改写敌人数据
        /// </summary>
        static void transferSpecialValue(JValue specialValue, JObject enemy)
        {
            if (specialValue.Type == JTokenType.Integer)
            {
                switch ((int)specialValue)
                {
                    case 7: //破甲
                        enemy["counterAttack"] = enemy["atkValue"];
                        enemy.Remove("atkValue");
                        break;
                    case 8: //反击
                        enemy["breakArmor"] = enemy["defValue"];
                        enemy.Remove("defValue");
                        break;
                    case 9: //净化
                        enemy["purify"] = enemy["n"];
                        enemy.Remove("purify");
                        break;
                    case 11://吸血
                        enemy["vampire"] = enemy["value"];
                        enemy.Remove("value");
                        break;
                    case 15://领域
                        enemy["zone"] = enemy["value"];
                        enemy.Remove("value");
                        break;
                    case 18://阻击
                        enemy["repulse"] = enemy["value"];
                        enemy.Remove("value");
                        break;
                    case 24://激光
                        enemy["laser"] = enemy["value"];
                        enemy.Remove("value");
                        break;
                    case 25: //光环
                        enemy["haloRange"] = enemy["range"];
                        enemy["haloSquare"] = enemy["zoneSquare"];
                        enemy["haloAdd"] = enemy["add"];
                        enemy["hpBuff"] = enemy["value"];
                        enemy["atkBuff"] = enemy["atkValue"];
                        enemy["defBuff"] = enemy["defValue"];
                        enemy.Remove("range");
                        enemy.Remove("zoneSquare");
                        enemy.Remove("add");
                        break;
                }
            }
        }

        /// <summary>
        /// 根据特殊属性列表JArray改写敌人数据
        /// </summary>
        static void transferSpecialArr(JArray specialValueArr, JObject enemy)
        {
            HashSet<int> specialValuesToCheck = new HashSet<int> { 7, 8, 15, 18, 24, 25 };
            HashSet<int> foundSpecialValues = new HashSet<int>();

            foreach (JToken item in specialValueArr)
            {
                if (item.Type == JTokenType.Integer && specialValuesToCheck.Contains((int)item))
                {
                    foundSpecialValues.Add((int)item);
                }
            }

            if (foundSpecialValues.Contains(7))
            {
                enemy["counterAttack"] = enemy["atkValue"];
                if (!foundSpecialValues.Contains(21)) enemy.Remove("atkValue");
            }
            if (foundSpecialValues.Contains(8))
            {
                enemy["breakArmor"] = enemy["defValue"];
                if (!foundSpecialValues.Contains(21)) enemy.Remove("defValue");
            }
            if (foundSpecialValues.Contains(9))
            {
                enemy["purify"] = enemy["n"];
                if (!foundSpecialValues.Contains(6)) enemy.Remove("purify");
            }
            if (foundSpecialValues.Contains(11))
            {
                enemy["vampire"] = enemy["value"];
                if (!foundSpecialValues.Contains(21)) enemy.Remove("value");
            }
            if (foundSpecialValues.Contains(15))
            {
                enemy["zone"] = enemy["value"];
                enemy.Remove("value");
            }
            if (foundSpecialValues.Contains(18))
            {
                enemy["repulse"] = enemy["value"];
                enemy.Remove("value");
            }
            if (foundSpecialValues.Contains(24))
            {
                enemy["laser"] = enemy["value"];
                enemy.Remove("value");
            }
            if (foundSpecialValues.Contains(25))
            {
                enemy["haloRange"] = enemy["range"];
                enemy["haloSquare"] = enemy["zoneSquare"];
                enemy["haloAdd"] = enemy["add"];
                enemy["hpBuff"] = enemy["value"];
                enemy["atkBuff"] = enemy["atkValue"];
                enemy["defBuff"] = enemy["defValue"];
                if (!foundSpecialValues.Contains(11)) enemy.Remove("add");
                if (!foundSpecialValues.Contains(15))
                {
                    enemy.Remove("range");
                    enemy.Remove("zoneSquare");
                }
            }
        }
    }
}
