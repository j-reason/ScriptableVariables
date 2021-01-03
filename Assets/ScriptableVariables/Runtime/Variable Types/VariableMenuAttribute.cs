using System;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using UnityEditor.Compilation;

namespace Variables {

    public class VariableMenuAttribute : Attribute
    {
        public string menuName = "";
        public int order;

        public VariableMenuAttribute()
        {
        }

        public string GetNameOnly()
        {
            int lastIndex = menuName.LastIndexOf('/');
            if (lastIndex >= 0 && lastIndex != menuName.Length - 1)
            {
                return menuName.Substring(menuName.LastIndexOf('/') + 1);
            }
            else
            {
                return menuName;
            }
        }

        public string GetSubMenuOnly()
        {
            int lastIndex = menuName.LastIndexOf('/');
            if (lastIndex >= 0)
            {
                return menuName.Substring(0, lastIndex + 1);
            }
            else
            {
                return menuName;
            }
        }


        public static Dictionary<Type, VariableMenuAttribute> GetAll()
        {
            Dictionary<Type, VariableMenuAttribute> retVal = new Dictionary<Type, VariableMenuAttribute>();
            foreach (Type type in AppDomain.CurrentDomain.GetAssemblies().AsParallel().SelectMany(p => p.GetTypes()))
            {
                
                var attrbiute = type.GetCustomAttribute<VariableMenuAttribute>();
                if (attrbiute != null)
                {
                    retVal.Add(type, attrbiute);
                }else if (!type.IsAbstract && !type.IsGenericType && typeof(Variable).IsAssignableFrom(type))
                {
                    retVal.Add(type, new VariableMenuAttribute()
                    {
                        menuName = type.FullName.Replace('.', '/'),
                        order = 1000
                    });
   
                }
            }

            var duplicateValues = retVal.GroupBy(x => x.Value.menuName).Where(x => x.Count() > 1)
                .Select(x => new
                {
                    Occurrences = x.Select(o => new {Key = o.Key, Value = o.Value})
                });

            foreach (var duplication in duplicateValues.SelectMany(p => p.Occurrences))
            {
                duplication.Value.menuName += $" ({duplication.Key.Name})";
            }


            return retVal;
        }

    }

    

}
