using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Collections;
using System.Reflection;
using Inspector = UnityEditor.Editor;


namespace Variables.Editor
{
    [CustomEditor(typeof(Variable<>), true)]
    public class VariableDrawer : Inspector
    {

        SerializedProperty m_currentValue;
        SerializedProperty m_defaultValue;

        private string displayName;




        public void OnEnable()
        {
            m_currentValue = serializedObject.FindProperty("m_currentValue");
            m_defaultValue = serializedObject.FindProperty("defaultValue");

            displayName = VariableMenuUtility.CachedAttributes[target.GetType()].GetNameOnly();
        }

        public override void OnInspectorGUI()
        {

            GUILayout.BeginHorizontal();

            EditorGUILayout.PrefixLabel("Type");

            Rect buttonPos = GUILayoutUtility.GetRect(new GUIContent(displayName), EditorStyles.popup);
            if (GUI.Button(buttonPos,displayName, EditorStyles.popup))
            {
                GenericMenu typeMenu = VariableMenuUtility.GetMenu(ChangeType, target.GetType());
                typeMenu.DropDown(buttonPos);
            }
            GUILayout.EndHorizontal();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            SerializedProperty _property = (EditorApplication.isPlaying) ? m_currentValue : m_defaultValue;

            if (_property != null)
            {
                _property.isExpanded = true;
                EditorGUILayout.PropertyField(_property, new GUIContent(_property.propertyType.ToString()), true);
                serializedObject.ApplyModifiedProperties();
            }
        }

        private void ChangeType(Type type)
        {
#pragma warning disable

            Debug.Log($"Changing to: {type}");
            string path = AssetDatabase.GetAssetPath(target);
            string parentFolder = System.IO.Path.GetDirectoryName(path);
            ScriptableObject newAsset = ScriptableObject.CreateInstance(type);
            newAsset.name = target.name + "copy";
            AssetDatabase.DeleteAsset(path);
            //AssetDatabase.SaveAssets();

            AssetDatabase.CreateAsset(newAsset, path);
            AssetDatabase.SaveAssets();
            Selection.activeObject = newAsset;

#pragma warning enable
        }

        public static Type[] GetChildClasses(System.Type parentType)
        {
            return VariableMenuUtility.CachedAttributes.Keys.ToArray();
        }

    }
}