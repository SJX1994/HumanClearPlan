using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using System.Collections.Generic;
using System.Linq;

namespace KreliStudio.Internal
{
    public class ComponentManager : EditorWindow
    {
        #region Main Window
        // main tool bar variable
        private readonly string[] tabNames = { "Component Copier", "Component Finder" };
        public int tabIndex = 0;
        public bool showFullName;
        public bool showId;
        public bool showContextMenu;


        // Instance and window creator
        public static ComponentManager Instance { get; private set; }
        [MenuItem("Window/Component Manager")]
        public static void GetWindow()
        { 
            Instance = GetWindow<ComponentManager>();
            Instance.titleContent = new GUIContent(" Components", EditorGUIUtility.IconContent("FilterByType").image);
            Instance.Focus();
            Instance.Repaint();
            Instance.minSize = new Vector2(650, 400);
            Instance.Show();

            Instance.copyData = new CopyData();
            Instance.tabIndex = EditorPrefs.GetInt("tabIndex",0);
        }
        
        // Handler methods
        private void OnEnable()
        {
            if (sourceTreeViewState == null)
                sourceTreeViewState = new TreeViewState();
            sourceTreeView = new SourceTreeView(sourceTreeViewState);

            if (targetTreeViewState == null)
                targetTreeViewState = new TreeViewState();
            targetTreeView = new TargetTreeView(targetTreeViewState);

            if (finderSceneTreeView == null)
                finderSceneTreeViewState = new TreeViewState();
            finderSceneTreeView = new FinderTreeView(finderSceneTreeViewState);


        }
        private void OnFocus()
        {
            if (Instance == null) GetWindow();
            switch (Instance.tabIndex)
            {
                case 0:
                    if (sourceTreeView != null)
                        sourceTreeView.Reload();

                    if (targetTreeView != null)
                        targetTreeView.Reload();
                    break;
                case 1:
                    if (finderSceneTreeView != null)
                        finderSceneTreeView.Reload();
                    RefreshSearchView();
                    break;
            }   
            Repaint();
        }
        private void OnSelectionChange()
        {
            switch (tabIndex)
            {
                case 0:
                    if (sourceTreeView != null)
                        sourceTreeView.SetSelection(Selection.instanceIDs);

                    if (targetTreeView != null)
                        targetTreeView.SetSelection(Selection.instanceIDs);
                    break;
                case 1:
                    if (finderSceneTreeView != null)
                        finderSceneTreeView.SetSelection(Selection.instanceIDs);
                    break;
            }
            Repaint();
        }
        private void OnHierarchyChange()
        {
            switch (tabIndex)
            {
                case 0:
                    if (sourceTreeView != null)
                        sourceTreeView.Reload();

                    if (targetTreeView != null)
                        targetTreeView.Reload();
                    break;
                case 1:
                    if (finderSceneTreeView != null)
                        finderSceneTreeView.Reload();
                    RefreshSearchView();
                    break;
            }
            Repaint();
        }
        private void OnGUI()
        {
            DrawToolbarMain();
            switch (tabIndex)
            {
                case 0:
                    DrawSourceView();
                    DrawArrowView();
                    DrawTargetView();
                    DrawSettingsView();
                    break;
                case 1:
                    DrawSearchBar();
                    DrawFindertView();
                    break;
            }
        }
        
        // Toolbars
        private void DrawToolbarMain()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            EditorGUI.BeginChangeCheck();

            tabIndex = GUILayout.Toolbar(tabIndex, tabNames, EditorStyles.toolbarButton);

            GUILayout.FlexibleSpace();

            showFullName = GUILayout.Toggle(showFullName, "Full Names", EditorStyles.toolbarButton);
            showId = GUILayout.Toggle(showId, "Instance Ids", EditorStyles.toolbarButton);
            showContextMenu = GUILayout.Toggle(showContextMenu, "Context Menu", EditorStyles.toolbarButton);

            if (EditorGUI.EndChangeCheck())
            {
                if (sourceTreeView != null)
                    sourceTreeView.Reload();

                if (targetTreeView != null)
                    targetTreeView.Reload();

                if (finderSceneTreeView != null)
                    finderSceneTreeView.Reload();

                Repaint();
                EditorPrefs.SetInt("tabIndex", tabIndex);

                if (tabIndex == 1) RefreshSearchView(); 
            }
            GUILayout.EndHorizontal();
        }
        #endregion

