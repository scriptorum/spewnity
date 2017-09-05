//
// http://www.shaderslab.com/demo-38---vhs-tape-effect.html
//
Shader "Spewnity/Ripple"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Ripple ("Ripple Amount", Range(.3, .99)) = 0.5		
	}
	
	SubShader
	{
		Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Blend SrcAlpha OneMinusSrcAlpha
	
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				float2 uv2 : TEXCOORD1;
			};


			v2f vert (appdata_base v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = v.texcoord;
				return o;
			}
			
			sampler2D _MainTex;
			half _Ripple;

			fixed4 frag (v2f i) : SV_Target
			{
				if(_Ripple > 0.3)
					i.uv = float2(frac(i.uv.x + cos((i.uv.y + _CosTime.y) * 100) / ((1 - _Ripple) * 500)), frac(i.uv.y));
				return tex2D(_MainTex, i.uv);
			}
			ENDCG
		}
	}
}