﻿using System;
using UnityEngine;
using System.Collections.Generic;

namespace Variables
{
    /// <summary>
    /// Generic base variable that all other variables inherit
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [System.Serializable]
    public class Variable<T> : ScriptableObject
    {
        /// <summary>
        /// Value of the variable set in the inspector, used to set m_currentValue at the game starts
        /// </summary>
        [SerializeField]
        private T m_defaultValue;

        /// <summary>
        /// Value of the variable at runtime, reset when the game starts
        /// </summary>
        [SerializeField]
        protected T m_currentValue;

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
                m_currentValue = value;
                OnValueChanged?.Invoke(value);
            }

        }

        /// <summary>
        /// Get current Value of the Variable
        /// </summary>
        /// <returns>Value of the variable</returns>
        public T GetValue()
        {
            return m_currentValue;
        }


        private void OnEnable()
        {
            //set the current Variable to default on start
            SetValue(Utility.TryDeepCopy(m_defaultValue));
        }

        public static implicit operator T(Variable<T> variable) => variable.Value;


    }

}