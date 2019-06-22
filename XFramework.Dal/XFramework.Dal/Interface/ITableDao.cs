using System.Collections.Generic;
using System.Threading.Tasks;
using XFramework.Dal.Orm;

namespace XFramework.Dal.Interface
{
    public interface ITableDao
    {
        Task<List<T>> Query<T>(T criteria, int? recordCount = null) where T : class, new();

        Task<T> QueryByPk<T>(string value) where T : class, new();

        Task<T> QueryByPk<T>(long value) where T : class, new();

        Task<T> QueryByPk<T>(T criteria) where T : class, new();

        Task<int> Insert<T>(T entity) where T : class, new();

        Task<int> Update<T>(T entity) where T : class, new();

        Task<int> Delete<T>(T entity) where T : class, new();

        Task<List<T>> QueryLike<T>(IDatabaseParameterLink group, int? recordCount = null) where T : class, new();
    }
}
