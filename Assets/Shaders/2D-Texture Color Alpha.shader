//
// Draw a texture as-is with a color tint and alpha blending:
// Source: http://forum.unity3d.com/threads/shaders-for-2d-games.71748/
//
// Use case:
//   Flat planes (quads) with textures that have transparency and need to be colored or faded. 
//   Combine with iTween animations for extremely smooth looking, fading UI elements! :)
//

Shader "2D/Texture Color Alpha"
{  
    Properties
    {
        _Color ("Color Tint", Color) = (1,1,1,1)
        _MainTex ("Texture", 2D) = ""
    }
   
    Category
    {
        ZWrite Off
        Tags {"Queue" = "Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        SubShader
        {
            Pass
            {
                SetTexture[_MainTex] {Combine texture * constant ConstantColor[_Color]}
            }
        }
    }
}
