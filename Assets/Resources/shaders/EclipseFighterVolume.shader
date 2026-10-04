Shader "Eclipse/FighterVolume"
{
    Properties { _Color ("Surface", Color) = (.03,.03,.025,1) _EdgeColor ("Edge", Color) = (.85,.75,.58,1) _Exposure ("Exposure", Float) = 1 }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        Cull Off ZWrite On ZTest LEqual
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            struct appdata { float4 vertex : POSITION; float3 normal : NORMAL; };
            struct v2f { float4 vertex : SV_POSITION; float3 normal : TEXCOORD0; float3 world : TEXCOORD1; };
            fixed4 _Color, _EdgeColor;
            float _Exposure;
            v2f vert(appdata v)
            {
                v2f o; o.vertex = UnityObjectToClipPos(v.vertex);
                o.normal = UnityObjectToWorldNormal(v.normal);
                o.world = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }
            fixed4 frag(v2f i, fixed facing : VFACE) : SV_Target
            {
                float3 n = normalize(i.normal) * (facing >= 0 ? 1 : -1);
                float3 view = normalize(_WorldSpaceCameraPos - i.world);
                // The native world and reflected camera use downward-positive Y.
                float3 key = normalize(float3(-.45,-.75,-.6));
                float diffuse = smoothstep(-.35,.85,dot(n,key));
                float light = .09 + diffuse * 1.15;
                float rim = pow(1-saturate(dot(n,view)),4) * saturate(dot(n,key));
                // Matte soft key and a faint warm directional edge; shaded areas
                // retain solid ink instead of outlining every primitive.
                return fixed4((_Color.rgb*light + rim*_EdgeColor.rgb*.008)*_Exposure,_Color.a);
            }
            ENDCG
        }
    }
}
