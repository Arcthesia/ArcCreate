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
        [SerializeField] private float timingFieldRatio = 0.2f;
        [SerializeField] private float maxNumVisibleFields = 4;
        private readonly List<TMP_Text> nameTexts = new List<TMP_Text>();

        public void SetupFields(string[] names)
        {
            foreach (var txt in nameTexts)
            {
                Destroy(txt.gameObject);
            }

            nameTexts.Clear();

            RectTransform rect = timingName.GetComponent<RectTransform>();
            float timingFieldWidth = names.Length == 0 ? 1 : timingFieldRatio;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = new Vector2(timingFieldWidth, 1);

            if (names.Length > 0)
            {
                float fieldWidth = Mathf.Max((1 - timingFieldRatio) / maxNumVisibleFields, (1 - timingFieldRatio) / names.Length);
                int i = 0;
                foreach (string name in names)
                {
                    GameObject go = Instantiate(namePrefab, nameTransform);
                    RectTransform r = go.GetComponent<RectTransform>();
                    r.anchorMin = new Vector2(fieldWidth * i, 0);
                    r.anchorMax = new Vector2(fieldWidth * (i + 1), 1);

                    TMP_Text txt = go.GetComponent<TMP_Text>();
                    txt.text = name;
                    nameTexts.Add(txt);
                    i++;
                }
            }
        }

        public void SetFieldOffsetX(float x)
        {
            nameTransform.anchoredPosition = new Vector2(x, nameTransform.anchoredPosition.y);
        }
    }
}