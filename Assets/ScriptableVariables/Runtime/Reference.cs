using System;
using UnityEngine.Assertions;

namespace Variables
{
    /// Generic base variable reference that all other variable references inherit
    [System.Serializable]
    public class Reference<T>
    {
        public bool UseConstant = true;
        public T ConstantValue;
        public Variable<T> Variable;

        public Reference() { }

        public Reference(T value): this() {
            UseConstant = true;
            ConstantValue = value;
        }
        
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
    
        public T Value
        {
            get
            {
                return UseConstant ? ConstantValue : Variable.currentValue;;                
            }
            set {
                if (UseConstant)
                    ConstantValue = value;
                else
                    Variable.currentValue = value;
            }
        }

        public static implicit operator T(Reference<T> reference)
        {
            return reference.Value;
        }
    }
}