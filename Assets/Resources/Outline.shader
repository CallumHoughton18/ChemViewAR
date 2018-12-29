﻿// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Outlined/Silhouetted Diffuse Texture" {
     Properties {
		_Color ("Main Color", Color) = (.5,.5,.5,1)
		_MainTex ("Base (RGB)", 2D) = "white" { }
     }
  
 CGINCLUDE
 #include "UnityCG.cginc"
  
 struct appdata {
     float4 vertex : POSITION;
     float3 normal : NORMAL;
 };
  
 struct v2f {
     float4 pos : POSITION;
     float4 color : COLOR;
 };
  
 uniform float _Outline;
 uniform float4 _OutlineColor;
  
 v2f vert(appdata v) {
     // just make a copy of incoming vertex data but scaled according to normal direction
     v2f o;
     o.pos = UnityObjectToClipPos(v.vertex);
  
     float3 norm   = mul ((float3x3)UNITY_MATRIX_IT_MV, v.normal);
     float2 offset = TransformViewToProjection(norm.xy);
  
     o.pos.xy += offset * o.pos.z * _Outline;
     o.color = _OutlineColor;
     return o;
 }
 ENDCG
  
     SubShader {
         Tags { "Queue" = "Transparent" }
  
         // note that a vertex shader is specified here but its using the one above
         Pass {
             Name "OUTLINE"
             Tags { "LightMode" = "Always" }
             Cull Off
             ZWrite Off
             ColorMask RGB // alpha not used
  
             // you can choose what kind of blending mode you want for the outline
             Blend SrcAlpha OneMinusSrcAlpha // Normal
             //Blend One One // Additive
             //Blend One OneMinusDstColor // Soft Additive
             //Blend DstColor Zero // Multiplicative
             //Blend DstColor SrcColor // 2x Multiplicative
  
             CGPROGRAM
             #pragma vertex vert
             #pragma fragment frag
             
             half4 frag(v2f i) :COLOR {
                 return i.color;
             }
             ENDCG
         }
  
         CGPROGRAM
           #pragma surface surf Lambert
   
           sampler2D _MainTex;
           fixed4 _Color;
   
           struct Input {
               float2 uv_MainTex;
           };
   
           void surf (Input IN, inout SurfaceOutput o) {
               fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
               o.Albedo = c.rgb;
               o.Alpha = c.a;
           }
         ENDCG
     }
  
     SubShader {
         Tags { "Queue" = "Transparent" }
  
         Pass {
             Name "OUTLINE"
             Tags { "LightMode" = "Always" }
             Cull Front
             ZWrite Off
             ColorMask RGB
  
             // you can choose what kind of blending mode you want for the outline
             Blend SrcAlpha OneMinusSrcAlpha // Normal
             //Blend One One // Additive
             //Blend One OneMinusDstColor // Soft Additive
             //Blend DstColor Zero // Multiplicative
             //Blend DstColor SrcColor // 2x Multiplicative
  
             CGPROGRAM
             #pragma vertex vert
             #pragma exclude_renderers gles xbox360 ps3
             ENDCG
             SetTexture [_MainTex] { combine primary }
         }
  
         CGPROGRAM
           #pragma surface surf Lambert
   
           sampler2D _MainTex;
           fixed4 _Color;
   
           struct Input {
               float2 uv_MainTex;
           };
   
           void surf (Input IN, inout SurfaceOutput o) {
               fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
               o.Albedo = c.rgb;
               o.Alpha = c.a;
           }
           ENDCG
     }
  
     Fallback "Diffuse"
 }