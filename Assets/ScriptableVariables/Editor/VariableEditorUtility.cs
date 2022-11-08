using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEditor;

namespace Variables.Editor
{

    /// <summary>
    /// Provides a collection of helper functions to help with Editor Scripts
    /// </summary>
    public static class EditorVariableUtility
    {

        /// <summary>
        /// Sets the Default Value of a variable through reflection
        /// </summary>
        /// <param name="variable">Variable to set the default value of</param>
        /// <param name="value">new value to set as</param>
        public static void SetDefaultValue<T>(Variable<T> variable, T value)
        {
            //using reflection to set it
            variable.GetType().BaseType.GetField(
                "m_value",
                BindingFlags.NonPublic | BindingFlags.Instance
                ).SetValue(variable, value);
        }

        /// <summary>
        /// Gets the 'Default Value' of a variable through reflection
        /// </summary>
        /// <param name="variable">Variable to get the 'Default Value' of</param>
        public static T GetDefaultValue<T>(Variable<T> variable)
        {
            //using reflection to get it
            return (T) variable.GetType().BaseType.GetField(
               "m_value",
               BindingFlags.NonPublic | BindingFlags.Instance
               ).GetValue(variable);
        }


        public static void InvokeVariableEvent(dynamic variable)
        {
            dynamic value = variable.Value;
            variable.OnValueChanged?.Invoke(value);
        }

        public static void InvokeLocalReferenceEvent(dynamic reference)
        {
            dynamic value = reference.m_localValue;         
        }

        public static void AttachAssetTo(ScriptableObject asset,Object newRoot)
        {
            string path = AssetDatabase.GetAssetPath(asset);
            
        }

        public static void AttachAssetTo(ScriptableObject asset, string newPath)
        {
            string path = AssetDatabase.GetAssetPath(asset);

            if (path.Equals(newPath))
                return;

            Debug.Log(newPath);
            Object[] data = AssetDatabase.LoadAllAssetsAtPath(path);


            foreach (ScriptableObject dataObject in data)
            {
                AssetDatabase.RemoveObjectFromAsset(dataObject);
                AssetDatabase.AddObjectToAsset(dataObject, newPath);
            }

            AssetDatabase.DeleteAsset(path);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }


        public static void DeattachAsset(ScriptableObject asset)
        {

            bool isSub = AssetDatabase.IsSubAsset(asset);
            string path = AssetDatabase.GetAssetPath(asset);
            Object[] data = AssetDatabase.LoadAllAssetsAtPath(path);

            string Directory = System.IO.Path.GetDirectoryName(path);

            foreach (ScriptableObject dataObject in data)
            {
                if ((isSub && dataObject.Equals(asset)) || (!isSub))
                {
                    Debug.Log("Unzipping: " + dataObject.name);
                    try
                    {
                        AssetDatabase.RemoveObjectFromAsset(dataObject);
                        AssetDatabase.SaveAssets();

                        string uniquePath = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(Directory, dataObject.name + ".asset"));

                        AssetDatabase.CreateAsset(dataObject, uniquePath);
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }






    }

}
