using Variables;
using System;

namespace Variables.Types
{
    [UnityEngine.CreateAssetMenu(fileName = "new variable", menuName = "Variable", order = 120)]
    [VariableMenu(menuName = "Default/Int", order = 1)]
    public class Int32Variable : Variable<Int32> { }
}
