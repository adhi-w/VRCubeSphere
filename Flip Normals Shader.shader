
// This is unlit shaders that flip the normal
// !!! Note: The mirrored-outside option is specifically used for Equirectangular mapping
// otherwise you have to rearrange the uv in mesh generator

Shader "Inverted Flip Normal" {
	Properties{
		_MainTex("Base (RGB)", 2D) = "white" {}
	
	}

		SubShader{
			Tags { "RenderType" = "Opaque" }			
			LOD 100
		
			Pass {
				CGPROGRAM
					#pragma vertex vert
					#pragma fragment frag
					// make fog work
					#pragma multi_compile_fog

					#include "UnityCG.cginc"

					struct appdata_t {
						float4 vertex : POSITION;
						float2 texcoord : TEXCOORD0;
					};

					struct v2f {
						float4 vertex : SV_POSITION;
						half2 texcoord : TEXCOORD0;
					};
					
					//
					sampler2D _MainTex;
					float4 _MainTex_ST;

					v2f vert(appdata_t v) {
						v2f o;
						o.vertex = UnityObjectToClipPos(v.vertex);						
					//	v.texcoord.x = v.texcoord.x;		// Mirrored outside, Normal Inside
						v.texcoord.x = 1 - v.texcoord.x;	// Normal outside, Mirrored Inside
						o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
						UNITY_TRANSFER_FOG(o, o.vertex); 
						return o;
					}

					fixed4 frag(v2f i) : SV_Target {
						fixed4 col = tex2D(_MainTex, i.texcoord);
						// apply fog
						UNITY_APPLY_FOG(i.fogCoord, col);
						return col;
					}

				ENDCG
				}
	}
}
