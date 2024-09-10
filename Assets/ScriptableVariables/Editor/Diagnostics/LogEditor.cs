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
        private static GUIStyle s_miniTextField;


        private Variable m_target;
        private Vector2 m_scrollPosition;
        private StackLogger.LogData m_expandedLog;
        private bool m_isCollapsed = false;
        private string m_filter;

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
            var rect = GUILayoutUtility.GetLastRect();
            rect.width = Mathf.Min(rect.width - 5, 200);
            rect.height *= 0.8f;
            rect.y += EditorGUIUtility.singleLineHeight * (1-0.75f)/2.0f;
            rect.x += 5;

            m_filter = EditorGUI.TextField(rect, m_filter, s_miniTextField);


            GUILayout.EndHorizontal();

            GUILayout.Space(-EditorGUIUtility.singleLineHeight * 0.18f);

        }


        private bool DrawStackFrame(StackLogger.LogData data, bool isOdd, bool isExpanded)
        {

          
            GUIStyle style = isOdd ? EditorStyles.label : s_darkBackground;
            var rect = EditorGUILayout.BeginVertical(style);
            GUILayout.BeginHorizontal();

            //DrawFrames(frames);
            isExpanded = DrawLog(data, isExpanded, m_filter);

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

        private bool DrawLog(StackLogger.LogData data, bool isExpanded, string filter)
        {
            


            IEnumerable<StackFrame> frames = data.StackTrace.GetFilteredFrames();
            StackFrame firstFrame = frames.FirstOrDefault();
            IEnumerable<StackFrame> otherFrames = frames.Skip(1);

            //return if filter not met
            if (!string.IsNullOrWhiteSpace(filter) && !frames.Any(p => p.GetMethod().Name.Contains(filter)))
                return false;

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

            GUILayout.BeginVertical();

            DrawFrame(firstFrame);


            //GUILayout.EndHorizontal();


            if (isExpanded)
            {
                
                foreach (var frame in otherFrames)
                {
                    DrawFrame(frame);
                }
                
            }
            GUILayout.EndVertical();



            return isExpanded;
        }


        private void DrawFrame(StackFrame frame)
        {
            string relativePath = GetPathRelative(frame.GetFileName(), Application.dataPath);

            string methodName = $"{frame.GetMethod().DeclaringType.FullName}:{frame.GetMethod().Name}()";
            string line = $"{relativePath}:{frame.GetFileLineNumber()}";


            GUILayout.Space(s_richLabel.fontSize * 0.3f);
            if (GUILayout.Button($"<b>{methodName}</b> <size={s_richLabel.fontSize * 0.9f}><i><color=blue>{line}</color></i></size>", s_richLabel, GUILayout.Height(EditorGUIUtility.singleLineHeight * 0.75f)))
            {
                var file = AssetDatabase.LoadAssetAtPath<TextAsset>(relativePath);
                AssetDatabase.OpenAsset(file, frame.GetFileLineNumber(), frame.GetFileColumnNumber());

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
            s_linkLabel.normal.textColor *= EditorGUIUtility.isProSkin ? 1.1f : 1.0f; 

            s_darkBackground = new GUIStyle(EditorStyles.label);

            Color oddColor = EditorGUIUtility.isProSkin ? Color.white * 0.2f : Color.white * 0.75f;
            oddColor.a = 1;

            var consoleBackground = new Texture2D(1, 1, TextureFormat.RGBAFloat, false);
            consoleBackground.SetPixel(0, 0, oddColor);
            consoleBackground.Apply();
            s_darkBackground.normal.background = consoleBackground;
            



            s_richLabel = new GUIStyle(EditorStyles.label);
            s_richLabel.richText = true;

            s_miniTextField = new GUIStyle(EditorStyles.toolbarSearchField);
            s_miniTextField.fixedHeight = EditorGUIUtility.singleLineHeight * 0.75f;


        }

    }
}
