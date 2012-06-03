using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ienablemuch.ToTheEfnhX.ForImplementorsOnly
{
    public static class RepositoryConstants
    {
        // rationales: 
        // http://msdn.microsoft.com/en-us/library/ms182256(v=vs.80).aspx
        // http://10rem.net/articles/net-naming-conventions-and-programming-standards---best-practices
        // http://stackoverflow.com/questions/596062/net-naming-convention-for-id-anything-identification-capitalization
        public readonly static string IdSuffix = "Id";


        public readonly static string RowversionName = "RowVersion";

    }


    public static class Helper
    {
        public static object GetDefault(this Type type)
        {
            if (type.IsValueType)
            {
                return Activator.CreateInstance(type);
            }
            return null;
        }


        public static void DetectIdType(this Type ent, string primaryKeyName, object id)
        {
            if (ent.GetProperty(primaryKeyName).PropertyType != id.GetType())
                throw new IdParameterIsInvalidTypeException("Id is invalid type: " + id.GetType() + ", didn't matched the repository's primary key. Most IDs are of primitive type. Contact the dev to fix this problem");
        }
    }


}
