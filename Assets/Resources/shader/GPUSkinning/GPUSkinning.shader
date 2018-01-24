Shader "GPUSkinning/GPUSkinning"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Color("Color", Color) = (1,1,1,1)

		[HideInInspector] _Mode("__mode", Float) = 0.0
		[HideInInspector] _SrcBlend("__src", Float) = 1.0
		[HideInInspector] _DstBlend("__dst", Float) = 0.0
		[HideInInspector] _ZWrite("__zw", Float) = 1.0
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 300

		Pass
		{
			CGPROGRAM
// Upgrade NOTE: excluded shader from DX11 because it uses wrong array syntax (type[size] name)
#pragma exclude_renderers d3d11
			#pragma target 3.0
			#pragma exclude_renderers gles

			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : 	POSITION;
				float2 uv : 		TEXCOORD0;
				int4   boneIndex : 	TEXCOORD2;
				float4 boneWeight : TEXCOORD3;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;

			fixed4	_Color;

			UNITY_INSTANCING_CBUFFER_START(MatrixPalettes)
				UNITY_DEFINE_INSTANCED_PROP(float4, _MatrixPalette[35 * 3])
			UNITY_INSTANCING_CBUFFER_END
			
			inline float4x4 GetMatrix(int idx)
			{
				int idx *= 3;
				return float4x4(_MatrixPalette[idx], _MatrixPalette[idx + 1], _MatrixPalette[idx + 2], _MatrixPalette[idx + 3]);
			}

			float4 skin4(appdata v)
			{
				// 先用原始的方式实现，如果没问题再抄 cocos 的
				float4x4 matrix0 = GetMatrix(v.boneIndex.x) * v.boneWeight.x;
				matrix0 += GetMatrix(v.boneIndex.y) * v.boneWeight.y;
				matrix0 += GetMatrix(v.boneIndex.z) * v.boneWeight.z;
				matrix0 += GetMatrix(v.boneIndex.w) * v.boneWeight.w;

				return mul(matrix0, v.vertex);
			}

			v2f vert (appdata v)
			{
				v2f o = (v2f)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);

				float4 pos = skin4(v);
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv);
				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG
		}
	}
}
