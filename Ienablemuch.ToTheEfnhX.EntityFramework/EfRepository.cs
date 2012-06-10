using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Collections;
using System.Reflection;
using System.Data.Objects;
using System.Data;

using System.Linq.Dynamic;

using Ienablemuch.ToTheEfnhX.ForImplementorsOnly;
using System.Diagnostics;


namespace Ienablemuch.ToTheEfnhX.EntityFramework
{
    public class Repository<TEnt> : IRepository<TEnt> where TEnt : class
    {
        DbContext _ctx = null;
        public Repository(DbContext ctx)
        {

            _ctx = ctx;
            // Convention-over-configuration :-)
            PrimaryKeyName = typeof(TEnt).Name + RepositoryConstants.IdSuffix;
            VersionName = "RowVersion";
        }

        public string PrimaryKeyName { get; set; }
        public string VersionName { get; set; }



        public IQueryable<TEnt> All
        {
            get {  return _ctx.Set<TEnt>().AsNoTracking(); } 
        }



        public void Save(TEnt ent, byte[] version)
        {
            try
            {
                Type entType = typeof(TEnt);

                object pkValue = entType.InvokeMember(PrimaryKeyName, System.Reflection.BindingFlags.GetProperty, null, ent, new object[] { });


                object pkDefaultValue = entType.GetProperty(PrimaryKeyName).PropertyType.GetDefault();

                TEnt liveObj = null;

                bool isNew;
                if (object.Equals(pkValue, pkDefaultValue))
                {
                    isNew = true;
                    _ctx.Set<TEnt>().Add(ent);
                }
                else
                {
                    isNew = false;

                    
                    Evict(pkValue);                    
                    
                    /*
                    _ctx.Entry(ent).State = System.Data.EntityState.Modified;
                    _ctx.Entry(ent).Property(VersionName).OriginalValue = version;
                    */
                    
                    
                    var query = _ctx.Set<TEnt>().Where(string.Format("{0} = @0", PrimaryKeyName), pkValue);

                    liveObj = query.SingleOrDefault();

                    if (liveObj == null) throw new DbChangesConcurrencyException();

                    foreach (PropertyInfo pi in typeof(TEnt).GetProperties())
                    {
                        bool isCollection = pi.PropertyType.IsGenericType && typeof(IEnumerable).IsAssignableFrom(pi.PropertyType);

                        if (!isCollection && pi.Name != VersionName)
                        {
                            if (!pi.PropertyType.IsClass || pi.PropertyType == typeof(string))
                            {
                                object val = entType.InvokeMember(pi.Name, BindingFlags.GetProperty, null, ent, new object[] { });
                                entType.InvokeMember(pi.Name, BindingFlags.SetProperty, null, liveObj, new object[] { val });
                            }
                            else
                            {
                                object refObj = entType.InvokeMember(pi.Name, BindingFlags.GetProperty, null, ent, new object[] { });
                                string pkName = pi.Name + RepositoryConstants.IdSuffix; // temporarily
                                object refPk = refObj.GetType().InvokeMember(pkName, BindingFlags.GetProperty, null, refObj, new object[] { });
                                object val = _ctx.Set(pi.PropertyType).Find(refPk);
                                /*dynamic x = val;
                                throw new Exception("Yo " + refPk + " " + x.CountryName);*/
                                entType.InvokeMember(pi.Name, BindingFlags.SetProperty, null, liveObj, new object[] { val });
                            }
                        }
                    }


                    _ctx.Entry(liveObj).Property(VersionName).OriginalValue = version;
                    

                }


                _ctx.SaveChanges();

                if (!isNew)
                {
                    pkValue = entType.InvokeMember(PrimaryKeyName, System.Reflection.BindingFlags.GetProperty, null, liveObj, new object[] { });
                    typeof(TEnt).InvokeMember(PrimaryKeyName, System.Reflection.BindingFlags.SetProperty, null, ent, new object[] { pkValue });

                    object retRowVersion = typeof(TEnt).InvokeMember(VersionName, System.Reflection.BindingFlags.GetProperty, null, liveObj, new object[] { });
                    typeof(TEnt).InvokeMember(VersionName, System.Reflection.BindingFlags.SetProperty, null, ent, new object[] { retRowVersion });
                }

                
                


                    

            }
            catch (DbUpdateConcurrencyException)
            {
                throw new DbChangesConcurrencyException();
            }

        }






        
        


