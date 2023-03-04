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

        public ValueChannel EdgeLAlpha { get; set; }
        public ValueChannel EdgeRAlpha { get; set; }
        public ValueChannel Lane1Alpha { get; set; }
        public ValueChannel Lane2Alpha { get; set; }
        public ValueChannel Lane3Alpha { get; set; }
        public ValueChannel Lane4Alpha { get; set; }
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

        [EmmyDoc("Sets the sprite of the track. Will not take effect if the provided name is invalid.")]
        public void SetTrackSprite(string name)
        {
            name = name.ToLower();
            var (trackSprite, extraLaneSprite) = Services.Skin.GetTrackSprite(name);
            SpriteRenderer.sprite = trackSprite;
            EdgeExtraL.SpriteRenderer.sprite = trackSprite;
            EdgeExtraR.SpriteRenderer.sprite = trackSprite;

            if (ExtraL != null && ExtraR != null)
            {
                ExtraL.SpriteRenderer.sprite = extraLaneSprite;
                ExtraR.SpriteRenderer.sprite = extraLaneSprite;
            }
        }
    }
}