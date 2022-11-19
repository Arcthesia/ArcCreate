/*
 * Flexible Color Picker
 * Free asset by Unity
 * Made by Ward "WARdd" Dehairs
 * contact at info@WARddDev.com
 * More info at www.WARddDev.com
 *
 * Last Updated 20/04/2022
 *
 * Additonal contributions by
 * Taha Sanli, ibrahimtahasanli@gmail.com
 *
 */

// Uncomment this line to switch from using UnityEngin.UI inputfield and dropdown to TMPro version
// #define TMPro

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#pragma warning disable
#if TMPro
using TMPro;
#endif


/// <summary>
/// Main controller script for the flexible color picker system
/// </summary>
public class FlexibleColorPicker : MonoBehaviour {

    /*----------------------------------------------------------
    * ----------------------- PARAMETERS -----------------------
    * ----------------------------------------------------------
    */

    //Unity connections
    [Tooltip("Connections to the FCP's picker images, this should not be adjusted unless in advanced use cases.")]
    [SerializeField]
    private Picker[] pickers;
    [Serializable]
    private struct Picker {
        public Image image;
        public Sprite dynamicSprite;
        public Sprite staticSpriteHor;
        public Sprite staticSpriteVer;
        public Material dynamicMaterial;
    }
    private enum PickerType {
        Main, R, G, B, H, S, V, A, Preview, PreviewAlpha
    }

    [Tooltip("Connection to the FCP's hexadecimal input field.")]
    [SerializeField]
#if TMPro
    private TMPro.TMP_InputField hexInput;
#else
    private InputField hexInput;
#endif

    [Tooltip("Connection to the FCP's mode dropdown menu.")]
    [SerializeField]
#if TMPro
    private TMP_Dropdown modeDropdown;
#else
    private Dropdown modeDropdown;
#endif

    private Canvas canvas;

    [Tooltip("The (starting) 2D picking mode, i.e. the 2 color values that can be picked with the large square picker.")]
    [SerializeField]
    private MainPickingMode mode;

    [Tooltip("Sprites to be used in static mode on the main picker, one for each 2D mode.")]
    [SerializeField]
    private Sprite[] staticSpriteMain;
    public enum MainPickingMode {
        HS, HV, SH, SV, VH, VS
    }

    //private state
    private BufferedColor bufferedColor;
    private Picker focusedPicker;
    private PickerType focusedPickerType;
    private MainPickingMode lastUpdatedMode;
    private bool typeUpdate;
    private bool triggeredStaticMode;
    private bool materialsSeperated;

    //public settings
    [Tooltip("Color set to the color picker on Start(). If you wish to set a starting color via script please used the standard color parameter of the FCP in stead.")]
    [SerializeField]
    private Color startingColor = Color.white;
    [Tooltip("Use static mode: picker images are replaced by static images in stead of adaptive Unity shaders.")]
    public bool staticMode = false;
    [Tooltip("Make sure FCP seperates its picker materials so that the dynamic mode works consistently, even when multiple FPCs are active at the same time. Turning this off yields a slight performance boost.")]
    public bool multiInstance = true;

    public ColorUpdateEvent onColorChange;

    [Serializable]
    public class ColorUpdateEvent : UnityEvent<Color> { }

    //constants
    private const float HUE_LOOP = 5.9999f;
    private const string SHADER_MODE = "_Mode";
    private const string SHADER_C1 = "_Color1";
    private const string SHADER_C2 = "_Color2";
    private const string SHADER_DOUBLE_MODE = "_DoubleMode";
    private const string SHADER_HSV = "_HSV";
    private const string SHADER_HSV_MIN = "_HSV_MIN";
    private const string SHADER_HSV_MAX = "_HSV_MAX";

    //advanced settings
    [Tooltip("More specific settings for color picker. Changes are not applied immediately, but require an FCP update to trigger.")]
    public AdvancedSettings advancedSettings;
    [Serializable]
    public class AdvancedSettings {

        public bool mainStatic = true;

        public PSettings r;
        public PSettings g;
        public PSettings b;
        public PSettings h;
        public PSettings s;
        public PSettings v;
        public PSettings a;

        [Serializable]
        public class PSettings {
            [Tooltip("Value can be used to restrict slider range")]
            public Vector2 range = new Vector2(0f, 1f);
            [Tooltip("Make the picker associated with this value act static, even in a dynamic color picker setup")]
            public bool overrideStatic = false;
        }

