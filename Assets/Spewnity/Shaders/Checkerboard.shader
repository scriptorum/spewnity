//
// http://www.shaderslab.com/demo-52---checkerboard.html
//
Shader "Spewnity/Checkerboard"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Color ("Color", color) = (1, 1, 1, 1)
		[PowerSlider(1.0)] _Val ("Size", Range(0.0, 1)) = 0
		[PowerSlider(1.0)] _Tint ("Tint", Range(0.0, 1)) = 0
	}

	SubShader
	{
		Pass
		{
			Tags { "RenderType"="Transparent" }
			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			fixed4 _Color;
			half _Val;
			half _Tint;

			fixed4 frag(v2f_img i) : SV_Target 
			{
				float2 val = floor(i.pos.xy * _Val) * 0.5;
				fixed4 result = tex2D(_MainTex, i.uv);

				if(frac(val.x + val.y) > 0)
				{
					result.rgb = result.rgb * (1 - _Tint) + _Color * _Tint;
					result.a *= _Color.a;
				}

				return result;
			}

			ENDCG
		}
	}
}