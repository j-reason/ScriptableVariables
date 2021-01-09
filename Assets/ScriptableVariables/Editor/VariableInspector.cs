using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Collections;
using System.Reflection;
using Inspector = UnityEditor.Editor;


namespace Variables.Editor
{
    /// <summary>
    /// Custom Inspector for Variable Assets
    /// </summary>
    [CustomEditor(typeof(Variable<>), true)]
    public class VariableInspector : Inspector
    {
        //Key for dialogue box
        const string ChangeTypeDialogueDecisionKey = "ScriptableVariables.ChangeTypeDialogue";

        #region Private Variables
        //References to serialized propertyes
        private SerializedProperty m_currentValue;
        private SerializedProperty m_defaultValue;

        //Name of Variable Type
        private string m_TypeName;
        #endregion

        #region Unity Functions
        public void OnEnable()
        {
            //set up values
            m_currentValue = serializedObject.FindProperty("m_currentValue");
            m_defaultValue = serializedObject.FindProperty("m_defaultValue");
            m_TypeName = VariableMenuUtility.CachedAttributes[target.GetType()].GetNameOnly();
        }

        public override void OnInspectorGUI()
        {
            //show TypeGUI
            TypeGUI();

            //Show default value when not in play, else show currnet value
            SerializedProperty _property = (EditorApplication.isPlaying) ? m_currentValue : m_defaultValue;

            if (_property != null)
            {
                _property.isExpanded = true; //force property to be expanded (e.g. Quaternions)

                //display and apply property
                EditorGUILayout.PropertyField(_property, new GUIContent(_property.propertyType.ToString()), true);
                
            }

            EditorGUILayout.Space();
            serializedObject.ApplyModifiedProperties();
            serializedObject.UpdateIfRequiredOrScript();

        }
        #endregion

        #region GUI Functions
        /// <summary>
        /// GUI for creating a dropdown to Change the Variable Type
        /// </summary>
        private void TypeGUI()
        {

            GUILayout.BeginHorizontal(); //Draw all on one line
            EditorGUILayout.PrefixLabel("Type"); //Label

            //Create a button but in the style of a dropdown
            Rect buttonPos = GUILayoutUtility.GetRect(new GUIContent(m_TypeName), EditorStyles.popup);
            if (GUI.Button(buttonPos, m_TypeName, EditorStyles.popup))
            {
                //when clicked display dropdown
                GenericMenu typeMenu = VariableMenuUtility.GetMenu(ChangeType, target.GetType());
                typeMenu.DropDown(buttonPos);
            }

            //End horizontal + draw line underneath
            GUILayout.EndHorizontal();//E
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        }

      
        #endregion GUI Functions

        #region Helper Functions

        /// <summary>
        /// Deletes this asset and Creates a new asset with the supplied Type in it's place
        /// </summary>
        /// <param name="type">Type to change Variable to</param>
        private void ChangeType(Type type)
        {

            //Show dialogue warning that this could be harmful
            bool isAgreed = EditorUtility.DisplayDialog("Change Variable Type",
                "Changing the variable type will break references to this variable.\n\nThis can not be undone.",
                "Continue", "Cancel", DialogOptOutDecisionType.ForThisMachine, ChangeTypeDialogueDecisionKey);


            //if user wants to continue change type
            if (isAgreed)
            {
                string path = AssetDatabase.GetAssetPath(target);
                string parentFolder = System.IO.Path.GetDirectoryName(path);
                ScriptableObject newAsset = ScriptableObject.CreateInstance(type);
                newAsset.name = target.name + "copy";
                AssetDatabase.DeleteAsset(path);

                AssetDatabase.CreateAsset(newAsset, path);
                AssetDatabase.SaveAssets();
                Selection.activeObject = newAsset;
            }

        } 
        #endregion

    }
}