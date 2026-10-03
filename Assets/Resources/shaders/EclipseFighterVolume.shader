Shader "Eclipse/FighterVolume"
{
    Properties { _Color ("Surface", Color) = (.2,.25,.3,1) }
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
            fixed4 _Color;
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
                float light = .35 + .85 * saturate(dot(n,key));
                float rim = pow(1 - saturate(dot(n,view)),3);
                float spec = pow(saturate(dot(n,normalize(key+view))),28);
                return fixed4(_Color.rgb * light + rim * float3(.08,.13,.18) + spec * .22, _Color.a);
            }
            ENDCG
        }
    }
}
