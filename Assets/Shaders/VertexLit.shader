Shader "Custom/VertexLit"
{
	Properties
	{

	}

	SubShader
	{
		LOD 200

		CGPROGRAM

		#pragma surface Surf Lambert addshadow
		#pragma target 3.0


		struct Input
		{
			fixed4 m_tColor		: COLOR0;
		};

		void Surf(Input tIn, inout SurfaceOutput tOut)
		{
			tOut.Albedo = tIn.m_tColor;
			tOut.Alpha = 1.0f;
		}

		ENDCG
	}

	FallBack "Diffuse"
}