        #region Component Copier Tab
        // Data variables
        public CopyData copyData;
        private ComponentUtility.ComponentReplaceType componentReplaceType;
        private ComponentUtility.LocalReferencesType localReferencesType;
        private ComponentUtility.LocalReferencesNullIssue localReferencesNullIssue;
        private bool enableLogs;

        // Tree view variables
        private TreeViewState sourceTreeViewState;
        private ComponentTreeView sourceTreeView;
        private TreeViewState targetTreeViewState;
        private ComponentTreeView targetTreeView;
        private List<Transform> tmpTargetPrefabInstances = new List<Transform>();

        // rect & GUI variables
        private float margin = 5f;
        private Vector2 sourceScroll = Vector2.zero;
        private Vector2 targetScroll = Vector2.zero;
        private Rect SourceRect
        {
            get
            {
                return new Rect(
                x: margin,
                y: 15f + margin,
                width: position.width * 0.5f - margin - 20,
                height: position.height - 120f);
            }
        }
        private Rect ArrowRect
        {
            get
            {
                return new Rect(
                x: position.width * 0.5f - margin - 10,
                y: 110f + margin,
                width: 40,
                height: position.height - 215f);
            }
        }
        private Rect TargetRect
        {
            get
            {
                return new Rect(
                x: position.width * 0.5f + margin + 20,
                y: 15f + margin,
                width: position.width * 0.5f - margin * 2f - 20,
                height: position.height - 120f);
            }
        }
        private Rect BottomRect
        {
            get
            {
                return new Rect(
                x: 5f,
                y: position.height - 100f + margin,
                width: position.width - 10f,
                height: 100f);
            }
        }

