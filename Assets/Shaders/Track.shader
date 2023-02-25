Shader "Gameplay/Track"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Offset ("Offset", Float) = 1
		_Color ("Color", Color) = (1,1,1,1)
		_Modify("Modify", Vector) = (0, 0, 1, 1)
		_EdgeLAlpha ("Left edge", Float) = 1
		_Lane1Alpha ("Lane 1", Float) = 1
		_Lane2Alpha ("Lane 2", Float) = 1
		_Lane3Alpha ("Lane 3", Float) = 1
		_Lane4Alpha ("Lane 4", Float) = 1
		_EdgeRAlpha ("Right edge", Float) = 1

		_BoundaryL1 ("BoundaryL1", Float) = 0
		_Boundary12 ("Boundary12", Float) = 0
		_Boundary23 ("Boundary23", Float) = 0
		_Boundary34 ("Boundary34", Float) = 0
		_Boundary4R ("Boundary4R", Float) = 0
	}
	SubShader
	{
		Tags { "Queue" = "Transparent"  "RenderType"="Transparent" "CanUseSpriteAtlas"="true"  }
		ZWrite Off
		ZTest Always
		Cull Off
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float4 color : COLOR;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				fixed4 color : COLOR;
				float2 uv : TEXCOORD0;
			};

			float _Offset;
			sampler2D _MainTex;
			fixed4 _Color;
			float4 _Modify;
			float _EdgeLAlpha, _EdgeRAlpha, _Lane1Alpha, _Lane2Alpha, _Lane3Alpha, _Lane4Alpha;
			float _BoundaryL1, _Boundary12, _Boundary23, _Boundary34, _Boundary4R;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.color = v.color * _Color;
				o.uv = v.uv;
				return o;
			}
			
			half4 frag (v2f i) : SV_Target
			{
			    float2 p = i.uv;
				p.x += _Modify.x;
				p.x *= _Modify.z;
				p.x %= 1;
				p.y += _Modify.y;
				p.y *= _Modify.w;
				p.y %= 1;
				p.y += + _Offset; 

				float alphaModify = 1;
				if (p.x <= _BoundaryL1) alphaModify = _EdgeLAlpha;
				else if (p.x <= _Boundary12) alphaModify = _Lane1Alpha;
				else if (p.x <= _Boundary23) alphaModify = _Lane2Alpha;
				else if (p.x <= _Boundary34) alphaModify = _Lane3Alpha;
				else if (p.x <= _Boundary4R) alphaModify = _Lane4Alpha;
				else alphaModify = _EdgeRAlpha;

				float4 c= tex2D(_MainTex,p) * i.color;
				c.a *= alphaModify;
				return c;
			}
			ENDCG
		}
	}
}
