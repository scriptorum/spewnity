//
// Draw a texture as-is with alpha blending:
// Source: http://forum.unity3d.com/threads/shaders-for-2d-games.71748/
//
// Use case:
// Flat planes (quads) with textures that have transparency. For example UI elements and/or 2D sprites.
//

Shader "2D/Texture Only Alpha"
{  
    Properties
    {
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
                SetTexture[_MainTex]
            }
        }
    }
}
