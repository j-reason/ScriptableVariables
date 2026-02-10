using System;
using UnityEngine;
using System.Collections.Generic;
using Variables.Diagnostics;

namespace Variables
{
    /// <summary>
    /// Generic base variable that all other variables inherit
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [System.Serializable]
    public class Variable<T> : Variable
    {
        /// <summary>
        /// Value of the variable, accessible from the inspector
        /// </summary>
        [SerializeField]
        [UnityEngine.Serialization.FormerlySerializedAs("m_defaultValue")]
        private T m_value;

        /// <summary>
        /// If this is set to true, setting the same value will notify the observers
        /// </summary>
        [HideInInspector]
        public bool allowValueRepeating;

        

        /// <summary>
        /// Current value of the Variable changes 
        /// </summary>
        public virtual T Value
        {
            get
            {
                return GetValue();
            }

            set
            {
                SetValue(value);
            }
        }

        /// <summary>
        /// Called when the Value of variable changes and passes the new value as the parameter
        /// </summary>
        /// <remarks>
        /// Use OnChange if you don't want the new value
        /// </remarks>
        public Action<T> OnValueChanged;

        /// <summary>
        /// Called when the Value of variable changes
        /// </summary>
        /// <remarks>
        /// Use OnValueChanged if you need the new value
        /// </remarks>
        public Action OnChange;

        /// <summary>
        /// Set a new value for the variable
        /// </summary>
        /// <param name="variable"></param>
        public void SetValue(Variable<T> variable)
        {
            SetValue(variable.Value);
        }

        /// <summary>
        /// Set a new value for the variable
        /// </summary>
        /// <param name="variable"></param>
        public void SetValue(T value, bool skipLogging = false)
        {
            // TODO: Make the comparer setable
            if (allowValueRepeating || !EqualityComparer<T>.Default.Equals(GetValue(skiplogging: true), value))
            {
                m_value = value;

#if UNITY_EDITOR
                if (StackLogger.isVariableLogged(this) && !skipLogging)
                {
                    Variables.Diagnostics.StackLogger.LogUsage(this, Diagnostics.StackLogger.FunctionType.Set);
                }
#endif

                OnChange?.Invoke();
                OnValueChanged?.Invoke(value);
                
            }

        }

        /// <summary>
        /// Get current Value of the Variable
        /// </summary>
        /// <returns>Value of the variable</returns>
        public T GetValue(bool skiplogging = false)
        {
#if UNITY_EDITOR
            if (StackLogger.isVariableLogged(this) && !skiplogging)
            {
                Variables.Diagnostics.StackLogger.LogUsage(this, Diagnostics.StackLogger.FunctionType.Get);
            }
#endif

            return m_value;
        }

        public override object GetVariableObject(bool skipLogging = false)
        {
            return GetValue(skipLogging);
        }


        /// <summary>
        /// Implicit cast from Variable<T> type to T
        /// </summary>
        public static implicit operator T(Variable<T> variable) => variable.Value;


    }

    [System.Serializable]
    public class Variable : ScriptableObject
    {
        
        public enum ResetBehaviour
        {
            NeverReset,
            SkipResetThisTime,
            ResetThisTime,
            AlwaysReset
        }


        public ResetBehaviour ResetOption = ResetBehaviour.AlwaysReset;


        public virtual object GetVariableObject(bool skipLogging = false) { return null; }

        public override string ToString()
        {
            return $"[{name}]:{GetVariableObject(skipLogging: true)}";
        }
    }


}