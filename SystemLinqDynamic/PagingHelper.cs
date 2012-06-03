using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Linq.Dynamic
{
    public static class PagingHelper
    {
        public static IQueryable<T> LimitAndOffset<T>(this IQueryable<T> q,
                            int pageSize, int pageOffset)
        {
            return (IQueryable<T>) q.Skip((pageOffset - 1) * pageSize).Take(pageSize);
        }
    }
}
