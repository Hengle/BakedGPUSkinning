// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

/*
Shader "epic/unit/color3_rim"
{
	Properties
	{
		_Alpha("Transparent", Range(0,1)) = 1
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_Color("Color", Color) = (1,1,1)
		_MainColorIntensity("Color Intensity", Range(0,1)) = 1
				
		_RimPower("Rim Power", Range(0,5)) = 1
		_FresnelColor("Hit Color", Color) = (1,1,1)
		_FresnelPower("Hit Power", Range(0,3)) = 0			

		_MatCap("MatCap (RGB:color, Alpha:Rim Mask)", 2D) = "white" {}
		_MatIntensity("Mat Intensity", Range(0, 2)) = 1.18
	}

	SubShader
	{
			Tags
		{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
		}

		Blend SrcAlpha OneMinusSrcAlpha
		Cull back Lighting Off
		LOD 400

		pass
		{
			NAME "FORWARD"
			Tags{ "LightMode" = "Always" }

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag ARB_precision_hint_fastest

			#include "UnityCG.cginc"


			sampler2D _MainTex;			
			sampler2D _MatCap;
			sampler2D _MaskTex;
			fixed _Alpha;
			fixed3 _Color;
			fixed _MainColorIntensity;
			fixed _RimPower;			
			fixed3 _FresnelColor;
			fixed _FresnelPower;
			fixed _MatIntensity;

			struct v2f
			{
				float4 vertex	: SV_POSITION;
				float2 uv 		: TEXCOORD0;
				float3 cap		: TEXCOORD1;
			};

			v2f vert(appdata_base v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = v.texcoord;

				float3 worldNorm = UnityObjectToWorldNormal(v.normal);
				worldNorm = mul((float3x3)UNITY_MATRIX_V, worldNorm);
				o.cap.xy = worldNorm.xy * 0.5 + 0.5;

				float3 viewDir = normalize(ObjSpaceViewDir(v.vertex));                
				o.cap.z = 1.0 - saturate( dot( v.normal, viewDir ) ); 
				return o;
			}

			fixed4 frag(v2f i) : COLOR
			{
				fixed3 mainTex = tex2D(_MainTex, i.uv);				
				fixed4 matTex = tex2D(_MatCap, i.cap);

				fixed4 c = 1;				
				c.rgb = ((mainTex * (matTex * _MatIntensity) * (_Color * _MainColorIntensity)) + (matTex.a * _RimPower * mainTex) + (i.cap.z * _FresnelPower * _FresnelColor));
				c.a = _Alpha;

				return c;
			}
				ENDCG
		} // end of pass
	} // end of subshader

	SubShader
	{
		Tags
		{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
		}

		Blend SrcAlpha OneMinusSrcAlpha
		Cull back Lighting Off
		LOD 300

		pass
		{
			NAME "FORWARD"
			Tags{ "LightMode" = "Always" }

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag ARB_precision_hint_fastest

			#include "UnityCG.cginc"


			sampler2D _MainTex;
			sampler2D _MaskTex;
			fixed _Alpha;
			fixed3 _Color;
			fixed _MainColorIntensity;
			fixed3 _FresnelColor;
			fixed _FresnelPower;
			

			struct v2f
			{
				half4 vertex	: SV_POSITION;
				half2 uv 		: TEXCOORD0;				
			};

			v2f vert(appdata_base v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = v.texcoord;

				return o;
			}

			fixed4 frag(v2f i) : COLOR
			{
				fixed3 mainTex = tex2D(_MainTex, i.uv);
				
				fixed4 c = 1;
				c.rgb = mainTex + (clamp(_FresnelPower, 0.0, 0.3) * _FresnelColor) * (_Color * _MainColorIntensity);
				c.a = _Alpha;

				return c;
			}
				ENDCG
		} // end of pass
	} // end of subshader

	
	FallBack "VertexLit"
}
// */

//*
Shader "us/cha_mobile_diff_vertexlit" {
	Properties{
		_Alpha("Transparent", Range(0,1)) = 1
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_MatCap("MatCap (RGB)", 2D) = "white" {}
		_MatRim("Fresnel Tex(RGB)", 2D) = "white" {}
		_RimP("Rim Power", Range(0,3)) = 1

		
		_RimPower("Fresnel Power", Range(0,3)) = 0
		_RimColor("RimColor", Color) = (1,1,0,1)
		//연출
		_Cutoff("Cutoff", Range(0,1.5)) = 0
		[HideInInspector]
		_LightmapProbe("Lightmap Probe", 2D) = "white" {}



	}

	SubShader
	{
		Tags
		{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
		}

		Blend SrcAlpha OneMinusSrcAlpha
		Cull back Lighting Off

		pass
		{
			NAME "FORWARD"
			Tags{ "LightMode" = "Always" }

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag ARB_precision_hint_fastes		
			#include "UnityCG.cginc"


			sampler2D _MainTex;
			sampler2D _MatCap;
			sampler2D _MatRim;
			
			fixed _RimP;
			fixed _RimPower;
			fixed4 _RimColor;
			fixed4 _Color;
			fixed _Cutoff;
			fixed _Alpha;
			sampler2D _LightmapProbe;
			float4 _ProbeXZAndSize;


			struct v2f
			{
				fixed4 pos : SV_POSITION;
				fixed2 uv : TEXCOORD0;
				fixed2 cap : TEXCOORD1;
				float4 worldpos : TEXCOORD2;
			};

			uniform fixed4 _MainTex_ST;

			v2f vert(appdata_base v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);

				fixed3 worldNorm = normalize(unity_WorldToObject[0].xyz * v.normal.x + unity_WorldToObject[1].xyz * v.normal.y + unity_WorldToObject[2].xyz * v.normal.z);
				worldNorm = mul((fixed3x3)UNITY_MATRIX_V, worldNorm);
				o.cap.xy = worldNorm.xy * 0.5 + 0.5;
				o.worldpos = mul(unity_ObjectToWorld, v.vertex);

				return o;
			}

			fixed4 frag(v2f i) : COLOR
			{
				fixed4 m = tex2D(_MainTex, i.uv);
				fixed4 mc = tex2D(_MatCap, i.cap);
				fixed4 mcr = tex2D(_MatRim, i.cap);
				//fixed4 rim = tex2D (_RimTex, i.cap);  

				fixed4 c = 0;
				c.rgb = ((m * (mc*1.18)) + (mcr.r * _RimP) + (mcr.b * _RimPower * _RimColor)  * _Color);
				c.a = _Alpha;
				if (c.r < _Cutoff)
					// alpha value less than user-specified threshold?
				{
					discard; // yes: discard this fragment
				}

				half2 probeUV = (i.worldpos.xz - _ProbeXZAndSize.xy) / (_ProbeXZAndSize.zw);
				half3 lm = tex2D(_LightmapProbe, probeUV).rgb;
				c.rgb *= lm;

				return c;

			}
		ENDCG
		}
	}
	FallBack "VertexLit"
}
// */