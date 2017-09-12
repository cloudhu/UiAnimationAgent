// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "UI/ImageEffectShader"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color("Tint", Color) = (1.000000,1.000000,1.000000,1.000000)
		_StencilComp("Stencil Comparison", Float) = 8.000000
		_Stencil("Stencil ID", Float) = 0.000000
		_StencilOp("Stencil Operation", Float) = 0.000000
		_StencilWriteMask("Stencil Write Mask", Float) = 255.000000
		_StencilReadMask("Stencil Read Mask", Float) = 255.000000
		_ColorMask("Color Mask", Float) = 15.000000
		[Toggle(UNITY_UI_ALPHACLIP)]  _UseUIAlphaClip("Use Alpha Clip", Float) = 1.000000
	}
	SubShader
	{
		Tags{ "QUEUE" = "Transparent" "IGNOREPROJECTOR" = "true" "RenderType" = "Transparent" "PreviewType" = "Plane" "CanUseSpriteAtlas" = "true" }

		Stencil
		{
			Ref[_Stencil]
			Comp[_StencilComp]
			Pass[_StencilOp]
			ReadMask[_StencilReadMask]
			WriteMask[_StencilWriteMask]
		}

		// No culling or depth
		Cull Off Lighting Off ZWrite Off ZTest[unity_GUIZTestMode]
		Blend SrcAlpha OneMinusSrcAlpha
		ColorMask[_ColorMask]

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 color : COLOR;
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				fixed4 color : COLOR;
				float4 vertex : SV_POSITION;
			};

			int _type;
			fixed4 _Color;

			v2f vert(appdata IN)
			{
				v2f OUT;
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.uv = IN.uv;
#ifdef UNITY_HALF_TEXEL_OFFSET  
				OUT.vertex.xy += (_ScreenParams.zw - 1.0)*float2(-1, 1);
#endif  
				OUT.color = IN.color * _Color;
				return OUT;
			}
			
			sampler2D _MainTex;

			fixed4 frag(v2f IN) : SV_Target
			{
				half4 color = tex2D(_MainTex, IN.uv) * IN.color;
				clip(color.a - 0.01);

				if (_type == 1)
				{
					color = color * 1.8f;
				}
				else if (_type == 2)
				{
					color = color * 1.0f;
				}
				else if (_type == 3)
				{
					float grey = dot(color.rgb, fixed3(0.22, 0.707, 0.071));
					color = half4(grey,grey,grey,color.a);
				}
				return color;
			}
			ENDCG
		}
	}
}
