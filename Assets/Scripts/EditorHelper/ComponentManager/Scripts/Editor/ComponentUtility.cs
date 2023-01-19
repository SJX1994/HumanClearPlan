using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEngine;
using UnityComponentUtility = UnityEditorInternal.ComponentUtility;

namespace KreliStudio
{
    public static class ComponentUtility
    {
        public enum ComponentReplaceType
        {
            PasteAsNew,
            OverrideValues,
            DontCopy
        }
        public enum LocalReferencesType
        {
            CopyAsIs,
            RedirectionLocalReferences
        }
        public enum LocalReferencesNullIssue
        {
            SetOldValue,
            SetNull
        }
        /// <summary>
        /// Hide or show component in inspector.
        /// </summary>
        public static void SetVisibility(this Component component, bool isVisible)
        {
            component.hideFlags = (isVisible) ? (component.hideFlags & ~HideFlags.HideInInspector) : (component.hideFlags | HideFlags.HideInInspector);
        }
        /// <summary>
        /// Lock or unlock ability to edit component in inspector. If it's not editable it still can be modify by scripts.
        /// </summary>
        public static void SetEditable(this Component component, bool isEditable)
        {
            component.hideFlags = (isEditable) ? (component.hideFlags & ~HideFlags.NotEditable) : (component.hideFlags | HideFlags.NotEditable);
        }
        /// <summary>
        /// Check if componen is visible in inspector.
        /// </summary>
        public static bool IsVisible(this Component component)
        {
            return !component.HasFlag(HideFlags.HideInInspector);
        }
        /// <summary>
        /// Check if component is editable in inspector
        /// </summary>
        public static bool IsEditable(this Component component)
        {
            return !component.HasFlag(HideFlags.NotEditable);
        }
        /// <summary>
        /// Check if component has specific flag
        /// </summary>
        public static bool HasFlag(this Component component, HideFlags flag)
        {
            return (component.hideFlags & flag) == flag;
        }


        /// <summary>
        /// Copy many components to specify game object
        /// </summary>
        /// <param name="components">Array of source components</param>
        /// <param name="targetGameObject">Game object which is a target receiver</param>
        /// <param name="componentReplaceType">What component should do if target already has same component</param>
        /// <returns>List with pasted components</returns>
        public static List<Component> CopyComponents(Component[] components, GameObject targetGameObject, ComponentReplaceType componentReplaceType)
        {
            List<Component> comp = new List<Component>();
            for (int i = 0; i < components.Length; i++)
            {
                comp.Add( CopyComponent(components[i], targetGameObject, componentReplaceType));
            }
            return comp;
        }
        /// <summary>
        /// Copy one component to specify game object
        /// </summary>
        /// <param name="component">source component</param>
        /// <param name="targetGameObject">Game object which is a target receiver</param>
        /// <param name="componentReplaceType">What component should do if target already has same component</param>
        /// <returns></returns>
        public static Component CopyComponent(Component component, GameObject targetGameObject, ComponentReplaceType componentReplaceType)
        {
            // if there is any missing values then return
            if (targetGameObject == null || component == null)
            {
                Debug.LogError("Can't copy component properly because some parameters is missing.");
                return null;
            }

            UnityComponentUtility.CopyComponent(component);
            // get same component from target object
            Component targetEquivalentComponent = targetGameObject.GetComponent(component.GetType());
            if (targetEquivalentComponent == null)
            {
                UnityComponentUtility.PasteComponentAsNew(targetGameObject);
                return targetGameObject.GetComponent(component.GetType());
            }
            else
            {
                switch (componentReplaceType)
                {
                    case ComponentReplaceType.PasteAsNew:
                        UnityComponentUtility.PasteComponentAsNew(targetGameObject);
                        // if component was added as new then get all of them and return the last one 
                        Component[] tmpComponents = targetGameObject.GetComponents(component.GetType());
                        return tmpComponents[tmpComponents.Length-1];
                    case ComponentReplaceType.OverrideValues:
                        UnityComponentUtility.PasteComponentValues(targetEquivalentComponent);
                        break;
                    case ComponentReplaceType.DontCopy:
                        // Do nothing
                        break;
                }
            }
            return targetEquivalentComponent;
        }

