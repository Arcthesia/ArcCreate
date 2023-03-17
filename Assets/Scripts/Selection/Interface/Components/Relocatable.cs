using UnityEngine;

namespace ArcCreate.Selection.Interface
{
    [ExecuteAlways]
    public class Relocatable : MonoBehaviour
    {
        private static readonly Vector3[] Corners = new Vector3[4];
        [SerializeField] private RectTransform rect;
        [SerializeField] private Camera renderingCamera;
        [SerializeField] private Rect[] locations = new Rect[1];
        [SerializeField] private Rect test;
        [SerializeField] private Rect test2;

        private void Awake()
        {
            TestLocation();
        }

#if UNITY_EDITOR
        private void Update()
        {
            if (!Application.isPlaying)
            {
                TestLocation();
            }
        }
#endif

        private void TestLocation()
        {
            foreach (Rect r in locations)
            {
                SetPosition(r);
                if (IsValidPosition())
                {
                    return;
                }
            }

            SetPosition(locations[0]);
        }

        private void SetPosition(Rect r)
        {
            rect.anchoredPosition = new Vector2(r.x, r.y);
            rect.sizeDelta = new Vector2(r.width, r.height);
        }

        private bool IsValidPosition()
        {
            Rect visibleRect = new Rect(0, 0, Screen.width, Screen.height);

            rect.GetWorldCorners(Corners);
            for (var i = 0; i < Corners.Length; i++)
            {
                Corners[i] = renderingCamera.WorldToScreenPoint(Corners[i]);
            }

            foreach (var corner in Corners)
            {
                if (!visibleRect.Contains(corner))
                {
                    return false;
                }
            }

            return true;
        }
    }
}