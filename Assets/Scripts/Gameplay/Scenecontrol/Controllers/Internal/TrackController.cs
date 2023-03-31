using EmmySharp;
using MoonSharp.Interpreter;
using UnityEngine;

namespace ArcCreate.Gameplay.Scenecontrol
{
    [MoonSharpUserData]
    [EmmyDoc("Controller for gameplay field's track")]
    public class TrackController : SpriteController, ITrackController
    {
        private static int edgeLAlphaShaderID = Shader.PropertyToID("_EdgeLAlpha");
        private static int edgeRAlphaShaderID = Shader.PropertyToID("_EdgeRAlpha");
        private static int lane1AlphaShaderID = Shader.PropertyToID("_Lane1Alpha");
        private static int lane2AlphaShaderID = Shader.PropertyToID("_Lane2Alpha");
        private static int lane3AlphaShaderID = Shader.PropertyToID("_Lane3Alpha");
        private static int lane4AlphaShaderID = Shader.PropertyToID("_Lane4Alpha");

#pragma warning disable
        private ValueChannel edgeLAlpha;
        private ValueChannel edgeRAlpha;
        private ValueChannel lane1Alpha;
        private ValueChannel lane2Alpha;
        private ValueChannel lane3Alpha;
        private ValueChannel lane4Alpha;
        [SerializeField] private SpriteController divideLine01;
        [EmmyDoc("Gets the divide line between lane 0 and 1")]
        public SpriteController DivideLine01
        {
            get
            {
                Services.Scenecontrol.AddReferencedController(divideLine01);
                return divideLine01;
            }
        }
        [EmmyDoc("Gets the divide line between lane 1 and 2")]
        [SerializeField] private SpriteController divideLine12;
        public SpriteController DivideLine12
        {
            get
            {
                Services.Scenecontrol.AddReferencedController(divideLine12);
                return divideLine12;
            }
        }
        [SerializeField] private SpriteController divideLine23;
        [EmmyDoc("Gets the divide line between lane 2 and 3")]
        public SpriteController DivideLine23
        {
            get
            {
                Services.Scenecontrol.AddReferencedController(divideLine23);
                return divideLine23;
            }
        }
        [SerializeField] private SpriteController divideLine34;
        [EmmyDoc("Gets the divide line between lane 3 and 4")]
        public SpriteController DivideLine34
        {
            get
            {
                Services.Scenecontrol.AddReferencedController(divideLine34);
                return divideLine34;
            }
        }
        [SerializeField] private SpriteController divideLine45;
        [EmmyDoc("Gets the divide line between lane 4 and 5")]
        public SpriteController DivideLine45
        {
            get
            {
                Services.Scenecontrol.AddReferencedController(divideLine45);
                return divideLine45;
            }
        }
        [SerializeField] private SpriteController criticalLine0;
        [EmmyDoc("Gets the critical line of lane 0")]
        public SpriteController CriticalLine0
        {
            get
            {
                Services.Scenecontrol.AddReferencedController(criticalLine0);
                return criticalLine0;
            }
        }
        [SerializeField] private SpriteController criticalLine1;
        [EmmyDoc("Gets the critical line of lane 1")]
        public SpriteController CriticalLine1
        {
            get
            {
                Services.Scenecontrol.AddReferencedController(criticalLine1);
                return criticalLine1;
            }
        }
        [SerializeField] private SpriteController criticalLine2;
        [EmmyDoc("Gets the critical line of lane 2")]
        public SpriteController CriticalLine2
        {
            get
            {
                Services.Scenecontrol.AddReferencedController(criticalLine2);
                return criticalLine2;
            }
        }
        [SerializeField] private SpriteController criticalLine3;
        [EmmyDoc("Gets the critical line of lane 3")]
        public SpriteController CriticalLine3
        {
            get
            {
                Services.Scenecontrol.AddReferencedController(criticalLine3);
                return criticalLine3;
            }
        }
        [SerializeField] private SpriteController criticalLine4;
        [EmmyDoc("Gets the critical line of lane 4")]
        public SpriteController CriticalLine4
        {
            get
            {
                Services.Scenecontrol.AddReferencedController(criticalLine4);
                return criticalLine4;
            }
        }
        [SerializeField] private SpriteController criticalLine5;
        [EmmyDoc("Gets the critical line of lane 5")]
        public SpriteController CriticalLine5
        {
            get
            {
                Services.Scenecontrol.AddReferencedController(criticalLine5);
                return criticalLine5;
            }
        }
        [SerializeField] private SpriteController extraL;
        [EmmyDoc("Gets the left extra lane")]
        public SpriteController ExtraL
        {
            get
            {
                Services.Scenecontrol.AddReferencedController(extraL);
                return extraL;
            }
        }
        [SerializeField] private SpriteController extraR;
        [EmmyDoc("Gets the right extra lane")]
        public SpriteController ExtraR
        {
            get
            {
                Services.Scenecontrol.AddReferencedController(extraR);
                return extraR;
            }
        }
        [SerializeField] private SpriteController edgeExtraL;
        [EmmyDoc("Gets the left extra lane's edge")]
        public SpriteController EdgeExtraL
        {
            get
            {
                Services.Scenecontrol.AddReferencedController(edgeExtraL);
                return edgeExtraL;
            }
        }
        [SerializeField] private SpriteController edgeExtraR;
        [EmmyDoc("Gets the right extra lane's edge")]
        public SpriteController EdgeExtraR
        {
            get
            {
                Services.Scenecontrol.AddReferencedController(edgeExtraR);
                return edgeExtraR;
            }
        }

