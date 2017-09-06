Shader "Spewnity/Scrolling"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_ScrollX ("Scroll X", float) = 0.0
		_ScrollY ("Scroll Y", float) = 0.0
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
			float _ScrollX;
			float _ScrollY;
			
			fixed4 frag (v2f_img i) : SV_Target
			{
				half2 uv2 = i.uv + half2(-_ScrollX * _Time.y, -_ScrollY * _Time.y);

				if(uv2.x < 0 || uv2.x > 1) uv2.x -= floor(uv2.x);
				if(uv2.y < 0 || uv2.y > 1) uv2.y -= floor(uv2.y);

				fixed4 result = tex2D(_MainTex, i.uv);
				result.rgb = tex2D(_MainTex, uv2).rgb;

				return result;
			}
			ENDCG
		}
	}
}