        // http://stackoverflow.com/questions/1158422/entity-framework-detach-and-keep-related-object-graph
        static object YetAnotherCloner(object x)
        {
            using (var stream = new System.IO.MemoryStream())
            {
                var formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                formatter.Serialize(stream, x);
                stream.Position = 0;
                return formatter.Deserialize(stream);
            }
        }

        
        public void Merge(TEnt ent, byte[] version)
        {
            // Rationale for detaching/evicting: 
            // http://msdn.microsoft.com/en-us/library/bb738697.aspx
            // http://msdn.microsoft.com/en-us/library/bb896271.aspx

            try
            {
                
                // http://stackoverflow.com/questions/1158422/entity-framework-detach-and-keep-related-object-graph
                // Entity Framework shreds the original object when detaching

                // Be smart on cloning, use a built-in one if the object is serializable

                TEnt clonedEnt;

                
                if (ent.GetType().GetCustomAttributes(typeof(SerializableAttribute), false).Length == 1)
                {
                    clonedEnt = (TEnt)YetAnotherCloner(ent);
                }
                else
                {
                    clonedEnt = (TEnt)ent.Clone();
                }
                
                


                Type entType = typeof(TEnt);

                object pkValue = entType.InvokeMember(PrimaryKeyName, System.Reflection.BindingFlags.GetProperty, null, clonedEnt, new object[] { });


                object pkDefaultValue = entType.GetProperty(PrimaryKeyName).PropertyType.GetDefault();







                
                if (object.Equals(pkValue, pkDefaultValue))
                {                    
                    _ctx.Set<TEnt>().Add(clonedEnt);
                }
                else
                {
                 





                    // dynamic rrr = clonedEnt;
                    // throw new Exception("Hello moto" + rrr.PriceList.Count);



                    // var query = this.All.Where(string.Format("{0} = @0", PrimaryKeyName), pkValue);





                    var query = _ctx.Set<TEnt>().Where(string.Format("{0} = @0", PrimaryKeyName), pkValue);

                        

                    // if there's no proxy for root descendant, we will not be able navigate the collections. we must eagerly load the objects
                    if (!_ctx.Configuration.ProxyCreationEnabled)
                    {
                        /*                        
                        string[] s = GetCollectionPaths(entType).ToArray();
                        throw new Exception(string.Join("; ", s));
                        */
                        /*
                        var query = new EfRepository<Question>().All.Where(x => x.QuestionId == id);
                        query = query.Include("Answers");
                        query = query.Include("Answers.Comments");
                        query = query.Include("Comments");*/

                        foreach (string path in GetCollectionPaths(entType))
                            query = query.Include(path);

                    }

                    TEnt liveParent = query.Single();

                    
                    foreach (PropertyInfo pi in typeof(TEnt).GetProperties())
                    {
                        
                        // throw new Exception("Test " + pi.Name + pi.IsG);
                        // throw new Exception("Test " + pi.Name + " " +  (pi.PropertyType.IsGenericType && typeof(IEnumerable).IsAssignableFrom(pi.PropertyType)));

                        bool isCollection = pi.PropertyType.IsGenericType && typeof(IEnumerable).IsAssignableFrom(pi.PropertyType);
                        
                        if (!isCollection && pi.Name != VersionName)
                        {
                            object transientVal = entType.InvokeMember(pi.Name, BindingFlags.GetProperty, null, clonedEnt, new object[] { });
                            entType.InvokeMember(pi.Name, BindingFlags.SetProperty, null, liveParent, new object[] { transientVal });
                        }
                        else if (isCollection)
                        {


                            MergeCollection(
                                liveParent,
                                (IList)entType.InvokeMember(pi.Name, BindingFlags.GetProperty, null, liveParent, new object[] { }),
                                clonedEnt,
                                (IList)entType.InvokeMember(pi.Name, BindingFlags.GetProperty, null, clonedEnt, new object[] { })
                            );
                        }

                    }


                    PropertyInfo rowVersionProp = entType.GetProperty(VersionName, BindingFlags.Public | BindingFlags.Instance);
                    // rowversion property existing
                    if (rowVersionProp != null)                        
                        _ctx.Entry(liveParent).Property(VersionName).OriginalValue = version;

                }



                _ctx.SaveChanges();




                pkValue = entType.InvokeMember(PrimaryKeyName, System.Reflection.BindingFlags.GetProperty, null, clonedEnt, new object[] { });
                typeof(TEnt).InvokeMember(PrimaryKeyName, System.Reflection.BindingFlags.SetProperty, null, ent, new object[] { pkValue });


                PropertyInfo retRowVersionProp = entType.GetProperty(VersionName, BindingFlags.Public | BindingFlags.Instance);
                // rowversion property existing
                if (retRowVersionProp != null)
                {
                    // Change these:
                    // object retRowVersion = typeof(TEnt).InvokeMember(VersionName, System.Reflection.BindingFlags.GetProperty, null, clonedEnt, new object[] { });
                    // typeof(TEnt).InvokeMember(VersionName, System.Reflection.BindingFlags.SetProperty, null, ent, new object[] { retRowVersion });

                    // To these:
                    object retRowVersion = retRowVersionProp.GetValue(clonedEnt, null);
                    retRowVersionProp.SetValue(ent, retRowVersion, null);
                }






                // Detaching objects via Entity Framework shreds collection, hence we just deep clone the object at the initial phase of this code,
                // so Entity Framework cannot meddle with the original object's states
                // see this problem: http://stackoverflow.com/questions/1158422/entity-framework-detach-and-keep-related-object-graph
                // Why Entity Framework have to empty the collection when detaching objects? it could just mark in its own object states repository that some objects are not attached anymore.                                                


                // Rationale for detaching/evicting: 
                // http://msdn.microsoft.com/en-us/library/bb738697.aspx
                // http://msdn.microsoft.com/en-us/library/bb896271.aspx
                Evict(pkValue);
                



                    
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new DbChangesConcurrencyException();
            }
        }

