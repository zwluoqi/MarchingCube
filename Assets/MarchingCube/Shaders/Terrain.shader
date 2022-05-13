Shader "MarchingCube/Terrain"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        normalOffsetWeight("normalOffsetWeight",float) = 0
        minMax("minMax",Vector) = (0,0,0,0)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 normal : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;

                float3 wsPos : TEXCOORD1;
                float3 wsNormal : TEXCOORD2;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float normalOffsetWeight;
            float2 minMax;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                o.wsNormal = UnityObjectToWorldNormal(v.normal);
                o.wsPos = mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1.0)).xyz;
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                // return float4(i.wsNormal.xyz,1);
                float height = i.wsPos.y + i.wsNormal.y * normalOffsetWeight;
                float h = smoothstep( minMax.x,minMax.y,height);
                
                fixed4 col = tex2D(_MainTex, float2(h,0.5f));
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
