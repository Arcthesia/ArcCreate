using ArcCreate.Gameplay.Chart;
using ArcCreate.Gameplay.Effect;
using ArcCreate.Gameplay.Judgement;
using ArcCreate.Gameplay.Skin;
using UnityEngine;

namespace ArcCreate.Gameplay
{
    public class Services : MonoBehaviour
    {
        [SerializeField] private SkinService skin;
        [SerializeField] private ChartService chart;
        [SerializeField] private EffectService effect;
        [SerializeField] private JudgementService judgement;

        public static ISkinService Skin { get; private set; }

        public static IChartService Chart { get; private set; }

        public static IEffectService Effect { get; private set; }

        public static IJudgementService Judgement { get; private set; }

        private void Awake()
        {
            Skin = skin;
            Chart = chart;
            Effect = effect;
            Judgement = judgement;
        }
    }
}