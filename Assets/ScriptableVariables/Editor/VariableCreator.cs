using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace Variables.Editor
{

    /// <summary>
    /// Custom Editor window for Creating Variable<> Classes
    /// </summary>
    public class VariableCreator : EditorWindow
    {

        public string filter = "";
        public Type selectedType;

        private VariableMenuAttribute attribute = new VariableMenuAttribute();

        private bool m_showAdvanced;
        private string m_namespace = "Variables.Types";
        private string AssetPath = "";

        private GUIStyle richLabel;
        private GUIStyle postFoldout;

        // Add menu named "My Window" to the Window menu
        [MenuItem("Tools/Create Variable Type")]
        public static void Open()
        {
            // Get existing open window or if none, make a new one:
            VariableCreator window = (VariableCreator)EditorWindow.GetWindow<VariableCreator>(true, "Create new Variable", true);          
            window.Show();
        }


        private void OnEnable()
        {
            richLabel = new GUIStyle(EditorStyles.label);
            richLabel.richText = true;

            postFoldout = new GUIStyle(EditorStyles.foldoutHeader);
            postFoldout.imagePosition = ImagePosition.ImageLeft;
            postFoldout.padding = new RectOffset(-120, 0, 0, 0);
            postFoldout.border = new RectOffset(120, 0, 0, 0);

            AssetPath = EditorPrefs.GetString("VariableAssetPath", "Scripts/Variable Types");
        }

        private void OnGUI()
        {
            TypeGUI();
            SettingsGUI();
            CreationGUI();

        }


        private void TypeGUI()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("<b>Select Type</b>", richLabel);


            filter = EditorGUILayout.TextField("Filter", filter);

            GUILayout.BeginHorizontal();

            EditorGUILayout.LabelField("Type <color=red>*</color>", richLabel);

            Rect buttonPos = GUILayoutUtility.GetRect(new GUIContent(selectedType?.Name ?? "Select Type"), EditorStyles.popup);
            if (GUI.Button(buttonPos, selectedType?.Name ?? "Select Type", EditorStyles.popup))
            {
                GenericMenu typeMenu = GetTypeMenu();
                typeMenu.DropDown(buttonPos);
            }
            GUILayout.EndHorizontal();
        }

        private void SettingsGUI()
        {


            //create Line + title
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);

            //disable if type hasn't been selected
            GUI.enabled = selectedType != null;

            //UI for menu attribute
            attribute.menuName = EditorGUILayout.TextField("Menu Name", attribute.menuName);
            attribute.order = EditorGUILayout.IntField("Menu Order", attribute.order);
            GUI.enabled = true;


            //advanced settings
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Advanced Settings", EditorStyles.boldLabel);



            EditorGUILayout.LabelField("Path", richLabel);
            GUILayout.BeginHorizontal();
            AssetPath = EditorGUILayout.TextField(AssetPath);
            if (GUILayout.Button("Set", GUILayout.Width(40)))
            {
                string absolutePath = Path.Combine(Application.dataPath, AssetPath);
                if (!Directory.Exists(absolutePath))
                {
                    absolutePath = Application.dataPath;
                }
                else
                {
                    absolutePath = EditorUtility.OpenFolderPanel("Select Folder", absolutePath, "Folder");
                }
                AssetPath = GetLocalPath(absolutePath);

                EditorPrefs.SetString("VariableAssetPath", AssetPath);
            }
            GUILayout.EndHorizontal();

            EditorGUILayout.LabelField("Namespace", richLabel);
            m_namespace = EditorGUILayout.TextField(m_namespace);



            GUI.enabled = true;
        }

        private void CreationGUI()
        {

            //Force to bottom of UI
            GUILayout.FlexibleSpace();

            //Make buttons next to each other
            GUILayout.BeginHorizontal();

            //Create cancel button to close on click
            if (GUILayout.Button("Cancel"))
                Close();

            //Button to create type on click then close
            GUI.enabled = selectedType != null; //disable if type hasn't been selected
            if (GUILayout.Button("Create"))
            {
                CreateVariableType(selectedType, Path.Combine(Application.dataPath, AssetPath), attribute, m_namespace);
                Close();
            }

            //end horizontal and re-enable UI
            GUILayout.EndHorizontal();
            GUI.enabled = true;
        }


        public void SetType(object type)
        {
            selectedType = (Type)type;
            attribute.menuName = selectedType.FullName.Replace('.', '/').Replace("UnityEngine", "Unity");
        }

        private string GetLocalPath(String path)
        {
            path = path.Replace(Application.dataPath, "");
            path = path.TrimStart('/');
            return path;
        }


        public GenericMenu GetTypeMenu()
        {
            GenericMenu menu = new GenericMenu();

            foreach (Type type in AppDomain.CurrentDomain.GetAssemblies().AsParallel().SelectMany(p => p.GetTypes()))
            {
                if (isUsable(type) && type.FullName.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    string path = type.FullName.Replace('.', '/');
                    menu.AddItem(new GUIContent(path), false, SetType, type);
                }
            }
            if (menu.GetItemCount() == 0)
            {
                menu.AddDisabledItem(new GUIContent($"'{filter}' not found"));
            }

            return menu;
        }

        public bool isUsable(Type type)
        {
            return ((type.IsClass && type.IsSerializable || typeof(UnityEngine.Object).IsAssignableFrom(type)) || type.IsValueType) && type.IsVisible && type.IsPublic && !type.IsAbstract && !type.IsGenericType;
        }

        public void CreateVariableType(Type type, string folderPath, VariableMenuAttribute attribute, string nameSpace = null)
        {
            string variableName = type.Name + "Variable";
            string fullPath = Path.Combine(folderPath, variableName + ".cs");


            DirectoryInfo di = Directory.CreateDirectory(folderPath);


            // Create a file to write to.
            using (StreamWriter sw = File.CreateText(fullPath))
            {
                sw.WriteLine("using Variables;");

                if (type?.Namespace != null && type.Namespace.Length > 0)
                    sw.WriteLine($"using {type.Namespace};");

                sw.WriteLine("");

                if (!string.IsNullOrEmpty(nameSpace))
                {
                    sw.WriteLine($"namespace {nameSpace}");
                    sw.WriteLine("{");
                }

                sw.WriteLine($"[VariableMenu(menuName = \"{attribute.menuName}\", order = {attribute.order})]");
                sw.WriteLine($"public class {variableName} : Variable<{type.Name}> {{ }}");
                if (!string.IsNullOrEmpty(nameSpace))
                    sw.WriteLine("}");
            }

            AssetDatabase.Refresh();
            Debug.Log($"Created : {fullPath}");




        }

    }

}