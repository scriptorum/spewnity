
//
// https://forum.unity3d.com/threads/uv-offset-region-shader.266952/
//
Shader "Spewnity/Region Offset" 
{
    Properties 
    {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _PreviewColor ("Preview Color", Color) = (1, 0, 0, 0.5)
        _RectMinX ("Rectangle Min X (Percent)", Float) = 25
        _RectMaxX ("Rectangle Max X (Percent)", Float) = 75
        _RectMinY ("Rectangle Min y (Percent)", Float) = 25
        _RectMaxY ("Rectangle Max Y (Percent)", Float) = 75
       
        _OffsetU ("Offset U", Float) = 0
        _OffsetV ("Offset V", Float) = 0       
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
            fixed4 _PreviewColor; // set to 0 alpha to turn off region preview
        
            fixed _RectMinX;
            fixed _RectMaxX;
            fixed _RectMinY;
            fixed _RectMaxY;
        
            fixed _OffsetU;
            fixed _OffsetV;
            
            fixed4 frag (v2f_img i) : SV_Target
            {
                //Below divides all rectangle parameters by 100, so they are not super-sensitive in the unity editor
                _RectMinX /= 100;
                _RectMaxX /= 100;
                _RectMinY /= 100;
                _RectMaxY /= 100;
            
                //preRect x and y each represent 1 dimensional min and max ranges for rectangle. When they are multiplied together, they form a white rectangle mask (where they intersect).
                float2 preRect;
                preRect.x = (i.uv.x > _RectMinX) - (i.uv.x > _RectMaxX);
                preRect.y = (i.uv.y > _RectMinY) - (i.uv.y > _RectMaxY);
                half rectMask = preRect.x * preRect.y;
            
                //uv_OffsetCoord.x and y copy the uv coordinates of the main texture and are offsetted.
                //Then, the old uv coordinates are blended with the new uv coordinates, using the rectangle as a mask.
                float2 uv_OffsetCoord = i.uv;
            
                //add minimum rectangle limits so the region's lowest UV value is 0
                uv_OffsetCoord.x -= _RectMinX;
                uv_OffsetCoord.y -= _RectMinY;
            
                //multiply values so the highest UV value of the region is 1. Now the region is normalized to a 0-1 range.
                uv_OffsetCoord.x *= (1 / ( _RectMaxX - _RectMinX ) );
                uv_OffsetCoord.y *= (1 / ( _RectMaxY - _RectMinY ) );
            
            
                //Offset the newly normalized coordinates.  
                uv_OffsetCoord.x += _OffsetU;
                uv_OffsetCoord.y += _OffsetV;
            
                //So now, the problem is, offsetting will cause the UV values will go lower than 0 or higher than 1.
                //Well, fortunately, we can use frac() to continuously repeat the texture (between 0 and 1) forever!
                uv_OffsetCoord.x = frac(uv_OffsetCoord.x);
                uv_OffsetCoord.y = frac(uv_OffsetCoord.y);
            
                //Below runs the normalization process in reverse
                uv_OffsetCoord.x *= ( _RectMaxX - _RectMinX );
                uv_OffsetCoord.y *= ( _RectMaxY - _RectMinY );
            
                uv_OffsetCoord.x += _RectMinX;
                uv_OffsetCoord.y += _RectMinY;
            
                //Blend old uv coordinates with new offsetted uv coordinates, using the rectangle as a mask
                float2 blend_uv = (i.uv * (1-rectMask) ) + (uv_OffsetCoord * rectMask);
            
                //Apply image map to blended UV coordinates
                fixed4 result = tex2D (_MainTex, blend_uv);
            
                if(rectMask >= 0.5)
    				result.rgb = _PreviewColor.rgb * _PreviewColor.a + result.rgb * (1 - _PreviewColor.a);            
                result.a = tex2D(_MainTex, i.uv).a;

                return result;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}