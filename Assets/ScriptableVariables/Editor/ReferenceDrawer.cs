﻿using System.Collections;
using UnityEditor;
using UnityEngine;

namespace Variables.Editor
{
    [CustomPropertyDrawer(typeof(Reference<>), true)]
    public class ReferenceDrawer : PropertyDrawer
    {

        // Get properties
        SerializedProperty useConstant;
        SerializedProperty constantValue;
        SerializedProperty variable;




        /// <summary>
        /// Options to display in the popup to select constant or variable.
        /// </summary>
        private readonly string[] popupOptions =
            { "Use Constant", "Use Variable" };

        /// <summary> Cached style to use to draw the popup button. </summary>
        private GUIStyle popupStyle;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {

            if (popupStyle == null)
            {
                popupStyle = new GUIStyle(GUI.skin.GetStyle("PaneOptions"));
                popupStyle.imagePosition = ImagePosition.ImageOnly;
            }

            FindProperties(property);

            label = EditorGUI.BeginProperty(position, label, property);
            position = EditorGUI.PrefixLabel(position, label);

            EditorGUI.BeginChangeCheck();

            // Calculate rect for configuration button
            Rect buttonRect = new Rect(position);
            buttonRect.yMin += popupStyle.margin.top;
            buttonRect.width = popupStyle.fixedWidth + popupStyle.margin.right;
            position.xMin = buttonRect.xMax;
            if (useConstant.boolValue && constantValue.hasChildren)
                position.xMin += 10;

            // Store old indent level and set it to 0, the PrefixLabel takes care of it
            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            int result = EditorGUI.Popup(buttonRect, useConstant.boolValue ? 0 : 1, popupOptions, popupStyle);

            useConstant.boolValue = result == 0;

            EditorGUI.PropertyField(position,
                useConstant.boolValue ? constantValue : variable,
                GUIContent.none, true);

            if (EditorGUI.EndChangeCheck())
                property.serializedObject.ApplyModifiedProperties();

            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }


        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            FindProperties(property);
            int height = (useConstant.boolValue) ? constantValue.CountInProperty() : 1;

            return 20 * height;


        }

        private void FindProperties(SerializedProperty property)
        {
                useConstant = property.FindPropertyRelative("UseConstant");

                constantValue = property.FindPropertyRelative("ConstantValue");

                variable = property.FindPropertyRelative("Variable");

        }

    }
}