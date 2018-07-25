using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DapperRepository
{
    public interface IDbConfig
    {
        /// <summary>
        /// Gets or sets the name of the database provider.
        /// Odbc Data Provider: System.Data.Odbc
        /// OleDb Data Provider: System.Data.OleDb
        /// OracleClient Data Provider: System.Data.OracleClient
        /// SqlClient Data Provider: System.Data.SqlClient
        /// Sqlite Data Provider: System.Data.Sqlite
        /// Microsoft SQL Server Compact Data Provider 4.0: System.Data.SqlServerCe.4.0
        /// MySQL Data Provider: MySql.Data.MySqlClient
        /// PostgreSql Data Provider: Npgsql
        /// Firebird Data Provider: FirebirdSql.Data.FirebirdClient
        /// </summary>
        /// <value>
        /// The name of the provider.
        /// </value>
        string ProviderName { get; set; }
        string ConnectionString { get; set; }
    }
}
