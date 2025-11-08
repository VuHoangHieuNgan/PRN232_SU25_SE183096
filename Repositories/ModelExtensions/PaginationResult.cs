using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.ModelExtensions
{
    public class PaginationResult<T> where T : class
    {
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        public int PageSizes { get; set; }
        public int CurrentPages { get; set; }

        public T Items { get; set; }

    }
}
