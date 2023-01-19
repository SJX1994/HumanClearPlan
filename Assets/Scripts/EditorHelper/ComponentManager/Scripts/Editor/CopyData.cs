using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace KreliStudio.Internal
{
    /// <summary>
    /// Copy Data class contains root object and selected components
    /// for Component Copier system.
    /// </summary>
    [System.Serializable]
    public class CopyData
    {
        /// Instance id of root object
        private int rootGameObjectId = -1;
        /// Instance id list of selected components
        private List<int> selectedIds = new List<int>();

        /// <summary>
        /// Clear list with selected ids
        /// </summary>
        public void Clear()
        {
            selectedIds.Clear();
        }
        /// <summary>
        /// Check is specify component selected
        /// </summary>
        /// <param name="id">Component instance id</param>
        /// <returns>Is selected</returns>
        public bool IsSelected(int id)
        {
            return (selectedIds.FindIndex(obj => obj == id) > -1);
        }
        /// <summary>
        /// Print to console actual status of selected components
        /// </summary>
        public void PrintData()
        {
            string data = "Root ID: " +rootGameObjectId +"\n";

            for (int i = 0; i < selectedIds.Count; i++)
            {
                data += i + ". ID: " + selectedIds[i] + "\n";
            }
            Debug.Log(data);
        }
        /// <summary>
        /// Return Instance id list of selected components
        /// </summary>
        /// <returns>List with instance ids</returns>
        public List<int> GetSelectedIds()
        {
            return selectedIds;
        }
        /// <summary>
        /// Return list of selected components
        /// </summary>
        /// <returns>Components list</returns>
        public List<Component> GetSelectedComponents()
        {
            return selectedIds.Select(obj => EditorUtility.InstanceIDToObject(obj) as Component).ToList();
        }
        /// <summary>
        /// Return root game object
        /// </summary>
        /// <returns>Instance id </returns>
        public int GetRootGameObjectId()
        {
            return rootGameObjectId;
        }
        /// <summary>
        /// Return root game object
        /// </summary>
        /// <returns>Root game object </returns>
        public GameObject GetRootGameObject()
        {
            return EditorUtility.InstanceIDToObject(rootGameObjectId) as GameObject;
        }
        /// <summary>
        /// Set new root game object and clear list of selected ids 
        /// </summary>
        /// <param name="gameObject">root game object</param>
        /// <param name="clear">true - call Clear() Method</param>
        public void SetRootGameObject(GameObject gameObject, bool clear)
        {
            if (clear) Clear();

            if (gameObject)
                rootGameObjectId = gameObject.GetInstanceID();
            else
                rootGameObjectId = -1;
        }
        /// <summary>
        /// Add component instance id to selected ids list or remove it from there
        /// </summary>
        /// <param name="id">component instance id</param>
        /// <param name="isSelected"> true - add, false - remove</param>
        public void SetSelection(int id, bool isSelected)
        {
            if (isSelected)
            {
                if (!IsSelected(id))
                    selectedIds.Add(id);
            }
            else
            {
                selectedIds.Remove(id);
            }
        }
    }
}

