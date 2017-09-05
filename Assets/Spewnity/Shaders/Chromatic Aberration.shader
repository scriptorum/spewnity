//
// http://www.shaderslab.com/demo-78---chromatic-aberration.html
//
Shader "Spewnity/Chromatic Aberration"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Source ("Source Alpha", Range(0.0, 1.0)) = 0.0

		[Header(Red)]
		_RedX ("Offset X", Range(-0.5, 0.5)) = 0.0
		_RedY ("Offset Y", Range(-0.5, 0.5)) = 0.0

		[Header(Green)]
		_GreenX ("Offset X", Range(-0.5, 0.5)) = 0.0
		_GreenY ("Offset Y", Range(-0.5, 0.5)) = 0.0

		[Header(Blue)]
		_BlueX ("Offset X", Range(-0.5, 0.5)) = 0.0
		_BlueY ("Offset Y", Range(-0.5, 0.5)) = 0.0
	}

	SubShader
	{
		Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			float _Source;
			float _RedX;
			float _RedY;
			float _GreenX;
			float _GreenY;
			float _BlueX;
			float _BlueY;
			
			fixed4 frag (v2f_img i) : SV_Target
			{
				fixed4 result = fixed4(1, 1, 1, 1);

				float2 red_uv = i.uv + float2(_RedX, _RedY);
				fixed4 red_col = tex2D(_MainTex, red_uv);
				result.r = red_col.r * red_col.a;

				float2 green_uv = i.uv + float2(_GreenX, _GreenY);
				fixed4 green_col = tex2D(_MainTex, green_uv);
				result.g = green_col.g * green_col.a;

				float2 blue_uv = i.uv + float2(_BlueX, _BlueY);
				fixed4 blue_col = tex2D(_MainTex, blue_uv);
				result.b = blue_col.b * blue_col.a;

				fixed4 source = tex2D(_MainTex, i.uv);
				result.rgb = source.rgb * _Source + result.rgb * (1 - _Source);
				
				// result.a = tex2D(_MainTex, i.uv).a;
				
				result.a = red_col.a;
				if(green_col.a > result.a)
					result.a = green_col.a;
				if(blue_col.a > result.a)
					result.a = blue_col.a;

				return result;
			}
			ENDCG
		}
	}
}