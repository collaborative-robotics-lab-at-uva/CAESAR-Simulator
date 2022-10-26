 Shader "Custom/Grayscale"
 {
     Properties
     {
         _MainTex("MainTex", 2D) = "white" {}
     }
 
     SubShader
     {
         // No culling or depth
         Cull Off ZWrite Off ZTest Always
 
         Blend SrcAlpha OneMinusSrcAlpha
         Tags{ "Queue" = "Transparent" }
 
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
                 float2 uv : TEXCOORD0;
                 float4 vertex : SV_POSITION;
             };
 
             v2f vert(appdata v)
             {
                 v2f o;
                 o.vertex = UnityObjectToClipPos(v.vertex);
                 o.uv = v.uv;
                 return o;
             }
             sampler2D _MainTex;
             
 
             fixed4 frag(v2f i) : SV_Target
             {
                 fixed4 color = tex2D(_MainTex, i.uv);
                 
             
             float greyscaleAverage = (color.r + color.g + color.b) / 3.0f + .5f;
 
                 return greyscaleAverage;
             }
             ENDCG
         }
     }
 }