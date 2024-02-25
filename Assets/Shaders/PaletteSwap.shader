Shader "Custom/PaletteSwap"
{
	// Based on work by SPG-Vulpine https://discussions.unity.com/t/shader-changing-sprite-colour-palette/253464

	Properties
	{
		_MainTex("Texture", 2D) = "white" {}

		_DefaultPaletteTex("Default Palette", 2D) = "white"
		_CustomPaletteTex("Custom Palette", 2D) = "white"
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		Blend SrcAlpha OneMinusSrcAlpha

		Tags
		{
			"Queue" = "Transparent"
		}

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			
			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}

			sampler2D _MainTex;
			sampler2D _DefaultPaletteTex;
			sampler2D _CustomPaletteTex;

			float4 _DefaultPaletteTex_TexelSize;
			float4 _CustomPaletteTex_TexelSize;

			struct Input {
				float2 uv_MainTex;
			};

			#define INC 0.0625 // 1/16
			fixed4 frag(v2f i) : SV_Target
			{
				float4 main = tex2D(_MainTex, i.uv);
				for (float _DefaultPalette_x = 0; _DefaultPalette_x < 1; _DefaultPalette_x += INC)
				{
					float4 palette = tex2D(_DefaultPaletteTex, float2(_DefaultPalette_x, 0));

					if (main.r == palette.r && main.g == palette.g && main.b == palette.b && main.a == palette.a)
					{
						float4 c = tex2D(_CustomPaletteTex, float2(_DefaultPalette_x, 0));
						return c;
					}
				}
				return main;
			}
			ENDCG
		}
	}
}
