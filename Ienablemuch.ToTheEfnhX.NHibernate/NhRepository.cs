using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using NHibernate;
using NHibernate.Linq;


using Ienablemuch.ToTheEfnhX.ForImplementorsOnly;
using System.Collections;
using System.Reflection;

namespace Ienablemuch.ToTheEfnhX.NHibernate
{

    public class Repository<TEnt> : IRepository<TEnt> where TEnt : class
    {

        ISession _session = null;



        public Repository(ISession sess)
        {
            _session = sess;
            // Convention-over-configuration :-)
            PrimaryKeyName = typeof(TEnt).Name + RepositoryConstants.IdSuffix;
            VersionName = RepositoryConstants.RowversionName;



        }

        public string PrimaryKeyName { get; set; }
        public string VersionName { get; set; }



        public IQueryable<TEnt> All
        {
            get
            {

                var x = _session.Query<TEnt>();
                return x;
            }
        }



        public void Merge(TEnt ent, byte[] version)
        {
            ITransaction tx = null;

            try
            {

                if (!_session.Transaction.IsActive)
                    tx = _session.BeginTransaction();

                object pkValue =
                    typeof(TEnt).InvokeMember(
                        PrimaryKeyName,
                        System.Reflection.BindingFlags.GetProperty, null, ent, new object[] { });

                object pkDefaultValue = typeof(TEnt).GetProperty(PrimaryKeyName).PropertyType.GetDefault();


                // usually GetDefault is: 0, Guid.Empty
                if (!object.Equals(pkValue, pkDefaultValue))
                    _session.Evict(_session.Load<TEnt>(pkValue));





                Type entType = typeof(TEnt);

                {
                    PropertyInfo rowVersionProp = entType.GetProperty(VersionName, BindingFlags.Public | BindingFlags.Instance);

                    if (rowVersionProp != null)
                        // change this:
                        // entType.InvokeMember(VersionName, System.Reflection.BindingFlags.SetProperty, null, ent, new object[] { version });
                        // to this:
                        rowVersionProp.SetValue(ent, version, null);
                }



                TEnt retObject = (TEnt)_session.Merge(ent);


                if (tx != null)
                {
                    tx.Commit();
                    tx.Dispose();
                }



                if (object.Equals(pkValue, pkDefaultValue))
                {
                    object pkGenerated = typeof(TEnt).InvokeMember(PrimaryKeyName, System.Reflection.BindingFlags.GetProperty, null, retObject, new object[] { });
                    typeof(TEnt).InvokeMember(PrimaryKeyName, System.Reflection.BindingFlags.SetProperty, null, ent, new object[] { pkGenerated });
                }


                {
                    PropertyInfo rowVersionProp = entType.GetProperty(VersionName, BindingFlags.Public | BindingFlags.Instance);
                    if (rowVersionProp != null)
                    {
                        // changed these:
                        // object retRowVersion = typeof(TEnt).InvokeMember(VersionName, System.Reflection.BindingFlags.GetProperty, null, retObject, new object[] { });                        
                        // typeof(TEnt).InvokeMember(VersionName, System.Reflection.BindingFlags.SetProperty, null, ent, new object[] { retRowVersion });

                        // to these:
                        object retRowVersion = rowVersionProp.GetValue(retObject, null);
                        rowVersionProp.SetValue(ent, retRowVersion, null);
                    }
                }

            }
            catch (StaleObjectStateException)
            {
                if (tx != null)
                {
                    tx.Rollback();
                    tx.Dispose();
                }

                throw new DbChangesConcurrencyException();
            }
        }

        public void Save(TEnt ent, byte[] version)
        {
            ITransaction tx = null;

            try
            {

                
                if (!_session.Transaction.IsActive)
                    tx = _session.BeginTransaction();

                object pkValue =
                    typeof(TEnt).InvokeMember(
                        PrimaryKeyName,
                        System.Reflection.BindingFlags.GetProperty, null, ent, new object[] { });



                object pkDefaultValue = typeof(TEnt).GetProperty(PrimaryKeyName).PropertyType.GetDefault();

                // usually GetDefault is: 0, Guid.Empty
                if (!object.Equals(pkValue, pkDefaultValue))
                    _session.Evict(_session.Load<TEnt>(pkValue));



                Type entType = typeof(TEnt);
                entType.InvokeMember(VersionName, System.Reflection.BindingFlags.SetProperty, null, ent, new object[] { version });

                _session.SaveOrUpdate(ent);

                if (tx != null)
                {
                    tx.Commit();
                    tx.Dispose();
                }



            }
            catch (StaleObjectStateException)
            {
                if (tx != null)
                {
                    tx.Rollback();
                    tx.Dispose();
                }

                throw new DbChangesConcurrencyException();
            }
        }




        public TEnt Get(object id)
        {
            return _session.Get<TEnt>(id);
        }

        public void Delete(object id, byte[] version)
        {
            ITransaction tx = null;

            try
            {
                typeof(TEnt).DetectIdType(PrimaryKeyName, id);
                
                
                if (!_session.Transaction.IsActive)
                    tx = _session.BeginTransaction();

                
                object objStub = LoadDeleteStub(id, version);
                _session.Delete(objStub);
                


                if (tx != null)
                {
                    tx.Commit();
                    tx.Dispose();
                }
            }
            catch (StaleObjectStateException)
            {

                if (tx != null)
                {
                    tx.Rollback();
                    tx.Dispose();
                }

                throw new DbChangesConcurrencyException();
            }
        }


        public void DeleteCascade(object id, byte[] version)
        {

            ITransaction tx = null;

            try
            {
                
                if (!_session.Transaction.IsActive)
                    tx = _session.BeginTransaction();

                TEnt ent = _session.Load<TEnt>(id);
                typeof(TEnt).InvokeMember(VersionName, System.Reflection.BindingFlags.SetProperty, null, ent, new object[] { version });


                DeleteRecursively(ent); // do this, instead of this: _session.Delete(ent);

                if (tx != null)
                {
                    tx.Commit();
                    tx.Dispose();
                }


            }
            catch (StaleObjectStateException)
            {

                if (tx != null)
                {
                    tx.Rollback();
                    tx.Dispose();
                }


                throw new DbChangesConcurrencyException();
            }
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


            _session.Delete(root);
        }

        public TEnt LoadStub(object id)
        {
            return _session.Load<TEnt>(id);
        }





        private TEnt LoadDeleteStub(object id, byte[] version)
        {
            /*
            Ent obj = LoadStub(id);

            
            typeof(Ent).InvokeMember(VersionName, 
                System.Reflection.BindingFlags.SetProperty, null, obj, 
                new object[] { version });


            return obj;*/



            Evict(id);


            TEnt stub = (TEnt)Activator.CreateInstance(typeof(TEnt));


            Type entType = typeof(TEnt);
            entType.InvokeMember(PrimaryKeyName, System.Reflection.BindingFlags.SetProperty, null, stub, new object[] { id });
            entType.InvokeMember(VersionName, System.Reflection.BindingFlags.SetProperty, null, stub, new object[] { version });


            return stub;
        }



        public void Evict(object id)
        {
            typeof(TEnt).DetectIdType(PrimaryKeyName, id);
            _session.Evict(LoadStub(id));
        }







        public TEnt GetCascade(object id)
        {
            typeof(TEnt).DetectIdType(PrimaryKeyName, id);
            return Get(id);
        }
    }

}