        IEnumerable<string> GetCollectionPaths(Type root)
        {

            foreach (PropertyInfo pi in root.GetProperties().Where(x => x.PropertyType.IsGenericType && typeof(IEnumerable).IsAssignableFrom(x.PropertyType)))
            {

                yield return pi.Name;

                // ICollection<listElemType> p; IList derives from ICollection, just use the base interface
                Type listElemType =
                    pi.PropertyType.GetInterfaces().Where(x =>
                    x.IsGenericType && x.GetGenericTypeDefinition() == typeof(ICollection<>)).Single().GetGenericArguments()[0];


                foreach (string subPath in GetCollectionPaths(listElemType))
                {
                    yield return pi.Name + "." + subPath;
                }

            }
        }



        void MergeCollection(object liveParent, IList colLive, object transientParent, IList colTransient)
        {
            if (object.ReferenceEquals(liveParent, transientParent))
                throw new Exception("An anomaly occured, contact the developer");
            if (object.ReferenceEquals(colLive, colTransient))
                throw new Exception("An anomaly occured, contact the developer");


            // The Merge sequence we do for Entity Framework is DELETE, UPDATE, INSERT. This prevent the NHibernate merge problem:
            // http://stackoverflow.com/questions/706673/forcing-nhibernate-to-cascade-delete-before-inserts
            // We don't need to repeat the mistake of NHibernate


            // http://stackoverflow.com/questions/1043755/c-generic-list-t-how-to-get-the-type-of-t
            // e.g. IList<listElemType> p; ICollection<listElemType> p;  IList derived from ICollection, just use the base interface

            if (colTransient == null) return;
            Type listElemType = colTransient.GetType().GetInterfaces().Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(ICollection<>)).Single().GetGenericArguments()[0];



            string childPk = listElemType.Name + RepositoryConstants.IdSuffix;

            // throw new Exception("Hello " + childPk);