        // Views
        private void DrawSourceView()
        {
            GUILayout.BeginArea(SourceRect);
            DrawTitle("Source Object (Copy from)");
            EditorGUI.BeginChangeCheck();
            GameObject sourceGameObject = ((SourceTreeView)sourceTreeView).RootGameObject;
            sourceGameObject = (GameObject)EditorGUILayout.ObjectField(sourceGameObject, typeof(GameObject), true);
            if (EditorGUI.EndChangeCheck())
            {
                if (sourceGameObject != null && IsNotInstantiatedPrefab(sourceGameObject.transform))
                {
                    sourceGameObject = null;
                    Debug.LogError("Can not use prefab as source object. Create instance of this prefab in current scene and then add it to Component Manager.");
                }
                
                    copyData.SetRootGameObject(sourceGameObject, true);
                    sourceTreeView.AddRootObject(sourceGameObject);
                    sourceTreeView.Reload();
            }
            GUILayout.BeginVertical(Styles.TreeViewArea);
            //DrawToolbarSource();
            sourceScroll = GUILayout.BeginScrollView(sourceScroll);            
                    sourceTreeView.OnGUI(GUILayoutUtility.GetRect(0, sourceTreeView.totalHeight));
                GUILayout.EndScrollView();
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
        private void DrawTargetView()
        {
            GUILayout.BeginArea(TargetRect);
            DrawTitle("Target Objects (Paste to)");
            DrawDropArea();

            GUILayout.BeginVertical(Styles.TreeViewArea);
            //DrawToolbarTarget();

            targetScroll = GUILayout.BeginScrollView(targetScroll);
            targetTreeView.OnGUI(GUILayoutUtility.GetRect(0, targetTreeView.totalHeight));
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
        private void DrawSettingsView()
        {
            GUILayout.BeginArea(BottomRect);
            DrawTitle("Settings");
            GUILayout.BeginHorizontal(Styles.BottomSettigns);

            GUILayout.BeginVertical(GUILayout.Width(Screen.width * 0.33f ));
            DrawCopyTypeControl();
            DrawReferencesControl();
            DrawReferencesNullIssue();
            GUILayout.EndVertical();

            GUILayout.BeginVertical(GUILayout.Width(Screen.width * 0.33f));
            DrawReferencesLogs();
            GUILayout.EndVertical();

            GUILayout.BeginVertical(GUILayout.Width(Screen.width * 0.33f));
            DrawSummary();
            GUILayout.EndVertical();

            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }
        private void DrawArrowView(){
            GUILayout.BeginArea(ArrowRect);
            GUILayout.BeginVertical(GUILayout.Height(ArrowRect.height));
            GUILayout.FlexibleSpace();
            GUILayout.Label(new GUIContent(EditorGUIUtility.IconContent("breadcrump mid on").image));
            GUILayout.FlexibleSpace();
            GUILayout.Label(new GUIContent(EditorGUIUtility.IconContent("breadcrump mid on").image));
            GUILayout.FlexibleSpace();
            GUILayout.Label(new GUIContent(EditorGUIUtility.IconContent("breadcrump mid on").image));
            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
        
        // Controls
        private void DrawTitle(string text)
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label(text, Styles.Title);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
        private void DrawDropArea()
        {
            Rect _dropAreaRect = GUILayoutUtility.GetRect(TargetRect.width, 70);
            int id = GUIUtility.GetControlID(new GUIContent("Drop here"), FocusType.Passive, _dropAreaRect);
            if (Event.current.type == EventType.Repaint)
                EditorStyles.objectFieldThumb.Draw(_dropAreaRect, new GUIContent("Drop target here (Game Object)"), id);

            if (_dropAreaRect.Contains(Event.current.mousePosition))
            {
                switch (Event.current.type)
                {
                    case EventType.DragPerform:
                        if (DragAndDrop.objectReferences.Length == 1)
                        {
                            GameObject droppedObject = DragAndDrop.objectReferences[0] as GameObject;
                            if (droppedObject != null)
                            {
                                targetTreeView.AddRootObject(droppedObject);
                                HandleUtility.Repaint();
                            }
                        }
                        break;
                    case EventType.DragUpdated:
                        if (DragAndDrop.objectReferences.Length == 1)
                        {
                            DragAndDrop.AcceptDrag();
                            DragAndDrop.activeControlID = id;
                            DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
                        }
                        break;
                }
            }
        }
        private void DrawReferencesControl()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(new GUIContent("Mirror local references", "Component Manager try to find all local references and replace by target's equivalent references where its pasted to."));
            localReferencesType = (ComponentUtility.LocalReferencesType)EditorGUILayout.EnumPopup(localReferencesType);
            GUILayout.EndHorizontal();
        }
        private void DrawReferencesNullIssue()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(new GUIContent("If redirecred reference is not exist?", "Component Manager will does it if redirection is failed"));
            localReferencesNullIssue = (ComponentUtility.LocalReferencesNullIssue)EditorGUILayout.EnumPopup(localReferencesNullIssue);
            GUILayout.EndHorizontal();
        }
        private void DrawCopyTypeControl()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(new GUIContent("If component already exists?", "If copied component exist in target object."));
            componentReplaceType = (ComponentUtility.ComponentReplaceType)EditorGUILayout.EnumPopup(componentReplaceType);
            GUILayout.EndHorizontal();

        }
        private void DrawReferencesLogs()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(10);
            enableLogs = EditorGUILayout.ToggleLeft(new GUIContent("Enable logs", "Enable logs while reference redirection process"),enableLogs);
            GUILayout.EndHorizontal();
        }
        private void DrawSummary()
        {
            GUILayout.BeginVertical();
            GUILayout.Space(10);
            if (GUILayout.Button(new GUIContent(" Copy Components",EditorGUIUtility.IconContent("ShurikenCheckMarkNormalOn").image),GUILayout.Width(Screen.width * 0.2f), GUILayout.Height(35f)))
            {
                StartCopyComponentsOperation();
            }
            GUILayout.EndVertical();
        }
        
        // Component Operations
        private void StartCopyComponentsOperation()
        {
            GameObject originRootParent = copyData.GetRootGameObject();
            List<Component> originComponents = copyData.GetSelectedComponents();            
            List<Transform> targetRootParent = ((TargetTreeView)targetTreeView).RootGameObjects.Select(obj => obj.transform).ToList();
            List<Component> targetComponents = new List<Component>();
            // log errors
            if (originRootParent == null) { Debug.LogWarning("Source Game Object not found. Select source game object first."); return; }
            if (originComponents == null || originComponents.Count == 0) { Debug.LogWarning("Nothing to copy. Select some components first."); return; }
            if (targetRootParent == null || targetRootParent.Count == 0) { Debug.LogWarning("Target Game Objects not found. Select target game objects first."); return; }
            
            // do it for all selected components
            for (int i = 0; i < originComponents.Count; i++)
            {
                // hierarchy structure from component's GameObject to OriginRootParent and save it to list as names
                List<string> hierarchyNames = ComponentUtility.GetComponentHierarchy(originRootParent.transform, originComponents[i]);
                // do it for all target objects
                for (int j = 0; j < targetRootParent.Count; j++)
                {
                    // if target is a prefab from project folders then create its instance
                    Transform tmpTargetRootParent = GetInstanceOfPrefab(targetRootParent[j]);

                    // try find hierarchy structure in target object. if it is not exist then create it and return deeper target child
                    GameObject targetChildGameObject = ComponentUtility.FindOrCreateHierarchy(tmpTargetRootParent, hierarchyNames);
                    if (targetChildGameObject)
                    {
                        // and at the finish copy component to this object
                        targetComponents.Add(ComponentUtility.CopyComponent(originComponents[i], targetChildGameObject, componentReplaceType));
                    } 
                }
            }
            


            if (localReferencesType == ComponentUtility.LocalReferencesType.RedirectionLocalReferences)
            {
                for (int i = 0; i < originComponents.Count; i++)
                {
                    for (int j = 0; j < targetRootParent.Count; j++)
                    {
                        Transform tmpTargetRootParent = GetInstanceOfPrefab(targetRootParent[j]);

                        int targetComponentIndex = i * targetRootParent.Count + j;                        
                        ComponentUtility.RedirectionLocalReferences(originRootParent.transform, tmpTargetRootParent, originComponents[i], targetComponents[targetComponentIndex], localReferencesNullIssue, enableLogs);
                    }
                }
            }

            if (tmpTargetPrefabInstances.Count > 0)
            {
                SavePrefabsAndDestroyTmpInstances();
            }

        }
        private bool IsNotInstantiatedPrefab(Transform prefab)
        {
#if UNITY_2018_3_OR_NEWER
            return (PrefabUtility.GetPrefabInstanceStatus(prefab) == PrefabInstanceStatus.NotAPrefab && PrefabUtility.GetPrefabAssetType(prefab) != PrefabAssetType.NotAPrefab);
#else
            return (PrefabUtility.GetPrefabParent(prefab) == null && PrefabUtility.GetPrefabObject(prefab) != null);
#endif
        }
        private Transform GetInstanceOfPrefab(Transform prefab)
        {
            Transform instance = null;
            if (IsNotInstantiatedPrefab(prefab))
            {
                instance = GetTempInstanceFromList(prefab);
                if (instance == null)
                {
                    instance = PrefabUtility.InstantiatePrefab(prefab) as Transform;
                    tmpTargetPrefabInstances.Add(instance);
                }
            }
            else
            {
                instance = prefab;
            }
            return instance;
        }
        private Transform GetTempInstanceFromList(Transform prefab)
        {
            foreach (Transform tmpInstance in tmpTargetPrefabInstances)
            {
#if UNITY_2018_3_OR_NEWER
                if (PrefabUtility.GetCorrespondingObjectFromSource(tmpInstance) == prefab)
#else
                if (PrefabUtility.GetPrefabParent(tmpInstance) == prefab)
#endif
                {
                    return tmpInstance;
                }
            }
            return null;
        }
        private void SavePrefabsAndDestroyTmpInstances()
        {
            for (int i = 0; i < tmpTargetPrefabInstances.Count; i++)
            {
#if UNITY_2018_3_OR_NEWER
                Object prefab = PrefabUtility.GetCorrespondingObjectFromSource(tmpTargetPrefabInstances[i]);
                string path = AssetDatabase.GetAssetPath(prefab);
                PrefabUtility.SaveAsPrefabAssetAndConnect(tmpTargetPrefabInstances[i].gameObject, path,InteractionMode.AutomatedAction);
#else
                Object prefab = PrefabUtility.GetPrefabParent(tmpTargetPrefabInstances[i]);
                PrefabUtility.ReplacePrefab(tmpTargetPrefabInstances[i].gameObject, prefab, ReplacePrefabOptions.ConnectToPrefab);
#endif
                DestroyImmediate(tmpTargetPrefabInstances[i].gameObject);
            }
            tmpTargetPrefabInstances.Clear();
        }
