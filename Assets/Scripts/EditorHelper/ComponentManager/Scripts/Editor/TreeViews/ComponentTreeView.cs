using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace KreliStudio.Internal
{
    public class ComponentTreeView : TreeView
    {
        
        public ComponentTreeView(TreeViewState treeViewState) : base(treeViewState)
        {
            showAlternatingRowBackgrounds = true;
            rowHeight = 20f;

            Reload();
        }
        /// <summary>
        /// Convert game object to tree view item object
        /// </summary>
        /// <param name="gameObject">Game object you want to convert</param>
        /// <returns>New tree view item which contane information about specify game object</returns>
        public static TreeViewItem CreateTreeViewItemForGameObject(GameObject gameObject)
        {
            string name = gameObject.name;
            name += ComponentManager.Instance.showId ? (", (id: " + gameObject.GetInstanceID().ToString() +")") : "";
            return new TreeViewItem(gameObject.GetInstanceID(), -1, name);
        }
        /// <summary>
        /// Convert component to tree view item object
        /// </summary>
        /// <param name="component">Component you want to convert</param>
        /// <returns>New tree view item which contane information about specify component</returns>
        public static TreeViewItem CreateTreeViewItemForComponent(Component component)
        {
            string name = "Missing Component";
            int instanceId = 0;
            if (component != null)
            {
                instanceId = component.GetInstanceID();
                name = ComponentManager.Instance.showFullName ? component.GetType().FullName : component.GetType().Name;
                name += ComponentManager.Instance.showId ? (", (id: " + instanceId.ToString() + ")") : "";
            }
            return new TreeViewItem(instanceId, -1, name);
        }
        /// <summary>
        /// Convert id from tree view item to UnityEngine.Object
        /// </summary>
        /// <param name="instanceID">Instance id from tree view object</param>
        /// <returns>Converted UnityEngine.Object from instance id</returns>
        public static UnityObject GetRowObject(int instanceID)
        {
            return EditorUtility.InstanceIDToObject(instanceID);
        }
        /// <summary>
        /// Check if type of UnityEngine.Object is a GameObject
        /// </summary>
        public static bool IsUnityObjectAsGameObject(UnityObject unityObject)
        {
            return (unityObject as GameObject) != null;
        }
        /// <summary>
        /// Check if type of UnityEngine.Object is a Component
        /// </summary>
        public static bool IsUnityObjectAsComponent(UnityObject unityObject)
        {
            return (unityObject as Component) != null;
        }

        /// <summary>
        /// Empty method for overriding.
        /// Add game object to tree view as root object
        /// </summary>
        public virtual void AddRootObject(GameObject gameObject) { }
        /// <summary>
        /// Empty method for overriding.
        /// Remove root game object from tree view
        /// </summary>
        public virtual void RemoveRootObject(GameObject gameObject) { }

        

        protected override void RowGUI(RowGUIArgs args)
        {
            // Draw Header for Game Object
            if (IsUnityObjectAsGameObject(GetRowObject(args.item.id)))
            {
                // draw header
                DrawItemHeader(args);

                if (ComponentManager.Instance && ComponentManager.Instance.showContextMenu)
                {
                    GameObject gameObject = GetRowObject(args.item.id) as GameObject;
                    DrawContextMenuGameObject(args, gameObject);
                }
            }
            else
            {
                if (ComponentManager.Instance && ComponentManager.Instance.showContextMenu)
                {
                    Component component = GetRowObject(args.item.id) as Component;
                    if (component is Transform)
                    {
                        DrawContextMenuTransform(args, component);
                    }
                    else
                    {
                        DrawContextMenuComponent(args, component);
                    }
                }
            }
            // draw Game Object icon
            DrawItemIcon(args);
            args.rowRect.width -= 120f;
            base.RowGUI(args);

        }
        protected override void DoubleClickedItem(int id)
        {
            if (id != 0)
            {
                base.DoubleClickedItem(id);
                Selection.activeObject = GetRowObject(id);
                if (SceneView.lastActiveSceneView)
                {
                    SceneView.lastActiveSceneView.FrameSelected();
                }
            }
        }
        protected override TreeViewItem BuildRoot()
        {
            return new TreeViewItem { id = 0, depth = -1 };
        }

        protected virtual void DrawContextMenuComponent(RowGUIArgs args, Component component)
        {
            float contextWidth = 118f;
            Rect rect = new Rect(args.rowRect);
            rect.x = rect.width - contextWidth;
            rect.width = contextWidth;

            EditorGUI.BeginChangeCheck();
            GUI.BeginGroup(rect);
            rect = new Rect(0, 2, 22, 16);
            if (GUI.Button(rect, new GUIContent(EditorGUIUtility.IconContent("Toolbar Plus").image, "Move up"), EditorStyles.miniButtonLeft))
            {
                UnityEditorInternal.ComponentUtility.MoveComponentUp(component);
            }
            rect.x += rect.width;
            if (GUI.Button(rect, new GUIContent(EditorGUIUtility.IconContent("Toolbar Minus").image, "Move down"), EditorStyles.miniButtonMid))
            {
                UnityEditorInternal.ComponentUtility.MoveComponentDown(component);
            }
            rect.x += rect.width;
            if (GUI.Button(rect, new GUIContent(EditorGUIUtility.IconContent("TreeEditor.Duplicate").image, "Copy"), EditorStyles.miniButtonMid))
            {
                UnityEditorInternal.ComponentUtility.CopyComponent(component);
            }
            rect.x += rect.width;
            if (GUI.Button(rect, new GUIContent(EditorGUIUtility.IconContent("UnityEditor.SceneHierarchyWindow").image, "Paste value"), EditorStyles.miniButtonMid))
            {
                UnityEditorInternal.ComponentUtility.PasteComponentValues(component);
            }
            rect.x += rect.width;
            if (GUI.Button(rect, new GUIContent(EditorGUIUtility.IconContent("TreeEditor.Trash").image, "Remove"), EditorStyles.miniButtonRight))
            {
                UnityEngine.Object.DestroyImmediate(component);
            }
            if (EditorGUI.EndChangeCheck())
            {
                Reload();
            }
            GUI.EndGroup();
        }
        protected virtual void DrawContextMenuTransform(RowGUIArgs args, Component component)
        {
            float contextWidth = 52f;
            Rect rect = new Rect(args.rowRect);
            rect.x = rect.width - contextWidth;
            rect.width = contextWidth;

            EditorGUI.BeginChangeCheck();
            GUI.BeginGroup(rect);
            rect = new Rect(0, 2, 22, 16);
            if (GUI.Button(rect, new GUIContent(EditorGUIUtility.IconContent("TreeEditor.Duplicate").image, "Copy"), EditorStyles.miniButtonLeft))
            {
                UnityEditorInternal.ComponentUtility.CopyComponent(component);
            }
            rect.x += rect.width;
            if (GUI.Button(rect, new GUIContent(EditorGUIUtility.IconContent("UnityEditor.SceneHierarchyWindow").image, "Paste value"), EditorStyles.miniButtonRight))
            {
                UnityEditorInternal.ComponentUtility.PasteComponentValues(component);
            }
            if (EditorGUI.EndChangeCheck())
            {
                Reload();
            }
            GUI.EndGroup();
        }
        protected virtual void DrawContextMenuGameObject(RowGUIArgs args, GameObject gameObject)
        {
            float contextWidth = 52f;
            Rect rect = new Rect(args.rowRect);
            rect.x = rect.width - contextWidth;
            rect.width = contextWidth;

            EditorGUI.BeginChangeCheck();
            GUI.BeginGroup(rect);
            rect = new Rect(0, 0, 22, 16);
            if (GUI.Button(rect, new GUIContent(EditorGUIUtility.IconContent("UnityEditor.ConsoleWindow").image, "Paste as new"), EditorStyles.miniButtonLeft))
            {
                UnityEditorInternal.ComponentUtility.PasteComponentAsNew(gameObject);
            }
            if (EditorGUI.EndChangeCheck())
            {
                Reload();
            }
            GUI.EndGroup();
        }
        protected virtual void DrawItemHeader(RowGUIArgs args)
        {
            if (!args.selected && Event.current.type == EventType.Repaint)
            {
                var bgRect = args.rowRect;
                bgRect.width = Mathf.Max(bgRect.width, 155f);
                bgRect.height = Styles.TreeViewHeader.fixedHeight;
                Styles.TreeViewHeader.Draw(bgRect, false, false, false, false);
            }
        }
        protected virtual void DrawItemIcon(RowGUIArgs args)
        {
            UnityObject unityObject = GetRowObject(args.item.id);

            Rect _rect = args.rowRect;
            _rect.x += GetContentIndent(args.item);
            _rect.width = 22f;
            _rect.height = 22f;

            if (Event.current.type == EventType.MouseDown && _rect.Contains(Event.current.mousePosition))
                SelectionClick(args.item, false);

            GUIContent content = new GUIContent();
            if (IsUnityObjectAsGameObject(unityObject))
            {
                content = GetObjectIcon(unityObject as GameObject);
            }
            else if (IsUnityObjectAsComponent(unityObject))
            {

                content = GetComponentIcon(unityObject as Component);
            }

            GUI.Label(_rect, content);
        }
        protected virtual GUIContent GetComponentIcon(Component component)
        {
            System.Type compType = component.GetType();
            Texture icon = EditorGUIUtility.ObjectContent(null, compType).image;
            if (!icon) icon = EditorGUIUtility.IconContent("cs Script Icon").image;
            return new GUIContent(icon);
        }
        protected virtual GUIContent GetObjectIcon(GameObject gameObject)
        {
#if UNITY_2018_3_OR_NEWER
            if ((PrefabUtility.GetPrefabAssetType(gameObject) != PrefabAssetType.NotAPrefab))
            {
                return new GUIContent(EditorGUIUtility.IconContent("d_Prefab Icon").image);
# else
            if (PrefabUtility.GetPrefabObject(gameObject) != null)
            {
                return new GUIContent(EditorGUIUtility.IconContent("PrefabNormal Icon").image);
#endif
            }
            else
            {
                return new GUIContent(EditorGUIUtility.IconContent("GameObject Icon").image);
            }
        }

        protected void AddChildrenRecursive(GameObject go, TreeViewItem item, IList<TreeViewItem> rows)
        {
            int childCount = go.transform.childCount;

            item.children = new List<TreeViewItem>();
            AddComponents(go, item, rows);
            for (int i = 0; i < childCount; ++i)
            {
                var childTransform = go.transform.GetChild(i);
                var childItem = CreateTreeViewItemForGameObject(childTransform.gameObject);
                item.AddChild(childItem);
                rows.Add(childItem);

                if (IsExpanded(childItem.id))
                {
                    AddChildrenRecursive(childTransform.gameObject, childItem, rows);
                }
                else
                {
                    childItem.children = CreateChildListForCollapsedParent();
                }
            }
        }
        protected void AddComponents(GameObject go, TreeViewItem item, IList<TreeViewItem> rows)
        {
            Component[] components = go.GetComponents<Component>();
            for (int i = 0; i < components.Length; ++i)
            {
                var childItem = CreateTreeViewItemForComponent(components[i]);
                item.AddChild(childItem);
                rows.Add(childItem);
            }                
        }
        protected void AddComponents(List<Component> components, TreeViewItem item, IList<TreeViewItem> rows)
        {
            for (int i = 0; i < components.Count; ++i)
            {
                var childItem = CreateTreeViewItemForComponent(components[i]);
                item.AddChild(childItem);
                rows.Add(childItem);
            }
        }

        
    }
}
