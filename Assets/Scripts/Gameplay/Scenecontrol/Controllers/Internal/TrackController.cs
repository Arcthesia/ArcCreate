using MoonSharp.Interpreter;
using UnityEngine;

namespace ArcCreate.Gameplay.Scenecontrol
{
    [MoonSharpUserData]
    public class TrackController : SpriteController, ITrackController
    {
        private static int edgeLAlphaShaderID = Shader.PropertyToID("_EdgeLAlpha");
        private static int edgeRAlphaShaderID = Shader.PropertyToID("_EdgeRAlpha");
        private static int lane1AlphaShaderID = Shader.PropertyToID("_Lane1Alpha");
        private static int lane2AlphaShaderID = Shader.PropertyToID("_Lane2Alpha");
        private static int lane3AlphaShaderID = Shader.PropertyToID("_Lane3Alpha");
        private static int lane4AlphaShaderID = Shader.PropertyToID("_Lane4Alpha");
        private float offset = 0;

#pragma warning disable
        [SerializeField] private SpriteController divideLine01;
        public SpriteController DivideLine01
        {
            get
            {
                Services.Scenecontrol.AddReferencedController(divideLine01);
                return divideLine01;
            }
        }
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
        public SpriteController DivideLine23
        {
            get
            {
                Services.Scenecontrol.AddReferencedController(divideLine23);
                return divideLine23;
            }
        }
        [SerializeField] private SpriteController divideLine34;
        public SpriteController DivideLine34
        {
            get
            {
                Services.Scenecontrol.AddReferencedController(divideLine34);
                return divideLine34;
            }
        }
        [SerializeField] private SpriteController divideLine45;
        public SpriteController DivideLine45
        {
            get
            {
                Services.Scenecontrol.AddReferencedController(divideLine45);
                return divideLine45;
            }
        }
        [SerializeField] private SpriteController criticalLine0;
        public SpriteController CriticalLine0
        {
            get
            {
                Services.Scenecontrol.AddReferencedController(criticalLine0);
                return criticalLine0;
            }
        }
        [SerializeField] private SpriteController criticalLine1;
        public SpriteController CriticalLine1
        {
            get
            {
                Services.Scenecontrol.AddReferencedController(criticalLine1);
                return criticalLine1;
            }
        }
        [SerializeField] private SpriteController criticalLine2;
        public SpriteController CriticalLine2
        {
            get
            {
                Services.Scenecontrol.AddReferencedController(criticalLine2);
                return criticalLine2;
            }
        }
        [SerializeField] private SpriteController criticalLine3;
        public SpriteController CriticalLine3
        {
            get
            {
                Services.Scenecontrol.AddReferencedController(criticalLine3);
                return criticalLine3;
            }
        }
        [SerializeField] private SpriteController criticalLine4;
        public SpriteController CriticalLine4
        {
            get
            {
                Services.Scenecontrol.AddReferencedController(criticalLine4);
                return criticalLine4;
            }
        }
        [SerializeField] private SpriteController criticalLine5;
        public SpriteController CriticalLine5
        {
            get
            {
                Services.Scenecontrol.AddReferencedController(criticalLine5);
                return criticalLine5;
            }
        }
        [SerializeField] private SpriteController extraL;
        public SpriteController ExtraL
        {
            get
            {
                Services.Scenecontrol.AddReferencedController(extraL);
                return extraL;
            }
        }
        [SerializeField] private SpriteController extraR;
        public SpriteController ExtraR
        {
            get
            {
                Services.Scenecontrol.AddReferencedController(extraR);
                return extraR;
            }
        }
        [SerializeField] private SpriteController edgeExtraL;
        public SpriteController EdgeExtraL
        {
            get
            {
                Services.Scenecontrol.AddReferencedController(edgeExtraL);
                return edgeExtraL;
            }
        }
        [SerializeField] private SpriteController edgeExtraR;
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

        public override SpriteController Copy()
        {
            var c = base.Copy() as TrackController;
            return c;
        }

        public void UpdateLane(float edgeL, float edgeR, float lane1, float lane2, float lane3, float lane4)
        {
            SpriteRenderer.material.SetFloat(edgeLAlphaShaderID, edgeL);
            SpriteRenderer.material.SetFloat(edgeRAlphaShaderID, edgeR);
            SpriteRenderer.material.SetFloat(lane1AlphaShaderID, lane1);
            SpriteRenderer.material.SetFloat(lane2AlphaShaderID, lane2);
            SpriteRenderer.material.SetFloat(lane3AlphaShaderID, lane3);
            SpriteRenderer.material.SetFloat(lane4AlphaShaderID, lane4);
        }

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