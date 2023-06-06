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
        [SerializeField, Tooltip("Reference to the Variable Asset")]
        private Variable<T> Variable;

        [SerializeField, Tooltip("Set to true if you don't want to reference a Variable")]
        private bool m_useLocal = true;

        [SerializeField, Tooltip("Where value is stored if UseConstant is set to true")]
        private T m_localValue;

        #endregion Serialised Fields

        private Action<T> m_onLocalChange;


        #region Contructors
        public Reference() { }

        public Reference(T value) : this()
        {
            m_useLocal = true;
            m_localValue = value;
        }
        #endregion Contructors

        /// <summary>
        /// Current Value of the reference, Either stored in the referenced variable, or stored by the reference itself
        /// </summary>
        public T Value
        {
            get
            {
                return m_useLocal ? m_localValue : Variable.Value;
            }
            set
            {
                if (m_useLocal)
                {
                    m_localValue = value;
                    m_onLocalChange?.Invoke(value);
                }
                else
                    Variable.Value = value;
            }
        }


        /// <summary>
        /// Invoked when the value of the refence changes
        /// </summary>
        public event Action<T> OnValueChanged
        {
            add
            {

                if (!m_useLocal && Variable != null)
                    Variable.OnValueChanged += value;

                m_onLocalChange += value;

            }
            remove
            {

                if (!m_useLocal && Variable != null)
                    Variable.OnValueChanged -= value;

                m_onLocalChange -= value;

            }
        }


        /// <summary>
        /// Sets the variable reference
        /// If null sets the variable to use the local value
        /// 
        /// </summary>
        /// <param name="variable">Variable to set as reference</param>
        public void SetReference(Variable<T> variable)
        {
            //if this reference isn't set to local and there are events remove them from the variable reference
            if (!m_useLocal && Variable != null && m_onLocalChange != null)
                Variable.OnValueChanged -= m_onLocalChange;

            //Set the variable and if check if it's null
            Variable = variable;
            m_useLocal = (variable == null);

            //If the variable isn't null add all of the event listeners to it's event
            if (!m_useLocal && m_onLocalChange != null)
                Variable.OnValueChanged += m_onLocalChange;
        }

        /// <summary>
        /// Try to get's the Variable that this reference points to.
        /// 
        /// Returns false if reference is set to local, or there is no variable
        /// </summary>
        /// <param name="variable">variable this reference points to</param>
        /// <returns>Returns false if reference is set to local, or there is no variable</returns>
        public bool TryGetVariable(out Variable<T> variable)
        {
            //Set out argument 
            variable = Variable;

            //return false if no variable            
            if (m_useLocal || Variable == null) //could bundle this into one return but this is easier to read
                return false;

            return true;
        }


        public static implicit operator T(Reference<T> value) => value.Value;
    }

}