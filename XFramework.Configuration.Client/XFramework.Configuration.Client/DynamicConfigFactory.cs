using System.Collections.Concurrent;
using XFramework.Core.Abstractions.Error;

namespace XFramework.Configuration.Client
{
    public static class DynamicConfigFactory
    {
        private static readonly ConcurrentDictionary<string, IDynamicConfigProvider> map = 
            new ConcurrentDictionary<string, IDynamicConfigProvider>();
        
        public static IDynamicConfigProvider GetInstance(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new FrameworkException(ErrorCode.InvalidConfigurationFile, "Configuration filename is empty");
            }
            
            return map.GetOrAdd(fileName, (file) => new DynamicConfigProvider(file.Trim().ToLower()));
        }
    }
}