        /// <summary>
        /// Get all reference variable from origin component and check if they has local values. 
        /// If some variable has local reference then try trace mirror reference in target game object.
        /// </summary>
        /// <param name="originRootParent">Transform of game object from which component references are copied</param>
        /// <param name="targetRootParent">Transform of game object to which component references are copied</param>
        /// <param name="originComponent">Component from origin game object</param>
        /// <param name="targetComponent">Component from target game object</param>
        public static void RedirectionLocalReferences(Transform originRootParent, Transform targetRootParent, Component originComponent, Component targetComponent)
        {
            RedirectionLocalReferences(originRootParent, targetRootParent, originComponent, targetComponent, LocalReferencesNullIssue.SetNull, true);
        }
        /// <summary>
        /// Get all reference variable from origin component and check if they has local values. 
        /// If some variable has local reference then try trace mirror reference in target game object.
        /// </summary>
        /// <param name="originRootParent">Transform of game object from which component references are copied</param>
        /// <param name="targetRootParent">Transform of game object to which component references are copied</param>
        /// <param name="originComponent">Component from origin game object</param>
        /// <param name="targetComponent">Component from target game object</param>
        /// <param name="localReferencesNullIssue">What component should do if target variable has null reference</param>
        /// <param name="enableLogs">Print logs, warning and errors to console</param>
        public static void RedirectionLocalReferences(Transform originRootParent, Transform targetRootParent, Component originComponent, Component targetComponent, LocalReferencesNullIssue localReferencesNullIssue, bool enableLogs)
        {

            if (originComponent.GetType() != targetComponent.GetType())
            {
                if (enableLogs)
                {
                    Debug.LogError("[ComponentManager.RedirectionLocalReferences] Different types of components: origin is " + originComponent.GetType() + " and target is " + targetComponent.GetType());
                }
                return;
            }

            // get type of component
            System.Type originComponentType = originComponent.GetType();
            // get all public and private fields
            List<FieldInfo> componentFields = new List<FieldInfo>(originComponentType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance));
            // separate only filds with references (inheriting from UnityObject)
            componentFields.RemoveAll(obj => !obj.FieldType.IsSubclassOf(typeof(UnityEngine.Object)));
            // summary raport log
            string summaryLog = "[Summary Raport for target " +targetComponent.name +"." + targetComponent.GetType() +"]";

            for (int i = 0; i < componentFields.Count; i++)
            {
                if (enableLogs)
                {
                    summaryLog += "\n   Reference field: " + originComponentType +"."+ componentFields[i].Name;
                }
                Component originReferencedComponent = componentFields[i].GetValue(originComponent) as Component;
                bool isReferencedToGameObject = false;
                if (originReferencedComponent == null)
                {
                    GameObject gameObject = componentFields[i].GetValue(originComponent) as GameObject;
                    if (gameObject != null)
                    {
                        isReferencedToGameObject = true;
                        originReferencedComponent = gameObject.transform;
                    }
                    else 
                    {
                        if (enableLogs)
                        {
                            if (componentFields[i].GetValue(originComponent) as UnityEngine.Object == null)
                            {
                                summaryLog += "\n   [Redirection: Skiped (Empty)]\n\n";
                                Debug.LogWarning("[ComponentManager.RedirectionLocalReferences] This reference is empty (field name: " + componentFields[i].Name + ")");
                            }
                            else
                            {
                                summaryLog += "\n   [Redirection: Skiped (not local reference)]\n\n";
                            }

                        }
                        continue;
                    }
                }
                // Generate heirarchy if it is a local reference
                List<string> hierarchy = GetComponentHierarchy(originRootParent, originReferencedComponent);
                // if it is not a local references then move to next variable
                if (hierarchy == null)
                {
                    if (enableLogs)
                    {
                        summaryLog += "\n   [Redirection: Skiped (not local reference)]\n\n";
                    }
                    continue;
                }

                if (enableLogs)
                {
                    summaryLog += "\n   Hierarchy: " + originRootParent.name;
                    foreach (string obj in hierarchy)
                    {
                        summaryLog += "/" + obj;
                    }
                    summaryLog += " -> ";// + originComponentType + "." + componentFields[i].Name;

                    if (isReferencedToGameObject)
                    {
                        summaryLog += " (UnityEngine.GameObject)";
                    }
                    else
                    {
                        summaryLog += " (" + originReferencedComponent.GetType() + ")";
                    }
                }

                GameObject targetChild = FindInHierarchy(targetRootParent, hierarchy);
                if (targetChild == null)
                {
                    if (enableLogs)
                    {
                        summaryLog += "\n   [Redirection: FAIL (mirror object not found)]\n\n";
                        Debug.LogWarning("[ComponentManager.RedirectionLocalReferences] Can not find local reflection for references " + originReferencedComponent + "(field name: " + componentFields[i].Name + ")");
                    }

                    if (localReferencesNullIssue == LocalReferencesNullIssue.SetNull)
                    {
                        componentFields[i].SetValue(targetComponent, null);
                    }

                    continue;
                }
                
                // if in rootParent is more than one same component then find index (position)
                int originReferencedComponentIndex = GetComponentIndex(originReferencedComponent);
                if (originReferencedComponentIndex == -1)
                {
                    if (enableLogs)
                    {
                        summaryLog += "\n   [Redirection: FAIL (can find object with index)]\n\n";
                        Debug.LogWarning("[ComponentManager.RedirectionLocalReferences] Something goes wrong with " + originReferencedComponent + "(field name: " + componentFields[i].Name + ")" + ". Can not find component index in origin object.");
                    }
                    continue;
                }

            

            //int targetComponentIndex = GetComponentIndex(targetReferencedComponent);
            //if (originComponentIndex == -1) { Debug.LogWarning("[RedirectionLocalReferences] Something goes wrong with " + targetReferencedComponent + "(field name: " + componentFields[i].Name + ")" + ". Can not find component index in target object."); continue; }
            //targetChild

            // get all same components of type which we are looking for
            List<Component> targetSameComponents = new List<Component>(targetChild.GetComponents(originReferencedComponent.GetType()));

                // if can not find any components with reference type then move to next
                if (targetSameComponents == null || targetSameComponents.Count == 0)
                {
                    if (enableLogs)
                    {
                        summaryLog += "\n   [Redirection: FAIL (mirror object do not contain target component)]\n\n";
                        Debug.LogWarning("[ComponentManager.RedirectionLocalReferences] Something goes wrong with " + originReferencedComponent + "(field name: " + componentFields[i].Name + ")" + ". Can not find any " + componentFields[i].GetType() + " in target object (target name: " + targetChild.name + ")");
                    }
                    continue;
                }
                // if components counts in different and origin comp id is out of range for target component list then take first target component
                if (originReferencedComponentIndex >= targetSameComponents.Count)
                    originReferencedComponentIndex = 0;

                UnityEngine.Object targetReferencedObject;
                if (isReferencedToGameObject)
                    targetReferencedObject = targetSameComponents[originReferencedComponentIndex].gameObject;
                else
                    targetReferencedObject = targetSameComponents[originReferencedComponentIndex];
                
                
                // redirect values
                componentFields[i].SetValue(targetComponent, targetReferencedObject);



                if (enableLogs)
                {
                    summaryLog += "\n   [Redirection: SUCCESS!]\n\n";
                }
            }
            if (enableLogs)
            {
                summaryLog += "\n\n";
                if (componentFields.Count > 0)
                    Debug.Log(summaryLog);
            }
        }
        