            IEnumerable<PropertyInfo> parentPropertyMatches = listElemType.GetProperties().Where(x => x.PropertyType.IsAssignableFrom(liveParent.GetType()));



            // process live
            {
                IList liveList = new ArrayList();

                
                foreach (object elemLive in colLive)
                {
                    bool isExisting = false;


                    foreach (object elemTransient in colTransient)
                    {
                        object transientPk = listElemType.InvokeMember(childPk, BindingFlags.GetProperty, null, elemTransient, new object[] { });
                        object livePk = listElemType.InvokeMember(childPk, BindingFlags.GetProperty, null, elemLive, new object[] { });

                        if (object.Equals(transientPk, livePk))
                        {
                            isExisting = true;
                            break;
                        }
                    }



                    if (!isExisting)
                        liveList.Add(elemLive);
                }



                foreach (object e in liveList)
                {
                    // DELETE
                    DeleteRecursively(e);
                    // _ctx.Entry(e).State = EntityState.Deleted;

                }
            }//process live


            
            Action<object> assignChildToLiveParent = (e) =>
            {

                parentPropertyMatches.Where(x =>
                {
                    object elemParent = listElemType.InvokeMember(x.Name, BindingFlags.GetProperty, null, e, new object[] { });

                    return object.Equals(elemParent, transientParent);

                }).ToList().ForEach(x =>
                {
                    listElemType.InvokeMember(x.Name, BindingFlags.SetProperty, null, e, new object[] { liveParent });
                }
                );
            };



            // process transient
            {

                IList transientAddList = new ArrayList();

                IList transientEditList = new ArrayList();

                // throw new Exception("Test ok " + (colLive == null) + " " + (colTransient == null));



                foreach (object transientItem in colTransient)
                {


                    bool isExisting = false;
                    object liveMatchFound = null;
                    if (colLive != null)
                        foreach (object liveItem in colLive)
                        {

                            object transientPk = listElemType.InvokeMember(childPk, BindingFlags.GetProperty, null, transientItem, new object[] { });
                            object livePk = listElemType.InvokeMember(childPk, BindingFlags.GetProperty, null, liveItem, new object[] { });



                            if (object.Equals(livePk, transientPk))
                            {
                                liveMatchFound = liveItem;
                                isExisting = true;
                                break;
                            }

                        }




                    if (!isExisting)
                    {
                        // we cannot add to colLive directly as we are enumerating it here, buffer it first on list


                        transientAddList.Add(transientItem);
                    }
                    else
                    {

                        // recursively, assign all collections, these are grandchildren and so on

                        // http://stackoverflow.com/questions/6033638/an-object-with-the-same-key-already-exists-in-the-objectstatemanager-the-object

                        

                        foreach (PropertyInfo pi in listElemType.GetProperties().Where(x => x.PropertyType.IsGenericType && typeof(IEnumerable).IsAssignableFrom(x.PropertyType)))
                        {
                            MergeCollection(
                                liveMatchFound,
                                (IList)listElemType.InvokeMember(pi.Name, BindingFlags.GetProperty, null, liveMatchFound, new object[] { }),
                                transientItem,
                                (IList)listElemType.InvokeMember(pi.Name, BindingFlags.GetProperty, null, transientItem, new object[] { }));
                        }

                        // UPDATE

                        
                        transientEditList.Add(transientItem);
                        
                        

                        Type tx = liveMatchFound.GetType();

                        
                        if (tx.BaseType != null && tx.Namespace == ObjectCloner.EFProxyNamespace)
                        {
                             tx = tx.BaseType;
                        }

                        foreach (PropertyInfo px in tx.GetProperties())
                        {
                            PropertyInfo txPi = transientItem.GetType().GetProperty(px.Name);
                            object txVal = txPi.GetValue(transientItem, null);


                            // don't overwrite live entity's referenced object
                            bool isValueInIdentityMap = 
                                _ctx.ChangeTracker.Entries()
                                    .Where(x => ObjectContext.GetObjectType(x.Entity.GetType()) == px.PropertyType)
                                    .Any(x => object.ReferenceEquals(px.GetValue(liveMatchFound, null), x.Entity));

                            
                            if (!isValueInIdentityMap)
                            {
                                px.SetValue(liveMatchFound, txVal, null);
                            }

                        }
                                                
                        assignChildToLiveParent(liveMatchFound); 
                        
                        


                        



                    }


                }//loop transient



                // INSERT
                foreach (object e in transientAddList)
                {
                    assignChildToLiveParent(e); 


                    colLive.GetType().InvokeMember("Add", BindingFlags.InvokeMethod, null, colLive, new object[] { e });

                    // I think we should do this, instead of the above line :-) 
                    // We became so fond of reflection :D
                    // colLive.Add(e);

                }

            }//process transient


        }

