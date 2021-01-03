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

        public static GenericMenu GetMenu(Action<Type> callback, Type currentType)
        {
            
            GenericMenu menu = new GenericMenu(); //return value
            Dictionary<string, int> seperatorCheck = new Dictionary<string, int>(); //holds last entry to a sub menu

            //Loop through all Variable Types
            foreach (KeyValuePair<Type, VariableMenuAttribute> pair in CachedAttributes.OrderBy(p => p.Value, new VariableAttributeComparer()))
            {

                VariableMenuAttribute current = pair.Value;
                string path = pair.Value.menuName;
                string subMenu = pair.Value.GetSubMenuOnly();

                //if first instance of submenu add
                if (!seperatorCheck.ContainsKey(subMenu))
                    seperatorCheck.Add(current.GetSubMenuOnly(), current.order);

                //Check if there is a gap of 10 between this and the last attribute, if so add a seperator
                if (current.order - seperatorCheck[subMenu] >= 10)
                    menu.AddSeparator(subMenu);
                seperatorCheck[subMenu] = current.order;

                //Add item to menu, highlight if it is the current type
                menu.AddItem(new UnityEngine.GUIContent(path), currentType?.Equals(pair.Key) ?? false, (p => callback((Type)p)), pair.Key);

            }

            //Add seperator and create new button at end
            menu.AddSeparator("");
            menu.AddItem(new UnityEngine.GUIContent("Create New"), false, VariableCreator.Open);

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