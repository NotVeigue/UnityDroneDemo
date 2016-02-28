Shader "Custom/CustomWater" {
	Properties {
		_WaveHeightmap ("Wave Height Map", 2D) = "white" {}
		_NormalMap ("Wave Normal Map", 2D) = "white" {}
		_ReflectionTex ("Reflection Texture", 2D) = "white" {}
		_NormalMapDir ("Wave Normal Scroll Dir", Vector) = (1.0, 0.0, 0.0, 1.0)
		_NormalMapTiling ("Wave Normal Tiling", Vector) = (1.0, 1.0, 1.0, 1.0)
		_NormalMapScale ("Wave Normal Scale", Range(0.0, 1.0)) = 1.0
		_RefractionScale ("Refraction Scale", Range(0.0, 1.0)) = 0.5

		_DepthScale ("Depth Scale", Float) = 0.1
		_FoamCutoff ("Foam Cutoff Depth", Float) = 0.1
		_ShallowCutoff ("Shallows Cutoff Depth", Float) = 0.4

		_FoamTex ("Foam Texture", 2D) = "white" {}
		_FoamColor ("Foam Color", Color) = (1,1,1,1)

		_ShallowColor ("Shallows Color", Color) = (0, .6, 1, 1)
		_DeepColor	("Depths Color", Color) = (.1, .3, .95, 1)

		_FresnelScale ("Fresnel Scale", Float) = 1.0
		_FresnelTint ("Fresnel Tint", Color) = (1.0, 1.0, 1.0, 1.0)

		_Shininess ("Shininess", Float) = 10.0
		_SpecularTint("Specular Tint", Color) = (1.0, 1.0, 1.0, 1.0)
	}
	SubShader {
		//Tags {"Queue"="Geometry" "RenderType"="Opaque"}
		Tags {"Queue"="Transparent" "RenderType"="Transparent"}
		LOD 200

		GrabPass { "_SceneCopy" }

		Pass {
			//Blend SrcAlpha OneMinusSrcAlpha
			ZTest LEqual
			ZWrite Off
			//Cull Off

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#include "UnityCG.cginc"

			struct v2f
			{
				float4 pos : POSITION;
				float4 screenPos : TEXCOORD0;
				float4 worldPos : TEXCOORD1;
				float4 grabPassUV : TEXCOORD2;
				float4 normalMapCoords : TEXCOORD3;
				float height : TEXCOORD4;
				//float3 ray : TEXCOORD3;
				//float depth01 : TEXCOORD1;
			};

			sampler2D _WaveHeightmap;
			float4 _NormalMapDir;
			float4 _NormalMapTiling;

			v2f vert(appdata_full v)
			{

				v2f o;
				o.worldPos = mul(_Object2World, v.vertex);
				float4 h = tex2Dlod(_WaveHeightmap, float4((o.worldPos.xz + _Time.y * 10.0) * 0.001, 0.0, 0.0));
				h = h * 2.0 - 1.0;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex /*+ float4(0.0, h.r * 10.0, 0.0, 0.0)*/);
				o.screenPos = ComputeScreenPos(o.pos);
				o.height = 1.0;//h;

				#if UNITY_UV_STARTS_AT_TOP
				float scale = -1.0;
				#else
				float scale = 1.0;
				#endif
				o.grabPassUV.xy = (float2(o.pos.x, o.pos.y * scale) + o.pos.w) * 0.5;
				o.grabPassUV.zw = o.pos.zw;

				o.normalMapCoords = (o.worldPos.xzxz + _Time.yyyy * _NormalMapDir) * _NormalMapTiling;
				//o.depth01 = -(mul(UNITY_MATRIX_MV, v.vertex).z * _ProjectionParams.w);
				//o.ray = mul (UNITY_MATRIX_MV, v.vertex).xyz * float3(-1,-1,1);
				return o;
			}

			/*
			CBUFFER_START(UnityPerCamera2)
			float4x4 _CameraToWorld;
			CBUFFER_END
			*/

			sampler2D_float _CameraDepthTexture;
			sampler2D _SceneCopy;
			sampler2D _NormalMap;
			sampler2D _ReflectionTex;
			float _NormalMapScale;
			float _RefractionScale;

			float _DepthScale;
			float _FoamCutoff;
			float _ShallowCutoff;

			sampler2D _FoamTex;
			float4 _FoamColor;

			float4 _ShallowColor;
			float4 _DeepColor;

			float _FresnelScale;
			float4 _FresnelTint;

			float _Shininess;
			float4 _SpecularTint;

			
			float calcFresnel(float3 rd, float3 n)
			{
			    // Schlick's Approximation
			    float fresnel = 2.0;
			    float r0 = pow((1.0 - fresnel)/(1.0 + fresnel), 2.0);
			    return r0 + (1.0 - r0) * pow(1.0 - dot(-rd, n), 5.0);
			}

			half4 frag(v2f i) : SV_TARGET
			{
				
				// Calc world-space normal at this pixel
				float dx = ddx(i.height);
				float dy = ddy(i.height);
				float3 normal = normalize(float3(dx, 0.05, dy));
				float3 waveNormal = UnpackNormal(tex2D(_NormalMap, i.normalMapCoords.xy)) + UnpackNormal(tex2D(_NormalMap, i.normalMapCoords.zw)) * 0.5;
				normal = normal + waveNormal.xxy * _NormalMapScale * float3(1.0, 0.0, 1.0);
				normal = normalize(normal);
				float4 refractionOffset = float4(waveNormal.xz * _RefractionScale * 10.0, 0, 0);

				// Calc Water Color
				float depth = tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.screenPos + refractionOffset));
				depth = LinearEyeDepth(depth);
				depth = _DepthScale * (depth - i.screenPos.w);

				float t = saturate(depth);
				
				float4 sourceColor = tex2Dproj(_SceneCopy, (i.grabPassUV + refractionOffset));
				float4 waterColor = _ShallowColor;//lerp(sourceColor, _ShallowColor, smoothstep(0.0, _FoamCutoff, t));
				waterColor = lerp(waterColor, _DeepColor, saturate((t - _FoamCutoff)/(_ShallowCutoff - _FoamCutoff)));
				waterColor = lerp(sourceColor, waterColor, t);//exp(-2.0 * pow(saturate(1.0 - t), 2.0)));
				//waterColor.a = exp(-1.0 * pow(saturate(1.0 - t), 1.0)) * 0.5;

				float4 foamColor = (1.0 - smoothstep(_FoamCutoff - _FoamCutoff * 0.8, _FoamCutoff, t)) * tex2D(_FoamTex, i.worldPos.xz * 0.1) * _FoamColor;
				waterColor = lerp(waterColor, foamColor, foamColor.r);

				// Calc Lighing
				float3 light = normalize(_WorldSpaceLightPos0.xyz);
				float3 ray = normalize(i.worldPos - _WorldSpaceCameraPos);

				// Fresnel reflection color
				float4 reflectionColor = tex2Dproj(_ReflectionTex, UNITY_PROJ_COORD(i.grabPassUV - refractionOffset));
				float R = _FresnelScale * calcFresnel(ray, normal);

				// DEBUG
				//waterColor += R * _FresnelTint;
				waterColor = lerp(waterColor, reflectionColor, R);

				// Specular Color From Main Light Source
				float3 H = normalize(-ray + light);
				float specular = max(0.0, dot(H, normal));
				waterColor += pow(specular, _Shininess) * _SpecularTint;
				
				// ToDo:
				// Write an algorithm to calculate dynamic wave patterns and the normals created by them.
				// Use worldPos and Light pos the calculate speculars/fresnel reflections based on wave normals.
				// Combine specular/reflection colors with water color to complete effect.
				// Consider adding a max transparency value to water color to scale minimum visibility of submerged objects.

				return waterColor;//lerp(sourceColor, float4(0.0, 1.0, 0.6, 1.0), t);
				// Method 1
				/*
				float d = LinearEyeDepth(tex2D(_DepthCopy, i.screenPos.xy/i.screenPos.w).r);///float4(i.screenPos.xyz/i.screenPos.w, 1.0);
				float d2 = i.depth01;
				d = saturate(d - d2);
				*/

				// Method 3
				/*
				float2 uv = i.screenPos.xy/i.screenPos.w;
				float zOverW = tex2D(_DepthCopy, uv);
				float4 H = float4(uv.x * 2.0 - 1.0, (1.0 - uv.y) * 2.0 - 1.0, zOverW, 1.0);
				float4 D = mul(_CameraToWorld, H);
				float4 WP = D / D.w;
				float d = length(i.worldPos - WP)/10.0;
				*/

				// Method 4
				/*
				float2 uv = i.screenPos.xy / i.screenPos.w;
				// read depth and reconstruct world position
				float depth = SAMPLE_DEPTH_TEXTURE(_DepthCopy, uv);
				depth = Linear01Depth (depth);
				float4 vpos = float4(i.ray * depth, 1);
				float3 wpos = mul (_CameraToWorld, vpos).xyz;
				float d = length(wpos - i.worldPos) * 0.1;
				*/
				// Test
				//return d == 0.0 ? float4(1.0, 0.0, 0.0, 1.0) : float4(d, d, d, 1.0);
			}

			ENDCG
		}
	} 
	FallBack "Diffuse"
}
