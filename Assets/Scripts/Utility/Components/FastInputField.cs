using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ArcCreate.Utility
{
    /// <summary>
    /// Fast input field that can handle very large text (up in the scale of hundreds of kilobytes).
    /// Does not support character validation.
    /// Also very scuffed. Use at your own risk.
    /// </summary>
    public class FastInputField : InputField, IScrollHandler
    {
        [SerializeField] private Scrollbar horizontalScrollbar;
        [SerializeField] private Scrollbar verticalScrollbar;
        [SerializeField] private float marginLeft = 10;
        [SerializeField] private float marginRight = 10;

        private RectTransform textRect;
        private RectTransform rect;
        private FieldInfo allowInputProperty;
        private int prevDrawStart;
        private int prevDrawEnd;
        private Vector2 contentSize;
        private Vector2 previousSize;
        private bool textHasChanged;

        public event Action<float> OnScrollVerticalChanged;

        public event Action<float> OnScrollHorizontalChanged;

        public TextGenerator TextGenerator => cachedInputTextGenerator;

        private RectTransform Rect
        {
            get
            {
                if (rect == null)
                {
                    rect = GetComponent<RectTransform>();
                }

                return rect;
            }
        }

        private RectTransform TextRect
        {
            get
            {
                if (textRect == null)
                {
                    textRect = textComponent.GetComponent<RectTransform>();
                }

                return textRect;
            }
        }

        public void OnScroll(PointerEventData eventData)
        {
            Vector2 direction = eventData.scrollDelta;
            if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y) && Mathf.Abs(direction.x) > 0.1f)
            {
                float dx = Settings.ScrollSensitivityHorizontal.Value * direction.x;
                OnScrollHorizontal(dx);
            }
            else if (Mathf.Abs(direction.y) > 0.1f)
            {
                float dy = Settings.ScrollSensitivityVertical.Value * direction.y;
                OnScrollVertical(dy);
            }
        }

        public int GetStartLine()
        {
            return DetermineCharacterLine(m_DrawStart, cachedInputTextGenerator);
        }

        public void SetStartLine(int line)
        {
            int startLine = DetermineCharacterLine(m_DrawStart, cachedInputTextGenerator);
            int endLine = DetermineCharacterLine(m_DrawEnd, cachedInputTextGenerator);
            int dir = line - startLine;
            startLine += dir;
            endLine += dir;
            if (startLine < 0 || endLine < 0 || startLine >= cachedInputTextGenerator.lineCount || endLine >= cachedInputTextGenerator.lineCount)
            {
                return;
            }

            m_DrawStart = GetLineStartPosition(cachedInputTextGenerator, startLine);
            m_DrawEnd = GetLineEndPosition(cachedInputTextGenerator, endLine);
            m_DrawStart = Mathf.Clamp(m_DrawStart, 0, text.Length - 1);
            m_DrawEnd = Mathf.Clamp(m_DrawEnd, m_DrawStart, text.Length);
            ForceUnityToNotBeStupid();
        }

        public override void OnSelect(BaseEventData eventData)
        {
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            allowInputProperty = allowInputProperty ?? GetType().BaseType.GetField("m_AllowInput", BindingFlags.Instance | BindingFlags.NonPublic);
            allowInputProperty.SetValue(this, true);
            base.OnPointerDown(eventData);
        }

        /// <summary>
        /// Append the specified text to the end of the current text string. Appends character by character testing validation criteria i hate unity.
        /// </summary>
        /// <param name="input">The String to append.</param>
        protected override void Append(string input)
        {
            if (!InPlaceEditing())
            {
                return;
            }

            int startIndex = 0;
            int i = 0;
            for (int imax = input.Length; i < imax; ++i)
            {
                char c = input[i];

                bool validChar = (c >= ' ' || c == '\t' || c == '\r' || c == 10 || c == '\n') && !char.IsSurrogate(c);
                if (!validChar)
                {
                    int length = i - startIndex;
                    if (length > 0)
                    {
                        Insert(input.Substring(startIndex, length));
                    }

                    startIndex = i + 1;
                }
            }

            Insert(input.Substring(startIndex, i - startIndex));
        }

        protected override void Append(char input)
        {
            // We do not currently support surrogate pairs i hate unity
            if (char.IsSurrogate(input))
            {
                return;
            }

            if (!InPlaceEditing())
            {
                return;
            }

            // Append the character and update the label i hate unity
            Insert(input.ToString());
        }

        protected override void Awake()
        {
            horizontalScrollbar.onValueChanged.AddListener(OnHorizontalScrollbar);
            verticalScrollbar.onValueChanged.AddListener(OnVerticalScrollbar);
            onValueChanged.AddListener(OnTextChange);
            rect = GetComponent<RectTransform>();
            textRect = textComponent.GetComponent<RectTransform>();
        }

        protected override void OnDestroy()
        {
            horizontalScrollbar.onValueChanged.RemoveListener(OnHorizontalScrollbar);
            verticalScrollbar.onValueChanged.RemoveListener(OnVerticalScrollbar);
            onValueChanged.RemoveListener(OnTextChange);
        }

        protected override void LateUpdate()
        {
            textComponent.enabled = Rect.rect.size == previousSize;
            if (Rect.rect.size != previousSize)
            {
                UpdateScrollbarSizes();
            }

            previousSize = Rect.rect.size;

            if (m_DrawStart != prevDrawStart || m_DrawEnd != prevDrawEnd)
            {
                prevDrawStart = m_DrawStart;
                prevDrawEnd = m_DrawEnd;
                UpdateScrollbarVerticalPosition();
            }

            if (textHasChanged)
            {
                UpdateContentSizes();
                UpdateScrollbarSizes();
                textHasChanged = false;
            }
        }

        private void OnTextChange(string txt)
        {
            textHasChanged = true;
        }

        private void OnHorizontalScrollbar(float val)
        {
            float width = contentSize.x + marginLeft + marginRight;
            float x = val * (width - Rect.rect.size.x);
            TextRect.anchoredPosition = new Vector2(-x + marginLeft, TextRect.anchoredPosition.y);
            OnScrollHorizontalChanged.Invoke(-x + marginLeft);
        }

        private void OnVerticalScrollbar(float val)
        {
            TextGenerator gen = cachedInputTextGenerator;
            float y = val * (contentSize.y - gen.rectExtents.size.y);
            if (gen.lineCount <= 0)
            {
                return;
            }

            if (val == 0)
            {
                SetStartLine(0);
                return;
            }

            UILineInfo firstLine = gen.lines[0];
            for (int i = 0; i < gen.lineCount; i++)
            {
                UILineInfo line = gen.lines[i];
                float ySoFar = firstLine.topY - line.topY + line.height;

                if (ySoFar >= y)
                {
                    SetStartLine(i + 1);
                    return;
                }
            }
        }

        private void OnScrollHorizontal(float dx)
        {
            float width = contentSize.x - Rect.rect.size.x + marginLeft + marginRight;
            float newx = TextRect.anchoredPosition.x + dx;
            newx = Mathf.Clamp(newx, -width, 0);
            TextRect.anchoredPosition = new Vector2(newx + marginLeft, TextRect.anchoredPosition.y);
            OnScrollHorizontalChanged.Invoke(newx + marginLeft);
            horizontalScrollbar.SetValueWithoutNotify(-newx / width);
        }

        private void OnScrollVertical(float dy)
        {
            int startLine = DetermineCharacterLine(m_DrawStart, cachedInputTextGenerator);
            int endLine = DetermineCharacterLine(m_DrawEnd, cachedInputTextGenerator);
            int dir = -(int)Mathf.Sign(dy);
            startLine += dir;
            endLine += dir;
            if (startLine < 0 || endLine < 0 || startLine >= cachedInputTextGenerator.lineCount || endLine >= cachedInputTextGenerator.lineCount)
            {
                return;
            }

            m_DrawStart = GetLineStartPosition(cachedInputTextGenerator, startLine);
            m_DrawEnd = GetLineEndPosition(cachedInputTextGenerator, endLine);
            m_DrawStart = Mathf.Clamp(m_DrawStart, 0, text.Length - 1);
            m_DrawEnd = Mathf.Clamp(m_DrawEnd, m_DrawStart, text.Length);
            ForceUnityToNotBeStupid();
        }

        private void UpdateScrollbarSizes()
        {
            float width = contentSize.x;
            float height = contentSize.y;
            horizontalScrollbar.size = Mathf.Clamp(Rect.rect.size.x / width, 0, 1);
            verticalScrollbar.size = Mathf.Clamp(Rect.rect.size.y / height, 0, 1);
        }

        private void UpdateContentSizes()
        {
            TextGenerator gen = cachedInputTextGenerator;
            if (gen.lineCount <= 0)
            {
                return;
            }

            UILineInfo firstLine = gen.lines[0];
            UILineInfo lastLine = gen.lines[gen.lineCount - 1];
            float height = firstLine.topY - lastLine.topY + lastLine.height;
            float min = float.MaxValue;
            float max = float.MinValue;
            foreach (var c in gen.characters)
            {
                max = Mathf.Max(max, c.cursorPos.x + (c.charWidth / 2));
                min = Mathf.Min(min, c.cursorPos.x - (c.charWidth / 2));
            }

            float width = max - min;
            contentSize = new Vector2(width, height);
        }

        private void UpdateScrollbarVerticalPosition()
        {
            int line = GetStartLine();
            TextGenerator gen = cachedInputTextGenerator;
            UILineInfo firstLine = gen.lines[0];
            UILineInfo startLine = gen.lines[line];
            float y = firstLine.topY - startLine.topY;
            float val = y / (contentSize.y - gen.rectExtents.size.y);
            OnScrollVerticalChanged.Invoke(y);
            verticalScrollbar.SetValueWithoutNotify(val);
        }

        // Insert the character and update the label i hate unity
        private void Insert(string c)
        {
            string replaceString = c.ToString();
            Delete();

            // Can't go past the character limit i hate unity
            if (characterLimit > 0 && text.Length >= characterLimit)
            {
                return;
            }

            m_Text = text.Insert(m_CaretPosition, replaceString);
            caretSelectPositionInternal = caretPositionInternal += replaceString.Length;

            UpdateTouchKeyboardFromEditChanges();
            SendOnValueChanged();
        }

        private void Delete()
        {
            if (caretPositionInternal == caretSelectPositionInternal)
            {
                return;
            }

            if (caretPositionInternal < caretSelectPositionInternal)
            {
                m_Text = text.Substring(0, caretPositionInternal) + text.Substring(caretSelectPositionInternal, text.Length - caretSelectPositionInternal);
                caretSelectPositionInternal = caretPositionInternal;
            }
            else
            {
                m_Text = text.Substring(0, caretSelectPositionInternal) + text.Substring(caretPositionInternal, text.Length - caretPositionInternal);
                caretPositionInternal = caretSelectPositionInternal;
            }
        }

        private void UpdateTouchKeyboardFromEditChanges()
        {
            // Update the TouchKeyboard's text from edit changes i hate unity
            // if in-place editing is allowed i hate unity
            if (m_Keyboard != null && InPlaceEditing())
            {
                m_Keyboard.text = m_Text;
            }
        }

        private void SendOnValueChanged()
        {
            UISystemProfilerApi.AddMarker("InputField.value", this);
            onValueChanged?.Invoke(text);
        }

        private bool InPlaceEditing()
        {
            return !TouchScreenKeyboard.isSupported;
        }

        private int DetermineCharacterLine(int charPos, TextGenerator generator)
        {
            for (int i = 0; i < generator.lineCount - 1; ++i)
            {
                if (generator.lines[i + 1].startCharIdx > charPos)
                {
                    return i;
                }
            }

            return generator.lineCount - 1;
        }

        private int GetLineStartPosition(TextGenerator gen, int line)
        {
            line = Mathf.Clamp(line, 0, gen.lines.Count - 1);
            return gen.lines[line].startCharIdx;
        }

        private int GetLineEndPosition(TextGenerator gen, int line)
        {
            line = Mathf.Max(line, 0);
            if (line + 1 < gen.lines.Count)
            {
                return gen.lines[line + 1].startCharIdx - 1;
            }

            return gen.characterCountVisible;
        }

        private void ForceUnityToNotBeStupid()
        {
            string processed = text.Substring(m_DrawStart, Mathf.Min(m_DrawEnd, text.Length) - m_DrawStart);

            m_TextComponent.UnregisterDirtyVerticesCallback(UpdateLabel);
            m_TextComponent.UnregisterDirtyLayoutCallback(UpdateLabel);
            m_TextComponent.text = processed;
            m_TextComponent.RegisterDirtyVerticesCallback(UpdateLabel);
            m_TextComponent.RegisterDirtyLayoutCallback(UpdateLabel);

#if UNITY_EDITOR
            if (!Application.isPlaying || UnityEditor.PrefabUtility.IsPartOfPrefabAsset(gameObject))
            {
                return;
            }
#endif

            CanvasUpdateRegistry.RegisterCanvasElementForGraphicRebuild(this);
        }
    }

#if UNITY_EDITOR
#pragma warning disable
    [CustomEditor(typeof(FastInputField))]
    public class FastInputFieldEditor : Editor
#pragma warning restore
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
        }
    }
#endif
}