        /// <summary>
        /// Get PSettings for value associated with the given picker index.
        /// Returns default PSettings if none exists.
        /// </summary>
        public PSettings Get(int i) {
            if(i <= 0 | i > 7)
                return new PSettings();
            PSettings[] p = new PSettings[] { r, g, b, h, s, v, a };
            return p[i - 1];
        }
    }
    private AdvancedSettings avs => advancedSettings;







    /*----------------------------------------------------------
    * ------------------- MAIN COLOR GET/SET -------------------
    * ----------------------------------------------------------
    */

    public Color color {
        /* if called before init in Start(), the color state
         * is equivalent to the starting color parameter.
         */
        get {
            if(bufferedColor == null)
                return startingColor;
            return bufferedColor.color;
        }
        set {
            if(bufferedColor == null) {
                startingColor = value;
                return;
            }
            bufferedColor.Set(value);
            UpdateMarkers();
            UpdateTextures();
            UpdateHex();
            typeUpdate = true;
            onColorChange.Invoke(value);
        }
    }

    /// <summary>
    /// Equivalent to fcp.color
    /// Returns starting color if FCP is not initialized.
    /// </summary>
    public Color GetColor() {
        return color;
    }

    /// <summary>
    /// Equivalent to fcp.color
    /// Propogates color change to picker images, hex input and other components.
    /// Modifies starting color if FCP is not initialized.
    /// </summary>
    public void SetColor(Color color) {
        this.color = color;
    }

    /// <summary>
    /// Returns current fcp color, but with its alpha channel value set to max.
    /// </summary>
    public Color GetColorFullAlpha() {
        Color toReturn = color;
        toReturn.a = 1f;
        return toReturn;
    }

    /// <summary>
    /// Changes fcp color while maintaining its current alpha value.
    /// </summary>
    public void SetColorNoAlpha(Color color) {
        color.a = this.color.a;
        this.color = color;
    }









    /*----------------------------------------------------------
    * ------------------------- UPKEEP -------------------------
    * ----------------------------------------------------------
    */

    private void Awake() {
        canvas = GetComponentInParent<Canvas>();
    }

    private void OnEnable() {
        if(this.bufferedColor == null)
            this.bufferedColor = new BufferedColor(startingColor);

        if(multiInstance && !materialsSeperated) {
            SeperateMaterials();
            materialsSeperated = true;
        }
        triggeredStaticMode = staticMode;
        UpdateTextures();
        MakeModeOptions();
        UpdateMarkers();
        UpdateHex();
        onColorChange.Invoke(startingColor);
    }

    private void Update() {
        typeUpdate = false;
        if(lastUpdatedMode != mode)
            ChangeMode(mode);

        if(staticMode != triggeredStaticMode) {
            UpdateTextures();
            triggeredStaticMode = staticMode;
        }
        if(multiInstance && !materialsSeperated) {
            SeperateMaterials();
            materialsSeperated = true;
        }
    }

    /// <summary>
    /// Change picker that is being focused (and edited) using the pointer.
    /// </summary>
    /// <param name="i">Index of the picker image.</param>
    public void SetPointerFocus(int i) {
        if(i < 0 || i >= pickers.Length)
            Debug.LogWarning("No picker image available of type " + (PickerType)i + 
                ". Did you assign all the picker images in the editor?");
        else
            focusedPicker = pickers[i];
        focusedPickerType = (PickerType)i;
    }

    /// <summary>
    /// Update color based on the pointer position in the currently focused picker.
    /// </summary>
    /// <param name="e">Pointer event</param>
    public void PointerUpdate(BaseEventData e) {
        Vector2 v = GetNormalizedPointerPosition(canvas, focusedPicker.image.rectTransform, e);
        this.bufferedColor = PickColor(this.bufferedColor, focusedPickerType, v);

        UpdateMarkers();
        UpdateTextures();

        typeUpdate = true;
        UpdateHex();
        onColorChange.Invoke(bufferedColor.color);
    }

    /// <summary>
    /// Softly sanitize hex color input and apply it
    /// </summary>
    public void TypeHex(string input) {
        TypeHex(input, false);

        UpdateTextures();
        UpdateMarkers();
    }

    /// <summary>
    /// Strongly sanitize hex color input and apply it.
    /// (appends zeroes to fit proper length in the text box).
    /// </summary>
    public void FinishTypeHex(string input) {
        TypeHex(input, true);

        UpdateTextures();
        UpdateMarkers();
    }

    /// <summary>
    /// Change mode of the main, 2D picking image
    /// </summary>
    public void ChangeMode(int newMode) {
        ChangeMode((MainPickingMode)newMode);
    }

