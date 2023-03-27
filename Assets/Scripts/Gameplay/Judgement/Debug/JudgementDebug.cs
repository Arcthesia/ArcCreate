using System.Collections.Generic;
using System.Text;
using ArcCreate.Gameplay.Judgement.Input;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace ArcCreate.Gameplay.Judgement
{
    public class JudgementDebug : MonoBehaviour, IJudgementDebug
    {
        private const string Assigned = "Assigned";
        private const string Miss = "Miss";
        private const string Hit = "Hit";

        [SerializeField] private GameObject graceObject;
        [SerializeField] private Slider graceIndicator;
        [SerializeField] private GameObject[] lockedObject;
        [SerializeField] private Slider[] lockIndicator;
        [SerializeField] private GameObject[] arcExistsIndicator;
        [SerializeField] private RectTransform[] lineOrigins;
        [SerializeField] private TMP_Text[] assignedIndicator;
        [SerializeField] private Camera uiCamera;
        [SerializeField] private TMP_Text touchesInfo;

        [Header("Color")]
        [SerializeField] private Color assignedFingerColor;
        [SerializeField] private Color missColor;
        [SerializeField] private Color hitColor;

        private List<TouchInput> touches;
        private readonly Dictionary<int, GUILine> lineBuffer = new Dictionary<int, GUILine>();
        private Texture2D lineTex;

        public void SetTouchState(List<TouchInput> touches)
        {
            this.touches = touches;

            var touchArray = Touch.activeTouches;
            StringBuilder sb = new StringBuilder();
            sb.Append($"Touches: {touchArray.Count}");

            for (int i = 0; i < touchArray.Count; i++)
            {
                Touch touch = touchArray[i];
                sb.Append($"\n- {i}({touch.touchId}): {touch.phase}");
                if (touch.valid)
                {
                    sb.Append(" valid");
                }

                if (touch.inProgress)
                {
                    sb.Append(" inProgress");
                }

                if (touch.isInProgress)
                {
                    sb.Append(" isInProgress");
                }
            }

            touchesInfo.text = sb.ToString();
        }

        public void ShowInputLock(int color, float v)
        {
            if (color < 0 || color >= lockIndicator.Length)
            {
                return;
            }

            lockedObject[color].SetActive(v > Mathf.Epsilon);
            lockIndicator[color].value = v;
        }

        public void ShowGrace(float v)
        {
            graceIndicator.value = v;
            graceObject.SetActive(v > Mathf.Epsilon);
        }

        public void ShowAssignedFinger(int color, int fingerId)
        {
            if (color >= 0 && color < lineOrigins.Length
             && TryGetFingerPosition(fingerId, out Vector2 pos))
            {
                Vector2 origin = GetScreenPos(lineOrigins[color]);
                AddLine(color, origin, pos, assignedFingerColor, Assigned);
            }

            assignedIndicator[color].gameObject.SetActive(fingerId != ArcColorLogic.UnassignedFingerId);
            assignedIndicator[color].text = fingerId.ToString();
        }

        public void ShowFingerMiss(int color, int fingerId)
        {
            if (color >= 0 && color < lineOrigins.Length
             && TryGetFingerPosition(fingerId, out Vector2 pos))
            {
                Vector2 origin = GetScreenPos(lineOrigins[color]);
                AddLine(color + lineOrigins.Length, origin, pos, missColor, Miss);
            }
        }

        public void ShowExistsArc(int color, bool exists)
        {
            if (color >= 0 && color < arcExistsIndicator.Length)
            {
                arcExistsIndicator[color].SetActive(exists);
            }
        }

        public void ShowFingerHit(int color, int fingerId)
        {
            if (color >= 0 && color < lineOrigins.Length
             && TryGetFingerPosition(fingerId, out Vector2 pos))
            {
                Vector2 origin = GetScreenPos(lineOrigins[color]);
                AddLine(color + (lineOrigins.Length * 2), origin, pos, hitColor, Hit);
            }
        }

        private bool TryGetFingerPosition(int fingerId, out Vector2 result)
        {
            result = default;

            if (touches == null)
            {
                return false;
            }

            foreach (TouchInput touch in touches)
            {
                if (touch.Id == fingerId)
                {
                    result = touch.ScreenPos;
                    return true;
                }
            }

            return false;
        }

        private Vector2 GetScreenPos(RectTransform rect)
        {
            return RectTransformUtility.WorldToScreenPoint(uiCamera, rect.position);
        }

        private void DrawLine(Vector2 from, Vector2 to, Color color, string text)
        {
            from.y = Screen.height - from.y;
            to.y = Screen.height - to.y;

            GUIStyle style = new GUIStyle
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 22,
                normal = new GUIStyleState()
                {
                    textColor = color,
                },
            };

            Vector2 texpos = (from + to) / 2;
            Vector2 texsize = style.CalcSize(new GUIContent(text));
            GUI.Label(new Rect(texpos - (texsize / 2), texsize), text, style);

            Matrix4x4 matrixBackup = GUI.matrix;
            GUI.color = color;

            float width = 2.0f;
            float angle = Mathf.Atan2(to.y - from.y, to.x - from.x) * 180f / Mathf.PI;
            float length = (from - to).magnitude;

            float lengthSubtract1 = Mathf.Abs(texsize.y * length / (to.y - from.y));
            float lengthSubtract2 = Mathf.Abs(texsize.x * length / (to.x - from.x));
            Vector2 lengthSize = new Vector2((length - Mathf.Min(lengthSubtract1, lengthSubtract2)) / 2, width);

            GUIUtility.RotateAroundPivot(angle, from);
            GUI.DrawTexture(new Rect(from, lengthSize), lineTex);
            GUI.DrawTexture(new Rect(from + new Vector2(length - lengthSize.x, 0), lengthSize), lineTex);
            GUI.matrix = matrixBackup;
        }

        private void AddLine(int id, Vector2 from, Vector2 to, Color color, string content)
        {
            var line = new GUILine
            {
                From = from,
                To = to,
                Color = color,
                Content = content,
                Until = Time.realtimeSinceStartup + 0.1,
            };

            if (!lineBuffer.ContainsKey(id))
            {
                lineBuffer.Add(id, line);
            }
            else
            {
                lineBuffer[id] = line;
            }
        }

        private void Awake()
        {
            lineTex = new Texture2D(1, 1);
        }

        private void OnDestroy()
        {
            Destroy(lineTex);
        }

        private void OnGUI()
        {
            foreach (var pair in lineBuffer)
            {
                var line = pair.Value;
                if (Time.realtimeSinceStartup > line.Until)
                {
                    continue;
                }

                DrawLine(line.From, line.To, line.Color, line.Content);
            }

            GUIStyle style = new GUIStyle
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 30,
                normal = new GUIStyleState()
                {
                    textColor = Color.white,
                },
            };

            if (touches != null)
            {
                foreach (var touch in touches)
                {
                    var content = new GUIContent(touch.Id.ToString());
                    Vector2 pos = touch.ScreenPos;
                    pos.y = Screen.height - pos.y - 50;
                    GUI.Label(new Rect(pos, style.CalcSize(content)), content, style);
                }
            }
        }

        private struct GUILine
        {
            public Vector2 From;
            public Vector2 To;
            public Color Color;
            public string Content;
            public double Until;
        }
    }
}