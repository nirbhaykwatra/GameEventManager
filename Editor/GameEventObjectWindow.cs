using System;
using System.Reflection;
using GameEvents;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GameEventManager
{
    public class GameEventObjectWindow
    {
        private GameObject activeGameObject;
        
        [ShowInInspector, PropertyOrder(-1)]
        
        private string GameObjectName => activeGameObject.name;
        
        [ShowInInspector]
        [ReadOnly]
        private string ComponentType;
        
        [ShowInInspector]
        [ReadOnly]
        private object EventAsset = null;
        
        private Component eventComponent;

        public GameObject SelectedGameObject => activeGameObject;
        
        public GameEventObjectWindow(GameObject gameObject, Type type)
        {
            activeGameObject = gameObject;
            ComponentType = type.Name;
            eventComponent = activeGameObject.GetComponent(type);

            PropertyInfo propertyInfo = eventComponent.GetType().GetProperty("EventAsset");
            EventAsset = propertyInfo?.GetValue(eventComponent);
        }
        
        [Button(ButtonSizes.Gigantic, ButtonStyle.Box), PropertySpace(10, 10)]
        public void ShowEventAssetInProject()
        {
            if (EventAsset == null) return;
            EditorGUIUtility.PingObject((Object)EventAsset);
        }
        
        [Button(ButtonSizes.Gigantic, ButtonStyle.Box), PropertySpace(5, 10)]
        public void ShowEventObjectInScene()
        {
            if (EventAsset == null) return;
            Selection.activeObject = activeGameObject;
            SceneView.lastActiveSceneView.FrameSelected();
        }
    }
}