    /// <summary>
    /// Change mode of the main, 2D picking image
    /// </summary>
    public void ChangeMode(MainPickingMode mode) {
        this.mode = mode;

        triggeredStaticMode = false;
        UpdateTextures();
        UpdateMarkers();
        UpdateMode(mode);
    }

    private void SeperateMaterials() {
        for(int i = 0; i < pickers.Length; i++) {
            Picker p = pickers[i];
            if(IsPickerAvailable(i) & p.dynamicMaterial != null) {
                Material original = p.dynamicMaterial;
                Material seperate = new Material(original);
                p.dynamicMaterial = seperate;
                pickers[i] = p;
                if(!staticMode)
                    p.image.material = seperate;
            }
        }
    }








    /*----------------------------------------------------------
    * --------------------- COLOR PICKING ----------------------
    * ----------------------------------------------------------
    * 
    * Get a new color that is the currently selected color but with 
    * one or two values changed. This is the core functionality of 
    * the picking images and the entire color picker script.
    */
    
    /// <summary>
    /// Shift the FCP color by a small amount
    /// </summary>
    /// <param name="type">Select component of the color to change, 1: red, 2: green, 3:blue, 4:hue, 5:saturation, 6:value, 7:alpha</param>
    /// <param name="delta">How much should the color be changed, note this value is normalized and the result is clamped, including for hue.</param>
    public void ShiftColor(int type, float delta){
        PickerType pt = (PickerType)type;
        float value = GetValue1D(pt) + delta;
        this.bufferedColor = PickColor1D(bufferedColor, pt, value);
        
        UpdateMarkers();
        UpdateTextures();

        typeUpdate = true;
        UpdateHex();
        onColorChange.Invoke(bufferedColor.color);
    }
    
    /// <summary>
    /// Shift hue of the FCP color in a looping fashion
    /// </summary>
    public void ShiftHue(float delta){
        PickerType pt = PickerType.H;
        float h = Mathf.Repeat(GetValue1D(pt) + delta, 6f) / 6f;
        this.bufferedColor = PickColor1D(bufferedColor, pt, h);
        
        UpdateMarkers();
        UpdateTextures();

        typeUpdate = true;
        UpdateHex();
        onColorChange.Invoke(bufferedColor.color);
    }

    /// <summary>
    /// Get a color that is the current color, but changed by the given picker and value.
    /// </summary>
    /// <param name="type">Picker type to base change on</param>
    /// <param name="v">normalized x and y values (both values may not be used)</param>
    private BufferedColor PickColor(BufferedColor color, PickerType type, Vector2 v) {
        switch(type) {
        case PickerType.Main: return PickColorMain(color, v);

        case PickerType.Preview:
        case PickerType.PreviewAlpha:
        return color;

        default: return PickColor1D(color, type, v);
        }
    }

    private BufferedColor PickColorMain(BufferedColor color, Vector2 v) {
        return PickColorMain(color, this.mode, v);
    }

    private BufferedColor PickColor1D(BufferedColor color, PickerType type, Vector2 v) {
        bool horizontal = IsHorizontal(pickers[(int)type]);
        float value = horizontal ? v.x : v.y;
        return PickColor1D(color, type, value);
    }

    private BufferedColor PickColorMain(BufferedColor color, MainPickingMode mode, Vector2 v) {
        switch(mode) {
        case MainPickingMode.HS: return PickColor2D(color, PickerType.H, v.x, PickerType.S, v.y);
        case MainPickingMode.HV: return PickColor2D(color, PickerType.H, v.x, PickerType.V, v.y);
        case MainPickingMode.SH: return PickColor2D(color, PickerType.S, v.x, PickerType.H, v.y);
        case MainPickingMode.SV: return PickColor2D(color, PickerType.S, v.x, PickerType.V, v.y);
        case MainPickingMode.VH: return PickColor2D(color, PickerType.V, v.x, PickerType.H, v.y);
        case MainPickingMode.VS: return PickColor2D(color, PickerType.V, v.x, PickerType.S, v.y);
        default: return bufferedColor;
        }
    }

    private BufferedColor PickColor2D(BufferedColor color, PickerType type1, float value1, PickerType type2, float value2) {
        color = PickColor1D(color, type1, value1);
        color = PickColor1D(color, type2, value2);
        return color;
    }

