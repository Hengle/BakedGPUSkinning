Shader "GPUSkinning/BakedGPUSkinning"
{
	Properties
	{
		_Color("Color", Color) = (1,1,1,1)
		_MainTex ("Texture", 2D) = "white" {}

		[HideInInspector] _Mode("__mode", Float) = 0.0
		[HideInInspector] _SrcBlend("__src", Float) = 1.0
		[HideInInspector] _DstBlend("__dst", Float) = 0.0
		[HideInInspector] _ZWrite("__zw", Float) = 1.0
	}

	SubShader
	{
		Tags { "RenderType"="Opaque" "PerformanceChecks"="False" }
		LOD 300

		Pass
		{
			CGPROGRAM
			#pragma target 3.0
			#pragma exclude_renderers gles

			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog
			#pragma multi_compile_instancing
			#pragma multi_compile __ CROSS_FADING
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float4 blendIndex : TEXCOORD2;
				float4 blendWeight : TEXCOORD3;
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

			uniform sampler2D 	_BakedAnimTex;  	// 存储 BakeAnim 的纹理
			uniform float2 		_BakedAnimTexWH; 	// 纹理的宽高(像素)
			
			UNITY_INSTANCING_CBUFFER_START(MyProperties)
				UNITY_DEFINE_INSTANCED_PROP(float4, _AnimParam) // frameOffset, crossFadeOffset, percent(1.0时crossFade消失)
			UNITY_INSTANCING_CBUFFER_END

			
			// 转换一维索引到uv
			inline float4 indexToUV(float index)
			{
				int row = (int)(index / _BakedAnimTexWH.x);
				int col = index % _BakedAnimTexWH.x;
				return float4(col / _BakedAnimTexWH.x, row / _BakedAnimTexWH.y, 0, 0);
			}

			// 获取某根骨骼的矩阵
			inline float4x4 getBoneMatrix(int frameOffset, float boneIndex)
			{
				float matrixOffset = frameOffset + boneIndex * 3;
				float4 row0 = tex2Dlod(_BakedAnimTex, indexToUV(matrixOffset));
				float4 row1 = tex2Dlod(_BakedAnimTex, indexToUV(matrixOffset + 1));
				float4 row2 = tex2Dlod(_BakedAnimTex, indexToUV(matrixOffset + 2));
				float4 row3 = float4(0, 0, 0, 1);
				float4x4 m = float4x4(row0, row1, row2, row3);
				return m;
			}

			// 两根骨骼的蒙皮(采样时强制设置为2)
			inline float4 skin2(appdata v)
			{
				float4 animParam = UNITY_ACCESS_INSTANCED_PROP(_AnimParam);
				float4x4 matrix0 = getBoneMatrix(animParam.x, v.blendIndex.x);
				float4x4 matrix1 = getBoneMatrix(animParam.x, v.blendIndex.y);
				float4x4 matrix2 = getBoneMatrix(animParam.x, v.blendIndex.z);
				float4x4 matrix3 = getBoneMatrix(animParam.x, v.blendIndex.w);
				float4 pos = mul(matrix0, v.vertex) * v.blendWeight.x + mul(matrix1, v.vertex) * v.blendWeight.y + mul(matrix2, v.vertex) * v.blendWeight.z + mul(matrix3, v.vertex) * v.blendWeight.w;
				// matrix0 = matrix0 * v.blendWeight.x + matrix1 * v.blendWeight.y + matrix2 * v.blendWeight.z + matrix3 * v.blendWeight.w;
				// float4 pos = mul(matrix0, v.vertex);

#ifdef CROSS_FADING
				float4x4 matrixCF0 = getBoneMatrix(animParam.y, v.blendIndex.x);
				float4x4 matrixCF1 = getBoneMatrix(animParam.y, v.blendIndex.y);
				float4x4 matrixCF2 = getBoneMatrix(animParam.y, v.blendIndex.z);
				float4x4 matrixCF3 = getBoneMatrix(animParam.y, v.blendIndex.w);
				float4 posCF = mul(matrixCF0, v.vertex) * v.blendWeight.x + mul(matrixCF1, v.vertex) * v.blendWeight.y + mul(matrixCF2, v.vertex) * v.blendWeight.z; + mul(matrixCF3, v.vertex) * v.blendWeight.w;;
				// matrixCF0 = matrixCF0 * v.blendWeight.x + matrixCF1 * v.blendWeight.y + matrixCF2 * v.blendWeight.z + matrixCF3 * v.blendWeight.w;
				// float4 posCF = mul(matrixCF0, v.vertex);
				pos = posCF + (pos - posCF) * animParam.z;
#endif
				return pos;
			}

			v2f vert (appdata v)
			{
				v2f o = (v2f)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				
				float4 pos = skin2(v);
				o.vertex = UnityObjectToClipPos(pos);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(i);
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
