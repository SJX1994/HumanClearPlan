using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace KreliStudio.Internal
{
    public class SourceTreeView : ComponentTreeView
    {
        
        public GameObject RootGameObject { get; protected set; }

        public SourceTreeView(TreeViewState treeViewState) : base(treeViewState)
        {
            extraSpaceBeforeIconAndLabel = 22f;
            Reload();
        }
        public override void AddRootObject(GameObject gameObject) {
            RootGameObject = gameObject;
            Reload();
        }
        public override void RemoveRootObject(GameObject gameObject = null)
        {
            AddRootObject(null);
        }
        
        protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
        {
            List<TreeViewItem> rows = new List<TreeViewItem>();

            if (RootGameObject != null)
            {
                TreeViewItem item = CreateTreeViewItemForGameObject(RootGameObject);
                root.AddChild(item);
                rows.Add(item);

                if (IsExpanded(item.id))
                {
                    if (RootGameObject.transform.childCount > 0)
                    {
                        AddChildrenRecursive(RootGameObject, item, rows);
                    }
                    else
                    {
                        AddComponents(RootGameObject, item, rows);
                    }
                }
                else
                {
                    item.children = CreateChildListForCollapsedParent();
                }
            
               
                SetupDepthsFromParentsAndChildren(root);
            }
            return rows;
        }
        protected override void RowGUI(RowGUIArgs args)
        {
            Event evt = Event.current;

            if (IsUnityObjectAsComponent(GetRowObject(args.item.id)))
            {
                extraSpaceBeforeIconAndLabel = 40f;

                Rect toggleRect = args.rowRect;
                toggleRect.x += GetContentIndent(args.item) + 22f;
                toggleRect.width = 16f;

                if (evt.type == EventType.MouseDown && toggleRect.Contains(evt.mousePosition))
                    SelectionClick(args.item, false);
                bool isSelected = ComponentManager.Instance.copyData.IsSelected(args.item.id);
                isSelected = EditorGUI.Toggle(toggleRect, isSelected);
                ComponentManager.Instance.copyData.SetSelection(args.item.id, isSelected);
            }
            else
            {
                extraSpaceBeforeIconAndLabel = 22f;
            }

            base.RowGUI(args);
        }
    }
}