    private BufferedColor PickColor1D(BufferedColor color, PickerType type, float value) {
        var ps = avs.Get((int)type);
        value = Mathf.Lerp(ps.range.x, ps.range.y, value);

        switch(type) {
        case PickerType.R: return color.PickR(value);
        case PickerType.G: return color.PickG(value);
        case PickerType.B: return color.PickB(value);
        case PickerType.H: return color.PickH(value * HUE_LOOP); 
        case PickerType.S: return color.PickS(value);
        case PickerType.V: return color.PickV(value);
        case PickerType.A: return color.PickA(value);
        default:
            throw new Exception("Picker type " + type + " is not associated with a single color value.");
        }
    }










    /*----------------------------------------------------------
    * -------------------- MARKER UPDATING ---------------------
    * ----------------------------------------------------------
    * 
    * Update positions of markers on each picking texture, 
    * indicating the currently selected values.
    */


    private void UpdateMarkers() {
        for(int i = 0; i < pickers.Length; i++) {
            if(IsPickerAvailable(i)) {
                PickerType type = (PickerType)i;
                Vector2 v = GetValue(type);
                UpdateMarker(pickers[i], type, v);
            }
        }
    }

    private void UpdateMarker(Picker picker, PickerType type, Vector2 v) {
        switch(type) {
        case PickerType.Main:
        SetMarker(picker.image, v, true, true);
        break;

        case PickerType.Preview:
        case PickerType.PreviewAlpha:
        break;

        default:
        bool horizontal = IsHorizontal(picker);
        SetMarker(picker.image, v, horizontal, !horizontal);
        break;
        }
    }

    private void SetMarker(Image picker, Vector2 v, bool setX, bool setY) {
        RectTransform marker = null;
        RectTransform offMarker = null;
        if(setX && setY)
            marker = GetMarker(picker, null);
        else if(setX) {
            marker = GetMarker(picker, "hor");
            offMarker = GetMarker(picker, "ver");
        }
        else if(setY) {
            marker = GetMarker(picker, "ver");
            offMarker = GetMarker(picker, "hor");
        }
        if(offMarker != null)
            offMarker.gameObject.SetActive(false);

        if(marker == null)
            return;

        marker.gameObject.SetActive(true);
        RectTransform parent = picker.rectTransform;
        Vector2 parentSize = parent.rect.size;
        Vector2 localPos = marker.localPosition;

        if(setX)
            localPos.x = (v.x - parent.pivot.x) * parentSize.x;
        if(setY)
            localPos.y = (v.y - parent.pivot.y) * parentSize.y;
        marker.localPosition = localPos;
    }

    private RectTransform GetMarker(Image picker, string search) {
        for(int i = 0; i < picker.transform.childCount; i++) {
            RectTransform candidate = picker.transform.GetChild(i).GetComponent<RectTransform>();
            string candidateName = candidate.name.ToLower();
            bool match = candidateName.Contains("marker");
            match &= string.IsNullOrEmpty(search)
                || candidateName.Contains(search);
            if(match)
                return candidate;
        }
        return null;
    }











    /*----------------------------------------------------------
    * -------------------- VALUE RETRIEVAL ---------------------
    * ----------------------------------------------------------
    * 
    * Get individual values associated with a picker image from the 
    * currently selected color.
    * This is needed to properly update markers.
    */
    
    private Vector2 GetValue(PickerType type) {
        switch(type) {

        case PickerType.Main: return GetValue(mode);

        case PickerType.Preview:
        case PickerType.PreviewAlpha:
        return Vector2.zero;

        default:
        float value = GetValue1D(type);
        return new Vector2(value, value);

        }
    }

    /// <summary>
    /// Get normalized value of the current color according to the given picker.
    /// This value can be used to adjust the position of the marker on a slider.
    /// </summary>
    private float GetValue1D(PickerType type) {
        var ps = avs.Get((int)type);
        float min = ps.range.x;
        float max = ps.range.y;

        switch(type) {
        case PickerType.R: return Mathf.InverseLerp(min, max, bufferedColor.r);
        case PickerType.G: return Mathf.InverseLerp(min, max, bufferedColor.g);
        case PickerType.B: return Mathf.InverseLerp(min, max, bufferedColor.b);
        case PickerType.H: return Mathf.InverseLerp(min, max, bufferedColor.h / HUE_LOOP);
        case PickerType.S: return Mathf.InverseLerp(min, max, bufferedColor.s);
        case PickerType.V: return Mathf.InverseLerp(min, max, bufferedColor.v);
        case PickerType.A: return Mathf.InverseLerp(min, max, bufferedColor.a);
        default:
            throw new Exception("Picker type " + type + " is not associated with a single color value.");
        }
    }

