# References
References are how [Variables](https://github.com/j-reason/ScriptableVariables/blob/main/Assets/ScriptableVariables/Documentation%7E/Variables.md) are accessed in Code. References can also have a hold a local Value if they are not linked the a Variable Asset.

![Inspector](Media/ReferenceInspector.gif?raw=true)

## API
*Namespace:* Variables 

**Decleration**
`public Reference<T> ExampleReference` where `T` is the value type stored in the variable being referenced.
`public Reference<float> FloatReference` *(example)*

**Parameters**
| Type          | Name                 | Description|
|--------------|-----------------------|------------|
| `T`          | *Value*               | Value of the `Variable` being references, or the local value of the Reference     |
| `Action<T>`  | *OnValueChanged*      | An event which is called everytime `Value` is changed      |

**Functions**
**`void SetReference(Variable<T> variable)`**
Sets the Variable linked to the Reference.


## Examples

```cs
    //Reference to Vector3 Variable
    public Reference<Vector3> m_direction;

    private void Update()
    {
        //How to set the value of the variable
        m_direction.Value = transform.forward;

        //Can implicitly cast when getting the value of the variable
        transform.position += m_direction;

        //Sometimes you'll need to do this though
        transform.position += m_direction.Value * Time.deltaTime;
        transform.position += (Vector3)m_direction * Time.deltaTime;  
    }
```