        void DeleteRecursively(object root)
        {

            IList toMarkDeleted = new ArrayList();

            foreach (PropertyInfo pi in root.GetType().GetProperties().Where(x => x.PropertyType.IsGenericType && typeof(IEnumerable).IsAssignableFrom(x.PropertyType)))
            {
                IList list = (IList)root.GetType().InvokeMember(pi.Name, BindingFlags.GetProperty, null, root, new object[] { });

                if (list != null)
                    foreach (object e in list)
                    {
                        toMarkDeleted.Add(e);
                    }

            }


            foreach (object e in toMarkDeleted)
            {
                DeleteRecursively(e);
            }


            
            _ctx.Entry(root).State = EntityState.Deleted;
            
        }



        public TEnt Get(object id)
        {
           
            bool oldValue = _ctx.Configuration.ProxyCreationEnabled;

            // we need this to be true. if this is false, stub objects remain stubs
            _ctx.Configuration.ProxyCreationEnabled = true;

            // there's no Dynamic Linq for SingleOrDefault. Hence doing both Where and SingleOrDefault            
            TEnt x = this.All.Where(string.Format("{0} = @0", PrimaryKeyName), id).SingleOrDefault();
            

            _ctx.Configuration.ProxyCreationEnabled = oldValue;

            return x;
            // return (TEnt) x.Clone() ;
        }



        public void Delete(object id, byte[] version)
        {
            try
            {
                typeof(TEnt).DetectIdType(PrimaryKeyName, id);
                Evict(id);
                _ctx.Entry(LoadDeleteStub(id, version)).State = EntityState.Deleted;

                _ctx.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new DbChangesConcurrencyException();
            }
        }

        public void DeleteCascade(object id, byte[] version)
        {

            bool proxyCreation = _ctx.Configuration.ProxyCreationEnabled;
            try
            {
                typeof(TEnt).DetectIdType(PrimaryKeyName, id);

                
                _ctx.Configuration.ProxyCreationEnabled = true;

                object root = _ctx.Set<TEnt>().Find(id);
                DeleteRecursively(root);


                _ctx.SaveChanges();


            }
            catch (DbUpdateConcurrencyException)
            {
                throw new DbChangesConcurrencyException();
            }
            finally
            {
                _ctx.Configuration.ProxyCreationEnabled = proxyCreation;
            }
        }



        void EvictAll()
        {
            var cachedEnt =
                _ctx.ChangeTracker.Entries();


            foreach (var x in cachedEnt)
                _ctx.Entry(x.Entity).State = EntityState.Detached;
        }


        public void Evict(object id)
        {
            // made Evict functionality behave the same as NHibernate's Evict
            // All children, grandchildren should be evicted too; that is, this Evict should evict recursively too like its NHibernate cousin.

            typeof(TEnt).DetectIdType(PrimaryKeyName, id);

            var cachedEnt =
                _ctx.ChangeTracker.Entries().Where(x => ObjectContext.GetObjectType(x.Entity.GetType()) == typeof(TEnt)).SingleOrDefault(x =>
                {
                    Type entType = x.Entity.GetType();
                    object value = entType.InvokeMember(PrimaryKeyName, System.Reflection.BindingFlags.GetProperty, null, x.Entity, new object[] { });

                    return value.Equals(id);
                });



            if (cachedEnt != null)
            {
                EvictRecursively(cachedEnt.Entity);
            }



        }

