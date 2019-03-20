using System.Collections.Generic;
using XFramework.Core.Abstractions.Configuration;
using XFramework.Core.Abstractions.Error;
using XFramework.Dal.DalClient;
using XFramework.Dal.Error;
using XFramework.Dal.Interface;
using XFramework.Dal.Orm;

namespace XFramework.Dal.SqlBuilder
{
    public static class SqlBuilderFactory
    {
        private static readonly Dictionary<DatabaseTypeEnum, ISqlBuilder> sqlBuilders = 
            new Dictionary<DatabaseTypeEnum, ISqlBuilder>()
        {
            { DatabaseTypeEnum.MySql, new MySqlBuilder() }
        };

        /// <summary>
        /// 根据泛型T，组装该类型的查询语句
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static ISqlBuilder GetInstance<T>()
        {
            var info = EntityMetaDataMapper.GetInstance().Get<T>();
            var dalClient = DalClientFactory.GetClient(info.DatabaseName);
            
            if (!sqlBuilders.ContainsKey(dalClient.DatabaseType))
            {
                throw new DalException(ErrorCode.SqlBuilderNotFound, $"There's no sql builder for {dalClient.DatabaseType}");
            }

            return sqlBuilders[dalClient.DatabaseType];
        }
    }
}
