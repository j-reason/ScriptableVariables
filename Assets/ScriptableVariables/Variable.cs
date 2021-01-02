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
        /// Default value of the variable, exposed in editor if T is serializable, should not be changed from game code without a good reason
        /// </summary>
        public T defaultValue;

        /// <summary>
        /// If this is set to true, setting the same value will notify the observers
        /// </summary>
        [HideInInspector]
        public bool allowValueRepeating;

        [SerializeField]
        protected T m_currentValue;

        /// <summary>
        /// Current value of the variable
        /// </summary>
        public virtual T currentValue
        {
            get
            {
                return m_currentValue;
            }
            set
            {
                // TODO: Make the comparer setable
                if (allowValueRepeating || !EqualityComparer<T>.Default.Equals(m_currentValue, value))
                {
                    m_currentValue = value;
                    OnValueChanged?.Invoke(value);
                }
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
            currentValue = variable.currentValue;
        }

        /// <summary>
        /// Set a new value for the variable
        /// </summary>
        /// <param name="variable"></param>
        public void SetValue(T value)
        {
            currentValue = value;
        }

        private void OnEnable()
        {
            currentValue = defaultValue;
        }

        public static implicit operator T(Variable<T> variable)
        {
            if (variable == null)
            {
                return default;
            }
            return variable.currentValue;
        }
    }

    public abstract class Variable : ScriptableObject
    { }

}