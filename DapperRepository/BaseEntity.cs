using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DapperRepository
{
    public class BaseEntity
    {
        public long Id { get; set; }
        public DateTime Created { get; set; }
        public DateTime LastModified { get; set; }

        public BaseEntity()
        {
            Created = DateTime.UtcNow;
            LastModified = DateTime.Now;
        }
        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
