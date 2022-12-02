using System.Collections.Generic;
using UnityEngine;

namespace ArcCreate.Gameplay.InputFeedback
{
    public class InputFeedbackService : MonoBehaviour, IInputFeedbackService
    {
        [SerializeField] private GameObject floatLinePrefab;
        [SerializeField] private GameObject laneHitPrefab;
        [SerializeField] private Transform laneHitParent;
        [SerializeField] private Transform floatLineParent;
        [SerializeField] private Transform skyInput;
        [SerializeField] private int floatLinePoolCount = 4;

        private Pool<Transform> floatLinePool;
        private readonly Dictionary<int, SpriteRenderer> laneFeedbacks = new Dictionary<int, SpriteRenderer>();

        public void UpdateInputFeedback()
        {
            float alphaDecrease = Values.LaneFeedbackMaxAlpha * (Time.deltaTime / Values.LaneFeedbackFadeoutDuration);
            foreach (var laneFeedback in laneFeedbacks.Values)
            {
                laneFeedback.color = new Color(1, 1, 1, Mathf.Max(laneFeedback.color.a - alphaDecrease, 0));
            }

            floatLinePool.ReturnAll();
        }

        public void LaneFeedback(int lane)
        {
            if (lane >= Values.LaneFrom || lane <= Values.LaneTo)
            {
                SpriteRenderer laneFeedback = GetLaneFeedback(lane);
                laneFeedback.color = new Color(1, 1, 1, Values.LaneFeedbackMaxAlpha);
                laneFeedback.transform.localPosition = new Vector3(ArcFormula.LaneToWorldX(lane), 0, 0);
            }
        }

        public void FloatlineFeedback(float y)
        {
            if (y >= Values.MinVerticalFeedbackY)
            {
                Transform verticalFeedback = floatLinePool.Get();
                y = Mathf.Min(y, skyInput.position.y);
                verticalFeedback.localPosition = new Vector3(0, y, 0);
            }
        }

        private SpriteRenderer GetLaneFeedback(int lane)
        {
            if (!laneFeedbacks.ContainsKey(lane))
            {
                GameObject go = Instantiate(laneHitPrefab, laneHitParent);
                SpriteRenderer sprite = go.GetComponent<SpriteRenderer>();
                laneFeedbacks.Add(lane, sprite);
                return sprite;
            }

            return laneFeedbacks[lane];
        }

        private void Awake()
        {
            Pools.New<Transform>("FloatLineInputFeedback", floatLinePrefab, floatLineParent, floatLinePoolCount);
            floatLinePool = Pools.Get<Transform>("FloatLineInputFeedback");
        }

        private void OnDestroy()
        {
            Pools.Destroy<Transform>("FloatLineInputFeedback");
        }
    }
}