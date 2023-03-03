using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace ArcCreate.Compose.EventsEditor
{
    public class ParametersRow : MonoBehaviour
    {
        [SerializeField] private RectTransform nameTransform;
        [SerializeField] private GameObject namePrefab;
        [SerializeField] private TMP_Text timingName;
        [SerializeField] private RectTransform timingRect;
        [SerializeField] private float timingFieldRatio = 0.2f;
        private readonly List<GameObject> nameTexts = new List<GameObject>();
        private float scrollableLength;

        public void SetupFields(string[] names, int maxNumVisibleFields)
        {
            foreach (var txt in nameTexts)
            {
                Destroy(txt);
            }

            nameTexts.Clear();

            float timingFieldWidth = names.Length == 0 ? 1 : timingFieldRatio;
            timingRect.anchorMin = Vector2.zero;
            timingRect.anchorMax = new Vector2(timingFieldWidth, 1);
            timingRect.offsetMin = Vector2.zero;
            timingRect.offsetMax = Vector2.zero;

            if (names.Length > 0)
            {
                float fieldWidth = Mathf.Max((1 - timingFieldRatio) / maxNumVisibleFields, (1 - timingFieldRatio) / names.Length);
                scrollableLength = (fieldWidth * names.Length) - 1 + timingFieldWidth;
                int i = 0;
                foreach (string name in names)
                {
                    GameObject go = Instantiate(namePrefab, nameTransform);
                    RectTransform r = go.GetComponent<RectTransform>();
                    r.anchorMin = new Vector2(timingFieldRatio + (fieldWidth * i), 0);
                    r.anchorMax = new Vector2(timingFieldRatio + (fieldWidth * (i + 1)), 1);
                    r.offsetMin = Vector2.zero;
                    r.offsetMax = Vector2.zero;

                    TMP_Text txt = go.GetComponentInChildren<TMP_Text>();
                    txt.text = name;
                    nameTexts.Add(go);
                    i++;
                }
            }
        }

        public void SetFieldOffsetX(float x)
        {
            nameTransform.anchorMin = new Vector2(-x * scrollableLength, 0);
            nameTransform.anchorMax = new Vector2(1 - (x * scrollableLength), 1);
        }
    }
}