        /// <summary>
        /// If in game object is more than one same components then return number of position for specify component from parameter
        /// </summary>
        /// <param name="component">Component which position we want to know</param>
        /// <returns>index - number of position in game object</returns>
        private static int GetComponentIndex(Component component)
        {
            List<Component> sameComponents = new List<Component>(component.GetComponents(component.GetType()));
            int componentIndex = 0;
            if (sameComponents.Count > 1)
            {
                componentIndex = sameComponents.FindIndex(obj => obj == component);
            }
            return componentIndex;
        }
        /// <summary>
        /// Generate hierarchy (list of names from root parent to components object)
        /// </summary>
        /// <returns>Hierarchy - list of game object names</returns>
        public static List<string> GetComponentHierarchy(Transform rootParent, Component component)
        {
            List<Transform> parents = new List<Transform>(component.GetComponentsInParent<Transform>());
            int rootParentIndex = parents.FindIndex(obj => obj == rootParent);
            if (rootParentIndex >= 0)
            {
                // remove older parents than rootParent
                if (rootParentIndex < parents.Count)
                {
                    int range = parents.Count - rootParentIndex;
                    parents.RemoveRange(rootParentIndex, range);
                }
                // get names and reverse list
                List<string> hierarchyNames = parents.Select(obj => obj.name).ToList();
                hierarchyNames.Reverse();
                return hierarchyNames;
            }
            else
            {
                // there is no root parent. Something goes wrong.
                return null;
            }
        }
        /// <summary>
        /// Find or create hierarchy structure in rootParent depend on names list
        /// </summary>
        /// <param name="rootParent">Game object which is root object</param>
        /// <param name="hierarchy">list of hierarchy names</param>
        /// <returns>Game object which is end of hierarchy</returns>
        public static GameObject FindOrCreateHierarchy(Transform rootParent, List<string> hierarchy)
        {
            Transform currentParent = rootParent;
            if (hierarchy == null || hierarchy.Count == 0) return rootParent.gameObject;
            for (int i = 0; i < hierarchy.Count; i++)
            {
                // get child name
                string childName = hierarchy[i];
                // try to find child
                Transform child = null;
                if (currentParent.childCount > 0)
                {
                    child = currentParent.Find(childName);
                }
                // if there is no child then create new
                if (child == null)
                {
                    child = new GameObject(childName).transform;
                    child.transform.SetParent(currentParent);
                    child.localPosition = Vector3.zero;
                }
                // set hierarchy deeper
                currentParent = child;
            }
            return currentParent.gameObject;
        }
        /// <summary>
        /// Similar to "FindOrCreateHierarchy" but it only looking for game object in specific hierarchy 
        /// </summary>
        /// <param name="rootParent">Game object which is root object</param>
        /// <param name="hierarchy">list of hierarchy names</param>
        /// <returns>Game object which is end of hierarchy</returns>
        public static GameObject FindInHierarchy(Transform rootParent, List<string> hierarchy)
        {
            Transform currentParent = rootParent;
            if (hierarchy == null || hierarchy.Count == 0) return null;
            for (int i = 0; i < hierarchy.Count; i++)
            {
                // get child name
                string childName = hierarchy[i];
                // try to find child
                Transform child = null;
                if (currentParent.childCount > 0)
                {
                    child = currentParent.Find(childName);
                }
                // if child not found it is mean hierarchy is not exist
                if (child == null)
                    return null;
                else // set hierarchy deeper
                currentParent = child;
            }
            return currentParent.gameObject;

        }
    }
}