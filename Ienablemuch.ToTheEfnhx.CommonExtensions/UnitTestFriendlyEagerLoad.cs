//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;


//using System.Linq.Expressions;
//using NHibernate.Linq;
//using System.Data.Entity;


//namespace Ienablemuch.ToTheEfnhX.CommonExtensions
//{
    
//    public static class UnitTestFriendlyEagerLoad
//    {

//        // public static IQueryable<T> Include<T, TProperty>(this IQueryable<T> source, Expression<Func<T, TProperty>> path) where T : class;





//        public static IQueryable<TOriginating> EagerLoad<TOriginating, TRelated>(this IQueryable<TOriginating> query, Expression<Func<TOriginating, TRelated>> relatedObjectSelector) where TOriginating : class
//        {
//            if (query.Provider is NHibernate.Linq.DefaultQueryProvider)
//                return query.Fetch(relatedObjectSelector);
//            else if (query.Provider.GetType().ToString() == "System.Data.Entity.Internal.Linq.DbQueryProvider")
//            {
//                // System.Data.Entity.Internal.Linq.DbQueryProvider is inaccessible due to its protection level. Testing by string would do the trick
//                return query.Include(relatedObjectSelector);
//            }
//            else
//                return query;
//        }


//        public static IQueryable<TOriginating> EagerLoadMany<TOriginating, TRelated>(this IQueryable<TOriginating> query, Expression<Func<TOriginating, IEnumerable<TRelated>>> relatedObjectSelector) where TOriginating : class
//        {



//            if (query.Provider is NHibernate.Linq.DefaultQueryProvider)
//                return query.FetchMany(relatedObjectSelector);
//            else if (query.Provider.GetType().ToString() == "System.Data.Entity.Internal.Linq.DbQueryProvider")
//            {
//                // System.Data.Entity.Internal.Linq.DbQueryProvider is inaccessible due to its protection level. Testing by string would do the trick
//                return query.Include(relatedObjectSelector);
//            }
//            else
//                return query;
//        }

//    }
//}
