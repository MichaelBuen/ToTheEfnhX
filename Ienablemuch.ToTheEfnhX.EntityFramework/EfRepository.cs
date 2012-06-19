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
            // PrimaryKeyName = typeof(TEnt).Name + RepositoryConstants.IdSuffix;

            PrimaryKeyName = _ctx.GetPrimaryKeyPropertyNames(typeof(TEnt))[0];
            VersionName = "RowVersion";
        }

        public DbContext DbContext { get { return _ctx; } }

        public string PrimaryKeyName { get; set; }
        public string VersionName { get; set; }



        public IQueryable<TEnt> All
        {
            get {  return _ctx.Set<TEnt>().AsNoTracking(); } 
        }



        public void Save(TEnt ent)
        {
            try
            {
                Type entType = typeof(TEnt);


                object pkValue = entType.InvokeMember(PrimaryKeyName, System.Reflection.BindingFlags.GetProperty, null, ent, new object[] { });


                object pkDefaultValue = entType.GetProperty(PrimaryKeyName).PropertyType.GetDefault();

                TEnt liveObj = null;

                AssignStub(null, ent, VersionName, _ctx);

                //object entx = ent;
                // ent = (TEnt) ent.Clone();

                bool isNew;
                if (object.Equals(pkValue, pkDefaultValue))
                {
                    isNew = true;

                    
                    
                    _ctx.Set<TEnt>().Add(ent);
                }
                else
                {
                    isNew = false;




                    
                    var query = _ctx.Set<TEnt>().Where(string.Format("{0} = @0", PrimaryKeyName), pkValue);

                    liveObj = query.SingleOrDefault();

                    // _ctx.Entry(liveObj).Reload();

                    

                    if (liveObj == null) throw new DbChangesConcurrencyException();

                    foreach (PropertyInfo pi in entType.GetProperties())
                    {
                        bool isCollection = pi.PropertyType.IsGenericType && typeof(IEnumerable).IsAssignableFrom(pi.PropertyType);

                        if (!isCollection && pi.Name != VersionName)
                        {

                            
                            
                            /*
                             * use this to check for reference-type-ness. to-do
                             * 
                             * if (!pi.PropertyType.IsValueType && pi.PropertyType != typeof(string))
                            {
                                var objectContext = ((System.Data.Entity.Infrastructure.IObjectContextAdapter)_ctx).ObjectContext;
                                MethodInfo m = objectContext.GetType().GetMethod("CreateObjectSet", new Type[] { });
                                MethodInfo generic = m.MakeGenericMethod(pi.PropertyType);
                                object set = generic.Invoke(objectContext, null);
                            }*/

                            
                            if (!pi.PropertyType.IsClass || pi.PropertyType == typeof(string))
                            {
                                object val = entType.InvokeMember(pi.Name, BindingFlags.GetProperty, null, ent, new object[] { });
                                entType.InvokeMember(pi.Name, BindingFlags.SetProperty, null, liveObj, new object[] { val });
                            }
                            else
                            {

                                


                                object refObj = entType.InvokeMember(pi.Name, BindingFlags.GetProperty, null, ent, new object[] { });

                                //if (pi.Name == "Customer")
                                //    throw new Exception("Customer " + pi.PropertyType.IsClass + " " + (refObj == null));

                                if (refObj == null)
                                {
                                    // throw new Exception("Hei");
                                    var dummy = entType.InvokeMember(pi.Name, BindingFlags.GetProperty, null, liveObj, new object[] { });                                    
                                }

                                object val = null;
                                if (refObj != null)
                                {
                                    // string pkName = pi.Name + RepositoryConstants.IdSuffix; // temporarily
                                    string pkName = _ctx.GetPrimaryKeyPropertyNames(pi.PropertyType)[0];

                                    object refPk = refObj.GetType().InvokeMember(pkName, BindingFlags.GetProperty, null, refObj, new object[] { });

                                    // instead of database roundtrip...
                                    // object val = _ctx.Set(pi.PropertyType).Find(refPk);

                                    // ...use stub:                                
                                    val = LoadStubX(pi.PropertyType, pkName, refPk, _ctx);
                                }
                                else
                                    val = null;

                                /*dynamic x = val;
                                throw new Exception("Yo " + refPk + " " + x.CountryName);*/
                                entType.InvokeMember(pi.Name, BindingFlags.SetProperty, null, liveObj, new object[] { val });
                            }
                        }
                    }


                    PropertyInfo rowVersionPI = entType.GetProperty(VersionName);
                    if (rowVersionPI != null)
                    {
                        byte[] version = (byte[])rowVersionPI.GetValue(ent, null);
                        _ctx.Entry(liveObj).Property(VersionName).OriginalValue = version;
                    }
                    

                }


                _ctx.SaveChanges();

                if (!isNew)
                {
                    string primaryKeyName = _ctx.GetPrimaryKeyPropertyNames(entType)[0];
                    pkValue = entType.InvokeMember(primaryKeyName, System.Reflection.BindingFlags.GetProperty, null, liveObj, new object[] { });
                    typeof(TEnt).InvokeMember(primaryKeyName, System.Reflection.BindingFlags.SetProperty, null, ent, new object[] { pkValue });



                    PropertyInfo rowVersionPI = entType.GetProperty(VersionName);

                    if (rowVersionPI != null)
                    {
                        object retRowVersion = rowVersionPI.GetValue(liveObj, null);
                        rowVersionPI.SetValue(ent, retRowVersion, null);                        
                    }
                }

                
                


                    

            }
            catch (DbUpdateConcurrencyException)
            {                
                throw new DbChangesConcurrencyException();
            }

        }

        static void AssignStub(object parent, object ent, string versionName, DbContext ctx)
        {
            return;

            // future plan.
            // for the meantime, use DitTO's AssignStub to re-hydrate stub objects

            foreach (PropertyInfo item in ent.GetType().GetProperties())
            {
                if (item.PropertyType.IsValueType || item.PropertyType == typeof(string) || item.Name == versionName) continue;

                // if (item.PropertyType == typeof(System.Data.Objects.DataClasses.RelationshipManager)) continue;

                bool isCollection = item.PropertyType.IsGenericType && typeof(IEnumerable).IsAssignableFrom(item.PropertyType);

                object val = item.GetValue(ent, null);

                if (val == null) continue;

                if (!isCollection && object.ReferenceEquals(val, parent)) continue;

                if (isCollection)
                {
                    IList elems = (IList)val;

                    foreach (object elem in elems)
                    {
                        AssignStub(ent, elem, versionName, ctx);
                    }



                    continue;
                }


                // e.g. Category of Product


                Debug.WriteLine("Tunay " + item.PropertyType.ToString() + " " + item.Name);
                string pkName = ctx.GetPrimaryKeyPropertyNames(item.PropertyType)[0];
                PropertyInfo pkPI = item.PropertyType.GetProperty(pkName);
                // e.g. CategoryId of Category
                object pkVal = pkPI.GetValue(val, null);
                

                object identityMappedObject = LoadStubX(item.PropertyType, pkName, pkVal, ctx);

                item.SetValue(ent, identityMappedObject, null);

            }

        }



        static object LoadStubX(Type t, string primaryKeyName, object id, DbContext db)
        {
            


            var cachedEnt =
                    db.ChangeTracker.Entries().Where(x => ObjectContext.GetObjectType(x.Entity.GetType()) == t).SingleOrDefault(x =>
                    {
                        Type entType = x.Entity.GetType();
                        object value = entType.InvokeMember(primaryKeyName, System.Reflection.BindingFlags.GetProperty, null, x.Entity, new object[] { });

                        return value.Equals(id);
                    });



            if (cachedEnt != null)
            {
                return cachedEnt.Entity;
            }
            else
            {
                object stub = Activator.CreateInstance(t);


                t.InvokeMember(primaryKeyName, System.Reflection.BindingFlags.SetProperty, null, stub, new object[] { id });

                db.Entry(stub).State = EntityState.Unchanged;

                return stub;
            }


        }//LoadStub



        
        


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

        
        public void SaveGraph(TEnt ent)
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


                AssignStub(null, clonedEnt, VersionName, _ctx);
                


                object pkValue = entType.InvokeMember(PrimaryKeyName, System.Reflection.BindingFlags.GetProperty, null, clonedEnt, new object[] { });


                object pkDefaultValue = entType.GetProperty(PrimaryKeyName).PropertyType.GetDefault();






                bool isNew;
                TEnt liveParent = null;

                if (object.Equals(pkValue, pkDefaultValue))
                {
                    isNew = true;
                    _ctx.Set<TEnt>().Add(clonedEnt);
                }
                else
                {
                    isNew = false;





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

                    liveParent = query.Single();



                    
                    foreach (PropertyInfo pi in typeof(TEnt).GetProperties())
                    {
                        
                        // throw new Exception("Test " + pi.Name + pi.IsG);
                        // throw new Exception("Test " + pi.Name + " " +  (pi.PropertyType.IsGenericType && typeof(IEnumerable).IsAssignableFrom(pi.PropertyType)));

                        bool isCollection = pi.PropertyType.IsGenericType && typeof(IEnumerable).IsAssignableFrom(pi.PropertyType);
                        
                        if (!isCollection && pi.Name != VersionName)
                        {                            
                            object transientVal = entType.InvokeMember(pi.Name, BindingFlags.GetProperty, null, clonedEnt, new object[] { });



                            if (transientVal == null)
                            {
                                // work-around for null Independent Association. Line #5
                                // http://www.codetuning.net/blog/post/Understanding-Entity-Framework-Associations.aspx
                                /*
                                // Example:   
                                using (var context = new OrderEntities())
                                {
                                    Order order = context.Orders.First(x => x.Id == 1);
                                    // dummy = order.Customer; // solves the issue   
                                    order.Customer = null;
                                    context.SaveChanges();
                                }*/
                                var dummy = entType.InvokeMember(pi.Name, BindingFlags.GetProperty, null, liveParent, new object[] { });
                            }

                            entType.InvokeMember(pi.Name, BindingFlags.SetProperty, null, liveParent, new object[] { transientVal });
                        }
                        else if (isCollection)
                        {


                            SaveGraphCollection(
                                liveParent,
                                (IList)entType.InvokeMember(pi.Name, BindingFlags.GetProperty, null, liveParent, new object[] { }),
                                clonedEnt,
                                (IList)entType.InvokeMember(pi.Name, BindingFlags.GetProperty, null, clonedEnt, new object[] { })
                            );
                        }

                    }


                    

                    PropertyInfo rowVersionProp = entType.GetProperty(VersionName);
                    // rowversion property existing
                    if (rowVersionProp != null)
                    {
                        byte[] version = (byte[])rowVersionProp.GetValue(clonedEnt, null);
                        _ctx.Entry(liveParent).Property(VersionName).OriginalValue = version;
                    }

                    

                }



                _ctx.SaveChanges();




                
                pkValue = entType.InvokeMember(PrimaryKeyName, System.Reflection.BindingFlags.GetProperty, null, clonedEnt, new object[] { });
                typeof(TEnt).InvokeMember(PrimaryKeyName, System.Reflection.BindingFlags.SetProperty, null, ent, new object[] { pkValue });


                PropertyInfo retRowVersionProp = entType.GetProperty(VersionName);
                // rowversion property existing

                
                if (retRowVersionProp != null)
                {
                    // Change these:
                    // object retRowVersion = typeof(TEnt).InvokeMember(VersionName, System.Reflection.BindingFlags.GetProperty, null, clonedEnt, new object[] { });
                    // typeof(TEnt).InvokeMember(VersionName, System.Reflection.BindingFlags.SetProperty, null, ent, new object[] { retRowVersion });

                    // To these:
                
                    object retRowVersion = retRowVersionProp.GetValue(!isNew ? liveParent : clonedEnt, null);

                    // Trace.WriteLine("hurray " + BitConverter.ToString((byte[])retRowVersion));                    
                    retRowVersionProp.SetValue(ent, retRowVersion, null);

                    // System.Windows.Forms.MessageBox.Show("EF " + BitConverter.ToString((byte[])retRowVersion));
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
                // throw;
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



        void SaveGraphCollection(object liveParent, IList colLive, object transientParent, IList colTransient)
        {
            string alertDev = "An anomaly occured, contact the developer";
            if (object.ReferenceEquals(liveParent, transientParent))
                throw new Exception(alertDev);
            if (object.ReferenceEquals(colLive, colTransient))
                throw new Exception(alertDev);


            // The Merge sequence we do for Entity Framework is DELETE, UPDATE, INSERT. This prevent the NHibernate merge problem:
            // http://stackoverflow.com/questions/706673/forcing-nhibernate-to-cascade-delete-before-inserts
            // We don't need to repeat the mistake of NHibernate


            // http://stackoverflow.com/questions/1043755/c-generic-list-t-how-to-get-the-type-of-t
            // e.g. IList<listElemType> p; ICollection<listElemType> p;  IList derived from ICollection, just use the base interface

            if (colTransient == null) return;
            Type listElemType = colTransient.GetType().GetInterfaces().Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(ICollection<>)).Single().GetGenericArguments()[0];



            // string childPk = listElemType.Name + RepositoryConstants.IdSuffix;
            string childPk = _ctx.GetPrimaryKeyPropertyNames(listElemType)[0];

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
                            SaveGraphCollection(
                                liveMatchFound,
                                (IList)listElemType.InvokeMember(pi.Name, BindingFlags.GetProperty, null, liveMatchFound, new object[] { }),
                                transientItem,
                                (IList)listElemType.InvokeMember(pi.Name, BindingFlags.GetProperty, null, transientItem, new object[] { }));
                        }

                        // UPDATE
                                                

                        Type elementType = liveMatchFound.GetType();

                        
                        if (elementType.BaseType != null && elementType.Namespace == ObjectCloner.EFProxyNamespace)
                        {
                             elementType = elementType.BaseType;
                        }

                        foreach (PropertyInfo px in elementType.GetProperties())
                        {
                            // forgot to check if it is collection. Collection is processed above, only scalar values here.
                            // here's the sample error when we don't detect if it is collection:
                            // "Test method TestDitTO.Tests.Test_nested_Live_Ef_SaveGraph_Two_Times threw exception:" 
                            // "System.InvalidOperationException: Multiplicity constraint violated. The role 'OrderLine_Comments_Source' of the relationship 'TestDitTO.OrderLine_Comments' has multiplicity 1 or 0..1."
                            if (px.PropertyType.IsGenericType && typeof(IEnumerable).IsAssignableFrom(px.PropertyType)) continue;



                            PropertyInfo transientPI = transientItem.GetType().GetProperty(px.Name);
                            object transientVal = transientPI.GetValue(transientItem, null);


                            // don't overwrite live entity's referenced object
                            bool isValueInIdentityMap =
                                _ctx.ChangeTracker.Entries()
                                    .Any(x => ObjectContext.GetObjectType(x.Entity.GetType()) == px.PropertyType
                                        && object.ReferenceEquals(px.GetValue(liveMatchFound, null), x.Entity)
                                    );
                                    
                            
                            if (!isValueInIdentityMap || transientVal == null)
                            {
                                //// work-around for null Independent Association. Line #5
                                //// http://www.codetuning.net/blog/post/Understanding-Entity-Framework-Associations.aspx
                                // Uncomment this if we encounter the null didn't take effect
                                //if (transientVal == null)
                                //{
                                //    var dummy = px.GetValue(liveMatchFound, null);
                                //}
                                px.SetValue(liveMatchFound, transientVal, null);
                            }

                        }
                                                
                        assignChildToLiveParent(liveMatchFound); 


                    }


                }//loop transient



                // INSERT
                foreach (object e in transientAddList)
                {
                    assignChildToLiveParent(e); 
                    colLive.Add(e);
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



            // dummy-read all Reference type(possible Independent Associations), to prevent null problem
            // Without dummy reading EF, this fails:
            // 
            // o.Customer = null
            // Assert.IsNull(o.Customer);
            if (x != null)
                foreach (PropertyInfo pi in x.GetType().GetProperties())
                    if (pi.PropertyType.IsClass && pi.PropertyType != typeof(string))
                        pi.GetValue(x, null);
                

            _ctx.Configuration.ProxyCreationEnabled = oldValue;

            return x;            
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


        public void Delete(object id)
        {
            try
            {
                typeof(TEnt).DetectIdType(PrimaryKeyName, id);
                Evict(id);
                _ctx.Entry(LoadDeleteStub(id, null)).State = EntityState.Deleted;

                _ctx.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new DbChangesConcurrencyException();
            }
        }


        public void DeleteCascade(object id)
        {
            DeleteCascade(id, null);
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
                
                
                PropertyInfo rowVersionProp = entType.GetProperty(VersionName);
                if (rowVersionProp != null)
                    rowVersionProp.SetValue(stub, version, null);
                    


                _ctx.Entry(stub).State = EntityState.Unchanged;

                return stub;
            }

        }




        
        public TEnt GetEager(object pkValue, params string[] paths)
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

            // dummy-read all Reference type(possible Independent Associations), to prevent null problem
            // Without dummy reading EF, this fails:
            // 
            // o.Customer = null
            // Assert.IsNull(o.Customer);
            if (r != null)
                foreach (PropertyInfo pi in r.GetType().GetProperties())
                    if (pi.PropertyType.IsClass && pi.PropertyType != typeof(string))
                        pi.GetValue(r, null);



            _ctx.Configuration.ProxyCreationEnabled = oldValue;

            return r;
            // return (TEnt) r.Clone();
        }
    }//EfRepository

    public static class Helper
    {
        public static string[] GetPrimaryKeyPropertyNames(this DbContext db, Type entType)
        {
            // logic sourced here: http://stackoverflow.com/questions/7253943/entity-framework-code-first-find-primary-key

            // Arrange
            var objectContext = ((System.Data.Entity.Infrastructure.IObjectContextAdapter)db).ObjectContext;
            

            // http://stackoverflow.com/a/232621
            MethodInfo m = objectContext.GetType().GetMethod("CreateObjectSet", new Type[] { });
            MethodInfo generic = m.MakeGenericMethod(entType);
            object set = generic.Invoke(objectContext, null);

            PropertyInfo entitySetPI = set.GetType().GetProperty("EntitySet");
            System.Data.Metadata.Edm.EntitySet entitySet = (System.Data.Metadata.Edm.EntitySet)entitySetPI.GetValue(set, null);

            


            // Act 
            IEnumerable<string> keyNames = entitySet.ElementType.KeyMembers.Select(k => k.Name);
            return keyNames.ToArray();

        }
    }
}