        public ValueChannel EdgeLAlpha
        {
            get => edgeLAlpha;
            set
            {
                edgeLAlpha = value;
                EnableTrackModule = true;
            }
        }
        public ValueChannel EdgeRAlpha
        {
            get => edgeRAlpha;
            set
            {
                edgeRAlpha = value;
                EnableTrackModule = true;
            }
        }
        public ValueChannel Lane1Alpha
        {
            get => lane1Alpha;
            set
            {
                lane1Alpha = value;
                EnableTrackModule = true;
            }
        }
        public ValueChannel Lane2Alpha
        {
            get => lane2Alpha;
            set
            {
                lane2Alpha = value;
                EnableTrackModule = true;
            }
        }
        public ValueChannel Lane3Alpha
        {
            get => lane3Alpha;
            set
            {
                lane3Alpha = value;
                EnableTrackModule = true;
            }
        }
        public ValueChannel Lane4Alpha
        {
            get => lane4Alpha;
            set
            {
                lane4Alpha = value;
                EnableTrackModule = true;
            }
        }

        public string CustomSkin { get; set; }

        public bool EnableTrackModule { get; set; }
#pragma warning restore

        [MoonSharpHidden]
        public override void SetupDefault()
        {
            base.SetupDefault();
            edgeLAlphaShaderID = Shader.PropertyToID("_EdgeLAlpha");
            edgeRAlphaShaderID = Shader.PropertyToID("_EdgeRAlpha");
            lane1AlphaShaderID = Shader.PropertyToID("_Lane1Alpha");
            lane2AlphaShaderID = Shader.PropertyToID("_Lane2Alpha");
            lane3AlphaShaderID = Shader.PropertyToID("_Lane3Alpha");
            lane4AlphaShaderID = Shader.PropertyToID("_Lane4Alpha");
        }

        [EmmyDoc("Creates a copy of this controller")]
        public override SpriteController Copy()
        {
            var c = base.Copy() as TrackController;
            return c;
        }

        [MoonSharpHidden]
        public void UpdateLane(float edgeL, float edgeR, float lane1, float lane2, float lane3, float lane4)
        {
            SpriteRenderer.material.SetFloat(edgeLAlphaShaderID, edgeL);
            SpriteRenderer.material.SetFloat(edgeRAlphaShaderID, edgeR);
            SpriteRenderer.material.SetFloat(lane1AlphaShaderID, lane1);
            SpriteRenderer.material.SetFloat(lane2AlphaShaderID, lane2);
            SpriteRenderer.material.SetFloat(lane3AlphaShaderID, lane3);
            SpriteRenderer.material.SetFloat(lane4AlphaShaderID, lane4);
        }

        [EmmyDoc("Sets the sprite of the track. Only works for copies of the original track.\nIf provided name is invalid, the default track skin for current side is used.")]
        public void SetTrackSprite(string name)
        {
            if (IsPersistent)
            {
                return;
            }

            name = name.ToLower();
            var (trackSprite, extraLaneSprite) = Services.Skin.GetTrackSprite(name);
            SpriteRenderer.sprite = trackSprite;
            edgeExtraL.SpriteRenderer.sprite = trackSprite;
            edgeExtraR.SpriteRenderer.sprite = trackSprite;

            if (extraL != null && extraR != null)
            {
                extraL.SpriteRenderer.sprite = extraLaneSprite;
                extraR.SpriteRenderer.sprite = extraLaneSprite;
            }

            CustomSkin = name;
        }

        [MoonSharpHidden]
        public void ApplySkin(string name)
        {
            if (IsPersistent || string.IsNullOrEmpty(name))
            {
                return;
            }

            SetTrackSprite(name);
        }

        private void Awake()
        {
            divideLine01.SerializedType = "divline01";
            divideLine12.SerializedType = "divline12";
            divideLine23.SerializedType = "divline23";
            divideLine34.SerializedType = "divline34";
            divideLine45.SerializedType = "divline45";
            criticalLine0.SerializedType = "critline0";
            criticalLine1.SerializedType = "critline1";
            criticalLine2.SerializedType = "critline2";
            criticalLine3.SerializedType = "critline3";
            criticalLine4.SerializedType = "critline4";
            criticalLine5.SerializedType = "critline5";
            edgeExtraL.SerializedType = "edgeextraL";
            edgeExtraR.SerializedType = "edgeextraR";
            extraL.SerializedType = "extraL";
            extraR.SerializedType = "extraR";
        }
    }
}