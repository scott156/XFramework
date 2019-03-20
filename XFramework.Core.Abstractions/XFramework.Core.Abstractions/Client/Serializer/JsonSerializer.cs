using Newtonsoft.Json;
using System;
using System.Text;

namespace XFramework.Core.Abstractions.Client.Serializer
{
    /// <summary>
    /// 用于序列化和反序列化对象
    /// </summary>
    public class JsonSerializer : IServiceSerializer
    {
        public T Deserialize<T>(byte[] response)
        {
            return JsonConvert.DeserializeObject<T>(UTF8Encoding.UTF8.GetString(response));
        }

        public byte[] Serialize<T>(T request)
        {
            return UTF8Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(request, Formatting.None));
        }
    }
}
