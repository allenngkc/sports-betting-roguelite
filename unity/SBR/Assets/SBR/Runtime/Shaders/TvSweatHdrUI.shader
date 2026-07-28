// TV sweat refinement — Part 3 (canvas HDR path).
//
// UGUI's CanvasRenderer bakes Graphic.color into a Color32 vertex attribute (one byte per channel),
// which clamps at 1.0 on write regardless of the camera's HDR setting or the active render pipeline.
// A world-space canvas Text/Image can therefore never exceed 1.0 through the ordinary `.color`
// setter, so DESIGN.md §3's L4 tier had nothing for the shared bloom volume to threshold against
// (room-lead-reply.md §1/§2: "World-space Canvas UI clamping at 1.0 ... this is where the brightness
// ladder actually lives").
//
// This is a copy of Unity's built-in UI/Default shader (same tags, same stencil/clip support, same
// blend state — swapping it in for a Graphic changes nothing else about how that Graphic renders)
// with one addition: `_HdrBoost`, a plain float material property. Material floats are not baked
// into vertex data and are never clamped, so multiplying the (still 0-1) vertex-blended colour by
// this boost is the one part of the pipeline that can genuinely exceed 1.0 and reach the bloom pass.
// Elements that don't need HDR keep the default UI material untouched.
Shader "SBR/TvSweatHdrUI"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _HdrBoost ("HDR Boost (unclamped)", Float) = 1.0

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255

        _ColorMask ("Color Mask", Float) = 15

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
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

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord  : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _HdrBoost;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, OUT);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);

                OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);

                OUT.color = v.color * _Color;
                return OUT;
            }

            sampler2D _GUIClipTexture;
            fixed4 frag(v2f IN) : SV_Target
            {
                half4 color = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;

                // The one line this shader adds over UI/Default: an unclamped brightness multiplier
                // so this Graphic can genuinely exceed 1.0 and read as a light source under bloom.
                // Alpha is left alone — boosting alpha would just fade the flood out faster, not
                // brighten it, and would fight the flood's own peakAlpha animation curve.
                color.rgb *= max(0.0, _HdrBoost);

                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip (color.a - 0.001);
                #endif

                return color;
            }
        ENDCG
        }
    }
}
