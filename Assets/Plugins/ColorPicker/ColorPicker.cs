
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using static UnityEngine.Mathf;

[ExecuteInEditMode, RequireComponent(typeof(Image))]
public class ColorPicker : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    private const float recip2Pi = 0.159154943f;
    private const string colorPickerShaderName = "UI/ColorPicker";

    private static readonly int _HSV = Shader.PropertyToID(nameof(_HSV));
    private static readonly int _AspectRatio = Shader.PropertyToID(nameof(_AspectRatio));
    private static readonly int _HueCircleInner = Shader.PropertyToID(nameof(_HueCircleInner));
    private static readonly int _SVSquareSize = Shader.PropertyToID(nameof(_SVSquareSize));

    [SerializeField, HideInInspector] private Shader colorPickerShader;
    private Material generatedMaterial;

    private enum PointerDownLocation { HueCircle, SVSquare, Outside }
    private PointerDownLocation pointerDownLocation = PointerDownLocation.Outside;

    private RectTransform rectTransform;
    private Image image;

    float h, s, v;

    public Color color
    {
        get { return Color.HSVToRGB(h, s, v); }
        set
        {
            Color.RGBToHSV(value, out h, out s, out v);
            ApplyColor();
        }
    }

    public event Action<Color> onColorChanged;

    public void SetColorWithoutNotify(Color c)
    {
        Color.RGBToHSV(c, out h, out s, out v);
        image.material.SetVector(_HSV, new Vector3(h, s, v));
    }

    private void Awake()
    {
        rectTransform = transform as RectTransform;
        image = GetComponent<Image>();

        h = s = v = 0;

        if (WrongShader())
        {
            Debug.LogWarning($"Color picker requires image material with {colorPickerShaderName} shader.");

            if (Application.isPlaying && colorPickerShader != null)
            {
                generatedMaterial = new Material(colorPickerShader);
                generatedMaterial.hideFlags = HideFlags.HideAndDontSave;
            }

            image.material = generatedMaterial;

            return;
        }

        ApplyColor();
    }

    private void Reset()
    {
        colorPickerShader = Shader.Find(colorPickerShaderName);
    }

    private bool WrongShader()
    {
        return image?.material?.shader?.name != colorPickerShaderName;
    }

    private void Update()
    {
        if (WrongShader()) return;

        var rect = rectTransform.rect;

        image.material.SetFloat(_AspectRatio, rect.width / rect.height);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (WrongShader()) return;

        var pos = GetRelativePosition(eventData);

        if (pointerDownLocation == PointerDownLocation.HueCircle)
        {
            h = (Atan2(pos.y, pos.x) * recip2Pi + 1) % 1;
            ApplyColor();
        }

        if (pointerDownLocation == PointerDownLocation.SVSquare)
        {
            var size = image.material.GetFloat(_SVSquareSize);

            s = InverseLerp(-size, size, pos.x);
            v = InverseLerp(-size, size, pos.y);
            ApplyColor();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (WrongShader()) return;

        var pos = GetRelativePosition(eventData);

        float r = pos.magnitude;

        if (r < .5f && r > image.material.GetFloat(_HueCircleInner))
        {
            pointerDownLocation = PointerDownLocation.HueCircle;
            h = (Atan2(pos.y, pos.x) * recip2Pi + 1) % 1;
            ApplyColor();
        }
        else
        {
            var size = image.material.GetFloat(_SVSquareSize);

            // s -> x, v -> y
            if (pos.x >= -size && pos.x <= size && pos.y >= -size && pos.y <= size)
            {
                pointerDownLocation = PointerDownLocation.SVSquare;
                s = InverseLerp(-size, size, pos.x);
                v = InverseLerp(-size, size, pos.y);
                ApplyColor();
            }
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        pointerDownLocation = PointerDownLocation.Outside;
    }

    private void ApplyColor()
    {
        image.material.SetVector(_HSV, new Vector3(h, s, v));

        onColorChanged?.Invoke(color);
    }

    private void OnDestroy()
    {
        if (generatedMaterial != null)
            DestroyImmediate(generatedMaterial);
    }

    /// <summary>
    /// Returns position in range -0.5..0.5 when it's inside color picker square area
    /// </summary>
    public Vector2 GetRelativePosition(PointerEventData eventData)
    {
        var rect = GetSquaredRect();

        Vector2 rtPos;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, eventData.pressEventCamera, out rtPos);

        return new Vector2(InverseLerpUnclamped(rect.xMin, rect.xMax, rtPos.x), InverseLerpUnclamped(rect.yMin, rect.yMax, rtPos.y)) - Vector2.one * 0.5f;
    }

    public Rect GetSquaredRect()
    {
        var rect = rectTransform.rect;
        var smallestDimension = Min(rect.width, rect.height);
        return new Rect(rect.center - Vector2.one * smallestDimension * 0.5f, Vector2.one * smallestDimension);
    }

    public float InverseLerpUnclamped(float min, float max, float value)
    {
        return (value - min) / (max - min);
    }

#if UNITY_EDITOR

    [UnityEditor.MenuItem("GameObject/UI/Color Picker", false, 10)]
    private static void CreateColorPicker(UnityEditor.MenuCommand menuCommand)
    {
        GameObject go = new GameObject("Color Picker");

        go.AddComponent<ColorPicker>();

        UnityEditor.GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);

        UnityEditor.Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
        UnityEditor.Selection.activeObject = go;
    }
#endif

}