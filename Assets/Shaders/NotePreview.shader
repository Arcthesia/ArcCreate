Shader "Compose/NotePreview"
{
	Properties
	{
		_Color ("Tint", Color) = (1,1,1,1)
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull", Float) = 0
	}

	SubShader
	{
		Tags
		{ 
			"Queue"="Transparent" 
			"IgnoreProjector"="True" 
			"RenderType"="Transparent" 
			"PreviewType"="Plane"
			"CanUseSpriteAtlas"="True"
		}

		Cull [_Cull]
		Lighting Off
		ZWrite On
		ZTest Always

		Pass
		{
			Stencil
			{
				Ref 0
				Comp Equal
				Pass IncrSat
			}

            Blend SrcAlpha OneMinusSrcAlpha
            CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			
			struct appdata_t
			{
				float4 vertex   : POSITION;
			};

			struct v2f
			{
				float4 vertex   : SV_POSITION;
			};
			
			fixed4 _Color;

			v2f vert(appdata_t IN)
			{
				v2f OUT;
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				return OUT;
			}

			fixed4 frag(v2f IN) : SV_Target
			{
				return _Color;
			}
            ENDCG
		}
	}
}