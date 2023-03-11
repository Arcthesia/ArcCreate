using System.Collections.Generic;
using EmmySharp;
using MoonSharp.Interpreter;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace ArcCreate.Gameplay.Scenecontrol
{
    [MoonSharpUserData]
    [EmmyAlias("PostProcessingController")]
    public abstract class PostProcessingController<T> : MonoBehaviour, IPostProcessing, ISceneController
        where T : PostProcessEffectSettings
    {
        [SerializeField] private PostProcessVolume volume;

        [MoonSharpHidden] private T targetEffect;

        public string SerializedType { get; set; }

        protected T TargetEffect => targetEffect;

        [MoonSharpHidden]
        public void Start()
        {
            volume.profile.TryGetSettings(out targetEffect);
            TargetEffect.enabled.Override(false);
            SetupDefault();
            Reset();
        }

        public abstract void EnableEffect(string[] effects);

        [MoonSharpHidden]
        public void TryUpdate(int timing)
        {
            if (TargetEffect.enabled.overrideState)
            {
                UpdateController(timing);
            }
        }

        [MoonSharpHidden]
        public abstract void UpdateController(int timing);

        [MoonSharpHidden]
        public void CleanController()
        {
            TargetEffect.enabled.Override(false);
            Reset();
        }

        [MoonSharpHidden]
        public abstract List<object> SerializeProperties(ScenecontrolSerialization serialization);

        [MoonSharpHidden]
        public abstract void DeserializeProperties(List<object> properties, ScenecontrolDeserialization deserialization);

        protected abstract void SetupDefault();

        protected abstract void Reset();
    }
}