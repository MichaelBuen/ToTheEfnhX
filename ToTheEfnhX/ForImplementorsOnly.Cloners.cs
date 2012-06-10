using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Collections;

namespace Ienablemuch.ToTheEfnhX.ForImplementorsOnly
{
    public static class ObjectCloner
    {
        public const string EFProxyNamespace = "System.Data.Entity.DynamicProxies";

        public static object Clone(this object root)
        {

            lock (root)
            {

                // Type rootType = root.GetType();

                Type realType = root.GetType();

                if (realType.BaseType != null && realType.Namespace == ObjectCloner.EFProxyNamespace)
                {
                    realType = realType.BaseType;
                }

                object clone = Activator.CreateInstance(realType);

                foreach (PropertyInfo pi in realType.GetProperties())
                {
                    


                    bool isCollection = pi.PropertyType.IsGenericType && typeof(IEnumerable).IsAssignableFrom(pi.PropertyType);
                    if (!isCollection)
                    {


                        object transientVal = realType.InvokeMember(pi.Name, BindingFlags.GetProperty, null, root, new object[] { });


                        realType.InvokeMember(pi.Name, BindingFlags.SetProperty, null, clone, new object[] { transientVal });
                    }
                    else
                    {
                        var colOrig = realType.InvokeMember(pi.Name, BindingFlags.GetProperty, null, root, new object[] { });

                        if (colOrig == null) continue;

                        object clonedList = null;
                        if (!colOrig.GetType().IsArray)
                            clonedList = Activator.CreateInstance(colOrig.GetType());
                        else
                        {
                            // This portion happened when an IList's List is automatically translated to array, scenario: WCF

                            // The problem: http://www.nichesoftware.co.nz/blog/201005/wcf-and-ilistt
                            // The easier solution: http://bartdesmet.net/blogs/bart/archive/2006/09/11/4410.aspx

                            Type elemType = colOrig.GetType().GetElementType();
                            clonedList = Create("System.Collections.Generic.List", elemType);                            
                        }
                        
                        realType.InvokeMember(pi.Name, BindingFlags.SetProperty, null, clone, new object[] { clonedList });
                        CloneCollection(root, (IList)colOrig, clone, (IList)clonedList);

                    }
                }
                return clone;
                
            }
        }


        private static object Create(string name, params Type[] types)
        {
            string t = name + "`" + types.Length;
            Type generic = Type.GetType(t).MakeGenericType(types);
            return Activator.CreateInstance(generic);
        }

        private static void CloneCollection(object origParent, IList origList, object cloneParent, IList cloneList)
        {            
            
            
            foreach (object item in origList)
            {
                object cloneItem = item.Clone();

                foreach(PropertyInfo pi in cloneItem.GetType().GetProperties().Where(x => x.PropertyType == origParent.GetType()))
                {
                    object val = cloneItem.GetType().InvokeMember(pi.Name, BindingFlags.GetProperty, null, cloneItem, new object[] { });

                    if (object.ReferenceEquals(val,origParent))
                    {
                        // point it to its new parent
                        cloneItem.GetType().InvokeMember(pi.Name, BindingFlags.SetProperty, null, cloneItem, new object[] { cloneParent }); 
                    }
                }

                cloneList.Add(cloneItem);
            }

        }

    }

}
