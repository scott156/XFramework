using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XFramework.Core.Abstractions.Error;
using XFramework.Dal.Error;
using XFramework.Dal.Interface;
using XFramework.Dal.Orm;

namespace XFramework.Dal.SqlBuilder
{
    public class MySqlBuilder : ISqlBuilder
    {
        public (string, List<DatabaseParameter>) BuildDelete<T>(T criteria)
        {
            if (criteria == null)
            {
                throw new DalException(ErrorCode.InvalidSqlParameters, "Invalid sql parameters, parameter is null");
            }

            var info = EntityMetaDataMapper.GetInstance().Get<T>();
            var sb = new StringBuilder($"DELETE FROM {info.TableName} ");
            (string sql, List<DatabaseParameter> pkParameters) = GeneratePrimaryKeyClause(criteria);
            sb.Append(sql);

            return (sb.ToString(), pkParameters);
        }

        public (string, List<DatabaseParameter>) BuildInsert<T>(T criteria)
        {
            if (criteria == null)
            {
                throw new DalException(ErrorCode.InvalidSqlParameters, "Invalid sql parameters, parameter is null");
            }

            var info = EntityMetaDataMapper.GetInstance().Get<T>();

            var sb = new StringBuilder($"INSERT INTO {info.TableName} (");
            var questionMarks = new List<string>();
            var parameters = new List<DatabaseParameter>();

            foreach (var column in info.Columns)
            {
                var value = column.ColumnProperty.GetValue(criteria);
                if (value == null) continue;

                sb.Append($"`{column.Attribute.Name}`, ");
                questionMarks.Add("?");
                parameters.Add(new DatabaseParameter()
                {
                    Name = column.Attribute.Name,
                    Value = value
                });
            };

            // 删除最后一个,
            sb.Remove(sb.Length - 2, 2);
            sb.Append($") VALUES ({string.Join(",", questionMarks)});");

            return (sb.ToString(), parameters);
        }

        public (string, List<DatabaseParameter>) BuildSelect<T>(T criteria, int? recordCount) where T : new()
        {
            var info = EntityMetaDataMapper.GetInstance().Get<T>();

            var sb = new StringBuilder(GetPrimarySql<T>());
            if (criteria == null)
            {
                sb.Append(";");
                return (sb.ToString(), null);
            }

            var parameters = new List<DatabaseParameter>();
            var isFirstCondition = true;
            foreach (var column in info.Columns)
            {
                var value = column.ColumnProperty.GetValue(criteria);
                if (value == null) continue;

                if (isFirstCondition == true)
                {
                    sb.Append(" WHERE ");
                    isFirstCondition = false;
                }
                else
                {
                    sb.Append(" AND ");
                }

                parameters.Add(new DatabaseParameter
                {
                    Name = column.Attribute.Name,
                    Value = value
                });

                sb.Append($"`{column.Attribute.Name}` = ?");
            }

            if (recordCount != null && recordCount > 0)
            {
                sb.Append($" limit {recordCount};");
            }
            else
            {
                sb.Append(";");
            }

            return (sb.ToString(), parameters);
        }

        private string GetPrimarySql<T>()
        {
            var info = EntityMetaDataMapper.GetInstance().Get<T>();

            var sb = new StringBuilder("SELECT ", 200);
            foreach (var column in info.Columns)
            {
                sb.Append($"`{column.Attribute.Name}`, ");
            }

            // 删除最后一个,
            sb.Remove(sb.Length - 2, 2);
            sb.Append($" FROM `{info.TableName}` ");

            return sb.ToString();
        }

        public (string, List<DatabaseParameter>) BuildUpdate<T>(T criteria)
        {
            if (criteria == null)
            {
                throw new DalException(ErrorCode.InvalidSqlParameters, "Invalid sql parameters, parameter is null");
            }

            var info = EntityMetaDataMapper.GetInstance().Get<T>();

            var sb = new StringBuilder($"UPDATE {info.TableName} SET ");
            var parameters = new List<DatabaseParameter>();

            foreach (var column in info.Columns)
            {
                // 主键不参与更新
                if (column.Attribute.PrimaryKey == true) continue;

                var value = column.ColumnProperty.GetValue(criteria);
                if (value == null) continue;

                sb.Append($"`{column.Attribute.Name}` = ?, ");
                parameters.Add(new DatabaseParameter()
                {
                    Name = column.Attribute.Name,
                    Value = value
                });
            };

            // 删除最后一个,
            sb.Remove(sb.Length - 2, 2);
            (string sql, List <DatabaseParameter> pkParameters) = GeneratePrimaryKeyClause(criteria);
            sb.Append(sql);
            sb.Append(";");
            parameters.AddRange(pkParameters);

            return (sb.ToString(), parameters);
        }

