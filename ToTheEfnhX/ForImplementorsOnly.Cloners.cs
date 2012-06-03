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

        public static object Clone(this object root)
        {

            lock (root)
            {

                Type rootType = root.GetType();
                object clone = Activator.CreateInstance(rootType);

                foreach (PropertyInfo pi in rootType.GetProperties())
                {
                    bool isCollection = pi.PropertyType.IsGenericType && typeof(IEnumerable).IsAssignableFrom(pi.PropertyType);
                    if (!isCollection)
                    {
                        object transientVal = rootType.InvokeMember(pi.Name, BindingFlags.GetProperty, null, root, new object[] { });
                        rootType.InvokeMember(pi.Name, BindingFlags.SetProperty, null, clone, new object[] { transientVal });
                    }
                    else
                    {
                        var colOrig = rootType.InvokeMember(pi.Name, BindingFlags.GetProperty, null, root, new object[] { });

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
                        
                        rootType.InvokeMember(pi.Name, BindingFlags.SetProperty, null, clone, new object[] { clonedList });
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
            
            lock(origParent) lock(origList) lock(cloneParent) lock(cloneList)            
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
