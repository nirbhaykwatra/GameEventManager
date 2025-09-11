using Sirenix.OdinInspector.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Sirenix.Utilities.Editor;
using Sirenix.OdinInspector;
using GameEvents;

namespace GameEventManager
{
    // TODO: Create a second version of this tool based on UI Toolkit instead. Reason: Odin does not yet have full
    //  capability of rendering UI such as GameObject inspector/components.
    
    // TODO: Add feature where we can select a GameEventAsset SO to show all of its callers and listeners in the active scene.
    // TODO: Create tab for GameEventAssets, where we can view the listeners and callers for that particular asset in the active scene.
    public class GameEventEditor : OdinMenuEditorWindow
    {
        private Editor gameObjectEditor;
        private GameObject selectedGameObject;
        private bool showOnSelection;
        
        private static readonly Type[] callerTypes = Enumerable.OrderBy(TypeCache.GetTypesDerivedFrom(typeof(GameEventCaller<>)), m => m.Name).ToArray();
        private static readonly Type[] listenerTypes = Enumerable.OrderBy(TypeCache.GetTypesDerivedFrom(typeof(GameEventListener<>)), m => m.Name).ToArray();
        
        private static List<GameObject> callerObjects = new List<GameObject>();
        private static List<GameObject> listenerObjects = new List<GameObject>();

        [MenuItem("Tools/Game Events/Game Event Manager", priority = - 10)]
        private static void OpenEditor() => GetWindow<GameEventEditor>();

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
            }
            SirenixEditorGUI.EndHorizontalToolbar();
            DetectEventObjects();
        }
        protected override OdinMenuTree BuildMenuTree()
        {
            var tree = new OdinMenuTree();
            tree.Selection.SelectionChanged += OnSelectionChanged;

            for (int x = 0; x < callerTypes.Length; x++)
            {
                Type callerType = callerTypes[x];
                if (callerObjects.Count <= 0) return tree;
                for (int y = 0; y < callerObjects.Count; y++)
                {
                    if (callerObjects[y] == null)
                    {
                        callerObjects.RemoveAt(y);
                        continue;
                    }
                    GameObject callerObject = callerObjects[y];
                    CheckGameObjectType(tree, callerType, callerObject);
                }
            }
            
            for (int x = 0; x < listenerTypes.Length; x++)
            {
                Type listenerType = listenerTypes[x];
                if (listenerObjects.Count <= 0) return tree;
                for (int y = 0; y < listenerObjects.Count; y++)
                {
                    if (listenerObjects[y] == null)
                    {
                        listenerObjects.RemoveAt(y);
                        continue;
                    }
                    GameObject listenerObject = listenerObjects[y];
                    CheckGameObjectType(tree, listenerType, listenerObject);
                }
            }
            
            return tree;
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

        protected void CheckGameObjectType(OdinMenuTree tree, Type type, GameObject gameObject)
        {
            if (gameObject.GetComponent(type) != null)
            {
                OdinMenuItem menuItem = new OdinMenuItem(tree, gameObject.name, gameObject);
                tree.AddMenuItemAtPath(type.Name, menuItem);
            }
        }
        
        protected void OnSelectionChanged(SelectionChangedType selectionChangedType)
        {
            selectedGameObject = MenuTree.Selection.SelectedValue as GameObject;;
                    
            Selection.activeGameObject = selectedGameObject;
            if (showOnSelection)
            {
                selectedGameObject = MenuTree.Selection.SelectedValue as GameObject;;
                    
                Selection.activeGameObject = selectedGameObject;
                    
                SceneView.lastActiveSceneView.FrameSelected();
            }
        }

        protected override void OnDestroy()
        {
            // Clean up when window is closed
            if (gameObjectEditor != null)
            {
                DestroyImmediate(gameObjectEditor);
            }
            base.OnDestroy();
        }
    }
}
