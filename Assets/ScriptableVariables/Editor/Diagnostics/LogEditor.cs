using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;
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
        private StackTrace m_expandedLog;

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

            //Create Scroll that lists will be drawn in
            m_scrollPosition = GUILayout.BeginScrollView(m_scrollPosition, EditorStyles.helpBox, GUILayout.MaxHeight(EditorGUIUtility.singleLineHeight * 10));

            //Get all logs
            var Logs = Diagnostics.StackLogger.GetUsage(m_target);
            if (Logs.Count == 0)
                GUILayout.FlexibleSpace();

            bool isOdd = false;
            foreach (var log in Logs)
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


        private bool DrawStackFrame(StackTrace trace, bool isOdd, bool isExpanded)
        {

            var frame = trace.GetFrame(1);
            var frames = trace.GetFrames();

            if (frame == null) return isExpanded;

            string reltaivePath = GetPathRelative(frame.GetFileName(), Application.dataPath);

            GUIStyle style = isOdd ? EditorStyles.label : s_darkBackground;
            var rect = EditorGUILayout.BeginVertical(style);
            GUILayout.BeginHorizontal();

            if (GUILayout.Button(GUIContent.none, EditorStyles.foldout, GUILayout.Width(10)))
                isExpanded = !isExpanded;

            GUILayout.BeginVertical();

            if (isExpanded)
                DrawFrames(frames);
            else
                DrawFrames(frame);


            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            return isExpanded;
        }

        private void DrawFrames(params StackFrame[] frames)
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
            s_darkBackground.normal.background= consoleBackground;
        }

    }
}
