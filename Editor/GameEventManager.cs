using Sirenix.OdinInspector.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Sirenix.Utilities.Editor;
using GameEvents;

namespace GameEventManager
{
    // TODO: Create a second version of this tool based on UI Toolkit instead. Reason: Odin does not yet have full
    //  capability of rendering UI such as GameObject inspector/components.
    
    // TODO: Add feature where we can select a GameEventAsset SO to show all of its callers and listeners in the active scene.
    public class GameEventManagerWindow : OdinMenuEditorWindow
    {
        private bool showOnSelection;
        private bool filterByPath;
        private bool sortByEventType;
        
        private static readonly Type[] callerTypes = Enumerable.OrderBy(TypeCache.GetTypesDerivedFrom(typeof(GameEventCaller<>)), m => m.Name).ToArray();
        private static readonly Type[] listenerTypes = Enumerable.OrderBy(TypeCache.GetTypesDerivedFrom(typeof(GameEventListener<>)), m => m.Name).ToArray();
        
        private static List<GameObject> callerObjects = new List<GameObject>();
        private static List<GameObject> listenerObjects = new List<GameObject>();

        [MenuItem("Tools/Game Events/Game Event Manager", priority = - 10)]
        private static void OpenEditor() => GetWindow<GameEventManagerWindow>();
        
        protected override void OnEnable()
        {
            base.OnEnable();
            titleContent = new GUIContent("Game Event Manager");
            AssetDatabase.Refresh();
            DetectEventObjects();
            ForceMenuTreeRebuild();
            MenuTree.Selection.SelectionChanged += OnSelectionChanged;
        }

        protected override void OnBeginDrawEditors()
        {
            MenuTree.CollapseEmptyItems();
            MenuTree.DrawSearchToolbar();
            OdinMenuTreeSelection selection = MenuTree.Selection;
            
            SirenixEditorGUI.BeginHorizontalToolbar();
            {
                GUILayout.FlexibleSpace();

                bool showInSceneView = SirenixEditorGUI.ToolbarToggle(showOnSelection, "Show In Scene View");
                if (showInSceneView != showOnSelection)
                {
                    showOnSelection = showInSceneView;
                }

                if (SirenixEditorGUI.ToolbarButton("Refresh Events"))
                {
                    DetectEventObjects();
                    ForceMenuTreeRebuild();
                }
                bool showByPath = SirenixEditorGUI.ToolbarToggle(filterByPath, "Filter By GameObject");
                if (showByPath != filterByPath)
                {
                    filterByPath = showByPath;
                    ForceMenuTreeRebuild();
                }
                
                bool showByType = SirenixEditorGUI.ToolbarToggle(sortByEventType, "Filter By Type");
                if (showByType != sortByEventType)
                {
                    sortByEventType = showByType;
                    ForceMenuTreeRebuild();
                }
                
            }
            SirenixEditorGUI.EndHorizontalToolbar();
            DetectEventObjects();
        }
        protected override OdinMenuTree BuildMenuTree()
        {
            OdinMenuTree tree = new OdinMenuTree();
            AddEventGameObjectsToMenuTree(tree);
            return tree;
        }

        private void AddEventGameObjectsToMenuTree(OdinMenuTree tree)
        {
            for (int x = 0; x < callerTypes.Length; x++)
            {
                Type callerType = callerTypes[x];
                if (callerObjects.Count <= 0) return;
                for (int y = 0; y < callerObjects.Count; y++)
                {
                    if (callerObjects[y] == null)
                    {
                        callerObjects.RemoveAt(y);
                        continue;
                    }
                    GameObject callerObject = callerObjects[y];
                    AddGameObjectToMenuTree(tree, callerType, callerObject);
                }
            }
            
            for (int x = 0; x < listenerTypes.Length; x++)
            {
                Type listenerType = listenerTypes[x];
                if (listenerObjects.Count <= 0) return;
                for (int y = 0; y < listenerObjects.Count; y++)
                {
                    if (listenerObjects[y] == null)
                    {
                        listenerObjects.RemoveAt(y);
                        continue;
                    }
                    GameObject listenerObject = listenerObjects[y];
                    AddGameObjectToMenuTree(tree, listenerType, listenerObject);
                }
            }
        }

        protected void DetectEventObjects()
        {
            foreach (GameObject go in (GameObject[])Resources.FindObjectsOfTypeAll(typeof(GameObject)))
            {
                if (!EditorUtility.IsPersistent(go.transform.root.gameObject) &&
                    !(go.hideFlags == HideFlags.NotEditable || go.hideFlags == HideFlags.HideAndDontSave))
                {
                    for (int i = 0; i < callerTypes.Length; i++)
                    {
                        var type = callerTypes[i];
                        
                        if (go.GetComponent(type) != null && !callerObjects.Contains(go))
                        {
                            callerObjects.Add(go);
                        }
                    }
        
                    for (int i = 0; i < listenerTypes.Length; i++)
                    {
                        var type = listenerTypes[i];
                        if (go.GetComponent(type) != null && !listenerObjects.Contains(go))
                        {
                            listenerObjects.Add(go);
                        }
                    }
                }
            }
        }

        protected void AddGameObjectToMenuTree(OdinMenuTree tree, Type type, GameObject gameObject)
        {
            if (gameObject.GetComponent(type) != null)
            {
                if (!filterByPath)
                {
                    if (sortByEventType)
                    {
                        if (type.ToString().Contains("Caller"))
                        {
                            OdinMenuItem menuItem = new OdinMenuItem(tree, gameObject.name, new GameEventObjectWindow(gameObject, type));
                            tree.AddMenuItemAtPath($"Callers", menuItem);
                        }
                        if (type.ToString().Contains("Listener"))
                        {
                            OdinMenuItem menuItem = new OdinMenuItem(tree, gameObject.name, new GameEventObjectWindow(gameObject, type));
                            tree.AddMenuItemAtPath($"Listeners", menuItem);
                        }
                    }
                    else
                    {
                        OdinMenuItem menuItem = new OdinMenuItem(tree, gameObject.name, new GameEventObjectWindow(gameObject, type));
                        tree.AddMenuItemAtPath(type.Name, menuItem);
                    }
                }
                else
                {
                    OdinMenuItem menuItem = new OdinMenuItem(tree, type.Name, new GameEventObjectWindow(gameObject, type));
                    tree.AddMenuItemAtPath(gameObject.name, menuItem);
                }
            }
        }
        
        protected void OnSelectionChanged(SelectionChangedType selectionChangedType)
        {
            GameEventObjectWindow eventObjectWindow = MenuTree.Selection.SelectedValue as GameEventObjectWindow;
                    
            Selection.activeGameObject = eventObjectWindow?.SelectedGameObject;
            if (showOnSelection)
            {
                SceneView.lastActiveSceneView.FrameSelected();
            }
        }

        protected override void OnDisable()
        {
            MenuTree.Selection.SelectionChanged -= OnSelectionChanged;
            base.OnDisable();
        }
    }
}
