using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace KreliStudio.Internal
{
    [CustomEditor(typeof(ComponentOrganizer))]
    public sealed class ComponentOrganizerEditor : Editor
    {
        ComponentOrganizer targetScript;
        Component[] components;

        void OnEnable()
        {
            targetScript = target as ComponentOrganizer;
            components = GetAllComponents();
        }


        public override void OnInspectorGUI()
        {
            foreach (var item in components)
            {
                DrawComponentItem(item);
            }
        }
        protected override void OnHeaderGUI()
        {
           // base.OnHeaderGUI();
        }
        public override GUIContent GetPreviewTitle()
        {
            return new GUIContent();
        }
        void DrawComponentItem(Component item)
        {
            bool visible = item.IsVisible();
            bool editable = item.IsEditable();
            GUIContent visibilityToggle = new GUIContent(visible ? EditorGUIUtility.IconContent("VisibilityOn").image : EditorGUIUtility.IconContent("VisibilityOff").image, "Set visibility component in inspector.");
            GUIContent editableToggle = new GUIContent(editable ? EditorGUIUtility.IconContent("editicon.sml").image : EditorGUIUtility.IconContent("InspectorLock").image, "Set editable component in inspector.");
            
            EditorGUILayout.BeginHorizontal();
            GUI.enabled = false;
            EditorGUILayout.ObjectField(item, item.GetType(), false);
            GUI.enabled = true;
            if (GUILayout.Button(visibilityToggle, EditorStyles.label, GUILayout.Width(22f)))
            {
                Undo.RecordObject(target, "Change Visibility Flags");
                item.SetVisibility(!visible);
            }
            if (GUILayout.Button(editableToggle, EditorStyles.label, GUILayout.Width(22f)))
            {
                Undo.RecordObject(target, "Change Editable Flags");
                item.SetEditable(!editable);
            }

            Behaviour behaviour = item as Behaviour;
            Collider collider = item as Collider; // Colliders 3D has own enable variable!? wtf
            if (behaviour != null)
            {
                GUIContent enableToggle = new GUIContent(string.Empty, "Set activity component in inspector.");

                Undo.RecordObject(target, "Change Enable Status");
                behaviour.enabled = EditorGUILayout.Toggle(enableToggle, behaviour.enabled, GUILayout.Width(22f));
            }
            else if(collider != null)
            {
                GUIContent enableToggle = new GUIContent(string.Empty, "Set activity component in inspector.");

                Undo.RecordObject(target, "Change Enable Status");
                collider.enabled = EditorGUILayout.Toggle(enableToggle, collider.enabled, GUILayout.Width(22f));
            }

            EditorGUILayout.EndHorizontal();

            if (GUI.changed)
                EditorUtility.SetDirty(target);
        }

        Component[] GetAllComponents()
        {
            List<Component> allComponents = new List<Component>(targetScript.GetComponents<Component>());
            allComponents.Remove(targetScript);
            return allComponents.ToArray();
        }


    }
}