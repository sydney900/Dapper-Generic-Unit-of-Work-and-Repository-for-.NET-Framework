using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;

namespace DapperRepository
{
    public class UnitOfWork : IUnitOfWork
    {
        private bool _disposed;
        private Dictionary<Type, dynamic> _dictRepositories;
        private IConnectionFactory _connectionFactory;
        private IDbConnection _connection;
        private IDbTransaction _transaction;

        public UnitOfWork(IConnectionFactory connectionFactory, params dynamic[] repositories)
        {
            if (connectionFactory == null)
                throw new ArgumentNullException("connectionFactory");

            if (repositories == null || repositories.Length == 0)
                throw new ArgumentNullException("repositories");

            _connectionFactory = connectionFactory;

            _connection = _connectionFactory.GetConnection();
            //_connection.Open();

            _dictRepositories = new Dictionary<Type, dynamic>();
            foreach (var item in repositories)
            {
                if (item == null)
                    throw new ArgumentNullException("repository");

                item.SetConnection(_connection);

                if (!_dictRepositories.ContainsKey(item.EntityType))
                    //_dictRepositories.Add(item.EntityType, item);
                    this[item.EntityType] = item;
            }
        }

        private dynamic this[Type type]
        {
            get
            {
                dynamic iRepository;
                _dictRepositories.TryGetValue(type, out iRepository);
                return iRepository;
            }
            set
            {
                _dictRepositories.Add(type, value);
            }
        }

        public IRepository<T> Repository<T>() where T : BaseEntity
        {
            IRepository<T> rep = this[typeof(T)];
            return rep;
        }

        public void BeginTransaction()
        {
            if (_transaction == null)
                _transaction = _connection.BeginTransaction();
            else
                throw new InvalidOperationException("A transaction already started");
        }

        public bool Commit()
        {
            if (_transaction != null)
            {
                _transaction.Commit();
                return true;
            }

            return false;
        }

        public void Rollback()
        {
            if (_transaction != null)
            {
                _transaction.Rollback();
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                try
                {
                    _dictRepositories.Clear();
                    if (_transaction != null)
                    {
                        _transaction.Dispose();
                    }
                }
                catch (ObjectDisposedException)
                {
                }

                if (_connection != null)
                {
                    _connection.Close();
                    _connection.Dispose();
                    _connection = null;
                }
            }

            _disposed = true;
        }
    }
}