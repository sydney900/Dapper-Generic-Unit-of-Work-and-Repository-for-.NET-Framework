using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper.Contrib.Extensions;

namespace DapperRepository
{
    public class GenericRepository<T> : Repository<T> where T : BaseEntity
    {
        private IConnectionFactory _connectionFactory;

        public GenericRepository(IConnectionFactory connectionFactory) : base (null)
        {
            _connectionFactory = connectionFactory;
        }
        
        public override List<T> GetAll()
        {
            using (_connection = _connectionFactory.GetConnection())
            {
                return base.GetAll();
            }
        }

        public override async Task<List<T>> GetAllAsync()
        {
            using (_connection = _connectionFactory.GetConnection())
            {                
                return await base.GetAllAsync();
            }
        }

        public override T Get(long id)
        {
            using (_connection = _connectionFactory.GetConnection())
            {
                return base.Get(id);
            }
        }

        public override async Task<T> GetAsync(long id)
        {
            using (_connection = _connectionFactory.GetConnection())
            {
                return await base.GetAsync(id);
            }
        }

        public override void Insert(T entity)
        {
            using (_connection = _connectionFactory.GetConnection())
            {
                base.Insert(entity);
            }
        }

        public override void Update(T entity)
        {
            using (_connection = _connectionFactory.GetConnection())
            {
                base.Update(entity);
            }
        }

        public override T Delete(long id)
        {
            using (_connection = _connectionFactory.GetConnection())
            {
                return base.Delete(id);
            }
        }

        public override async Task<T> DeleteAsync(long id)
        {
            using (_connection = _connectionFactory.GetConnection())
            {
                return await base.DeleteAsync(id);
            }
        }
    }
}
