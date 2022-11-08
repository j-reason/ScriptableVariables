using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;



namespace Variables.Editor
{


    /// <summary>
    /// Class to reset Variables when entering and exiting playmode in the Editor so they behave like monobehaviours
    /// </summary>
    public class VariableResetter
    {

        //Dictionary to store copies of variables
        //key is a reference to the asset
        //value is a copy which won't change
        private static Dictionary<Variable, Variable> m_storedCopies = new Dictionary<Variable, Variable>();


        [InitializeOnLoadMethod]//Makes unity call this when the editor starts
        private static void RegisterResets()
        {
            //listen for playmode changes
            EditorApplication.playModeStateChanged += OnEditorPlayModeChange;
        }

        /// <summary>
        /// Callback for when unity changes playmode
        /// </summary>
        /// <param name="mode">new playmode</param>
        private static void OnEditorPlayModeChange(PlayModeStateChange mode)
        {

            //Call backs have to happen after entering playmode and before exiting since m_storedCopies get reset when unity serializes everythings
            switch (mode)
            {
                case PlayModeStateChange.ExitingEditMode:
                    break;
                case PlayModeStateChange.EnteredPlayMode:
                    CreateCopies();
                    break;
                case PlayModeStateChange.ExitingPlayMode:
                    ResetCopies();
                    break;
                case PlayModeStateChange.EnteredEditMode:              
                    break;
            }
        }

        /// <summary>
        /// Finds all Variables and creates a copy of them
        /// </summary>
        private static void CreateCopies()
        {
            //Should be empty but good to call just incase
            ClearCopies();

            //Find all Variable Assets in Editor and create a copy of them
            foreach (Variable variable in GetAllInstances<Variable>())
            {
                Variable copy = ScriptableObject.Instantiate(variable);
                copy.name = variable.name; //this remove (Clone) from the name
                m_storedCopies.Add(variable, copy);
            }


        }

        /// <summary>
        /// Applies the original stored data back to Variable Assets
        /// </summary>
        private static void ResetCopies()
        {

            //loops through all Copies created
            foreach (var entry in m_storedCopies)
            {

                //Uses CopySerialized so references aren't broken
                EditorUtility.CopySerialized(entry.Value, entry.Key);


                //#TODO set it up so that users can choose not to reset some Variables
            }

            //clear list since all copies have been re-applied
            ClearCopies();
        }

        /// <summary>
        /// Clears m_storedCopies and destroys all copies of variables
        /// </summary>
        private static void ClearCopies()
        {

            //Destroy copies of variables
            //I don't know if this happens automatically when scriptable objects go out of scope so I'm doing this to be sure.
            foreach (Variable variable in m_storedCopies.Values)
                Object.Destroy(variable);

            //Clear the dictionary
            m_storedCopies.Clear();
        }


        /// <summary>
        /// Finds all Variables in the Assets Folder
        /// </summary>
        /// <typeparam name="T">Type of Variable to find</typeparam>
        /// <returns>Array of Variables</returns>
        private static T[] GetAllInstances<T>() where T : Variable
        {
            string[] guids = AssetDatabase.FindAssets("t:" + typeof(T).Name);  //FindAssets uses tags check documentation for more info
            T[] retVal = new T[guids.Length];
            for (int i = 0; i < guids.Length; i++)         //probably could get optimized 
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                retVal[i] = AssetDatabase.LoadAssetAtPath<T>(path);
            }

            return retVal;

        }
    } 
}
