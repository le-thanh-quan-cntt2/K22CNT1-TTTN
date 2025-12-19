using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace MenFashionProject.Helpers
{
    public static class MySessionHelper
    {
        // Lưu object vào Session
        public static void SetObjectAsJson(this ISession session, string key, object value)
        {
            session.SetString(key, JsonConvert.SerializeObject(value));
        }

        // Lấy object từ Session
        public static T GetObjectFromJson<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            return value == null ? default(T) : JsonConvert.DeserializeObject<T>(value);
        }
    }
}