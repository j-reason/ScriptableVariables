using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


namespace Variables.Editor
{

    /// <summary>
    /// Custom Property Drawer for Variables
    /// </summary>
    [CustomPropertyDrawer(typeof(Variable<>), false)]
    public class VariableDrawer : PropertyDrawer
    {

        #region Private Variables

        //Readable name of the type
        private string m_TypeName;

        #endregion Private Variables

        #region Unity Functions
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            //Make sure all values are setup
            CacheValues();

            //draw label and get rect pos without label
            label = EditorGUI.BeginProperty(position, label, property);
            position = EditorGUI.PrefixLabel(position, label);

            //Start the property
            EditorGUI.BeginChangeCheck();

            //If the object reference is null
            if (property.objectReferenceValue == null)
            {
                //Draw a clear Object field and then draw the type on after
                DrawClearObjectField(position, property, GUIContent.none);
                GUI.Label(position, $"None ({m_TypeName})");
            }

            //Else just draw the property field normally
            else
                EditorGUI.PropertyField(position, property, GUIContent.none);

            //if there has been a changes to the property apply them
            if (EditorGUI.EndChangeCheck())
                property.serializedObject.ApplyModifiedProperties();
            EditorGUI.EndProperty();

            ObjectPickerGUI(property, position);

            //Deal with object draggin
            DragGUI(position);
        }
        #endregion Unity Functions

        #region GUI Functions
        /// <summary>
        /// Generic Properties try and accept any object from the same generic parent being dragged in
        /// This function fixes the visuals of the Drag and Drop
        /// </summary>
        /// <param name="position">position of the property</param>


        private void ObjectPickerGUI(SerializedProperty property, Rect position)
        {
            //Gets the position we expect to find the picker button
            float size = position.height; //the button is square so this will probably be fine
            Rect rectPicker = new Rect(position.xMax - size, position.y, size, size);

            //EditorGUI.DrawRect(rectPicker, new Color(1, 0, 0, 0.5f));

            //Only mouse up is registered so we check if mouse-up happens over the picker
            Event e = Event.current;
            if (e.type == EventType.MouseUp && rectPicker.Contains(e.mousePosition))
            {
                Debug.Log($"Filter: t:{GetProabableName()}");

                EditorGUIUtility.ShowObjectPicker<Object>(property.objectReferenceValue, false, $"t:{GetProabableName()}", EditorGUIUtility.GetObjectPickerControlID());
                //property.objectReferenceValue = EditorGUIUtility.GetObjectPickerObject();

                property.serializedObject.ApplyModifiedProperties();
            }
            else
            {
                Debug.Log($"Event: {e.type}");
            }

        }


        private void DragGUI(Rect position)
        {
            //Check if mouse is hovering over the property and that it is dragging something
            Event e = Event.current;
            if (!position.Contains(e.mousePosition) || ((DragAndDrop.objectReferences?.Length ?? 0) == 0))
                return;

            //if multiple things are being dragged show that they can't be accepted in the one property
            if (DragAndDrop.objectReferences.Length != 1)
                DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;

            //if the dragged object isn't the correct type show the reject icon
            Object draggedObject = DragAndDrop.objectReferences[0];
            if (!fieldInfo.FieldType.IsAssignableFrom(draggedObject.GetType()))
                DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
        }
        #endregion

        #region Helper Functions
        /// <summary>
        /// Sets up any values which would need to be cached
        /// </summary>
        private void CacheValues()
        {
            //Get the name of the type if it's not yet setup 
            if (string.IsNullOrEmpty(m_TypeName))
                m_TypeName = GetReadableTypeName();
        }

        /// <summary>
        /// Draws an Object field but without the text in the centre of it
        /// </summary>
        /// <param name="position">Position to draw the Field at</param>
        /// <param name="property">Serialised property to draw the object field of</param>
        /// <param name="label">Label of the object field</param>
        private void DrawClearObjectField(Rect position, SerializedProperty property, GUIContent label)
        {

            //Property Fields can't have custom guistyles or else that would be the correct way to do it

            //Set the text size to one so you can't see it
            int fontSize = EditorStyles.objectField.fontSize;
            EditorStyles.objectField.fontSize = 1; //can't use zero, but it isn't noticable at this sizeo
            EditorGUI.PropertyField(position, property, label);

            //reset font
            EditorStyles.objectField.fontSize = fontSize;
        }

        /// <summary>
        /// Gets the correct type name for the Generic property
        /// </summary>
        /// <returns></returns>
        private string GetReadableTypeName()
        {
            //If it isn't a generic just pass return the class name
            if (!fieldInfo.FieldType.IsGenericType)
                return fieldInfo.FieldType.Name;

            //Gets primitive names (e.g. Float instead of single)
            var compiler = new Microsoft.CSharp.CSharpCodeProvider();
            string readableName = compiler.GetTypeOutput(new System.CodeDom.CodeTypeReference(fieldInfo.FieldType.GetGenericArguments()[0]));

            //Remove Namespace by finding last '.'
            int lastIndex = readableName.LastIndexOf('.');
            if (lastIndex >= 0 && lastIndex != readableName.Length - 1)
                readableName = readableName.Substring(lastIndex + 1);

            //Add variable to make clear it's a generic
            return $"Variable<{readableName}>";
        }

        private string GetProabableName()
        {
            //If it isn't a generic just pass return the class name
            if (!fieldInfo.FieldType.IsGenericType)
                return fieldInfo.FieldType.Name;


            return fieldInfo.FieldType.GetGenericArguments()[0].Name + "Variable";
        }

        #endregion Helper Functions

    }
}