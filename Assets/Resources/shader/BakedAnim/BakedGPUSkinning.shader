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

            float3x4 DQToMatrix(float4 Qn, float4 Qd)
            {	
                float3x4 M;
                float len2 = dot(Qn, Qn);
                float w = Qn.x, x = Qn.y, y = Qn.z, z = Qn.w;
                float t0 = Qd.x, t1 = Qd.y, t2 = Qd.z, t3 = Qd.w;
        
                M[0][0] = w*w + x*x - y*y - z*z; M[0][1] = 2*x*y - 2*w*z; M[0][2] = 2*x*z + 2*w*y;
                M[1][0] = 2*x*y + 2*w*z; M[1][1] = w*w + y*y - x*x - z*z; M[1][2] = 2*y*z - 2*w*x; 
                M[2][0] = 2*x*z - 2*w*y; M[2][1] = 2*y*z + 2*w*x; M[2][2] = w*w + z*z - x*x - y*y;
    
                M[0][3] = -2*t0*x + 2*w*t1 - 2*t2*z + 2*y*t3;
                M[1][3] = -2*t0*y + 2*t1*z - 2*x*t3 + 2*w*t2;
                M[2][3] = -2*t0*z + 2*x*t2 + 2*w*t3 - 2*t1*y;
    
                M /= len2;
    
                return M;
            }
            inline float2x4 QuatTrans2UDQ(float4 q0, float4 t) {
                // non-dual part (just copy q0):
                float4 vq = float4(0, 0, 0, 0);

               // dual part:
               vq.x = -0.5*(t[0]*q0[1] + t[1]*q0[2] + t[2]*q0[3]);
               vq.y = 0.5*( t[0]*q0[0] + t[1]*q0[3] - t[2]*q0[2]);
               vq.z = 0.5*(-t[0]*q0[3] + t[1]*q0[0] + t[2]*q0[1]);
               vq.w = 0.5*( t[0]*q0[2] - t[1]*q0[1] + t[2]*q0[0]);

               return float2x4(q0, vq);
            }

            inline float2x4 getDualQuaternion(int frameOffset, float boneIndex)
            {
                float matrixOffset = frameOffset + boneIndex * 2;
                float4 rot = tex2Dlod(_BakedAnimTex, indexToUV(matrixOffset));
                float4 pos = tex2Dlod(_BakedAnimTex, indexToUV(matrixOffset + 1));

                return float2x4(rot, pos);
            }

            // 获取某根骨骼的矩阵
            inline float4x4 getBoneMatrix(int frameOffset, float boneIndex)
            {
                float matrixOffset = frameOffset + boneIndex * 2;
                float4 rot = tex2Dlod(_BakedAnimTex, indexToUV(matrixOffset));
                float4 pos = tex2Dlod(_BakedAnimTex, indexToUV(matrixOffset + 1));

                float x = rot.x;
                float y = rot.y;
                float z = rot.z;
                float w = rot.w;

                float xx = x*x;
                float xy = x*y;
                float xz = x*z;
                float xw = x*w;

                float yy = y*y;
                float yz = y*z;
                float yw = y*w;
                float zz = z*z;
                float zw = z*w;
                float ww = w*w;

                float4 row0 = float4(1 - 2 * (yy + zz), 2 * (xy - zw), 2 * (xz + yw), pos.x);
                float4 row1 = float4(2 * (xy + zw), 1 - 2 * (xx + zz), 2 * (yz - xw), pos.y);
                float4 row2 = float4(2 * (xz - yw), 2 * (yz + xw), 1 - 2 * (xx + yy), pos.z);

                float4 row3 = float4(0, 0, 0, 1);

                float4x4 m = float4x4(row0, row1, row2, row3);
                return m;
            }
            // 两根骨骼的蒙皮(采样时强制设置为2)
            inline float3 skin2(appdata v)
            {

                float3 pos = float3(0, 0, 0);

                float4 animParam = UNITY_ACCESS_INSTANCED_PROP(_AnimParam);
                float vx = animParam.z;

                float2x4 dq0 = getDualQuaternion(animParam.x, v.blendIndex.x);
                float2x4 dq1 = getDualQuaternion(animParam.x, v.blendIndex.y);
                float2x4 dq2 = getDualQuaternion(animParam.x, v.blendIndex.z);
                float2x4 dq3 = getDualQuaternion(animParam.x, v.blendIndex.w);
                float2x4 blendDQ = v.blendWeight.x * dq0;
                blendDQ += v.blendWeight.y * dq1;
                blendDQ += v.blendWeight.z * dq2;
                blendDQ += v.blendWeight.w * dq3;
                float3x4 M1 = DQToMatrix(blendDQ[0], blendDQ[1]);
                float3x4 M = M1;
#ifdef CROSS_FADING
                dq0 = getDualQuaternion(animParam.y, v.blendIndex.x);
                dq1 = getDualQuaternion(animParam.y, v.blendIndex.y);
                dq2 = getDualQuaternion(animParam.y, v.blendIndex.z);
                dq3 = getDualQuaternion(animParam.y, v.blendIndex.w);

                blendDQ = v.blendWeight.x * dq0;
                blendDQ += v.blendWeight.y * dq1;
                blendDQ += v.blendWeight.z * dq2;
                blendDQ += v.blendWeight.w * dq3;
                float3x4 M2 = DQToMatrix(blendDQ[0], blendDQ[1]);

                dq0 = getDualQuaternion(animParam.w, v.blendIndex.x);
                dq1 = getDualQuaternion(animParam.w, v.blendIndex.y);
                dq2 = getDualQuaternion(animParam.w, v.blendIndex.z);
                dq3 = getDualQuaternion(animParam.w, v.blendIndex.w);

                blendDQ = v.blendWeight.x * dq0;
                blendDQ += v.blendWeight.y * dq1;
                blendDQ += v.blendWeight.z * dq2;
                blendDQ += v.blendWeight.w * dq3;
                float3x4 M3 = DQToMatrix(blendDQ[0], blendDQ[1]);
                M = lerp(M2, M3, vx);
                M = lerp(M,M1, vx);
#endif

                pos = mul(M, v.vertex);
                return pos;
            }

            v2f vert (appdata v)
            {
                v2f o = (v2f)0;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                
                float3 pos = skin2(v);
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
