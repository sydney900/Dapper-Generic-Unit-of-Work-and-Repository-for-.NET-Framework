using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DapperRepository
{
    public class ConnectionFactory : IConnectionFactory
    {
        IDbConfig _config;

        public ConnectionFactory(IDbConfig config)
        {
            _config = config;
        }

        public IDbConnection GetConnection()
        {
            var factory = DbProviderFactories.GetFactory(_config.ProviderName);
            var conn = factory.CreateConnection();
            conn.ConnectionString = _config.ConnectionString;
            conn.Open();
            return conn;
        }
    }
}
