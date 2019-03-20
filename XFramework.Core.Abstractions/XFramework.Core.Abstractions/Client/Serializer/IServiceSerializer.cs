using System;
using System.Collections.Generic;
using System.Text;

namespace XFramework.Core.Abstractions.Client.Serializer
{
    /// <summary>
    /// 用于序列化和反序列化对象
    /// </summary>
    public interface IServiceSerializer
    {
        byte[] Serialize<T>(T request);

        T Deserialize<T>(byte[] response);
    }
}
