// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "us/cha_mobile_diff_color3_vertexlit" {
	Properties{
		_Alpha("Transparent", Range(0,1)) = 1
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_MatCap("MatCap (RGB)", 2D) = "white" {}
		_MatRim("Fresnel Tex(RGB)", 2D) = "white" {}
		_RimP("Rim Power", Range(0,1)) = 1
		_MaskTex("Mask (RGB Color, A Reflection)", 2D) = "white" {}
		_CustomColor1("hair Color 1 (Mask Texture - R)", Color) = (1,1,1,1)
		_CustomColor2("skin Color 2 (Mask Texture - G)", Color) = (1,1,1,1)
		_CustomColor3("eyes Color 3 (Mask Texture - B)", Color) = (1,1,1,1)		
		_RimPower("Fresnel Power", Range(0,3)) = 0
		_RimColor("RimColor", Color) = (1,1,0,1)
		[HideInInspector]
		_LightmapProbe("Lightmap Probe", 2D) = "white" {}

	}
	SubShader{
		Tags
		{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
		}

		Blend SrcAlpha OneMinusSrcAlpha
		Cull back 
		Lighting Off

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
			sampler2D _MatRim;			
			fixed _RimP;
			fixed _RimPower;
			fixed4 _Color;
			fixed4 _RimColor;
			sampler2D _MaskTex;
			fixed4 _CustomColor1;
			fixed4 _CustomColor2;
			fixed4 _CustomColor3;
			fixed _Alpha;
			sampler2D _LightmapProbe;
			float4 _ProbeXZAndSize;

			struct v2f
			{
				fixed4 pos : SV_POSITION;
				fixed2 uv : TEXCOORD0;
				fixed2 cap : TEXCOORD1;
				half3 normal : TEXCOORD2;
				float4 worldpos : TEXCOORD3;
			};

			uniform float4 _MainTex_ST;

			v2f vert(appdata_base v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);

                fixed3 worldNorm = UnityObjectToWorldNormal(v.normal);// normalize(unity_WorldToObject[0].xyz * v.normal.x + unity_WorldToObject[1].xyz * v.normal.y + unity_WorldToObject[2].xyz * v.normal.z);
				worldNorm = mul((fixed3x3)UNITY_MATRIX_V, worldNorm);
				o.cap.xy = worldNorm.xy * 0.5 + 0.5;
                o.normal = worldNorm;// UnityObjectToWorldNormal(v.normal);
				o.worldpos = mul(unity_ObjectToWorld, v.vertex);

				return o;
			}

			fixed4 frag(v2f i) : COLOR
			{
				fixed4 m = tex2D(_MainTex, i.uv);
				fixed4 cm = tex2D(_MaskTex, i.uv);
				fixed4 mc = tex2D(_MatCap, i.cap);
				fixed4 mcr = tex2D(_MatRim, i.cap);
				//fixed4 rim = tex2D (_RimTex, i.cap);  

				half3 normalDir = normalize(i.normal);
				half3 viewDir = normalize(WorldSpaceViewDir(i.worldpos));
				half rim = 1.0 - saturate(dot(viewDir, normalDir));
				half3 rimColor = pow(rim, (1.0 - _RimP)) * mcr.r;

				half2 probeUV = (i.worldpos.xz - _ProbeXZAndSize.xy) / (_ProbeXZAndSize.zw);
				half3 lm = tex2D(_LightmapProbe, probeUV).rgb;

				fixed4 c = 0;
				fixed3 tex = m.rgb + (_CustomColor1 * cm.r) + (_CustomColor2 * cm.g) + (_CustomColor3 * cm.b);
				//c.rgb = ((tex * (mc*1.18)) + (mcr.r * _RimP) + (mcr.b * _RimPower * _RimColor)   * _Color);
				c.rgb = (tex * (mc*1.18) + (mcr.b * _RimPower * _RimColor)   * _Color) + rimColor;
				c.rgb *= lm;
				c.a = _Alpha;

				return c;
			}
			ENDCG
		}

	}
	FallBack "VertexLit"
}
// */
