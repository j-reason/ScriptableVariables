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
    /// <remarks>
    /// Variable Resetter is a ScriptableObject so that it can survive assembly reloading.
    /// If it was purely static it would loose it's data whenever a script it modified.
    /// </remarks>
    public class VariableResetter : ScriptableObject, ISerializationCallbackReceiver
    {

        //used to store a session state to determine if this scriptable object has already been created.
        //only one is nessesary for the lifetime of the Editor session
        const string SESSION_STATE_KEY = "k_variableResset.hasInitialised";


        //Dictionary to store copies of variables
        //key is a reference to the asset
        //value is a copy which won't change
        private Dictionary<Variable, Variable> m_storedCopies = new Dictionary<Variable, Variable>(); //ISerializationCallbackReceiver is used to serialize and rebuild on serialization






        [InitializeOnLoadMethod]//Makes unity call this when the editor starts
        private static void RegisterResets()
        {

            //Check to see if a variable resetter has already been created, there should only ever be one
            if (SessionState.GetBool(SESSION_STATE_KEY, false)) //SessionStates states have a lifetime of while the unity editor is open
            {
                //Debug.Log($"{nameof(VariableResetter)} already exists");
                return;
            }

            //set seesion state so another intance isn't created
            SessionState.SetBool(SESSION_STATE_KEY, true);

            //create an instance of the class 
            var instance = ScriptableObject.CreateInstance<VariableResetter>();
            instance.hideFlags = HideFlags.HideAndDontSave; //honestly I can't tell if this is nessessary, this is mixed opinions online.
            EditorApplication.playModeStateChanged += instance.OnEditorPlayModeChange; //the scriptableobject needs to know when playmode changed to revert any changes to the variables

        }



        /// <summary>
        /// Callback for when unity changes playmode
        /// </summary>
        /// <param name="mode">new playmode</param>
        private void OnEditorPlayModeChange(PlayModeStateChange mode)
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
        private void CreateCopies()
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
        private void ResetCopies()
        {

            //loops through all Copies created
            foreach (var entry in m_storedCopies)
            {


                switch (entry.Key.ResetOption)
                {
                    case Variable.ResetBehaviour.NeverReset: break;
                    case Variable.ResetBehaviour.SkipResetThisTime:
                        entry.Key.ResetOption = Variable.ResetBehaviour.AlwaysReset;
                        break;

                    case Variable.ResetBehaviour.ResetThisTime:
                        EditorUtility.CopySerialized(entry.Value, entry.Key); //Uses CopySerialized so references aren't broken
                        entry.Key.ResetOption = Variable.ResetBehaviour.NeverReset;
                        break;
                    case Variable.ResetBehaviour.AlwaysReset:
                        EditorUtility.CopySerialized(entry.Value, entry.Key); //Uses CopySerialized so references aren't broken
                        break;
                }


            }

            //clear list since all copies have been re-applied
            ClearCopies();
        }

        /// <summary>
        /// Clears m_storedCopies and destroys all copies of variables
        /// </summary>
        private void ClearCopies()
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


        #region ISerialization Implementation

        //Arrays used to store 'm_storedCopies' during serialization
        private Variable[] serializedKeys, serializedValues;

        /// <summary>
        /// Called by Unity before running Serialization
        /// </summary>
        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {

            //copy Dictionary into arrays
            serializedKeys = m_storedCopies.Keys.ToArray();
            serializedValues = m_storedCopies.Values.ToArray();
        }

        /// <summary>
        /// Called by Unity after deserializing
        /// </summary>
        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {

            //Remake dictionary from arrays
            m_storedCopies = new Dictionary<Variable, Variable>();

            //make sure the arrays are correct, if they aren't something has happened on Unity's side of things
            if (serializedKeys == null || serializedValues == null || serializedKeys.Length != serializedValues.Length)
                Debug.LogWarning("Variables not serialized correctly. Unable to Restore"); //if this gets called something bad has happened

            else
                //iterate through arrays and rebuild dictionary
                for (int i = 0; i < serializedKeys.Length; i++)
                    m_storedCopies.Add(serializedKeys[i], serializedValues[i]);

            //need to re-register to event after serialization
            EditorApplication.playModeStateChanged += OnEditorPlayModeChange;
        } 
        #endregion
    }
}
