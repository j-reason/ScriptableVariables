using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Compilation;

namespace Variables.Editor
{

    [InitializeOnLoad]
    public static class VariableMenuUtility
    {

        public static Dictionary<Type, VariableMenuAttribute> CachedAttributes;

        static VariableMenuUtility()
        {
            CachedAttributes = new Dictionary<Type, VariableMenuAttribute>();
            UpdateAttributes(null);
            CompilationPipeline.compilationFinished += UpdateAttributes;
        }


        private static void UpdateAttributes(object sender)
        {
            CachedAttributes = VariableMenuAttribute.GetAll();
        }

        public static GenericMenu GetMenu(Action<Type> callback,Type currentType)
        {
            GenericMenu menu = new GenericMenu();
            VariableMenuAttribute lastAttribute = null;

            foreach(KeyValuePair<Type, VariableMenuAttribute> pair in CachedAttributes.OrderBy(p => p.Value, new VariableAttributeComparer()))
            {
                UnityEngine.Debug.Log(pair.Value.menuName);
                VariableMenuAttribute current = pair.Value;
                if (lastAttribute == null)
                    lastAttribute = current;

                string path = pair.Value.menuName; //+ "/" + pair.Key.Name.Replace("Variable", "").Replace("variable", "");
   
                if (lastAttribute.GetSubMenuOnly().Equals(current.GetSubMenuOnly()))
                {
                    if (current.order - lastAttribute.order >= 10 || current.order < lastAttribute.order)
                        menu.AddSeparator(current.GetSubMenuOnly());
                }
              
                menu.AddItem(new UnityEngine.GUIContent(path), currentType?.Equals(pair.Key) ?? false, (p => callback((Type)p)), pair.Key);
                lastAttribute = current;
            }

            return menu;
        }




 
    }

    public class VariableAttributeComparer : IComparer<VariableMenuAttribute>
    {
        public int Compare(VariableMenuAttribute x, VariableMenuAttribute y)
        {



            int xCount = x.menuName.Count(c => c == '/');
            int yCount = y.menuName.Count(c => c == '/');

            if (xCount - yCount == 1)
            {
                UnityEngine.Debug.Log($"{x.menuName}[{xCount}] after {y.menuName}[{yCount}] (1)");
                return -1;
            }
            else if (yCount - xCount == 1)
            {
                UnityEngine.Debug.Log($"{x.menuName}[{xCount}] before {y.menuName}[{yCount}] (-1)");
                return 1;
            }
            else
            {

                return x.order.CompareTo(y.order);
            }
        }
    }


}