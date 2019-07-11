using MySql.Data.MySqlClient;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using XFramework.Configuration.Client;
using XFramework.Core.Abstractions.Configuration;
using XFramework.Dal.Data;
using XFramework.Dal.Orm;

namespace XFramework.Dal.DalClient
{
    public class MySqlDalClient : IDalClient
    {
        private readonly DatabaseSetInfo databaseSet;

        /// <summary>
        /// 获取数据库类型
        /// </summary>
        public DatabaseTypeEnum DatabaseType => databaseSet.DatabaseType;

        /// <summary>
        /// 从配置中心获取数据库的链接字符串
        /// </summary>
        private readonly IDynamicConfigProvider configProvider = DynamicConfigFactory.GetInstance("dal.connection.properties");

        public MySqlDalClient(DatabaseSetInfo databaseSet)
        {
            this.databaseSet = databaseSet;
        }

        public async Task<DataSet> Query(string sql, List<DatabaseParameter> parameters)
        {
            return await Execute(sql, parameters);
        }

        /// <summary>
        /// 执行查询语句
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private async Task<DataSet> Execute(string sql, List<DatabaseParameter> parameters)
        {
            using (var conn = new MySqlConnection(GetConnectionString()))
            {
                // 打开数据库链接
                await conn.OpenAsync();

                using (var command = BuildCommand(sql, parameters, conn))
                {
                    using (var adapter = new MySqlDataAdapter(command))
                    {
                        var dataSet = new DataSet();
                        // 填充数据到DataSet
                        await adapter.FillAsync(dataSet);                     

                        return dataSet;
                    }
                }
            }
        }
        
        private string GetConnectionString()
        {
            var setting = databaseSet.Connections[0];

            // 优先级先看ConnectionString，如果没有配置，那么使用配置中心的Key
            if (!string.IsNullOrEmpty(setting.ConnectionString))
            {
                return setting.ConnectionString;
            }

            return configProvider.GetStringProperty(setting.Key);
        }

        /// <summary>
        /// 执行数据库的更新操作
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <param name="dalHints"></param>
        /// <returns></returns>
        private async Task<ExecutionResult> ExecuteNonQuery(string sql, List<DatabaseParameter> parameters, DalHints dalHints)
        {
            using (var conn = new MySqlConnection(GetConnectionString()))
            {
                // 打开数据库链接
                await conn.OpenAsync();
                using (var command = BuildCommand(sql, parameters, conn))
                {
                    var result = new ExecutionResult()
                    {
                        // 执行Sql语句
                        ReturnCode = await command.ExecuteNonQueryAsync()
                    };

                    // 是否需要设置传回主键
                    if (DalHints.IsSet(dalHints, DalHint.SetIdentity))
                    {
                        using (var identityCommand = new MySqlCommand("SELECT @@Identity;", conn))
                        {
                            result.Identity = (ulong)await identityCommand.ExecuteScalarAsync();
                        }
                    }

                    return result;
                }
            }
        }

        private static MySqlCommand BuildCommand(string sql, List<DatabaseParameter> parameters, MySqlConnection connection)
        {
            MySqlCommand command = new MySqlCommand(sql, connection)
            {
                CommandText = sql,
                Connection = connection
            };

            if (parameters == null || parameters.Count == 0)
            {
                return command;
            }

            // 增加参数
            foreach (var param in parameters)
            {
                if (param.Value is ICollection value)
                {
                    var idx = 0;
                    foreach (var item in value)
                    {
                        command.Parameters.AddWithValue(param.Name + (idx++).ToString(), item);
                    }
                }
                else
                {
                    command.Parameters.AddWithValue(param.Name, param.Value);
                }
            }

            return command;
        }

        public async Task<ExecutionResult> Execute(string sql, List<DatabaseParameter> parameters, DalHints dalHints)
        {
            return await ExecuteNonQuery(sql, parameters, dalHints);
        }
    }
}
