using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Linq.Expressions;


using Ienablemuch.ToTheEfnhX.ForImplementorsOnly;


namespace Ienablemuch.ToTheEfnhX.Memory
{
    public class MemoryRepository<Ent> : IRepository<Ent> where Ent : class
    {


        IList<Ent> _list = null;
        IQueryable<Ent> _queryable = null;

        public MemoryRepository()
        {
            _list = new List<Ent>();
            _queryable = _list.AsQueryable();

            PrimaryKeyName = typeof(Ent).Name + RepositoryConstants.IdSuffix;
            VersionName = RepositoryConstants.RowversionName;
        }

        public IQueryable<Ent> All
        {
            get { return _queryable; }
        }


        int lastIntPk = 0;
        long lastLongPk = 0;

        public void Save(Ent ent, byte[] version)
        {
            Type entType = typeof(Ent);

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

                entType.InvokeMember(VersionName, System.Reflection.BindingFlags.SetProperty, null, ent, new object[] { Guid.NewGuid().ToByteArray() });


                _list.Add(ent);                
            }
            else
            {

                Delete(pk, version);

                typeof(Ent).InvokeMember(VersionName, System.Reflection.BindingFlags.SetProperty, null, ent, new object[] { Guid.NewGuid().ToByteArray() });

                _list.Add(ent);
            }
        }


        public Ent Get(object id)
        {
            typeof(Ent).DetectIdType(PrimaryKeyName, id);
            return Load(id);
        }



        public void Delete(object id, byte[] version)
        {
            typeof(Ent).DetectIdType(PrimaryKeyName, id);



            bool isExisting =
                _list.Any(x =>
                {
                    object existingPkValue = typeof(Ent).InvokeMember(PrimaryKeyName, System.Reflection.BindingFlags.GetProperty, null, x, new object[] { });

                    return object.Equals(existingPkValue, id);
                });

            int count =
                _list.Count(x =>
                {
                    object existingPkValue = typeof(Ent).InvokeMember(PrimaryKeyName, System.Reflection.BindingFlags.GetProperty, null, x, new object[] { });
                    object existingVersion = typeof(Ent).InvokeMember(VersionName, System.Reflection.BindingFlags.GetProperty, null, x, new object[] { });

                    // return object.Equals(existingPkValue, id) && object.Equals(existingVersion, version);
                    return object.Equals(existingPkValue, id) && ((byte[])existingVersion).SequenceEqual(version);
                });

            
            if (isExisting && count == 0)
                throw new DbChangesConcurrencyException("Row has been modified since you last loaded it");
            else if (!isExisting)
                throw new DbChangesConcurrencyException("Row has been deleted since you last loaded it");


            _list.Remove(Load(id));
        }

        public void DeleteCascade(object id, byte[] version)
        {
            typeof(Ent).DetectIdType(PrimaryKeyName, id);
            Delete(id, version);
        }

        public Ent LoadStub(object id)
        {
            typeof(Ent).DetectIdType(PrimaryKeyName, id);
            return LoadStubX(id);
        }

        public string PrimaryKeyName { get; set; }
        public string VersionName { get; set; }

        //////////

        private bool IsEntityExisting(object id)
        {
            typeof(Ent).DetectIdType(PrimaryKeyName, id);

            return
                _list.Any(x =>
                {
                    object value = x.GetType().InvokeMember(PrimaryKeyName, System.Reflection.BindingFlags.GetProperty, null, x, new object[] { id });

                    return value.Equals(id);
                });
        }

        private Ent Load(object id)
        {
            typeof(Ent).DetectIdType(PrimaryKeyName, id);
            return                
                _list.SingleOrDefault(x =>
                {
                    object value = typeof(Ent).InvokeMember(PrimaryKeyName, System.Reflection.BindingFlags.GetProperty, null, x, new object[] {  });

                    return value.Equals(id);
                });
        }

        private Ent LoadStubX(object id) 
        {
            string primaryKeyName = typeof(Ent).Name + RepositoryConstants.IdSuffix;
            return LoadStubX(primaryKeyName, id);
        }

        private Ent LoadStubX(string primaryKeyName, object id) 
        {
            typeof(Ent).DetectIdType(PrimaryKeyName, id);
            Ent stub = (Ent)Activator.CreateInstance(typeof(Ent));

            typeof(Ent).InvokeMember(primaryKeyName, System.Reflection.BindingFlags.SetProperty, null, stub, new object[] { id });

            return stub;
        }








        public void Evict(object id)
        {
            
        }


        public void Merge(Ent ent, byte[] version)
        {
            Save(ent, version);
        }


        public Ent GetCascade(object id)
        {
            return Get(id);
        }
    }

    
}