        void EvictRecursively(object ent)
        {

            if (ent == null) return;


            IList toMarkEvicted = new ArrayList();

            foreach (PropertyInfo pi in ent.GetType().GetProperties().Where(x => x.PropertyType.IsGenericType && typeof(IEnumerable).IsAssignableFrom(x.PropertyType)))
            {
                IList list = (IList)ent.GetType().InvokeMember(pi.Name, BindingFlags.GetProperty, null, ent, new object[] { });



                if (list != null)
                    foreach (object e in list)
                    {
                        toMarkEvicted.Add(e);
                    }

            }


            foreach (object e in toMarkEvicted)
            {
                EvictRecursively(e);
            }


            _ctx.Entry(ent).State = EntityState.Detached;

        }

        public TEnt LoadStub(object id)
        {


            var cachedEnt =
                    _ctx.ChangeTracker.Entries().Where(x => ObjectContext.GetObjectType(x.Entity.GetType()) == typeof(TEnt)).SingleOrDefault(x =>
                    {
                        Type entType = x.Entity.GetType();
                        object value = entType.InvokeMember(PrimaryKeyName, System.Reflection.BindingFlags.GetProperty, null, x.Entity, new object[] { });

                        return value.Equals(id);
                    });

            if (cachedEnt != null)
            {
                return (TEnt)cachedEnt.Entity;
            }
            else
            {
                TEnt stub = (TEnt)Activator.CreateInstance(typeof(TEnt));


                typeof(TEnt).InvokeMember(PrimaryKeyName, System.Reflection.BindingFlags.SetProperty, null, stub, new object[] { id });


                _ctx.Entry(stub).State = EntityState.Unchanged;

                return stub;
            }


        }





        ////////////


        private TEnt LoadDeleteStub(object id, byte[] version)
        {

            var cachedEnt =
                _ctx.ChangeTracker.Entries().Where(x => ObjectContext.GetObjectType(x.Entity.GetType()) == typeof(TEnt)).SingleOrDefault(x =>
                {
                    var entType = x.Entity.GetType();
                    var value = entType.InvokeMember(PrimaryKeyName, System.Reflection.BindingFlags.GetProperty, null, x.Entity, new object[] { });

                    return value.Equals(id);
                });

            if (cachedEnt != null)
            {
                return (TEnt)cachedEnt.Entity;
            }
            else
            {
                TEnt stub = (TEnt)Activator.CreateInstance(typeof(TEnt));


                Type entType = typeof(TEnt);
                entType.InvokeMember(PrimaryKeyName, System.Reflection.BindingFlags.SetProperty, null, stub, new object[] { id });
                
                
                // change this:
                // entType.InvokeMember(VersionName, System.Reflection.BindingFlags.SetProperty, null, stub, new object[] { version });

                // to this:
                PropertyInfo rowVersionProp = entType.GetProperty(VersionName, BindingFlags.Public | BindingFlags.Instance);
                if (rowVersionProp != null)
                    rowVersionProp.SetValue(stub, version, null);
                    


                _ctx.Entry(stub).State = EntityState.Unchanged;

                return stub;
            }

        }




        
        public TEnt GetCascade(object pkValue)
        {
            
            
            bool oldValue = _ctx.Configuration.ProxyCreationEnabled;

            // we need this to be true. if this is false, stub objects remain stubs
            _ctx.Configuration.ProxyCreationEnabled = true;
             

            Type entType = typeof(TEnt);
            var query = _ctx.Set<TEnt>().AsNoTracking().Where(string.Format("{0} = @0", PrimaryKeyName), pkValue);
            
            
            if (!_ctx.Configuration.ProxyCreationEnabled)
            {
                //  e.g.
                //var query = new EfRepository<Question>().All.Where(x => x.QuestionId == id);
                //query = query.Include("Answers");
                //query = query.Include("Answers.Comments");
                //query = query.Include("Comments");

                foreach (string path in GetCollectionPaths(entType))
                    query = query.Include(path);                

            }

            var r = query.SingleOrDefault();


            _ctx.Configuration.ProxyCreationEnabled = oldValue;

            return r;
            // return (TEnt) r.Clone();
        }
    }//EfRepository
}
