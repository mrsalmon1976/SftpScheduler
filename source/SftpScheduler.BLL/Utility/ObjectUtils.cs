using Newtonsoft.Json;

namespace SftpScheduler.BLL.Utility
{
    public class ObjectUtils
    {
        public static T? Clone<T>(T obj) where T : class
        {
            string json = JsonConvert.SerializeObject(obj);
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}
