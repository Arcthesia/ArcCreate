using MoonSharp.Interpreter;
using UnityEngine;

namespace ArcCreate.Gameplay.Scenecontrol
{
    [MoonSharpUserData]
    public class TrackController : SpriteController, ITrackController, ISyncToSpeedController
    {
        private static int offsetShaderId = Shader.PropertyToID("_Offset");
        private static int edgeLAlphaShaderID = Shader.PropertyToID("_EdgeLAlpha");
        private static int edgeRAlphaShaderID = Shader.PropertyToID("_EdgeRAlpha");
        private static int lane1AlphaShaderID = Shader.PropertyToID("_Lane1Alpha");
        private static int lane2AlphaShaderID = Shader.PropertyToID("_Lane2Alpha");
        private static int lane3AlphaShaderID = Shader.PropertyToID("_Lane3Alpha");
        private static int lane4AlphaShaderID = Shader.PropertyToID("_Lane4Alpha");
        private float offset = 0;

#pragma warning disable
        public SpriteController DivideLine01;
        public SpriteController DivideLine12;
        public SpriteController DivideLine23;
        public SpriteController DivideLine34;
        public SpriteController DivideLine45;
        public SpriteController[] DivideLines
            => new SpriteController[] { DivideLine01, DivideLine12, DivideLine23, DivideLine34, DivideLine45 };
        public SpriteController CriticalLine0;
        public SpriteController CriticalLine1;
        public SpriteController CriticalLine2;
        public SpriteController CriticalLine3;
        public SpriteController CriticalLine4;
        public SpriteController CriticalLine5;
        public SpriteController[] CriticalLines
            => new SpriteController[] { CriticalLine0, CriticalLine1, CriticalLine2, CriticalLine3, CriticalLine4, CriticalLine5 };
        public SpriteController ExtraL;
        public SpriteController ExtraR;
        public SpriteController EdgeExtraL;
        public SpriteController EdgeExtraR;

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
            offsetShaderId = Shader.PropertyToID("_Offset");
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

        public void UpdateToSpeed(float speed, float glow)
        {
            offset += Time.deltaTime * speed * 6;
            SpriteRenderer.material.SetFloat(offsetShaderId, offset);
        }
    }
}