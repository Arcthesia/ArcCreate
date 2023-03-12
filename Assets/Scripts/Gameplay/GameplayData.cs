using System;
using System.Collections.Generic;
using System.IO;
using ArcCreate.ChartFormat;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace ArcCreate.Gameplay
{
    /// <summary>
    /// Scriptable object acting as data channel for scenes linking to gameplay scene.
    /// </summary>
    [CreateAssetMenu(fileName = "GameplayData", menuName = "ScriptableObject/GameplayData")]
    public class GameplayData : ScriptableObject
    {
        [SerializeField] private Sprite defaultJacket;
        private bool isUsingDefaultBackground = true;
        private bool isUsingDefaultJacket = true;

        public event Action OnChartFileLoad;

        public event Action OnSkinValuesChange;

        public event Action OnChartTimingEdit;

        public event Action OnChartCameraEdit;

        public event Action OnChartScenecontrolEdit;

        public event Action OnChartEdit;

        public event Action<int> OnGameplayUpdate;

#pragma warning disable
        /// <summary>
        /// The background sprite.
        /// </summary>
        public State<Sprite> Background { get; } = new State<Sprite>();

        /// <summary>
        /// The jacket art sprite.
        /// </summary>
        public State<Sprite> Jacket { get; } = new State<Sprite>();

        /// <summary>
        /// The song's title.
        /// </summary>
        public State<string> Title { get; } = new State<string>();

        /// <summary>
        /// The composer's name.
        /// </summary>
        public State<string> Composer { get; } = new State<string>();

        /// <summary>
        /// The illustrator's name.
        /// </summary>
        public State<string> Illustrator { get; } = new State<string>();

        /// <summary>
        /// The charter's name.
        /// </summary>
        public State<string> Charter { get; } = new State<string>();

        /// <summary>
        /// The text of the difficulty display.
        /// </summary>
        public State<string> DifficultyName { get; } = new State<string>();

        /// <summary>
        /// The color of the difficulty text's background image.
        /// </summary>
        public State<Color> DifficultyColor { get; } = new State<Color>();

        /// <summary>
        /// The audio clip to play.
        /// </summary>
        public State<AudioClip> AudioClip { get; } = new State<AudioClip>();

        /// <summary>
        /// The audio offset value per chart.
        /// Use <see cref="Settings.GlobalAudioOffset"/> for global audio offset.
        /// Setting this value will cause a score reset.
        /// </summary>
        public State<int> AudioOffset { get; } = new State<int>();

        /// <summary>
        /// The base bpm value.
        /// Setting this value will cause a score reset.
        /// </summary>
        public State<float> BaseBpm { get; } = new State<float>();

        /// <summary>
        /// The timing point density factor value.
        /// Setting this value will cause a score reset.
        /// </summary>
        public State<float> TimingPointDensityFactor { get; } = new State<float>();

        /// <summary>
        /// The url to be played by video background renderer.
        /// Setting it to null or empty string will disable the renderer.
        /// </summary>
        public State<string> VideoBackgroundUrl { get; } = new State<string>();
#pragma warning restore

        /// <summary>
        /// Load the audio clip from the specified path.
        /// </summary>
        /// <param name="path">The path to load.</param>
        public void LoadAudio(string path)
        {
            if (AudioClip.Value != null)
            {
                Destroy(AudioClip.Value);
            }

            StartLoadingAudio(path).Forget();
        }

        /// <summary>
        /// Load the background from specified file path.
        /// </summary>
        /// <param name="path">The path to load.</param>
        public void LoadBackground(string path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                Background.Value = Services.Skin.DefaultBackground;
                isUsingDefaultBackground = true;
                return;
            }

            if (Background.Value != null && !isUsingDefaultBackground)
            {
                Destroy(Background.Value.texture);
                Destroy(Background.Value);
            }

            Texture2D t = new Texture2D(1, 1);
            t.LoadImage(File.ReadAllBytes(path), true);
            Sprite sprite = Sprite.Create(t, new Rect(0, 0, t.width, t.height), new Vector2(0.5f, 0.5f));
            Background.Value = sprite;
            isUsingDefaultBackground = false;
        }

        /// <summary>
        /// Load the jacket art from specified file path.
        /// </summary>
        /// <param name="path">The path to load.</param>
        public void LoadJacket(string path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                Jacket.Value = defaultJacket;
                isUsingDefaultJacket = true;
                return;
            }

            if (Jacket.Value != null && !isUsingDefaultJacket)
            {
                Destroy(Jacket.Value.texture);
                Destroy(Jacket.Value);
            }

            Texture2D t = new Texture2D(1, 1);
            t.LoadImage(File.ReadAllBytes(path), true);
            Sprite sprite = Sprite.Create(t, new Rect(0, 0, t.width, t.height), new Vector2(0.5f, 0.5f));
            Jacket.Value = sprite;
            isUsingDefaultJacket = false;
        }

        /// <summary>
        /// Set the chart file for this system.
        /// </summary>
        /// <param name="reader">The chart reader defining the chart.</param>
        /// <param name="sfxParentUri">The parent uri for loading custom SFX files.</param>
        public void LoadChart(ChartReader reader, string sfxParentUri)
        {
            Services.Chart.LoadChart(reader);
            Services.Hitsound.LoadCustomSfxs(sfxParentUri).Forget();
            OnChartFileLoad?.Invoke();
        }

        public void SetDefaultJacket()
        {
            Jacket.Value = defaultJacket;
            isUsingDefaultJacket = true;
        }

        public void SetDefaultBackground()
        {
            Background.Value = Services.Skin.DefaultBackground;
            isUsingDefaultBackground = true;
        }

        public async UniTask LoadAudioFromHttp(string uri, string ext)
        {
            if (AudioClip.Value != null)
            {
                Destroy(AudioClip.Value);
            }

            using (UnityWebRequest req = UnityWebRequestMultimedia.GetAudioClip(
                uri,
                ext == ".ogg" ? AudioType.OGGVORBIS : AudioType.WAV))
            {
                await req.SendWebRequest();
                if (!string.IsNullOrWhiteSpace(req.error))
                {
                    throw new IOException(I18n.S("Gameplay.Exception.LoadAudio", new Dictionary<string, object>()
                    {
                        { "Path", uri },
                        { "Error", req.error },
                    }));
                }

                AudioClip.Value = DownloadHandlerAudioClip.GetContent(req);
            }
        }

        public async UniTask LoadJacketFromHttp(string uri)
        {
            if (Jacket.Value != null && !isUsingDefaultJacket)
            {
                Destroy(Jacket.Value.texture);
                Destroy(Jacket.Value);
            }

            using (UnityWebRequest req = UnityWebRequestTexture.GetTexture(uri))
            {
                await req.SendWebRequest();
                if (!string.IsNullOrWhiteSpace(req.error))
                {
                    Jacket.Value = defaultJacket;
                    isUsingDefaultJacket = true;

                    Debug.LogWarning(I18n.S("Gameplay.Exception.Skin", new Dictionary<string, object>()
                    {
                        { "Path", uri },
                        { "Error", req.error },
                    }));
                    return;
                }

                Texture2D t = DownloadHandlerTexture.GetContent(req);
                Sprite sprite = Sprite.Create(t, new Rect(0, 0, t.width, t.height), new Vector2(0.5f, 0.5f));
                Jacket.Value = sprite;
                isUsingDefaultJacket = false;
            }
        }

        public async UniTask LoadBackgroundFromHttp(string uri)
        {
            if (Background.Value != null && !isUsingDefaultBackground)
            {
                Destroy(Background.Value.texture);
                Destroy(Background.Value);
            }

            using (UnityWebRequest req = UnityWebRequestTexture.GetTexture(uri))
            {
                await req.SendWebRequest();
                if (!string.IsNullOrWhiteSpace(req.error))
                {
                    Background.Value = Services.Skin.DefaultBackground;
                    isUsingDefaultBackground = true;

                    Debug.LogWarning(I18n.S("Gameplay.Exception.Skin", new Dictionary<string, object>()
                    {
                        { "Path", uri },
                        { "Error", req.error },
                    }));
                    return;
                }

                Texture2D t = DownloadHandlerTexture.GetContent(req);
                Sprite sprite = Sprite.Create(t, new Rect(0, 0, t.width, t.height), new Vector2(0.5f, 0.5f));
                Background.Value = sprite;
                isUsingDefaultBackground = false;
            }
        }

        internal async UniTask StartLoadingAudio(string path)
        {
            using (UnityWebRequest req = UnityWebRequestMultimedia.GetAudioClip(
                Uri.EscapeUriString("file:///" + path.Replace("\\", "/")),
                path.EndsWith("wav") ? AudioType.WAV : AudioType.OGGVORBIS))
            {
                await req.SendWebRequest();
                if (!string.IsNullOrWhiteSpace(req.error))
                {
                    throw new IOException(I18n.S("Gameplay.Exception.LoadAudio", new Dictionary<string, object>()
                    {
                        { "Path", path },
                        { "Error", req.error },
                    }));
                }

                AudioClip.Value = DownloadHandlerAudioClip.GetContent(req);
            }
        }

        internal void NotifySkinValuesChange()
        {
            if (isUsingDefaultBackground)
            {
                SetDefaultBackground();
            }

            OnSkinValuesChange?.Invoke();
        }

        internal void NotifyChartTimingEdit()
        {
            OnChartTimingEdit?.Invoke();
        }

        internal void NotifyChartCameraEdit()
        {
            OnChartCameraEdit?.Invoke();
        }

        internal void NotifyChartScenecontrolEdit()
        {
            OnChartScenecontrolEdit?.Invoke();
        }

        internal void NotifyChartEdit()
        {
            OnChartEdit?.Invoke();
        }

        internal void NotifyUpdate(int currentTiming)
        {
            OnGameplayUpdate?.Invoke(currentTiming);
        }
    }
}