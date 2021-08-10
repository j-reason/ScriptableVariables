using System;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;

namespace Variables {


    /// <summary>
    /// Attribute to control how a Variable Type will appear in the Change Variable Menu
    /// </summary>
    public class VariableMenuAttribute : Attribute
    {

        /// <summary>
        /// Path in Dropdown Item will appear at
        /// </summary>
        public string menuName = "";

        /// <summary>
        /// Order item will appear in submenu
        /// </summary>
        public int order;

        
        public VariableMenuAttribute(){ } //Constructor needed for Attribute


        /// <summary>
        /// Returns the Final Name in the Menu Path
        /// </summary>
        /// <returns>Name in Menu Path</returns>
        public string GetNameOnly()
        {
            //Find Last occurence of '/'
            int lastIndex = menuName.LastIndexOf('/');

            //If it exists return everything after it
            if (lastIndex >= 0 && lastIndex != menuName.Length - 1)
                return menuName.Substring(menuName.LastIndexOf('/') + 1);

            //Else retur entire path
            else
                return menuName;
        }

        /// <summary>
        /// Gets the Menu Path of the Variable without the Name
        /// </summary>
        /// <returns>Menu Path of Variable</returns>
        public string GetSubMenuOnly()
        {
            //Find Last occurence of '/'
            int lastIndex = menuName.LastIndexOf('/');

            //if it exists return string up to that point
            if (lastIndex >= 0)
                return menuName.Substring(0, lastIndex + 1);

            else //else return entire path
                return menuName;
        }


        /// <summary>
        /// Find all VariableAttribute Attributes
        /// Any Variable<> found without an attribute will given a default one
        /// </summary>
        /// <returns></returns>
        public static Dictionary<Type, VariableMenuAttribute> GetAll()
        {
            //return value
            Dictionary<Type, VariableMenuAttribute> retVal = new Dictionary<Type, VariableMenuAttribute>();

            //Loop through all loaded Types in the Assembly
            foreach (Type type in AppDomain.CurrentDomain.GetAssemblies().AsParallel().SelectMany(p => p.GetTypes()))
            {
                
                //Check if the type has the Attribute and add it to the dictionary
                var attrbiute = type.GetCustomAttribute<VariableMenuAttribute>();
                if (attrbiute != null)
                {
                    if (type.IsSubclassOf(typeof(Variable))){
                        retVal.Add(type, attrbiute);
                    }
                    else
                    {
                        UnityEngine.Debug.LogWarning($"The [VariableMenu] attribute can only be used on a class which inherits from Variable.\n" +
                            $"Class: <color=#4B79F0> {type.FullName}</color>");
                    }
                }

                //If it doen't have the attribute but it is of type Variable create a default Attribute for it
                else if (!type.IsAbstract && !type.IsGenericType && typeof(Variable<>).IsAssignableFrom(type))
                {
                    retVal.Add(type, new VariableMenuAttribute()
                    {
                        menuName = type.FullName.Replace('.', '/'),
                        order = 1000
                    });
   
                }//end If/Else
            }//end Foreach

            //Find any attributes with the same MenuName 
            var duplicateValues = retVal.GroupBy(x => x.Value.menuName).Where(x => x.Count() > 1)
                .Select(x => new
                {
                    Occurrences = x.Select(o => new {Key = o.Key, Value = o.Value})
                });

            //For each duplicated MenuName add the actual type on to the end of the name for clarification
            foreach (var duplication in duplicateValues.SelectMany(p => p.Occurrences))
            {
                duplication.Value.menuName += $" ({duplication.Key.Name})";
            }

            //Return all found Attributes
            return retVal;
        }

    }

    

}
