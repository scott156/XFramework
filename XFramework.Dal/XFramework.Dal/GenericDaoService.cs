using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using XFramework.Dal.Interface;
using XFramework.Dal.Orm;

namespace XFramework.Dal
{
    /// <summary>
    /// 通用的数据库Dao查询服务
    /// </summary>
    public class GenericDaoService : IGenericDaoService
    {
        private readonly ITableDao dao;

        public GenericDaoService()
        {
            dao = new TableDao();
        }

        public GenericDaoService(string logicDbName)
        {
            dao = new TableDao(logicDbName);
        }

        public async Task<int> Delete<T>(T entity) where T : class, new()
        {
            return await dao.Delete(entity);
        }

        public async Task<int> Insert<T>(T entity) where T : class, new()
        {
            return await dao.Insert(entity);
        }

        public async Task<List<T>> QueryAll<T>() where T : class, new()
        {
            return await dao.Query<T>(null);
        }

        public async Task<T> QueryByPk<T>(long identity) where T : class, new()
        {
            return await dao.QueryByPk<T>(identity);
        }

        public async Task<T> QueryByPk<T>(string identity) where T : class, new()
        {
            return await dao.QueryByPk<T>(identity);
        }

        public async Task<T> QueryByPk<T>(T criteria) where T : class, new()
        {
            return await dao.QueryByPk(criteria);
        }

        public async Task<T> QueryByPk<T>(int identity) where T : class, new()
        {
            return await dao.QueryByPk<T>(identity);
        }

        public async Task<T> QueryFirst<T>(IDatabaseParameterLink parameters) where T : class, new()
        {
            var list = await dao.QueryLike<T>(parameters, 1);
            if (list == null || list.Count == 0)
            {
                return null;
            }

            return list[0];
        }

        public async Task<T> QueryFirst<T>(T criteria) where T : class, new()
        {
            var list = await dao.Query(criteria, 1);
            if (list == null || list.Count == 0)
            {
                return null;
            }

            return list[0];
        }

        public async Task<List<T>> QueryLike<T>(T criteria) where T : class, new()
        {
            return await dao.Query<T>(criteria);
        }

        public async Task<List<T>> QueryLike<T>(IDatabaseParameterLink link) where T : class, new()
        {
            return await dao.QueryLike<T>(link);
        }

        public async Task<List<T>> QueryTop<T>(string sql, IDataParameterCollection parameters, int recordCount) 
            where T : class, new()
        {
            await Task.Run(() => { });

            throw new System.NotImplementedException();
        }

        public async Task<List<T>> QueryTop<T>(T criteria, int recordCount) where T : class, new()
        {
            return await dao.Query(criteria, recordCount);
        }

        public async Task<int> Update<T>(T entity) where T : class, new()
        {
            return await dao.Update(entity);
        }

    }
}
