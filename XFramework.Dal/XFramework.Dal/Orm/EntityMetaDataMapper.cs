using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using XFramework.Core.Abstractions.Error;
using XFramework.Core.Abstractions.Logger;
using XFramework.Core.Abstractions.Utility;
using XFramework.Dal.Error;

namespace XFramework.Dal.Orm
{
    public class EntityMetaDataMapper : Singleton<EntityMetaDataMapper>
    {
        private readonly ConcurrentDictionary<Type, EntityInfo> colunms 
            = new ConcurrentDictionary<Type, EntityInfo>();

        private static readonly ILogger logger = LogProvider.Create(typeof(EntityMetaDataMapper));

        /// <summary>
        /// 查询实体信息
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public EntityInfo Get<T>()
        {
            return colunms.GetOrAdd(typeof(T), p =>
            {
                var tableAttribute = p.GetCustomAttribute<TableAttribute>();
                if (tableAttribute == null || string.IsNullOrWhiteSpace(tableAttribute.Name))
                {
                    throw new DalException(ErrorCode.InvalidDalEntity, 
                        $"Table attribute is not defined for entity : {p.FullName}");
                }

                var databaseAttribute = p.GetCustomAttribute<DatabaseAttribute>();
                if (databaseAttribute == null || string.IsNullOrWhiteSpace(databaseAttribute.Name))
                {
                    throw new DalException(ErrorCode.InvalidDalEntity, 
                        $"Database attribute is not defined for entity : {p.FullName}");
                }

                var entity = new EntityInfo()
                {
                    DatabaseName = databaseAttribute.Name,
                    TableName = tableAttribute.Name,
                    Columns = new List<ColumnInfo>()
                };

                foreach (var property in p.GetProperties())
                {
                    var attr = property.GetCustomAttribute<ColumnAttribute>();
                    if (attr == null || string.IsNullOrWhiteSpace(attr.Name))
                    {
                        throw new DalException(ErrorCode.InvalidDalEntity,
                            $"Column attribute is not defined : {p.FullName}");
                    }

                    entity.Columns.Add(new ColumnInfo()
                    {
                        Attribute = attr,
                        ColumnProperty = property
                    });
                }

                return entity;
            });
        }

        /// <summary>
        /// 将DataSet转换为实体
        /// 可以技术优化，提高性能
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataSet"></param>
        /// <returns></returns>
        public List<T> Convert<T>(DataSet dataSet) where T : class, new()
        {
            if (dataSet == null || dataSet.Tables == null ||dataSet.Tables.Count == 0)
            {
                return null;
            }

            var table = dataSet.Tables[0];
            if (table.Rows.Count == 0)
            {
                return null;
            }

            var entityInfo = Get<T>();
           
            var list = new List<T>();
            foreach (DataRow row in table.Rows)
            {
                var entity = new T();
                foreach (var column in entityInfo.Columns)
                {
                    var value = row[column.Attribute.Name];
                    if (value is DBNull)
                    {
                        continue;
                    }

                    if (column.ColumnProperty.PropertyType == typeof(string))
                    {
                        column.ColumnProperty.SetValue(entity, value.ToString());
                    }
                    else
                    {
                        column.ColumnProperty.SetValue(entity, value);
                    }
                }

                list.Add(entity);
            }

            return list;
        }
    }



    public class EntityInfo
    {
        public string TableName { get; set; }
        public string DatabaseName { get; set; }
        public List<ColumnInfo> Columns { get; set; }
    }

    public class ColumnInfo
    {
        public PropertyInfo ColumnProperty { get; set; }
        public ColumnAttribute Attribute { get; set; }
    }
}
