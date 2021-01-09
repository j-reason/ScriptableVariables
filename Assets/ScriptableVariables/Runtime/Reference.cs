using System;
using UnityEngine;

namespace Variables
{
    /// <summary>
    /// Class to create a reference to a Variable Asset in code
    /// </summary>
    /// <typeparam name="T">Type we want the Variable to be</typeparam>
    [System.Serializable]
    public class Reference<T>
    {

        #region Serialised Fields
        [SerializeField,Tooltip("Reference to the Variable Asset")]
        private Variable<T> Variable;

        [SerializeField, Tooltip("Set to true if you don't want to reference a Variable")]
        private bool UseConstant = true;

        [SerializeField, Tooltip("Where value is stored if UseConstant is set to true")]
        private T ConstantValue;

        #endregion Serialised Fields

        #region Contructors
        public Reference() { }

        public Reference(T value) : this()
        {
            UseConstant = true;
            ConstantValue = value;
        }
        #endregion Contructors

        /// <summary>
        /// Current Value of the reference, Either stored in the referenced variable, or stored by the reference itself
        /// </summary>
        public T Value
        {
            get
            {
                return UseConstant ? ConstantValue : Variable.Value; ;
            }
            set
            {
                if (UseConstant)
                    ConstantValue = value;
                else
                    Variable.Value = value;
            }
        }


        /// <summary>
        /// Invoked when the value of the refence changes
        /// </summary>
        public Action<T> OnValueChanged
        {
            get
            {
                if (UseConstant)
                {
                    return new Action<T>(_ =>
                    {
                        
                    });
                }
                else
                {
                    return Variable.OnValueChanged;
                }
            }
            set
            {
                if (!UseConstant)
                {
                    Variable.OnValueChanged = value;
                }
            }
        }
    
        
        public static implicit operator T(Reference<T> value) => value.Value;
    }
}