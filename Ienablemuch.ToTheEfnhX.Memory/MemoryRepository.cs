using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Linq.Expressions;


using Ienablemuch.ToTheEfnhX.ForImplementorsOnly;
using System.Reflection;


namespace Ienablemuch.ToTheEfnhX.Memory
{
    public class MemoryRepository<TEnt> : IRepository<TEnt> where TEnt : class
    {


        IList<TEnt> _list = null;
        IQueryable<TEnt> _queryable = null;

        public MemoryRepository()
        {
            _list = new List<TEnt>();
            _queryable = _list.AsQueryable();

            PrimaryKeyName = typeof(TEnt).Name + RepositoryConstants.IdSuffix;
            VersionName = RepositoryConstants.RowversionName;
        }

        public IQueryable<TEnt> All
        {
            get { return _queryable; }
        }


        int lastIntPk = 0;
        long lastLongPk = 0;

        public void Save(TEnt ent)
        {
            Type entType = typeof(TEnt);

            object pk = entType.InvokeMember(PrimaryKeyName, System.Reflection.BindingFlags.GetProperty, null, ent, new object[] { });
            Type propertyType = entType.GetProperty(PrimaryKeyName).PropertyType;

            object pkDefault = propertyType.GetDefault();



            if (object.Equals(pk, pkDefault))
            {
                if (propertyType == typeof(int))
                {                    
                    entType.InvokeMember(PrimaryKeyName, System.Reflection.BindingFlags.SetProperty, null, ent, new object[] { ++lastIntPk });                    
                }
                else if (propertyType == typeof(long))
                {
                    entType.InvokeMember(PrimaryKeyName, System.Reflection.BindingFlags.SetProperty, null, ent, new object[] { ++lastLongPk });                    
                }
                else if (propertyType == typeof(Guid))
                {
                    entType.InvokeMember(PrimaryKeyName, System.Reflection.BindingFlags.SetProperty, null, ent, new object[] { Guid.NewGuid() });                    
                }


                PropertyInfo rowVersionPI = entType.GetProperty(VersionName);

                if (rowVersionPI != null)
                    rowVersionPI.SetValue(ent, Guid.NewGuid().ToByteArray(), null);
                


                _list.Add(ent);                
            }
            else
            {
                PropertyInfo rowVersionPI = entType.GetProperty(VersionName);

                byte[] version = null;
                if (rowVersionPI != null)
                    version = (byte[])rowVersionPI.GetValue(ent, null);

                Delete(pk, version);

                

                if (rowVersionPI != null)
                {
                    rowVersionPI.SetValue(ent, Guid.NewGuid().ToByteArray(), null);
                }

                _list.Add(ent);
            }
        }


        public TEnt Get(object id)
        {
            typeof(TEnt).DetectIdType(PrimaryKeyName, id);
            return Load(id);
        }


        public void Delete(object id)
        {
            Delete(id, null);
        }

        public void Delete(object id, byte[] version)
        {
            typeof(TEnt).DetectIdType(PrimaryKeyName, id);



            bool isExisting =
                _list.Any(x =>
                {
                    object existingPkValue = typeof(TEnt).InvokeMember(PrimaryKeyName, System.Reflection.BindingFlags.GetProperty, null, x, new object[] { });

                    return object.Equals(existingPkValue, id);
                });

            int count =
                _list.Count(x =>
                {
                    object existingPkValue = typeof(TEnt).InvokeMember(PrimaryKeyName, System.Reflection.BindingFlags.GetProperty, null, x, new object[] { });

                    PropertyInfo rowVersionPI = typeof(TEnt).GetProperty(VersionName);
                    byte[] existingVersion = null;
                    if (rowVersionPI != null)
                        existingVersion = (byte[])rowVersionPI.GetValue(x, null);
                    
                    if (rowVersionPI != null)
                        return object.Equals(existingPkValue, id) && existingVersion.SequenceEqual(version);
                    else
                        return object.Equals(existingPkValue, id);
                });

            
            if (isExisting && count == 0)
                throw new DbChangesConcurrencyException("Row has been modified since you last loaded it");
            else if (!isExisting)
                throw new DbChangesConcurrencyException("Row has been deleted since you last loaded it");


            _list.Remove(Load(id));
        }


        public void DeleteCascade(object id)
        {
            typeof(TEnt).DetectIdType(PrimaryKeyName, id);
            Delete(id, null);
        }

        public void DeleteCascade(object id, byte[] version)
        {
            typeof(TEnt).DetectIdType(PrimaryKeyName, id);
            Delete(id, version);
        }

        public TEnt LoadStub(object id)
        {
            typeof(TEnt).DetectIdType(PrimaryKeyName, id);
            return LoadStubX(id);
        }

        public string PrimaryKeyName { get; set; }
        public string VersionName { get; set; }

        //////////

        private bool IsEntityExisting(object id)
        {
            typeof(TEnt).DetectIdType(PrimaryKeyName, id);

            return
                _list.Any(x =>
                {
                    object value = x.GetType().InvokeMember(PrimaryKeyName, System.Reflection.BindingFlags.GetProperty, null, x, new object[] { id });

                    return value.Equals(id);
                });
        }

        private TEnt Load(object id)
        {
            typeof(TEnt).DetectIdType(PrimaryKeyName, id);
            return                
                _list.SingleOrDefault(x =>
                {
                    object value = typeof(TEnt).InvokeMember(PrimaryKeyName, System.Reflection.BindingFlags.GetProperty, null, x, new object[] {  });

                    return value.Equals(id);
                });
        }

        private TEnt LoadStubX(object id) 
        {
            string primaryKeyName = typeof(TEnt).Name + RepositoryConstants.IdSuffix;            
            return LoadStubX(primaryKeyName, id);
        }

        private TEnt LoadStubX(string primaryKeyName, object id) 
        {
            typeof(TEnt).DetectIdType(PrimaryKeyName, id);
            TEnt stub = (TEnt)Activator.CreateInstance(typeof(TEnt));

            typeof(TEnt).InvokeMember(primaryKeyName, System.Reflection.BindingFlags.SetProperty, null, stub, new object[] { id });

            return stub;
        }








        public void Evict(object id)
        {
            
        }


        public void Merge(TEnt ent)
        {
            Save(ent);
        }


        public TEnt GetCascade(object id)
        {
            return Get(id);
        }
    }

    
}