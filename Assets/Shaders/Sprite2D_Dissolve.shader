Shader "Custom/Sprite2D_Dissolve"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        
        // Dissolve Properties
        _DissolveTex ("Dissolve Texture", 2D) = "white" {}
        _DissolveAmount ("Dissolve Amount", Range(0, 1)) = 0.0
        _DissolveEdgeColor ("Edge Color", Color) = (1,0.5,0,1)
        _DissolveEdgeWidth ("Edge Width", Range(0, 0.5)) = 0.1
        _DissolveEdgeIntensity ("Edge Intensity", Range(0, 5)) = 2.0
        
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)
        [PerRendererData] _AlphaTex ("External Alpha", 2D) = "white" {}
        [PerRendererData] _EnableExternalAlpha ("Enable External Alpha", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex SpriteVert
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile_instancing
            #pragma multi_compile_local _ PIXELSNAP_ON
            #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
            #include "UnitySprites.cginc"

            sampler2D _DissolveTex;
            float _DissolveAmount;
            fixed4 _DissolveEdgeColor;
            float _DissolveEdgeWidth;
            float _DissolveEdgeIntensity;

            fixed4 frag(v2f IN) : SV_Target
            {
                fixed4 c = SampleSpriteTexture(IN.texcoord) * IN.color;
                
                // Sample dissolve texture
                float dissolve = tex2D(_DissolveTex, IN.texcoord).r;
                
                // Calculate dissolve
                float dissolveEdge = _DissolveAmount - dissolve;
                
                // Discard pixels based on dissolve
                if(dissolveEdge > 0)
                {
                    discard;
                }
                
                // Add edge glow
                if(dissolveEdge > -_DissolveEdgeWidth)
                {
                    float edgeFactor = 1.0 - (abs(dissolveEdge) / _DissolveEdgeWidth);
                    c.rgb += _DissolveEdgeColor.rgb * edgeFactor * _DissolveEdgeIntensity;
                }
                
                c.rgb *= c.a;
                return c;
            }
            ENDCG
        }
    }
}
