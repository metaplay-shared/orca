Shader "Orca/CloudTile"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _MaskTex ("MaskTexture", 2D) = "white" {}
        [PerRendererData]_HoleTex ("HolesTexture", 2D) = "white" {}
        _ScrollSpeed ("Scroll speed", float) = 1
    }
    SubShader
    {
        Tags
        {
            "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"
        }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 main_uv : TEXCOORD0;
                float2 hole_uv : TEXCOORD1;
                float4 vertex : SV_POSITION;
            };

            // Inspector variables, counterparts for the properties (See Properties block above)
            sampler2D _MainTex;
            sampler2D _MaskTex;
            sampler2D _HoleTex;
            float _ScrollSpeed;

            float4 _MainTex_ST;
            float4 _HoleTex_ST;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.main_uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.hole_uv = TRANSFORM_TEX(v.uv, _HoleTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Offset scrolls the cloud texture
                const float offset = (_Time * _ScrollSpeed) % 1;

                // Preprocess mask
                const float mask_col = tex2D(_MaskTex, i.main_uv + offset);
                const float hole_col = tex2D(_HoleTex, i.hole_uv);

                // Sample the cloud texture
                fixed4 col = tex2D(_MainTex, i.main_uv + offset);
                // TODO: both hole and mask could be smaller textures by discarding all the unused channels
                const fixed hole = hole_col.r;
                const fixed mask = mask_col.r;

                // If hole = 1 the mask is fully disabled (because we subtract hole from it)
                // If hole = 0 the mask would be fully enabled but gets counteracted by (1 - hole)
                // Only when hole is between 1 and 0 the mask is effective to some degree,
                // which creates the rough fade-in effect on the hole borders.
                // For a more convincing effect it is better when the mask texture
                // has a different pattern than the main texture.
                col.a *= mask - hole + (1 - hole);

                return col;
            }
            ENDCG
        }
    }
}