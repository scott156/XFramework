using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using XFramework.Core.Abstractions.Configuration;
using XFramework.Dal.Data;
using XFramework.Dal.Orm;

namespace XFramework.Dal.DalClient
{
    public interface IDalClient
    {
        /// <summary>
        /// 数据查询
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        Task<DataSet> Query(string sql, List<DatabaseParameter> parameters);

        DatabaseTypeEnum DatabaseType { get; }

        /// <summary>
        /// 数据库更新操作
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <param name="dalHints"></param>
        /// <returns></returns>
        Task<ExecutionResult> Execute(string sql, List<DatabaseParameter> parameters, DalHints dalHints);
    }
}
