using MoonSharp.Interpreter;
using UnityEngine;

namespace ArcCreate.Gameplay.Scenecontrol
{
    [MoonSharpUserData]
    public class SingleLineController : SpriteController, ISyncToSpeedController
    {
        private float offset = 0;
        private int offsetShaderId;

        public override void SetupDefault()
        {
            base.SetupDefault();
            offsetShaderId = Shader.PropertyToID("_Offset");
        }

        public void UpdateToSpeed(float speed, float glow)
        {
            offset += (speed >= 0) ? (Time.deltaTime * speed * 6) : (Time.deltaTime * 0.6f);
            SpriteRenderer.material.SetFloat(offsetShaderId, offset);
        }
    }
}