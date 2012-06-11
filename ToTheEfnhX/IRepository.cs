using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ienablemuch.ToTheEfnhX
{
    public interface IRepository<TEnt> where TEnt : class
    {
        IQueryable<TEnt> All { get; }
        
        void Save(TEnt ent);
        void SaveGraph(TEnt ent);
        TEnt Get(object id);
        TEnt GetCascade(object id);


        void Delete(object id);
        void DeleteCascade(object id);

        void Delete(object id, byte[] version);
        void DeleteCascade(object id, byte[] version);
        void Evict(object id);

        TEnt LoadStub(object id);

        string PrimaryKeyName { get; set; }
        string VersionName { get; set; }
    }
}
