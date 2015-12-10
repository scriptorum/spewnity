//
// Draw a texture as-is with no hassle:
// Source: http://forum.unity3d.com/threads/shaders-for-2d-games.71748/
//
// Use case:
//  Flat planes (quads) with textures. A background image in a 2D game perhaps?
//

Shader "2D/Texture Only"
{  
    Properties
    {
        _MainTex ("Texture", 2D) = ""
    }
 
    SubShader
    {
        ZWrite On // "Off" might make more sense in very specific games
        Pass
        {
            SetTexture[_MainTex]
        }
    }
}
