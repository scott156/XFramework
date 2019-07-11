using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using XFramework.Core.Abstractions.Error;
using XFramework.Dal.DalClient;
using XFramework.Dal.Data;
using XFramework.Dal.Error;
using XFramework.Dal.Interface;
using XFramework.Dal.Orm;
using XFramework.Dal.SqlBuilder;

namespace XFramework.Dal
{
    public class TableDao : ITableDao
    {
        private readonly string databaseName;

        public TableDao(string databaseName)
        {
            this.databaseName = databaseName;
        }

        public TableDao()
        {
            // 如果为空，默认使用泛型参数T的数据库名称 
        }

        /// <summary>
        /// 条件查询
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="criteria"></param>
        /// <returns></returns>
        public async Task<List<T>> Query<T>(T criteria, int? recordCount = null) where T : class, new()
        {
            var mapper = EntityMetaDataMapper.GetInstance();

            // 获取组装好的sql和参数
            (string sql, List<DatabaseParameter> parameters) =
                SqlBuilderFactory.GetInstance<T>().BuildSelect(criteria, recordCount);

            var logicName = databaseName;
            if (string.IsNullOrWhiteSpace(logicName))
            {
                logicName = mapper.Get<T>().DatabaseName;
            }

            var client = DalClientFactory.GetClient(logicName);
            var dataSet = await client.Query(sql, parameters);

            return mapper.Convert<T>(dataSet);
        }

        /// <summary>
        /// 根据主键查询，主键类型为string
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public async Task<T> QueryByPk<T>(string value) where T : class, new()
        {
            return await QueryByPk(BuildCriteria<T, string>(value));
        }

        /// <summary>
        /// 根据主键查询，主键类型为long
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public async Task<T> QueryByPk<T>(long value) where T : class, new()
        {
            return await QueryByPk(BuildCriteria<T, long>(value));
        }

        /// <summary>
        /// 根据主键查询，主键类型自定义
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="criteria"></param>
        /// <returns></returns>
        public async Task<T> QueryByPk<T>(T criteria) where T : class, new()
        {
            if (criteria == null)
            {
                throw new DalException(ErrorCode.PrimaryKeyIsEmpty, "Query criteria is empty");
            }

            var info = EntityMetaDataMapper.GetInstance().Get<T>();
            var primaryKeys = info.Columns.FindAll(p => p.Attribute.PrimaryKey == true);

            // 再一次校验是否是所有的主键都进行了设置
            foreach (var pk in primaryKeys)
            {
                if (pk.ColumnProperty.GetValue(criteria) == null)
                {
                    throw new DalException(ErrorCode.PrimaryKeyIsEmpty, $"Query primary key is empty : {pk.Attribute.Name}");
                }
            }

            var list = await Query<T>(criteria);
            if (list == null || list.Count == 0) return null;

            return list[0];
        }

        private T BuildCriteria<T, PKType>(PKType value) where T : class, new()
        {
            if (value == null)
            {
                throw new DalException(ErrorCode.PrimaryKeyIsEmpty, "Primary key is empty");
            }

            var info = EntityMetaDataMapper.GetInstance().Get<T>();

            // 检查主键类型，必须为字符串
            var identity = info.Columns.Find(p => p.Attribute.PrimaryKey == true);
            var property = identity.ColumnProperty.PropertyType;

            if (property == typeof(PKType) ||
                (property.IsGenericType &&
                property.GetGenericTypeDefinition() == typeof(Nullable<>) &&
                property.GetGenericArguments()[0] == typeof(PKType)))
            {
                var criteria = new T();
                identity.ColumnProperty.SetValue(criteria, value);

                return criteria;
            }

            throw new DalException(ErrorCode.PrimaryKeyIsEmpty,
                $"Primary key type mismatch, source type : {typeof(PKType)}, " +
                $"target type : {identity.ColumnProperty.PropertyType}");
        }

        private string GetLogicDatabaseName<T>() where T : class, new()
        {
            var entityInfo = EntityMetaDataMapper.GetInstance().Get<T>();

            var logicName = databaseName;
            if (string.IsNullOrWhiteSpace(logicName))
            {
                logicName = entityInfo.DatabaseName;
            }

            return logicName;
        }

        public async Task<int> Insert<T>(T entity) where T : class, new()
        {
            // 获取组装好的sql和参数
            (string sql, List<DatabaseParameter> parameters) =
                SqlBuilderFactory.GetInstance<T>().BuildInsert(entity);

            var client = DalClientFactory.GetClient(GetLogicDatabaseName<T>());
            // 将主键返回给实体
            var hints = DalHints.Create();

            // 如果当前实例含有自增主键，且自增主键的实体不为空，那么需要数据库返回自增住居
            var identity = EntityMetaDataMapper.GetInstance().Get<T>().Columns.Find(p => p.Attribute.Identity);
            
            if (identity != null && identity.Attribute.Identity && identity.ColumnProperty.GetValue(entity) == null)
            {
                hints.Add(DalHint.SetIdentity);
            }

            var result = await client.Execute(sql, parameters, hints);

            // 如果数据库返回了主键，那么将主键设置回实体
            // 暂时先这么写
            if (result.Identity > 0)
            {
                if (identity.ColumnProperty.PropertyType == typeof(int?))
                {
                    int.TryParse(result.Identity.ToString(), out var value);
                    identity.ColumnProperty.SetValue(entity, value);
                }
                else
                {
                    long.TryParse(result.Identity.ToString(), out var value);
                    identity.ColumnProperty.SetValue(entity, value);
                }
            }

            return result.ReturnCode;
        }

        public async Task<int> Update<T>(T entity) where T : class, new()
        {
            (string sql, List<DatabaseParameter> parameters) =
                SqlBuilderFactory.GetInstance<T>().BuildUpdate(entity);

            var result = await DalClientFactory.GetClient(GetLogicDatabaseName<T>())
                .Execute(sql, parameters, null);

            return result.ReturnCode;
        }

        public async Task<int> Delete<T>(T entity) where T : class, new()
        {
            (string sql, List<DatabaseParameter> parameters) =
                SqlBuilderFactory.GetInstance<T>().BuildDelete(entity);

            var result = await DalClientFactory.GetClient(GetLogicDatabaseName<T>()).Execute(sql, parameters, null);

            return result.ReturnCode;
        }

        public async Task<List<T>> QueryLike<T>(IDatabaseParameterLink link, int? recordCount = null) where T : class, new()
        {
            var mapper = EntityMetaDataMapper.GetInstance();

            // 获取组装好的sql和参数
            (string sql, List<DatabaseParameter> parameters) =
                SqlBuilderFactory.GetInstance<T>().BuildSelect<T>(link, recordCount);
            var dataSet = await DalClientFactory.GetClient(GetLogicDatabaseName<T>()).Query(sql, parameters);

            return mapper.Convert<T>(dataSet);
        }

        public async Task<T> QueryByPk<T>(int value) where T : class, new()
        {
            return await QueryByPk(BuildCriteria<T, int>(value));
        }
    }
}
