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
        //References to serialized properties
        private SerializedProperty m_value;

        //Name of Variable Type
        private string m_TypeName;
        #endregion

        #region Unity Functions
        public void OnEnable()
        {
            //set up values
            m_value = serializedObject.FindProperty("m_value");
            m_TypeName = VariableMenuUtility.CachedAttributes[target.GetType()].GetNameOnly();
        }

        public override void OnInspectorGUI()
        {
            //show TypeGUI
            TypeGUI();

            if (m_value != null)
            {
                m_value.isExpanded = true; //force property to be expanded (e.g. Quaternions)

                EditorGUI.BeginChangeCheck();
           
                //display and apply property
                EditorGUILayout.PropertyField(m_value, new GUIContent(m_value.propertyType.ToString()), true);

                serializedObject.ApplyModifiedProperties();
                serializedObject.UpdateIfRequiredOrScript();

                if (EditorGUI.EndChangeCheck())
                {
                    EditorVariableUtility.InvokeVariableEvent(target);
                } 
            }

            EditorGUILayout.Space();
            

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