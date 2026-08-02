Shader "SBR/TonerGrain"
{
    // The document's own toner grain (palette-surething.css --toner-grain-opacity).
    //
    // Why this needs a shader at all: the first attempt was an ordinary UI Image of white noise at
    // 5% alpha. Under normal alpha blending a white overlay can only ever ADD light, so instead of
    // texturing the sheet it bleached it — the ground measured (24,24,16) before and (52,52,48)
    // after, more than double the luminance and neutral grey where the ground is warm olive. Real
    // grain has to darken as well as lighten, and no fixed-function alpha blend can do that.
    //
    // Blend DstColor SrcColor is 2 x Dst x Src: a source of exactly 0.5 grey leaves the destination
    // untouched, above 0.5 lightens it and below 0.5 darkens it. So noise centred on 0.5 has a mean
    // effect of zero. The ground stays where the palette put it and only its texture changes, which
    // is the whole point of a grain pass.
    Properties
    {
        [PerRendererData] _MainTex ("Noise", 2D) = "gray" {}
        _Strength ("Strength", Range(0,1)) = 0.05
        _Color ("Tint (unused, UI plumbing)", Color) = (1,1,1,1)
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend DstColor SrcColor

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _MainTex_ST;
            float _Strength;

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                // Noise is authored around 0.5. Pulling it back toward 0.5 by _Strength is what
                // makes the pass subtle without changing its mean — lerp(0.5, n, s) keeps the
                // midpoint fixed, so lowering strength fades the grain out rather than tinting.
                half n = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv).r;
                half signedGrain = lerp(0.5h, n, _Strength);
                return half4(signedGrain, signedGrain, signedGrain, 1.0h);
            }
            ENDHLSL
        }
    }

    Fallback Off
}
