using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace H5MotaUpdate.ViewModels.Utils
{
    internal static class MotaEventParser
    {
        public static void parseOneMotaEvent(JObject motaEvent)
        {
            // 如果type为openShop，添加一个open:true的键
            if (motaEvent["type"] is JValue typeValue && typeValue.Value is string typeString && typeString == "openShop")
            {
                motaEvent["open"] = true;
            }
        }

        static void parseMotaEventArr(JArray motaEventArr)
        {
            foreach (JToken motaEventToken in motaEventArr)
            {
                // 只处理对象事件 字符串不计
                if (motaEventToken.Type == JTokenType.Object)
                {
                    JObject motaEvent = (JObject)motaEventToken;
                    parseOneMotaEvent(motaEvent);
                }
            }
        }

        public static void parseMotaEvent(JToken motaEvent)
        {
            if (motaEvent is JArray motaEventArr)
            {
                parseMotaEventArr(motaEventArr);
            }
            else if (motaEvent is JObject motaEventObj)
            { // 说明有覆盖触发器等，data中才是真正的事件
                if (motaEvent["data"] is JArray dataArr)
                {
                    parseMotaEventArr(dataArr);
                }
            }
        }
    }
}
