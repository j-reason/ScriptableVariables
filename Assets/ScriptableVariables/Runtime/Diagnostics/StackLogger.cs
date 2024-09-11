using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Variables.Diagnostics
{
    /// <summary>
    /// Class which deals with storing logs about Variable usage
    /// </summary>
    public static class StackLogger
    {

        #region Data Holders
        public enum FunctionType { Get, Set }

        public class LogData
        {
            public DateTime DateTime;
            public StackTrace StackTrace;
            public int Usage = 1; //how many times has this same log appeared
            public FunctionType FunctionType;

            public LogData(FunctionType FunctionType, StackTrace StrackTrace)
            {
                this.FunctionType = FunctionType;
                this.StackTrace = StrackTrace;
                this.DateTime = DateTime.Now;
            }

            public void AddUsage()
            {
                this.DateTime = DateTime.Now;
                Usage++;
            }

        }
        #endregion Data Holders

        #region Static Members
        private static Dictionary<Variable, List<LogData>> s_AllLogs = new Dictionary<Variable, List<LogData>>(); //Collection of all Logs, indexed by the variable that the log is about
        private static Dictionary<Variable, Dictionary<string, LogData>> s_uniqueLogs = new Dictionary<Variable, Dictionary<string, LogData>>(); //Collection of unique logs, indexed by the variable the log is about

        private static HashSet<Variable> s_loggedVariables = new HashSet<Variable>();
        #endregion Static Members

        #region Constants
        static readonly Type[] TYPES_TO_IGNORE = { typeof(StackLogger), typeof(Variable), typeof(Reference) }; //logs we wan't to ignore since they will be part of every variable call
        const string EDITOR_PREFS_KEY = "k_loggedVariable"; //used as ID when storing data in editorPrefs
        #endregion Constants


        /// <summary>
        /// Logs a new usage of a variable
        /// </summary>
        /// <param name="variable">which variable has been accessed</param>
        /// <param name="mode">has it been Set or Get</param>
        public static void LogUsage(Variable variable, FunctionType mode)
        {
            //if there is no entry for this variable create a new entry
            if (!s_AllLogs.TryGetValue(variable, out List<LogData> logs))
            {
                logs = new List<LogData>();
                s_AllLogs.Add(variable, logs);
            }

            //if there is no entry for this variable in Unique Logs create a new entry
            //This could be merged with the check above, but this is less error prone
            if (!s_uniqueLogs.TryGetValue(variable, out Dictionary<string, LogData> uniqueLog))
            {
                uniqueLog = new Dictionary<string, LogData>();
                s_uniqueLogs.Add(variable, uniqueLog);
            }

            //Get the stacktrace right now so we have data of how the variable was used
            var st = new StackTrace(3, true); //we can skip three frames of the stack trace, since those three will just be about logging which the user doesn't care about

            //check if this log is unique and added it to the collection if it is
            if (!uniqueLog.TryGetValue(st.ToString(), out var collapsedData))
            {
                collapsedData = new LogData(mode, st);
                uniqueLog.Add(st.ToString(), collapsedData);
            }
            else
            {
                collapsedData.AddUsage(); // if it isn't unique just increment how many times it's been used
            }
            
            //add a record of the log to the collection of all logs
            logs.Add(new LogData(mode, st));
        }

        public static List<LogData> GetUsage(Variable variable)
        {
            if (s_AllLogs.TryGetValue(variable, out List<LogData> logs))
                return logs;

            return new List<LogData>();
        }

        public static List<LogData> GetUniqueUsage(Variable variable)
        {
            if (s_uniqueLogs.TryGetValue(variable, out Dictionary<string, LogData> logs))
                return logs.Values.ToList();

            return new List<LogData>();
        }

        /// <summary>
        /// Removes all logs of a variable
        /// </summary>
        /// <param name="variable">variable to clear logs of</param>
        public static void ClearUsage(Variable variable)
        {
            s_AllLogs.Remove(variable);
            s_uniqueLogs.Remove(variable);
        }


        public static string PrintStackTrace(StackTrace ST)
        {
            string retVal = "";

            foreach (StackFrame frame in ST.GetFrames())
            {
                Debug.Log($"{frame.GetMethod().DeclaringType} == {typeof(StackLogger)} = {frame.GetMethod().DeclaringType == typeof(StackLogger)}");
                if (TYPES_TO_IGNORE.Contains(frame.GetMethod().DeclaringType))
                    continue;


                retVal += StackFrameToString(frame);

                retVal += "\r\n";
            }
            return retVal;
        }

        public static string StackFrameToString(StackFrame frame)
        {
            var methodData = frame.GetMethod();
            return $"{frame.GetMethod().DeclaringType.FullName}:{methodData.Name} (at {frame.GetFileName()}:{frame.GetFileLineNumber()})";
        }

        /// <summary>
        /// Extension method to get frames from stacktrace filtering out any methods which are just part of the Logging/Variable code 
        /// </summary>
        /// <param name="stackTrace">stack trace to get frames from</param>
        /// <returns>filtered array of stackframes</returns>
        public static StackFrame[] GetFilteredFrames(this StackTrace stackTrace)
        {
            return stackTrace.GetFrames().Where(p => !TYPES_TO_IGNORE.Any(q => q.IsAssignableFrom(p.GetMethod().DeclaringType))).ToArray();
        }


        /// <summary>
        /// Sets if the variable should be logged or not, only persists until editor is closed
        /// </summary>
        /// <param name="variable">variable to log</param>
        /// <param name="shouldLog">if variable should be logged</param>
        public static void SetVariableLogging(Variable variable, bool shouldLog)
        {

            //Logged variables are stored in a hashmap for lookup
            if (shouldLog)
                s_loggedVariables.Add(variable);
            else
                s_loggedVariables.Remove(variable);

        }

        /// <summary>
        /// Returns if the variable is currently being logged
        /// </summary>
        public static bool isVariableLogged(Variable variable)
        {
            //Logged variables are stored in a hashmap for lookup
            return s_loggedVariables.Contains(variable);
        }





        #region Assembly Reload Functions

#if UNITY_EDITOR

        /// <summary>
        /// Called by Unity Editor when it first loads (and other times)
        /// </summary>
        [InitializeOnLoadMethod]
        private static void RegisterCallBacks()
        {
            AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
            AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;
            EditorApplication.quitting += OnEditorQuit;
        }

        /// <summary>
        /// Called just before all the static data gets wiped from memory
        /// </summary>
        private static void OnBeforeAssemblyReload()
        {
            StoreVariableLogging();
        }

        /// <summary>
        /// Loads the needed data back into memory
        /// </summary>
        private static void OnAfterAssemblyReload()
        {
            LoadVariableLogging();
            ClearEditorPrefs();
        }

        /// <summary>
        /// Cleans up any data in editorprefs when unity quits
        /// </summary>
        /// <remarks>
        /// This shouldn't be needed since the data is cleaned up after it's used. but it feels like a good idea to have htis catch
        /// </remarks>
        private static void OnEditorQuit()
        {
            ClearEditorPrefs();
        }


        /// <summary>
        /// Stores the path to all Logged Variables in editor Prefs.
        /// </summary>
        /// <remarks>
        /// This is required since static members get reset when unity reloads assemblies
        /// (which happens on play)
        /// </remarks>
        private static void StoreVariableLogging()
        {

            EditorPrefs.SetInt($"{EDITOR_PREFS_KEY}_count", s_loggedVariables.Count); //can't store arrays so we need to first store how many variables are being stored

            int count = 0; //editor prefs can't store arrays so this count gets added to the Key as an identifier
            foreach (Variable variable in s_loggedVariables)
            {
                //get path of variable and store in editorprefs
                string path = AssetDatabase.GetAssetPath(variable);
                EditorPrefs.SetString($"{EDITOR_PREFS_KEY}_{count}", path);

                //increment count to use in editorprefs key
                count++;
            }

        }

        /// <summary>
        /// Loads all variables stored in editorprefs to be logged
        /// </summary>
        /// <remarks>
        /// This is required since static members get reset when unity reloads assemblies
        /// (which happens on play)
        /// </remarks>
        private static void LoadVariableLogging()
        {

            int loggedVariableCount = EditorPrefs.GetInt($"{EDITOR_PREFS_KEY}_count", 0); //Get count of how many variables are stored in editorprefs to be logged, (default 0)

            for (int i = 0; i < loggedVariableCount; i++)
            {
                //get variable path from editor prefs then load
                string path = EditorPrefs.GetString($"{EDITOR_PREFS_KEY}_{i}", "");
                Variable variable = AssetDatabase.LoadAssetAtPath<Variable>(path);

                //add to hashmap to be logged
                if (variable != null)
                    s_loggedVariables.Add(variable);
            }
        }

        /// <summary>
        /// Clears out editor prefs of any data about logged variables
        /// This gets called as much as possible to ensure that the editor prefs only stores the needed data temporarily
        /// </summary>
        /// <remarks>
        /// This could definitly be merged with LoadVariableLogging()
        /// but as a seperate funtion it can be called independantly
        /// </remarks>
        private static void ClearEditorPrefs()
        {

            int loggedVariableCount = EditorPrefs.GetInt($"{EDITOR_PREFS_KEY}_count", 0); //Get count of how many variables are stored in editorprefs to be logged, (default 0)
            EditorPrefs.DeleteKey($"{EDITOR_PREFS_KEY}_count"); //also delete this key

            //loop through and delete all keys
            for (int i = 0; i < loggedVariableCount; i++)
                EditorPrefs.DeleteKey($"{EDITOR_PREFS_KEY}_{i}");
        }


#endif

        #endregion Assembly Reload Functions
    }
}

