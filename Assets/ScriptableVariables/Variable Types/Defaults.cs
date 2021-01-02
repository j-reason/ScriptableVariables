using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Variables
{
    [CreateAssetMenu(fileName = "new variable", menuName = "Variable", order = 100)]
    [VariableMenu(menuName = "Default/Int", order = 1)]
    public class IntVariable : Variable<int> { }

    [VariableMenu(menuName = "Default/Float", order = 2)]
    public class FloatVariable : Variable<float> { }

    [VariableMenu(menuName = "Default/Bool", order = 3)]
    public class BoolVariable : Variable<bool> { }

    [VariableMenu(menuName = "Default/String", order = 4)]
    public class StringVariable : Variable<string> { }

    [VariableMenu(menuName = "Default/Vector2", order = 101)]
    public class Vector2Variable : Variable<Vector2> { }

    [VariableMenu(menuName = "Default/Vector3", order = 102)]
    public class Vector3Variable : Variable<Vector3> { }

    [VariableMenu(menuName = "Default/Vector4", order = 103)]
    public class Vector4Variable : Variable<Vector4> { }

    [VariableMenu(menuName = "Default/Quaternion", order = 104)]
    public class QuaternionVariables : Variable<Quaternion> { }

    [VariableMenu(menuName = "Default/Color", order = 201)]
    public class ColorVariable : Variable<Color> { }

    [VariableMenu(menuName = "Unity/GameObject", order = 1)]
    public class GameObjectVariable : Variable<GameObject> { }

    [VariableMenu(menuName = "Unity/Transform", order = 2)]
    public class TansformVariable : Variable<Transform> { }

    public class testVariable : Variable<Transform> { }
}

