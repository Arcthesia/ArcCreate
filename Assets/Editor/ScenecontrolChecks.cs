using System;
using System.IO;
using System.Linq;
using ArcCreate.Gameplay.Scenecontrol;
using ArcCreate.Utility.Animation;
using DG.DOTweenEditor;
using DG.Tweening;
using UnityEditor;
using UnityEngine;

namespace ArcCreate.EditorScripts
{
    public static class ScenecontrolChecks
    {
        public const string IOFolder = "Assets/Scripts/Gameplay/Scenecontrol/IO";
        public const string DeserializationFile = IOFolder + "/ScenecontrolDeserialization.cs";
        public const string SerializationFile = IOFolder + "/ScenecontrolSerialization.cs";

        [InitializeOnLoadMethod]
        public static void CheckScenecontrol()
        {
            var allChannelTypes = TypeCache.GetTypesDerivedFrom<IChannel>();

            var deser = File.ReadAllText(DeserializationFile);
            var ser = File.ReadAllText(SerializationFile);

            foreach (var chanType in allChannelTypes.Where(t => !Attribute.IsDefined(t, typeof(SerializationExemptAttribute))))
            {
                var chan = chanType.Name;

                if (!deser.Contains(chan))
                {
                    Debug.LogError($"Channel type {chanType} is missing deserialization code! Please add it!");
                }

                if (!ser.Contains(chan))
                {
                    Debug.LogError($"Channel type {chanType} is missing serialization code! Please add it!");
                }
            }
        }
    }
}