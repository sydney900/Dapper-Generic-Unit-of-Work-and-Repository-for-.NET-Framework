using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DapperRepository
{
    public interface IRepository<T> where T : BaseEntity
    {
        Type EntityType { get; }
        List<T> GetAll();
        Task<List<T>> GetAllAsync();
        T Get(long id);
        Task<T> GetAsync(long id);
        void Insert(T entity);
        void Update(T entity);
        T Delete(long id);
        Task<T> DeleteAsync(long id);
    }
}
