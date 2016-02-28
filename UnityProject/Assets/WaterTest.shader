Shader "Custom/WaterTest" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
	}
	SubShader {
		Tags { "Queue"="Transparent" }
		LOD 200
		
		Pass {
			//Tags { "Queue"="Transparent" "RenderType"="Transparent"}
			ZWRITE OFF
			CULL OFF
			BLEND SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
        	#pragma vertex vert
        	#pragma fragment frag

        	#include "UnityCG.cginc"
            #include "Lighting.cginc"

            struct vertexInput {
                float4 vertex : POSITION;
                //float4 texcoord0 : TEXCOORD0;
                float3 normal: NORMAL;
            };

            struct fragmentInput{
                float4 position : SV_POSITION;
                //float4 texcoord0 : TEXCOORD0;
                float3 normal: NORMAL;
            };

            float hash( float2 p ) 
            {
				float h = dot(p,float2(127.1,311.7));	
			    return frac(sin(h)*43758.5453123);
			}

			float noise( in float2 p ) 
			{
			    float2 i = floor( p );
			    float2 f = frac( p );	
				float2 u = f*f*(3.0-2.0*f);
			    return -1.0+2.0*lerp( lerp( hash( i + float2(0.0,0.0) ), 
			                                hash( i + float2(1.0,0.0) ), u.x),
			                          lerp( hash( i + float2(0.0,1.0) ), 
			                                hash( i + float2(1.0,1.0) ), u.x), u.y);
			}

			float sea_octave(float2 uv, float choppy) {
			    uv += noise(uv);        
			    float2 wv = 1.0-abs(sin(uv));
			    float2 swv = abs(cos(uv));    
			    wv = lerp(wv,swv,wv);
			    return pow(1.0-pow(wv.x * wv.y,0.65),choppy);
			}

			float waterHeight(float3 p)
			{
				const float2x2 octave_m = {1.6, 1.2,
									      -1.2, 1.6};

				float freq = 0.16;
				float amp = 0.6;
				float choppy = 9.0;
				float2 uv = p.xz;

				float d = 0.0, h = 0.0;
				for(int i = 0; i < 8; ++i)
				{
					d = sea_octave((uv + _Time.y) * freq, choppy);
					d += sea_octave((uv - _Time.y) * freq, choppy);
					h += d * amp;
					uv = mul(octave_m, uv); freq *= 1.9; amp *= 0.22;
					choppy = lerp(choppy, 1.0, 0.2);
				}

				return h;
			}

			float3 calcNormal(float3 p)
			{
				const float e = 0.001;

				float3 n;
				n.y = waterHeight(p);
				n.x = waterHeight(float3(p.x + e, p.y, p.z)) - n.y;
				n.z = waterHeight(float3(p.x, p.y, p.z + e)) - n.y;
				n.y = e;

				return normalize(n);
			}

            fragmentInput vert(vertexInput i)
            {
            	fragmentInput o;
            	o.position = mul(UNITY_MATRIX_MVP, i.vertex);
            	o.position.y += waterHeight(i.vertex.xyz);//sea_octave(i.vertex.xz + _Time.y, 4.0);//sin(i.vertex.x + _Time.y);
            	o.normal = i.vertex.xyz;

            	return o;
            }

            fixed4 frag(fragmentInput i) : SV_TARGET
            {
            	//float3 rd = normalize(mul((float3x3)_Object2World, i.normal));
            	float3 n = calcNormal(i.normal);
            	float3 camera = _WorldSpaceCameraPos;
            	float3 light = normalize(_WorldSpaceLightPos0.xyz);
            	float3 dist = camera - i.normal;

            	float diffuse = pow((dot(n, light) * 0.4 + 0.6), 80.0);
            	float3 c = float3(0.1,0.19,0.22) + float3(0.8,0.9,0.6) * diffuse * 0.12;

            	float atten = max(1.0 - dot(dist, dist) * 0.001, 0.0);
            	c += float3(0.8,0.9,0.6) * (i.normal.y - 0.6) * 0.18 * atten;

            	float specular = pow(max(dot(reflect(-light, n), normalize(dist)), 0.0), 60.0);
            	c += specular;

            	return float4(c, 1.0);
            }

            ENDCG
        }
    }
}
