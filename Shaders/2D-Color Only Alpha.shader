//
// Draw a solid color texture with alpha:
// Source: http://forum.unity3d.com/threads/shaders-for-2d-games.71748/
//
// Use case:
//   Flat planes (quads) with solid colors that can be faded. Great for placeholder elements or a 
//   full screen fade-to-grey quad behind your pause menu for that extra smooth feel.
//

Shader "2D/Color Only Alpha"
{  
    Properties
    {
        _Color ("Main Color", Color) = (1,1,1,1)
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
                Color [_Color]
            }
        }
    }
}
