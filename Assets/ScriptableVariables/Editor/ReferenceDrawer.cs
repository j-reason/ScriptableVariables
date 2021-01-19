using System.Collections;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Variables.Editor
{
    [CustomPropertyDrawer(typeof(Reference<>), true)]
    public class ReferenceDrawer : PropertyDrawer
    {

        // Get properties
        SerializedProperty m_useLocal;
        SerializedProperty m_localValue;
        SerializedProperty variable;




        /// <summary>
        /// Options to display in the popup to select constant or variable.
        /// </summary>
        private readonly string[] popupOptions =
            { "Use Local", "Use Variable" };

        /// <summary> Cached style to use to draw the popup button. </summary>
        private GUIStyle popupStyle;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {

            if (popupStyle == null)
            {
                popupStyle = new GUIStyle(GUI.skin.GetStyle("PaneOptions"));
                popupStyle.imagePosition = ImagePosition.ImageOnly;
            }

            FindProperties(property);

            label = EditorGUI.BeginProperty(position, label, property);
            position = EditorGUI.PrefixLabel(position, label);

            

            // Calculate rect for configuration button
            Rect buttonRect = new Rect(position);
            buttonRect.yMin += popupStyle.margin.top;
            buttonRect.width = popupStyle.fixedWidth + popupStyle.margin.right;
            position.xMin = buttonRect.xMax;
            if (m_useLocal.boolValue && m_localValue.hasChildren)
                position.xMin += 10;

            // Store old indent level and set it to 0, the PrefixLabel takes care of it
            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            EditorGUI.BeginChangeCheck();

            int result = EditorGUI.Popup(buttonRect, m_useLocal.boolValue ? 0 : 1, popupOptions, popupStyle);
            m_useLocal.boolValue = result == 0;

            if (EditorGUI.EndChangeCheck())
            {
                property.serializedObject.ApplyModifiedProperties();
                OnUseLocalChanged(property);
            }


            EditorGUI.BeginChangeCheck();

            EditorGUI.PropertyField(position,
                m_useLocal.boolValue ? m_localValue : variable,
                GUIContent.none, true);

            if (EditorGUI.EndChangeCheck())
            {
                property.serializedObject.ApplyModifiedProperties();

                if (m_useLocal.boolValue)
                    OnLocalValueChanged(property);

            }

            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }


        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            FindProperties(property);
            int height = (m_useLocal.boolValue) ? m_localValue.CountInProperty() : 1;

            return (EditorGUIUtility.singleLineHeight * height) + (EditorGUIUtility.standardVerticalSpacing * (height-1));
        }

        private void FindProperties(SerializedProperty property)
        {
                m_useLocal = property.FindPropertyRelative("m_useLocal");

                m_localValue = property.FindPropertyRelative("m_localValue");

                variable = property.FindPropertyRelative("Variable");

        }

        private void OnUseLocalChanged(SerializedProperty property)
        {
            object reference = fieldInfo.GetValue(property.serializedObject.targetObject);


            dynamic Variable = fieldInfo.FieldType.GetField(
               "Variable",
               BindingFlags.NonPublic | BindingFlags.Instance
               ).GetValue(reference);

            dynamic m_onLocalChange = fieldInfo.FieldType.GetField(
               "m_onLocalChange",
               BindingFlags.NonPublic | BindingFlags.Instance
               ).GetValue(reference);

            if (Variable == null || m_onLocalChange == null)
                return;

            if (m_useLocal.boolValue)
                Variable.OnValueChanged -= m_onLocalChange;
            else
                Variable.OnValueChanged += m_onLocalChange;
        }

        private void OnLocalValueChanged(SerializedProperty property)
        {

            object reference = fieldInfo.GetValue(property.serializedObject.targetObject);

            dynamic m_onLocalChange = fieldInfo.FieldType.GetField(
               "m_onLocalChange",
               BindingFlags.NonPublic | BindingFlags.Instance
               ).GetValue(reference);

            dynamic m_localValue = fieldInfo.FieldType.GetField(
               "m_localValue",
               BindingFlags.NonPublic | BindingFlags.Instance
               ).GetValue(reference);


            if (m_onLocalChange != null)
                m_onLocalChange.Invoke(m_localValue);
        }






    }
}