        /// <summary>
        /// 获取Where条件只有主键的查询条件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="criteria"></param>
        /// <returns></returns>
        private static (string, List<DatabaseParameter>) GeneratePrimaryKeyClause<T>(T criteria)
        {
            var parameters = new List<DatabaseParameter>();
            var sb = new StringBuilder(" WHERE ");
            var info = EntityMetaDataMapper.GetInstance().Get<T>();

            var primaryKeys = info.Columns.FindAll(p => p.Attribute.PrimaryKey == true);
            foreach (var pk in primaryKeys)
            {
                sb.Append($"`{pk.Attribute.Name}` = ?");
                sb.Append(" AND ");

                // 主键不能为空
                var value = pk.ColumnProperty.GetValue(criteria);
                if (value == null)
                {
                    throw new DalException(ErrorCode.InvalidSqlParameters,
                        $"Primary key can not be empty : {pk.Attribute.Name}");
                };

                parameters.Add(new DatabaseParameter()
                {
                    Name = pk.Attribute.Name,
                    Value = value
                });
            }

            sb.Remove(sb.Length - 4, 4);
            sb.Append(";");

            return (sb.ToString(), parameters);
        }

        public (string, List<DatabaseParameter>) BuildSelect<T>(IDatabaseParameterLink link, int? recordCount = null) where T : new()
        {
            // 先组装出基本Sql语句
            var sql = GetPrimarySql<T>();

            if (link == null)
            {
                if (recordCount != null && recordCount > 0)
                {
                    sql += $" limit {recordCount};";
                }

                return (sql, null);
            }

            (var subsql, var parameters) = Build(link);
            sql = string.Concat(sql, " WHERE ", subsql);
            if (recordCount != null && recordCount > 0)
            {
                sql += $" limit {recordCount};";
            }

            return (sql, parameters);
        }

        public (string, List<DatabaseParameter>) Build(IDatabaseParameterLink link)
        {
            if (link == null)
            {
                return (string.Empty, null);
            }

            List<DatabaseParameter> list = new List<DatabaseParameter>();
            
            var sql = new StringBuilder();
            if (link is DatabaseParameterLink parameterLink)
            {
                var operation = parameterLink.Parameter.Operation;
                if (parameterLink.Parameter.Value == null)
                {
                    operation = "IS NULL";
                }
                else if (typeof(ICollection).IsAssignableFrom(parameterLink.Parameter.Value.GetType()))
                {
                    var collection = parameterLink.Parameter.Value as ICollection;
                    var s = new StringBuilder(collection.Count * 2);
                    foreach (var item in collection)
                    {
                        s.Append("?");
                        s.Append(",");
                    }
                    s.Remove(s.Length - 1, 1);

                    operation = $"IN ({s.ToString()})";
                }
                else
                {
                    operation = $"{parameterLink.Parameter.Operation} ?";
                }

                sql.Append($"{link.Releation} `{parameterLink.Parameter.Name}` {operation} ");
                list.Add(parameterLink.Parameter);
            }
            else if (link is DatabaseParameterLinkCollection parameterCollection)
            {
                sql.Append(parameterCollection.Releation);
                sql.Append(" (");

                (var contentsql, var contentparam) = Build(parameterCollection.Content);

                sql.Append(contentsql);
                if (contentparam != null)
                {
                    list.AddRange(contentparam);
                }
                sql.Append(") ");
            }

            (var subsql, var param) = Build(link.Next);
            sql.Append(subsql);
            if (param != null)
            {
                list.AddRange(param);
            }

            return (sql.ToString(), list);
        }
    }
}
