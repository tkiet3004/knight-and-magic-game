Shader "Custom/Sprite2D_Glow"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        
        // Glow Properties
        _GlowColor ("Glow Color", Color) = (1,1,0,1)
        _GlowIntensity ("Glow Intensity", Range(0, 10)) = 1.5
        _GlowSpeed ("Glow Speed", Range(0, 10)) = 2.0
        _GlowMin ("Glow Min", Range(0, 1)) = 0.3
        _GlowMax ("Glow Max", Range(0, 1)) = 1.0
        _GlowRadius ("Glow Radius", Range(0, 0.5)) = 0.1
        
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

            fixed4 _GlowColor;
            float _GlowIntensity;
            float _GlowSpeed;
            float _GlowMin;
            float _GlowMax;
            float _GlowRadius;

            fixed4 frag(v2f IN) : SV_Target
            {
                fixed4 c = SampleSpriteTexture(IN.texcoord) * IN.color;
                
                // Calculate pulsing glow
                float pulse = sin(_Time.y * _GlowSpeed) * 0.5 + 0.5;
                pulse = lerp(_GlowMin, _GlowMax, pulse);
                
                // Multi-sample glow with varying distances for smoother effect
                float outerGlow = 0.0;
                float samples = 16.0; // Tăng samples để mượt hơn
                float layers = 3.0; // Multiple layers cho depth
                
                for(float layer = 1; layer <= layers; layer++)
                {
                    float layerRadius = _GlowRadius * (layer / layers);
                    for(float i = 0; i < samples; i++)
                    {
                        float angle = (i / samples) * 6.28318; // 2*PI
                        float2 offset = float2(cos(angle), sin(angle)) * layerRadius;
                        outerGlow += SampleSpriteTexture(IN.texcoord + offset).a;
                    }
                }
                outerGlow /= (samples * layers);
                outerGlow = pow(outerGlow, 0.5); // Gamma correction cho glow mượt hơn
                outerGlow *= (1.0 - c.a); // Only apply outside sprite
                
                // Apply glow
                fixed4 glowColor = _GlowColor * _GlowIntensity * pulse;
                c.rgb += glowColor.rgb * c.a; // Inner glow (on sprite)
                c.rgb += glowColor.rgb * outerGlow * 0.8; // Outer glow (around sprite), giảm intensity 1 chút
                c.a = saturate(c.a + outerGlow * 0.7);
                
                c.rgb *= c.a;
                return c;
            }
            ENDCG
        }
    }
}
