using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using XFramework.Core.Abstractions.Configuration;
using XFramework.Core.Abstractions.Error;
using XFramework.Core.Abstractions.Logger;
using XFramework.Dal.Error;
using XFramework.Dal.Interface;

namespace XFramework.Dal.DalClient
{
    /// <summary>
    /// 获取Dal的工厂类
    /// </summary>
    public static class DalClientFactory
    {
        private static ILogger logger = LogProvider.Create(typeof(DalClientFactory));
        
        private static readonly Dictionary<DatabaseTypeEnum, Type> dalClients = new Dictionary<DatabaseTypeEnum, Type>()
        {
            { DatabaseTypeEnum.MySql, typeof(MySqlDalClient) }
        };

        private static ConcurrentDictionary<string, IDalClient> clients = new ConcurrentDictionary<string, IDalClient>();

        public static IDalClient GetClient(string dbName)
        {
            if (string.IsNullOrWhiteSpace(dbName))
            {
                throw new DalException(ErrorCode.InitDalClientFailed,
                    "Database name is empty");
            }
            
            // 每一个数据库逻辑名称，对应一个Dal的实例
            var logicName = dbName.Trim().ToLower();
            return clients.GetOrAdd(logicName, p =>
            {
                var setting = AppSetting.GetInstance();
                if (setting.DatabaseSets == null || setting.DatabaseSets.Count == 0)
                {
                    throw new DalException(ErrorCode.InitDalClientFailed,
                        "Initialize dal client failed, could not find databasesets in appsettings.json");
                }

                var set = setting.DatabaseSets.Find(x => logicName.Equals(x.Name, StringComparison.InvariantCultureIgnoreCase)) ??
                    throw new DalException(ErrorCode.InitDalClientFailed,
                        $"Initialize dal client failed, invalid database name : {dbName}");

                logger.Info($"Create dal client, logic name : {set.Name}, database type : {set.DatabaseType}");

                if (dalClients.ContainsKey(set.DatabaseType) == false)
                {
                    logger.Error($"Unsupported database type : {set.DatabaseType} of {set.Name}");
                    throw new DalException(ErrorCode.UnsupportedDatabaseType,
                        $"Initialize dal client failed, invalid database name : {dbName}");
                }

                return (IDalClient)Activator.CreateInstance(dalClients[set.DatabaseType], set);
            });           
        }
    }
}
