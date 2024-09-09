using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental;
using UnityEditor.UIElements;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace Variables.Diagnostics.Editor
{
    public class LogEditor
    {

        private static GUIStyle s_linkLabel;
        private static GUIStyle s_darkBackground;
        private static GUIStyle s_richLabel;
        private static GUIStyle s_smallFoldout;


        private Variable m_target;
        private Vector2 m_scrollPosition;
        private StackLogger.LogData m_expandedLog;
        private bool m_isCollapsed = false;

        public LogEditor(Variable target)
        {
            m_target = target;


        }

        public void OnGUI()
        {


            if (s_linkLabel == null)
                SetUpStyles();

            //Draw line + title
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Diagnostics", EditorStyles.boldLabel);

            DrawBoolToggle();

            if (!StackLogger.isVariableLogged(m_target))
                return;

            TopBar();

            //Create Scroll that lists will be drawn in
            m_scrollPosition = GUILayout.BeginScrollView(m_scrollPosition, EditorStyles.helpBox, GUILayout.MaxHeight(EditorGUIUtility.singleLineHeight * 10));


            //Get all logs
            List<StackLogger.LogData> logs;
            if (!m_isCollapsed)
                logs = Diagnostics.StackLogger.GetUsage(m_target);
            else
                logs = Diagnostics.StackLogger.GetUniqueUsage(m_target);


            if (logs.Count == 0)
                GUILayout.FlexibleSpace();

            //Loop through all Logs
            bool isOdd = false; //used to alternate line colours




            foreach (var log in logs)
            {
                var isExpanded = log == m_expandedLog;
                isOdd = !isOdd;
                isExpanded = DrawStackFrame(log, isOdd, isExpanded);

                if (isExpanded)
                    m_expandedLog = log;
                else if (m_expandedLog == log)
                    m_expandedLog = null;
            }

            GUILayout.EndScrollView();
        }


        private void TopBar()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(2);
            if (GUILayout.Button("Clear", EditorStyles.miniButtonMid, GUILayout.Width(50.0f)))
            {
                Diagnostics.StackLogger.ClearUsage(m_target);
            }
            m_isCollapsed = GUILayout.Toggle(m_isCollapsed, "Collapse", EditorStyles.miniButtonMid, GUILayout.Width(75.0f));

            GUILayout.Box(GUIContent.none, EditorStyles.miniButtonMid);

            GUILayout.EndHorizontal();

            GUILayout.Space(-EditorGUIUtility.singleLineHeight * 0.18f);

        }


        private bool DrawStackFrame(StackLogger.LogData data, bool isOdd, bool isExpanded)
        {

          
            GUIStyle style = isOdd ? EditorStyles.label : s_darkBackground;
            var rect = EditorGUILayout.BeginVertical(style);
            GUILayout.BeginHorizontal();

            //DrawFrames(frames);
            isExpanded = DrawLog(data, isExpanded);

            if (data.Usage > 1)
            {
                GUILayout.FlexibleSpace();
                //GUILayout.BeginHorizontal(EditorStyles.helpBox);
                GUILayout.BeginVertical();
                GUILayout.Space(EditorGUIUtility.singleLineHeight * 0.5f);
                GUILayout.Label(data.Usage.ToString(), EditorStyles.helpBox);
                GUILayout.EndVertical();
                //GUILayout.EndHorizontal();
            }

            GUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            return isExpanded;
        }

        private bool DrawLog(StackLogger.LogData data, bool isExpanded)
        {

            IEnumerable<StackFrame> frames = data.StackTrace.GetFilteredFrames();
            StackFrame firstFrame = frames.FirstOrDefault();
            IEnumerable<StackFrame> otherFrames = frames.Skip(1);

            //GUILayout.BeginHorizontal();
            string mode = data.FunctionType == StackLogger.FunctionType.Get ? "GET" : "SET";
            
            
            var timeLabel = new GUIContent($"[{data.DateTime.ToString("HH:mm:ss")}]");
            var label = new GUIContent($"<b><size={s_richLabel.fontSize * 1.2}>{mode}</size></b>:");

            GUI.skin.label.CalcMinMaxWidth(label, out var minWidth, out var maxWidth);
            EditorGUILayout.LabelField(timeLabel,s_richLabel,GUILayout.Width(60));
            EditorGUILayout.LabelField(label, s_richLabel, GUILayout.Width(40));


            GUILayout.Space(5);
            if (otherFrames.Any()) 
            {
                Rect rect = EditorGUILayout.GetControlRect(GUILayout.Width(0));
                isExpanded = EditorGUI.Foldout(rect, isExpanded, GUIContent.none);
            }
            else
            {
                GUILayout.Space(0);
            }

            DrawFrame(firstFrame);


            //GUILayout.EndHorizontal();


            if (isExpanded)
            {
                GUILayout.BeginVertical();
                foreach (var frame in otherFrames)
                {
                    DrawFrame(frame);
                }
                GUILayout.EndVertical();
            }



            return isExpanded;
        }


        private void DrawFrame(StackFrame frame)
        {
            string relativePath = GetPathRelative(frame.GetFileName(), Application.dataPath);

            string methodName = $"{frame.GetMethod().DeclaringType.FullName}:{frame.GetMethod().Name}()";
            string line = $"{relativePath}:{frame.GetFileLineNumber()}";
            //GUILayout.BeginHorizontal();
            GUILayout.Space(s_richLabel.fontSize * 0.3f);
            if (GUILayout.Button($"<b>{methodName}</b> at <size={s_richLabel.fontSize * 0.9f}><i><color=blue>{line}</color></i></size>", s_richLabel, GUILayout.Height(EditorGUIUtility.singleLineHeight * 0.75f)))
            {
                var file = AssetDatabase.LoadAssetAtPath<TextAsset>(relativePath);
                AssetDatabase.OpenAsset(file, frame.GetFileLineNumber(), frame.GetFileColumnNumber());

            }
            //GUILayout.EndHorizontal();
        }



        private void DrawFrames(IEnumerable<StackFrame> frames)
        {
            foreach (var frame in frames)
            {
                //if (typeof(Variable).IsAssignableFrom(frame.GetMethod().DeclaringType))

                string relativePath = GetPathRelative(frame.GetFileName(), Application.dataPath);

                string methodName = $"{frame.GetMethod().DeclaringType.FullName}:{frame.GetMethod().Name}";
                string line = $"{relativePath}:{frame.GetFileLineNumber()}";

                if (GUILayout.Button($"{methodName} at <i><color=blue>{line}</color></i>", s_richLabel, GUILayout.Height(EditorGUIUtility.singleLineHeight * 0.75f)))
                {
                    var file = AssetDatabase.LoadAssetAtPath<TextAsset>(relativePath);
                    AssetDatabase.OpenAsset(file, frame.GetFileLineNumber(), frame.GetFileColumnNumber());

                }

                GUILayout.Space(EditorGUIUtility.singleLineHeight * 0.5f);

            }
        }


        private string GetPathRelative(string path, string relativeTo)
        {
            if (string.IsNullOrEmpty(relativeTo) || string.IsNullOrEmpty(path))
                return path;


            return new Uri(relativeTo).MakeRelativeUri(new Uri(path)).OriginalString;
        }

        private void DrawBoolToggle()
        {

            bool logOutput = StackLogger.isVariableLogged(m_target);

            logOutput = EditorGUILayout.Toggle("Log Usage", logOutput);

            StackLogger.SetVariableLogging(m_target, logOutput);


            //m_target.LogOutput = EditorGUILayout.Toggle("Log Usage", m_target.LogOutput);
        }

        private static void SetUpStyles()
        {
            s_linkLabel = new GUIStyle(EditorStyles.linkLabel);
            s_linkLabel.fontSize = (int)(EditorStyles.linkLabel.fontSize * 0.95f);

            s_darkBackground = new GUIStyle(EditorStyles.label);
            var consoleBackground = new Texture2D(1, 1, TextureFormat.RGBAFloat, false);
            consoleBackground.SetPixel(0, 0, new Color(0.75f, 0.75f, 0.75f, 1f));
            consoleBackground.Apply();
            s_darkBackground.normal.background = consoleBackground;

            s_richLabel = new GUIStyle(EditorStyles.label);
            s_richLabel.richText = true;

            s_smallFoldout = new GUIStyle(EditorStyles.foldout);
            s_smallFoldout.fixedWidth = -5;
            s_smallFoldout.stretchWidth = false;
        }

    }
}