    /// <summary>
    /// Get normalized 2D value of the current color according to the main picker mode.
    /// This value can be used to adjust the position of the 2D marker.
    /// </summary>
    private Vector2 GetValue(MainPickingMode mode) {
        switch(mode) {
        case MainPickingMode.HS: return new Vector2(GetValue1D(PickerType.H), GetValue1D(PickerType.S));
        case MainPickingMode.HV: return new Vector2(GetValue1D(PickerType.H), GetValue1D(PickerType.V));
        case MainPickingMode.SH: return new Vector2(GetValue1D(PickerType.S), GetValue1D(PickerType.H));
        case MainPickingMode.SV: return new Vector2(GetValue1D(PickerType.S), GetValue1D(PickerType.V));
        case MainPickingMode.VH: return new Vector2(GetValue1D(PickerType.V), GetValue1D(PickerType.H));
        case MainPickingMode.VS: return new Vector2(GetValue1D(PickerType.V), GetValue1D(PickerType.S));
        default: throw new Exception("Unkown main picking mode: " + mode);
        }
    }










    /*----------------------------------------------------------
    * -------------------- TEXTURE UPDATING --------------------
    * ----------------------------------------------------------
    * 
    * Update picker image textures that show gradients of colors 
    * that the user can pick.
    */

    private void UpdateTextures() {
        foreach(PickerType type in Enum.GetValues(typeof(PickerType))) {
            if(staticMode | avs.Get((int)type).overrideStatic | (type == PickerType.Main & avs.mainStatic))
                UpdateStatic(type);
            else
                UpdateDynamic(type);
        }
    }

    private void UpdateStatic(PickerType type) {
        if(!IsPickerAvailable(type))
            return;
        Picker p = pickers[(int)type];

        bool hor = IsHorizontal(p);
        Sprite s = hor ? p.staticSpriteHor : p.staticSpriteVer;
        if(s == null)
            s = hor ? p.staticSpriteVer : p.staticSpriteHor;

        p.image.sprite = s;
        p.image.material = null;
        p.image.color = Color.white;

        Color prvw = color;

        switch(type) {
        case PickerType.Main:
        p.image.sprite = staticSpriteMain[(int)mode];
        break;

        case PickerType.Preview:
        prvw.a = 1f;
        p.image.color = prvw;
        break;

        case PickerType.PreviewAlpha:
        p.image.color = prvw;
        break;
        }
    }

    private void UpdateDynamic(PickerType type) {
        if(!IsPickerAvailable(type))
            return;
        Picker p = pickers[(int)type];
        if(p.dynamicMaterial == null)
            return;

        p.image.material = p.dynamicMaterial;
        p.image.color = Color.white;
        p.image.sprite = p.dynamicSprite;

        Material m = p.image.materialForRendering;

        BufferedColor bc = this.bufferedColor;

        bool alpha = IsAlphaType(type);
        m.SetInt(SHADER_MODE, GetGradientMode(type));
        Color c1 = PickColor(bc, type, Vector2.zero).color;
        Color c2 = PickColor(bc, type, Vector2.one).color;
        if(!alpha) {
            c1 = new Color(c1.r, c1.g, c1.b);
            c2 = new Color(c2.r, c2.g, c2.b);
        }
        m.SetColor(SHADER_C1, c1);
        m.SetColor(SHADER_C2, c2);
        if(type == PickerType.Main)
            m.SetInt(SHADER_DOUBLE_MODE, (int)mode);
        m.SetVector(SHADER_HSV, new Vector4(bc.h / HUE_LOOP, bc.s, bc.v, alpha ? bc.a : 1f));
        m.SetVector(SHADER_HSV_MIN, new Vector4(avs.h.range.x, avs.s.range.x, avs.v.range.x));
        m.SetVector(SHADER_HSV_MAX, new Vector4(avs.h.range.y, avs.s.range.y, avs.v.range.y));
    }

    private int GetGradientMode(PickerType type) {
        int o = IsHorizontal(pickers[(int)type]) ? 0 : 1;
        switch(type) {
        case PickerType.Main: return 2;
        case PickerType.H: return 3 + o;
        default: return o;
        }
    }

    private bool IsPickerAvailable(PickerType type) {
        return IsPickerAvailable((int)type);
    }

    private bool IsPickerAvailable(int index) {
        if(index < 0 || index >= pickers.Length)
            return false;
        Picker p = pickers[index];
        if(p.image == null || !p.image.gameObject.activeInHierarchy)
            return false;
        return true;
    }







