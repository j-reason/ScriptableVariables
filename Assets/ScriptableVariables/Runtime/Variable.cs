using System;
using UnityEngine;
using System.Collections.Generic;

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
        /// Current value of the variable
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
        /// Delegate for holding actions to be called when value changes
        /// </summary>
        public Action<T> OnValueChanged;

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
        public void SetValue(T value)
        {
            // TODO: Make the comparer setable
            if (allowValueRepeating || !EqualityComparer<T>.Default.Equals(GetValue(), value))
            {
                m_value = value;
                OnValueChanged?.Invoke(value);
            }

        }

        /// <summary>
        /// Get current Value of the Variable
        /// </summary>
        /// <returns>Value of the variable</returns>
        public T GetValue()
        {
            return m_value;
        }


        /// <summary>
        /// Implicit cast from Variable<T> type to T
        /// </summary>
        public static implicit operator T(Variable<T> variable) => variable.Value;


    }

    [System.Serializable]
    public class Variable : ScriptableObject
    {

    }


}