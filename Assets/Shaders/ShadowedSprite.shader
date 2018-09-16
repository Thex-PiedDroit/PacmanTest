Shader "Custom/ShadowedSprite"
{
	Properties
	{
		_Color("Color", Color) = (1, 1, 1, 1)
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		_AlphaCutout("Alpha Cutout", Range(0, 1)) = 0.5
	}

	SubShader
	{
		Tags
		{
			"Queue" = "Transparent"
			"RenderType" = "TransparentCutout"
		}

		LOD 200

		CGPROGRAM

		#pragma surface Surf Lambert addshadow
		#pragma target 3.0


		sampler2D _MainTex;
		fixed4 _Color;
		half _AlphaCutout;

		struct Input
		{
			fixed4 m_tColor : COLOR0;
			half2 uv_MainTex	: TEXCOORD0;
		};

		void Surf(Input tIn, inout SurfaceOutput tOut)
		{
			fixed4 tColor = tex2D(_MainTex, tIn.uv_MainTex) * _Color;

			tOut.Albedo = tColor;
			tOut.Alpha = tColor.a;

			clip(tOut.Alpha - _AlphaCutout);
		}

		ENDCG
	}

	FallBack "Diffuse"
}
