using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using Variables;

namespace Variables.Editor
{

    [InitializeOnLoad]
    public class VariableDrag
    {
        static VariableDrag()
        {
            EditorApplication.projectWindowItemOnGUI += ProjectWindowItemOnGUI;
        }

        private static IEnumerable<Variable> DraggedVariables => DragAndDrop.objectReferences.OfType<Variable>();

        //Used when we need to draw to the GUI;
        private static List<Action> OnGUICallback = new List<Action>();

        /// <summary>
        /// Called for each item in the project window whenever the project window updates
        /// </summary>
        /// <note>
        /// This can get called a lot so we want to break out of the function as quick as possible if it's not needed
        /// </note>
        /// <param name="guid">ID of the item in the Project window</param>
        /// <param name="selectionRect">Rect of the item in the Project Window </param>
        static void ProjectWindowItemOnGUI(string guid, Rect selectionRect)
        {
            //Check the current event type
            EventType eventType = Event.current.type;

            //if we are in Repaint mode Draw any GUI functions added to GUI Callback
            //You can only call GUI.Draw() style functions in EventType.Repaint
            if (eventType == EventType.Repaint)
            {
                OnGUICallback.ForEach(p => p());
                OnGUICallback.Clear();
            }

            //if we aren't dragging anything don't do anything
            if (eventType != EventType.DragUpdated && eventType != EventType.DragPerform)
                return;

            //if not all dragging object are of type variable don't do any logic with them
            if (!DragAndDrop.objectReferences.All(p => p is Variable))
                return;

            //Call OnDraggingVariables
            OnDraggingVariables(guid, selectionRect);

            //If the mouse isn't above a object in the project view don't do anything
            //in the project view if the mouse is above the top/bottom quater of a file, it presumes you want to place the object before/after that file so GetDropRect is a thinner version of the rect
            if (!getDropRect(selectionRect).Contains(Event.current.mousePosition))
                return;


            //if the item being hovered over isn't a Variable don't do anything
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (!AssetDatabase.GetMainAssetTypeAtPath(path).IsSubclassOf(typeof(Variable)))
                return;

            //if we get here it means all we're hoving over a Variable and only are dragging variables

            //Get a path to all the dragged objects
            string[] draggedPaths = DragAndDrop.objectReferences.Select(p => AssetDatabase.GetAssetPath(p)).ToArray();
            OnAcceptableHover(guid, selectionRect, path, draggedPaths);

            //If the Drag event is finished perform the logic
            if (eventType == EventType.DragPerform)
            {
                OnAcceptableRelease(guid, selectionRect, path, draggedPaths);
                DragAndDrop.AcceptDrag();
                Event.current.Use();
            }
        }

        private static void OnDraggingVariables(string guid, Rect selectionRect)
        {

        }

        private static void OnAcceptableHover(string guid, Rect selectionRect, string hoverPath, string[] draggedPaths)
        {

            string infoMessage = "Attach";

            if (draggedPaths.All(p => p == hoverPath))
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Move;
                infoMessage = "Remove";
            }


            DragAndDrop.visualMode = DragAndDropVisualMode.Generic;

            OnGUICallback.Add(
                 () =>
                 {
                     Rect mousePos = new Rect(Event.current.mousePosition + Vector2.right * 10, new Vector2(70, EditorGUIUtility.singleLineHeight));

                     GUI.Box(selectionRect, " ");
                     GUI.Box(selectionRect, " ");

                     EditorGUI.HelpBox(mousePos, infoMessage, MessageType.Info);
                 }
                );

        }

        private static void OnAcceptableRelease(string guid, Rect selectionRect, string hoverPath, string[] draggedPaths)
        {
            if (draggedPaths.All(p => p == hoverPath))
                foreach (Variable var in DraggedVariables)
                    EditorVariableUtility.DeattachAsset(var);
            else
                foreach (Variable variable in DraggedVariables)
                    EditorVariableUtility.AttachAssetTo(variable, hoverPath);

        }





        static Rect getDropRect(Rect selectionRect)
        {
            selectionRect.y += selectionRect.height / 4 + 1;
            selectionRect.height = selectionRect.height / 2 - 1;
            return selectionRect;
        }

    }

}
