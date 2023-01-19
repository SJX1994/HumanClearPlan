using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace KreliStudio.Internal
{
    public class FinderTreeView : ComponentTreeView
    {
        /// <summary>
        /// Help class for contains information about components from same game object
        /// </summary>
        public class FinderTreeElement
        {
            public GameObject gameObject;
            public List<Component> components = new List<Component>();
        }
        /// <summary>
        /// Finder tree element list with all current finded components
        /// </summary>
        public List<FinderTreeElement> FinderTreeElements { get; protected set; }

        public FinderTreeView(TreeViewState treeViewState) : base(treeViewState)
        {
            if (FinderTreeElements == null)
            {
                FinderTreeElements = new List<FinderTreeElement>();
            }
            extraSpaceBeforeIconAndLabel = 22f;
            Reload();
        }
        /// <summary>
        /// Replace current list with new one
        /// </summary>
        /// <param name="finderList">new finder tree element list</param>
        public void SetFinderList(List<FinderTreeElement> finderList)
        {
            FinderTreeElements = finderList;
            Reload();
        }
        /// <summary>
        /// Clear list
        /// </summary>
        public void ClearFinerList()
        {
            FinderTreeElements = new List<FinderTreeElement>();
            Reload();
        }
        
        protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
        {
            List<TreeViewItem> rows = new List<TreeViewItem>();

            if (FinderTreeElements != null)
            {
                for (int i = 0; i < FinderTreeElements.Count; i++)
                {
                    if (FinderTreeElements[i] == null || FinderTreeElements[i].gameObject == null)
                    {
                        FinderTreeElements.RemoveAt(i);
                        return BuildRows(root);
                    }
                    TreeViewItem item = CreateTreeViewItemForGameObject(FinderTreeElements[i].gameObject);
                    root.AddChild(item);
                    rows.Add(item);


                    if (IsExpanded(item.id))
                    {
                        if (FinderTreeElements[i].components.Count > 0)
                        {
                            AddComponents(FinderTreeElements[i].components, item, rows);
                        }
                    }
                    else
                    {
                        item.children = CreateChildListForCollapsedParent();
                    }

                    SetExpanded(item.id, true);                  

                }
                SetupDepthsFromParentsAndChildren(root);
            }
            return rows;
        }  
    }
}
