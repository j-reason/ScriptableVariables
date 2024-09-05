using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace Variables.Diagnostics.Editor
{
    public class LogEditor
    {

        private static GUIStyle s_linkLabel;
        private static GUIStyle s_darkBackground;


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

            if (!m_target.LogOutput)
                return;

            TopBar();

            //Create Scroll that lists will be drawn in
            m_scrollPosition = GUILayout.BeginScrollView(m_scrollPosition, EditorStyles.helpBox, GUILayout.MaxHeight(EditorGUIUtility.singleLineHeight * 10));


            //Get all logs
            List<StackLogger.LogData> logs;
            if (!m_isCollapsed)
                logs = Diagnostics.StackLogger.GetUsage(m_target);
            else
                logs = Diagnostics.StackLogger.GetCollapsedUsage(m_target);


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

            IEnumerable<StackFrame> frames = data.StackTrace.GetFilteredFrames();





            GUIStyle style = isOdd ? EditorStyles.label : s_darkBackground;
            var rect = EditorGUILayout.BeginVertical(style);
            GUILayout.BeginHorizontal();

            if (frames.Count() > 1)
                if (GUILayout.Button(GUIContent.none, EditorStyles.foldout, GUILayout.Width(10)))
                    isExpanded = !isExpanded;

            GUILayout.BeginVertical();


            if (!isExpanded)
                frames = new StackFrame[] { frames.First() };

            if (frames == null || frames.All(p => p is null)) return isExpanded;

            DrawFrames(frames);



            GUILayout.EndVertical();

            if (data.CollapsedUsage > 1)
            {
                GUILayout.FlexibleSpace();
                //GUILayout.BeginHorizontal(EditorStyles.helpBox);
                GUILayout.BeginVertical();
                GUILayout.Space(EditorGUIUtility.singleLineHeight * 0.5f);
                GUILayout.Label(data.CollapsedUsage.ToString(), EditorStyles.helpBox);
                GUILayout.EndVertical();
                //GUILayout.EndHorizontal();
            }

            GUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            return isExpanded;
        }

        private void DrawFrames(IEnumerable<StackFrame> frames)
        {
            foreach (var frame in frames)
            {
                //if (typeof(Variable).IsAssignableFrom(frame.GetMethod().DeclaringType))

                string relativePath = GetPathRelative(frame.GetFileName(), Application.dataPath);

                GUILayout.Button($"{frame.GetMethod().DeclaringType.FullName}:{frame.GetMethod().Name}", EditorStyles.boldLabel, GUILayout.Height(EditorGUIUtility.singleLineHeight * 0.5f));
                if (GUILayout.Button($"    {relativePath}:{frame.GetFileLineNumber()}", s_linkLabel, GUILayout.Height(EditorGUIUtility.singleLineHeight * 0.75f)))
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
            m_target.LogOutput = EditorGUILayout.Toggle("Log Usage", m_target.LogOutput);
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
        }

    }
}
