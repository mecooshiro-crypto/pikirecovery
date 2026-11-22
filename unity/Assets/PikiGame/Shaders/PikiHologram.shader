// PIKI RECOVERY - Holograma: nucleo oscuro + bordes brillantes (fresnel) + lineas de escaneo.
// Shader sin iluminacion: funciona en Built-in y en URP.
Shader "Piki/Hologram"
{
    Properties
    {
        _Color ("Rim color", Color) = (0.25, 0.9, 1, 1)
        _Core ("Core color", Color) = (0.02, 0.12, 0.22, 0.55)
        _RimPower ("Rim power", Range(0.5, 8)) = 2.4
        _RimBoost ("Rim boost", Range(0, 4)) = 1.8
        _Lines ("Lines per meter", Float) = 70
        _Speed ("Line speed", Float) = 1.2
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True" }
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Back
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            fixed4 _Color, _Core; float _RimPower, _RimBoost, _Lines, _Speed;
            struct appdata { float4 vertex : POSITION; float3 normal : NORMAL; };
            struct v2f { float4 pos : SV_POSITION; float3 n : TEXCOORD0; float3 v : TEXCOORD1; float wy : TEXCOORD2; };
            v2f vert (appdata i)
            {
                v2f o; o.pos = UnityObjectToClipPos(i.vertex);
                float3 wp = mul(unity_ObjectToWorld, i.vertex).xyz;
                o.n = UnityObjectToWorldNormal(i.normal); o.v = _WorldSpaceCameraPos - wp; o.wy = wp.y; return o;
            }
            fixed4 frag (v2f i) : SV_Target
            {
                float rim = 1 - saturate(dot(normalize(i.n), normalize(i.v)));
                rim = pow(rim, _RimPower);
                float lines = 0.5 + 0.5 * sin((i.wy * _Lines - _Time.y * _Speed) * 6.2831853);
                lines = lerp(0.78, 1.0, lines);
                fixed3 col = _Core.rgb + _Color.rgb * (rim * _RimBoost + 0.18);
                float a = saturate(_Core.a + rim * _Color.a) * lines;
                return fixed4(col * lines, a);
            }
            ENDCG
        }
    }
    FallBack "Sprites/Default"
}