    /*----------------------------------------------------------
    * ------------------ HEX INPUT UPDATING --------------------
    * ----------------------------------------------------------
    * 
    * Provides an input field for hexadecimal color values.
    * The user can type new values, or use this field to copy 
    * values picked via the picker images.
    */

    private void UpdateHex() {
        if(hexInput == null || !hexInput.gameObject.activeInHierarchy)
            return;
        hexInput.SetTextWithoutNotify("#" + ColorUtility.ToHtmlStringRGB(this.color));
    }

    private void TypeHex(string input, bool finish) {
        if(typeUpdate)
            return;
        else
            typeUpdate = true;

        string newText = GetSanitizedHex(input, finish);
        string parseText = GetSanitizedHex(input, true);

        int cp = hexInput.caretPosition;
        hexInput.SetTextWithoutNotify(newText);
        if(hexInput.caretPosition == 0)
            hexInput.caretPosition = 1;
        else if(newText.Length == 2)
            hexInput.caretPosition = 2;
        else if(input.Length > newText.Length && cp < input.Length)
            hexInput.caretPosition = cp - input.Length + newText.Length;

        Color newColor;
        if(ColorUtility.TryParseHtmlString(parseText, out newColor)) {
            if(bufferedColor != null) {
                bufferedColor.Set(newColor);
                UpdateMarkers();
                UpdateTextures();
                onColorChange.Invoke(newColor);
            }
            else
                startingColor = newColor;
        }
    }












    /*----------------------------------------------------------
    * ---------------------- MODE UPDATING ---------------------
    * ----------------------------------------------------------
    * 
    * Allows user to change the 'Main picking mode' which determines 
    * the values shown on the main, 2D picking image.
    */

    private void MakeModeOptions() {
        if(modeDropdown == null || !modeDropdown.gameObject.activeInHierarchy)
            return;

        modeDropdown.ClearOptions();
        List<string> options = new List<string>();
        foreach(MainPickingMode mode in Enum.GetValues(typeof(MainPickingMode)))
            options.Add(mode.ToString());
        modeDropdown.AddOptions(options);

        UpdateMode(this.mode);
    }

    private void UpdateMode(MainPickingMode mode) {
        lastUpdatedMode = mode;
        if(modeDropdown == null || !modeDropdown.gameObject.activeInHierarchy)
            return;
        modeDropdown.value = (int)mode;
    }







    /*----------------------------------------------------------
    * ---------------- STATIC HELPER FUNCTIONS -----------------
    * ----------------------------------------------------------
    */

    private static bool IsPreviewType(PickerType type) {
        switch(type) {
        case PickerType.Preview: return true;
        case PickerType.PreviewAlpha: return true;
        default: return false;
        }
    }

    private static bool IsAlphaType(PickerType type) {
        switch(type) {
        case PickerType.A: return true;
        case PickerType.PreviewAlpha: return true;
        default: return false;
        }
    }

    /// <summary>
    /// Should given picker image be controlled horizontally?
    /// Returns true if the image is bigger in the horizontal direction.
    /// </summary>
    private static bool IsHorizontal(Picker p) {
        Vector2 size = p.image.rectTransform.rect.size;
        return size.x >= size.y;
    }

    /// <summary>
    /// Santiive a given string so that it encodes a valid hex color string
    /// </summary>
    /// <param name="input">Input string</param>
    /// <param name="full">Insert zeroes to match #RRGGBB format </param>
    public static string GetSanitizedHex(string input, bool full) {
        if(string.IsNullOrEmpty(input))
            return "#";

        List<char> toReturn = new List<char>();
        toReturn.Add('#');
        int i = 0;
        char[] chars = input.ToCharArray();
        while(toReturn.Count < 7 && i < input.Length) {
            char nextChar = char.ToUpper(chars[i++]);
            if(IsValidHexChar(nextChar))
                toReturn.Add(nextChar);
        }

        while(full && toReturn.Count < 7)
            toReturn.Insert(1, '0');

        return new string(toReturn.ToArray());
    }

    private static bool IsValidHexChar(char c) {
        bool valid = char.IsNumber(c);
        valid |= c >= 'A' & c <= 'F';
        return valid;
    }

    /// <summary>
    /// tries to parse given string input as hexadecimal color e.g.
    /// "#FF00FF" or "223344" returns black if string failed to 
    /// parse.
    /// </summary>
    public static Color ParseHex(string input) {
        return ParseHex(input, Color.black);
    }

