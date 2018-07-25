using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper.Contrib.Extensions;

namespace DapperRepository
{
    public class Repository<T> : IDisposable, IRepository<T> where T : BaseEntity
    {
        protected IDbConnection _connection;

        public Repository(IDbConnection connection)
        {
            SetConnection(connection);
        }

        public virtual void SetConnection(IDbConnection connection)
        {
            _connection = connection;
        }

        public Type EntityType
        {
            get
            {
                return typeof(T);
            }
        }

        public virtual List<T> GetAll()
        {
            return _connection.GetAll<T>().ToList();
        }

        public virtual async Task<List<T>> GetAllAsync()
        {
            var all = await _connection.GetAllAsync<T>();
            return all.ToList();
        }

        public virtual T Get(long id)
        {
            return _connection.Get<T>(id);
        }

        public virtual async Task<T> GetAsync(long id)
        {
            return await _connection.GetAsync<T>(id);
        }

        public virtual void Insert(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }
            _connection.Insert<T>(entity);
        }

        public virtual void Update(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }
            _connection.Update<T>(entity);
        }

        public virtual T Delete(long id)
        {
            T t = _connection.Get<T>(id);
            var deleted = _connection.Delete<T>(t);

            return deleted ? t : default(T);
        }

        public virtual async Task<T> DeleteAsync(long id)
        {
            T t = await _connection.GetAsync<T>(id);
            if (!await _connection.DeleteAsync<T>(t))
            {
                return default(T);
            }

            return t;
        }

        public virtual void Dispose()
        {
            if (_connection!=null)
            {
                _connection.Close();
                _connection.Dispose();
                _connection = null;
            }
        }
    }
}
