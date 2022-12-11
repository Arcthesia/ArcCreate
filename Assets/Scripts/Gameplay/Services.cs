using ArcCreate.Gameplay.Audio;
using ArcCreate.Gameplay.Chart;
using ArcCreate.Gameplay.GameplayCamera;
using ArcCreate.Gameplay.InputFeedback;
using ArcCreate.Gameplay.Judgement;
using ArcCreate.Gameplay.Particle;
using ArcCreate.Gameplay.Scenecontrol;
using ArcCreate.Gameplay.Score;
using ArcCreate.Gameplay.Skin;
using UnityEngine;

namespace ArcCreate.Gameplay
{
    public class Services : MonoBehaviour
    {
        [SerializeField] private SkinService skin;
        [SerializeField] private new AudioService audio;
        [SerializeField] private new CameraService camera;
        [SerializeField] private ChartService chart;
        [SerializeField] private ParticleService particle;
        [SerializeField] private JudgementService judgement;
        [SerializeField] private InputFeedbackService inputFeedback;
        [SerializeField] private ScoreService score;
        [SerializeField] private ScenecontrolService scenecontrol;

        public static ISkinService Skin { get; private set; }

        public static IChartService Chart { get; private set; }

        public static ICameraService Camera { get; private set; }

        public static IAudioService Audio { get; private set; }

        public static IParticleService Particle { get; private set; }

        public static IJudgementService Judgement { get; private set; }

        public static IInputFeedbackService InputFeedback { get; private set; }

        public static IScenecontrolService Scenecontrol { get; private set; }

        public static IScoreService Score { get; private set; }

        private void Awake()
        {
            Skin = skin;
            Chart = chart;
            Particle = particle;
            Judgement = judgement;
            Audio = audio;
            Score = score;
            InputFeedback = inputFeedback;
            Scenecontrol = scenecontrol;
            Camera = camera;
        }
    }
}