    /// <summary>
    /// tries to parse given string input as hexadecimal color e.g.
    /// "#FF00FF" or "223344" returns default color if string failed to 
    /// parse.
    /// </summary>
    public static Color ParseHex(string input, Color defaultColor) {
        string parseText = GetSanitizedHex(input, true);
        Color toReturn;
        if(ColorUtility.TryParseHtmlString(parseText, out toReturn))
            return toReturn;
        else
            return defaultColor;
    }

    /// <summary>
    /// Get normalized position of the given pointer event relative to the given rect.
    /// (e.g. return [0,1] for top left corner). This method correctly takes into 
    /// account relative positions, canvas render mode and general transformations, 
    /// including rotations and scale.
    /// </summary>
    /// <param name="canvas">parent canvas of the rect (and therefore the FCP)</param>
    /// <param name="rect">Rect to find relative position to</param>
    /// <param name="e">Pointer event for which to find the relative position</param>
    private static Vector2 GetNormalizedPointerPosition(Canvas canvas, RectTransform rect, BaseEventData e) {
        switch(canvas.renderMode) {

        case RenderMode.ScreenSpaceCamera:
        if(canvas.worldCamera == null)
            return GetNormScreenSpace(rect, e);
        else
            return GetNormWorldSpace(canvas, rect, e);

        case RenderMode.ScreenSpaceOverlay:
        return GetNormScreenSpace(rect, e);

        case RenderMode.WorldSpace:
        if(canvas.worldCamera == null) {
            Debug.LogError("FCP in world space render mode requires an event camera to be set up on the parent canvas!");
            return Vector2.zero;
        }
        return GetNormWorldSpace(canvas, rect, e);

        default: return Vector2.zero;

        }
    }

    /// <summary>
    /// Get normalized position in the case of a screen space (overlay) 
    /// type canvas render mode
    /// </summary>
    private static Vector2 GetNormScreenSpace(RectTransform rect, BaseEventData e) {
        Vector2 screenPoint = ((PointerEventData)e).position;
        Vector2 localPos = rect.worldToLocalMatrix.MultiplyPoint(screenPoint);
        float x = Mathf.Clamp01((localPos.x / rect.rect.size.x) + rect.pivot.x);
        float y = Mathf.Clamp01((localPos.y / rect.rect.size.y) + rect.pivot.y);
        return new Vector2(x, y);
    }

    /// <summary>
    /// Get normalized position in the case of a world space (or screen space camera) 
    /// type cavnvas render mode.
    /// </summary>
    private static Vector2 GetNormWorldSpace(Canvas canvas, RectTransform rect, BaseEventData e) {
        Vector2 screenPoint = ((PointerEventData)e).position;
        Ray pointerRay = canvas.worldCamera.ScreenPointToRay(screenPoint);
        Plane canvasPlane = new Plane(canvas.transform.forward, canvas.transform.position);
        float enter;
        canvasPlane.Raycast(pointerRay, out enter);
        Vector3 worldPoint = pointerRay.origin + enter * pointerRay.direction;
        Vector2 localPoint = rect.worldToLocalMatrix.MultiplyPoint(worldPoint);

        float x = Mathf.Clamp01((localPoint.x / rect.rect.size.x) + rect.pivot.x);
        float y = Mathf.Clamp01((localPoint.y / rect.rect.size.y) + rect.pivot.y);
        return new Vector2(x, y);
    }

    /// <summary>
    /// Get color from hue, saturation, value format
    /// </summary>
    /// <param name="hsv">Vector containing h, s and v values.</param>
    public static Color HSVToRGB(Vector3 hsv) {
        return HSVToRGB(hsv.x, hsv.y, hsv.z);
    }

    /// <summary>
    /// Get color from hue, saturation, value format
    /// </summary>
    /// <param name="h">hue value, ranging from 0 to 6; red to red</param>
    /// <param name="s">saturation, 0 to 1; gray to colored</param>
    /// <param name="v">value, 0 to 1; black to white</param>
    public static Color HSVToRGB(float h, float s, float v) {
        float c = s * v;
        float m = v - c;
        float x = c * (1f - Mathf.Abs(h % 2f - 1f)) + m;
        c += m;
        
        int range = Mathf.FloorToInt(h % 6f);
        
        switch(range) {
        case 0: return new Color(c, x, m);
        case 1: return new Color(x, c, m);
        case 2: return new Color(m, c, x);
        case 3: return new Color(m, x, c);
        case 4: return new Color(x, m, c);
        case 5: return new Color(c, m, x);
        default: return Color.black;
        }
    }

