using System;
using System.Threading.Tasks;

namespace DapperRepository
{
    public interface IUnitOfWork: IDisposable 
    {
        IRepository<T> Repository<T>() where T : BaseEntity;
        void BeginTransaction();
        bool Commit();
        void Rollback();
        void Dispose(bool disposing);
    }
}
