using Newtonsoft.Json;
using System.Collections.Generic;

namespace DtCoCo.ServiceFabric.Utility
{
    public static class JsonHelper
    {
        /// <summary>
        /// json格式转换成List
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="strJson"></param>
        /// <returns></returns>
        public static List<T> FromJsonList<T>(string strJson) where T : class
        {
            if (!string.IsNullOrEmpty(strJson))
                return JsonConvert.DeserializeObject<List<T>>(strJson);
            return null;
        }

        public static T ToObject<T>(this string Json)
        {
            return Json == null ? default(T) : JsonConvert.DeserializeObject<T>(Json);
        }

    }
}