#endregion

        #region Component Finder Tab
        // Data variables
        private string searchString = "";
        private bool onlyScene = true;

        // Tree view variables
        private TreeViewState finderSceneTreeViewState;
        private ComponentTreeView finderSceneTreeView;
        private List<FinderTreeView.FinderTreeElement> finderTreeElements = new List<FinderTreeView.FinderTreeElement>();

        // rect & GUI variables
        private Vector2 finderScroll = Vector2.zero;
        private Rect SearchBarRect
        {
            get
            {
                return new Rect(
                x: margin,
                y: 17f + margin,
                width: position.width - margin * 2.0f,
                height: 50f);
            }
        }
        private Rect FinderViewRect
        {
            get
            {
                return new Rect(
                x: margin,
                y: 55f + margin,
                width: position.width - margin * 2.0f,
                height: position.height - (60f + margin));
            }
        }
        
        // Views
        private void DrawSearchBar()
        {
            GUILayout.BeginArea(SearchBarRect);

            EditorGUI.BeginChangeCheck();
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            searchString = GUILayout.TextField(searchString, Styles.SearchTextField);
            if (GUILayout.Button("", Styles.SearchCancelButton))
            {
                // Remove focus if cleared
                searchString = "";
                GUI.FocusControl(null);
            }
            GUILayout.Space(10f);
            onlyScene = EditorGUILayout.ToggleLeft("Only scene", onlyScene, GUILayout.Width(100f));

            if (EditorGUI.EndChangeCheck())
            {
                RefreshSearchView();
            }

            GUILayout.EndHorizontal();
            if (searchString.Length > 0)
            {
                string objText = " object";
                if (finderTreeElements.Count > 1)
                    objText += "s";

                GUILayout.Label(finderTreeElements.Count +objText + " found.", EditorStyles.miniLabel);
            }

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
        private void DrawFindertView()
        {
            GUILayout.BeginArea(FinderViewRect);
            GUILayout.BeginVertical(Styles.TreeViewArea);
            finderScroll = GUILayout.BeginScrollView(finderScroll);
            finderSceneTreeView.OnGUI(GUILayoutUtility.GetRect(0, finderSceneTreeView.totalHeight));
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
       
        // Finder Operations
        private void RefreshSearchView()
        {
            if (searchString.Length > 0)
            {
                finderTreeElements = SearchComponents(searchString, onlyScene);
                if (finderSceneTreeView != null)
                    ((FinderTreeView)finderSceneTreeView).SetFinderList(finderTreeElements);
            }
            else
            {
                finderTreeElements.Clear();
                if (finderSceneTreeView != null)
                    ((FinderTreeView)finderSceneTreeView).ClearFinerList();
            }
        }
        private List <FinderTreeView.FinderTreeElement> SearchComponents(string searchName, bool onlyScene)
        {
            List<GameObject> allGameObjects = new List<GameObject>();
            List<FinderTreeView.FinderTreeElement> finalGameObjects = new List<FinderTreeView.FinderTreeElement>();
            // get all game objects from scene or scene + project
            if (onlyScene)
            {
                allGameObjects.AddRange(UnityEngine.Object.FindObjectsOfType<GameObject>());
            }
            else
            {
                allGameObjects.AddRange(Resources.FindObjectsOfTypeAll<GameObject>());
            }

            // remove all hidden objects
            allGameObjects.RemoveAll(x => x.hideFlags != HideFlags.None);
            

            foreach(GameObject obj in allGameObjects)
            {
                List<Component> components = new List<Component>(obj.GetComponents<Component>());

                for (int i = 0; i < components.Count; i++)
                {
                    if (components[i] == null)
                        continue;

                    // if componont type contain search string then add game object and this component to result
                    if (components[i].GetType().ToString().ToLower().Contains(searchName.ToLower()))
                    {
                        // already exist?
                        int index = finalGameObjects.FindIndex(x => x.gameObject == obj);
                        //if not then add gameobject
                        if (index == -1)
                        {
                            FinderTreeView.FinderTreeElement element = new FinderTreeView.FinderTreeElement();
                            element.gameObject = obj;
                            element.components.Add(components[i]);

                            finalGameObjects.Add(element);
                        }
                        else
                        {
                            finalGameObjects[index].components.Add(components[i]);
                        }
                    }
                }
            }
            return finalGameObjects;
        }
#endregion
    }
    public static class Styles
    {
        public static GUIStyle TreeViewHeader = "RL Header";
        public static GUIStyle TreeViewArea = "TextArea";
        public static GUIStyle Title = "BoldLabel";
        public static GUIStyle ToolbarButton = "toolbarbutton";
        public static GUIStyle BottomSettigns = "HelpBox";
        public static GUIStyle SearchTextField = "ToolbarSeachTextField";
        public static GUIStyle SearchCancelButton = "ToolbarSeachCancelButton";
    }

    

}
