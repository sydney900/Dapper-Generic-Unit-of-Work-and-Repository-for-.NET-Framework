using System.Data;

namespace DapperRepository
{
    public interface IConnectionFactory
    {
        IDbConnection GetConnection();
    }
}
