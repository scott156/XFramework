using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using XFramework.Dal.Orm;

namespace XFramework.Dal.Interface
{
    public interface IGenericDaoService
    {
        /// <summary>
        /// 根据主键查询, 支持主键为long类型的查询
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="identity"></param>
        /// <returns></returns>
        Task<T> QueryByPk<T>(long identity) where T : class, new();

        /// <summary>
        /// 根据主键查询, 支持主键为字符串类型的拆线呢
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="identity"></param>
        /// <returns></returns>
        Task<T> QueryByPk<T>(string identity) where T : class, new();

        /// <summary>
        /// 根据主键查询, 支持多主键查询
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="criteria"></param>
        /// <returns></returns>
        Task<T> QueryByPk<T>(T criteria) where T : class, new();

        /// <summary>
        /// 根据条件查询
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="criteria"></param>
        /// <returns></returns>
        Task<List<T>> QueryLike<T>(T criteria) where T : class, new();

        /// <summary>
        /// 查询所有
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task<List<T>> QueryAll<T>() where T : class, new();

        /// <summary>
        /// 查询第一条记录
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        Task<T> QueryFirst<T>(IDatabaseParameterLink parameters) where T : class, new();

        /// <summary>
        /// 查询第一条记录
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        Task<T> QueryFirst<T>(T criteria) where T : class, new();

        /// <summary>
        /// 查询Top
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        Task<List<T>> QueryTop<T>(T criteria, int recordCount) where T : class, new();

        /// <summary>
        /// 查询Top
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        Task<List<T>> QueryTop<T>(string sql, IDataParameterCollection parameters, int recordCount) 
            where T : class, new();

        /// <summary>
        /// 单表数据更新
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        Task<int> Update<T>(T entity) where T : class, new();

        /// <summary>
        /// 插入
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        Task<int> Insert<T>(T entity) where T : class, new();

        /// <summary>
        /// 删除
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        Task<int> Delete<T>(T entity) where T : class, new();

        /// <summary>
        /// 根据自定义的条件组进行查询
        /// </summary>
        /// <typeparam name="T">返回的实体对象</typeparam>
        /// <param name="group">条件组合</param>
        /// <returns></returns>
        Task<List<T>> QueryLike<T>(IDatabaseParameterLink link) where T : class, new();
    }
}
