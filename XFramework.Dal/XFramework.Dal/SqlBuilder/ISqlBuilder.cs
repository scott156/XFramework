using System.Collections.Generic;
using XFramework.Dal.Orm;

namespace XFramework.Dal.SqlBuilder
{
    public interface ISqlBuilder
    {
        /// <summary>
        /// 根据实体T，组装查询语句
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="criteria"></param>
        /// <returns></returns>
        (string, List<DatabaseParameter>) BuildSelect<T>(T criteria, int? recordCount = null) where T : new();

        /// <summary>
        /// 根据条件组动态生成Sql语句
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="criteria"></param>
        /// <param name="recordCount"></param>
        /// <returns></returns>
        (string, List<DatabaseParameter>) BuildSelect<T>(IDatabaseParameterLink link) where T : new();

        /// <summary>
        /// 组装Insert
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        (string, List<DatabaseParameter>) BuildInsert<T>(T criteria);

        /// <summary>
        /// 组装Update
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        (string, List<DatabaseParameter>) BuildUpdate<T>(T criteria);

        /// <summary>
        /// 组装Delete
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        (string, List<DatabaseParameter>) BuildDelete<T>(T criteria);

    }
}
