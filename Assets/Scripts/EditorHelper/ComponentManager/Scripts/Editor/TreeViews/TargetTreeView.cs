using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace KreliStudio.Internal
{
    public class TargetTreeView : ComponentTreeView
    {

        public List<GameObject> RootGameObjects { get; protected set; }

        public override void AddRootObject(GameObject gameObject)
        {
            if (!RootGameObjects.Contains(gameObject))
            {
                RootGameObjects.Add(gameObject);
            }
            Reload();
        }
        public override void RemoveRootObject(GameObject gameObject)
        {
            RootGameObjects.Remove(gameObject);
            Reload();
        }
        public TargetTreeView(TreeViewState treeViewState) : base(treeViewState)
        {
            if (RootGameObjects == null)
            {
                RootGameObjects = new List<GameObject>();
            }
            extraSpaceBeforeIconAndLabel = 22f;
            Reload();
        }

        protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
        {
            List<TreeViewItem> rows = new List<TreeViewItem>();

            if (RootGameObjects != null)
            {
                for (int i = 0; i < RootGameObjects.Count; i++)
                {
                    if (RootGameObjects[i] == null)
                    {
                        RootGameObjects.RemoveAt(i);
                        return BuildRows(root);
                    }
                    TreeViewItem item = CreateTreeViewItemForGameObject(RootGameObjects[i]);
                    root.AddChild(item);
                    rows.Add(item);


                    if (IsExpanded(item.id))
                    {
                        if (RootGameObjects[i].transform.childCount > 0)
                        {
                            AddChildrenRecursive(RootGameObjects[i], item, rows);
                        }
                        else
                        {
                            AddComponents(RootGameObjects[i], item, rows);
                        }
                    }
                    else
                    {
                        item.children = CreateChildListForCollapsedParent();
                    }
                
                }
                SetupDepthsFromParentsAndChildren(root);
            }
            return rows;
        }
        protected override void RowGUI(RowGUIArgs args)
        {
            // Text
            base.RowGUI(args);

            if (IsUnityObjectAsGameObject(GetRowObject(args.item.id)))
            {
                // if its root object then display CONTEXT MENU
                if (args.item.parent.id == 0)
                {
                    DrawItemContextMenu(args);
                }
            }
        }
        protected void DrawItemContextMenu(RowGUIArgs args)
        {
            Rect _rect = args.rowRect;
            _rect.x += _rect.width - 30f;
            _rect.width = 22f;
            _rect.height = 16f;
            if (GUI.Button(_rect, new GUIContent(EditorGUIUtility.IconContent("winbtn_win_close_a")), EditorStyles.miniButtonRight))
            {
                GameObject objectToRemove = GetRowObject(args.item.id) as GameObject;
                if (objectToRemove) {
                    RemoveRootObject(objectToRemove);
                }
            }
        }
    }


}