    /// <summary>
    /// Get hue, saturation and value of a color.
    /// Complementary to HSVToRGB
    /// </summary>
    public static Vector3 RGBToHSV(Color color) {
        float r = color.r;
        float g = color.g;
        float b = color.b;
        return RGBToHSV(r, g, b);
    }

    /// <summary>
    /// Get hue, saturation and value of a color.
    /// Complementary to HSVToRGB
    /// </summary>
    public static Vector3 RGBToHSV(float r, float g, float b) {
        float cMax = Mathf.Max(r,g,b);
        float cMin = Mathf.Min(r,g,b);
        float delta = cMax - cMin;
        float h = 0f;
        if(delta > 0f) {
            if(r >= b && r >= g)
                h = Mathf.Repeat((g - b) / delta, 6f);
            else if(g >= r && g >= b)
                h = (b - r) / delta + 2f;
            else if(b >= r && b >= g)
                h = (r - g) / delta + 4f;
        }
        float s = cMax == 0f ? 0f : delta / cMax;
        float v = cMax;
        return new Vector3(h, s, v);
    }








    /*----------------------------------------------------------
    * --------------------- HELPER CLASSES ---------------------
    * ----------------------------------------------------------
    */


    /// <summary>
    /// Encodes a color while buffering hue and saturation values.
    /// This is necessary since these values are singular for some 
    /// colors like unsaturated grays and would lead to undesirable 
    /// behaviour when moving sliders towards such colors.
    /// </summary>
    [Serializable]
    private class BufferedColor {
        public Color color;
        private float bufferedHue;
        private float bufferedSaturation;

        public float r { get { return color.r; } }
        public float g { get { return color.g; } }
        public float b { get { return color.b; } }
        public float a { get { return color.a; } }
        public float h { get { return bufferedHue; } }
        public float s { get { return bufferedSaturation; } }
        public float v { get { return RGBToHSV(color).z; } }


        public BufferedColor() {
            this.bufferedHue = 0f;
            this.bufferedSaturation = 0f;
            this.color = Color.black;
        }

        public BufferedColor(Color color) : this() {
            this.Set(color);
        }

        public BufferedColor(Color color, float hue, float sat) : this(color) {
            this.bufferedHue = hue;
            this.bufferedSaturation = sat;
        }

        public BufferedColor(Color color, BufferedColor source) : 
            this(color, source.bufferedHue, source.bufferedSaturation) {
            this.Set(color);
        }

        public void Set(Color color) {
            this.Set(color, this.bufferedHue, this.bufferedSaturation);
        }

        public void Set(Color color, float bufferedHue, float bufferedSaturation) {
            this.color = color;
            Vector3 hsv = RGBToHSV(color);

            bool hueSingularity = hsv.y == 0f || hsv.z == 0f;
            if(hueSingularity)
                this.bufferedHue = bufferedHue;
            else
                this.bufferedHue = hsv.x;

            bool saturationSingularity = hsv.z == 0f;
            if(saturationSingularity)
                this.bufferedSaturation = bufferedSaturation;
            else
                this.bufferedSaturation = hsv.y;
        }

        public BufferedColor PickR(float value) {
            Color toReturn = this.color;
            toReturn.r = value;
            return new BufferedColor(toReturn, this);
        }

        public BufferedColor PickG(float value) {
            Color toReturn = this.color;
            toReturn.g = value;
            return new BufferedColor(toReturn, this);
        }

        public BufferedColor PickB(float value) {
            Color toReturn = this.color;
            toReturn.b = value;
            return new BufferedColor(toReturn, this);
        }

        public BufferedColor PickA(float value) {
            Color toReturn = this.color;
            toReturn.a = value;
            return new BufferedColor(toReturn, this);
        }

        public BufferedColor PickH(float value) {
            Vector3 hsv = RGBToHSV(this.color);
            Color toReturn = HSVToRGB(value, hsv.y, hsv.z);
            toReturn.a = this.color.a;
            return new BufferedColor(toReturn, value, bufferedSaturation);
        }

        public BufferedColor PickS(float value) {
            Vector3 hsv = RGBToHSV(this.color);
            Color toReturn = HSVToRGB(bufferedHue, value, hsv.z);
            toReturn.a = this.color.a;
            return new BufferedColor(toReturn, bufferedHue, value);
        }

        public BufferedColor PickV(float value) {
            Color toReturn = HSVToRGB(bufferedHue, bufferedSaturation, value);
            toReturn.a = this.color.a;
            return new BufferedColor(toReturn, bufferedHue, bufferedSaturation);
        }
    }
}
