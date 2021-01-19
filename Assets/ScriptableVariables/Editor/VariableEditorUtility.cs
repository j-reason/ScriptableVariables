using System.Collections;
using System.Collections.Generic;
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
                "m_defaultValue",
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
               "m_defaultValue",
               BindingFlags.NonPublic | BindingFlags.Instance
               ).GetValue(variable);
        }


        public static void InvokeVariableEvent(dynamic variable)
        {
            dynamic value = variable.Value;
            if (!EditorApplication.isPlaying)
                value = GetDefaultValue(variable);

            variable.OnValueChanged?.Invoke(value);
        }

        public static void InvokeLocalReferenceEvent(dynamic reference)
        {
            dynamic value = reference.m_localValue;
            


        }




    }

}
