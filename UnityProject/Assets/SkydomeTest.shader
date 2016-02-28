Shader "Skybox/Test" {
	Properties {
        _PollutionColor ("Pollution Color", Color) = (0.5,0.5,1,1)
        _PollutionLevel	("Pollution Level", Float) = 0.0
    }
    SubShader {
        Pass {
        	Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        	Cull Off ZWrite Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "Lighting.cginc"

            float4 _PollutionColor;
            float  _PollutionLevel;

            struct vertexInput {
                float4 vertex : POSITION;
                float4 texcoord0 : TEXCOORD0;
                float3 normal: NORMAL;
            };

            struct fragmentInput{
                float4 position : SV_POSITION;
                float4 texcoord0 : TEXCOORD0;
                float3 normal: NORMAL;
            };

            float3 hash33(float3 p){
				p  = frac(p * float3(5.3983, 5.4427, 6.9371));
			    p += dot(p.yzx, p.xyz  + float3(21.5351, 14.3137, 15.3219));
				return frac(float3(p.x * p.z * 95.4337, p.x * p.y * 97.597, p.y * p.z * 93.8365));
			}

			float3 stars(in float3 p, float resolution)
			{
			    float3 c = float3(0.0,0.0,0.0);
			    float res = resolution * 1.5;
			    
				for (float i=0.;i<3.;i++)
			    {
			        float3 q = frac(p*(.15*res))-0.5;
			        float3 id = floor(p*(.15*res));
			        float rn = hash33(id).z;
			        float c2 = 1.-smoothstep(-0.2,.4,length(q));
			        c2 *= step(rn,0.005+i*0.014);
			        c += c2*(lerp(float3(1.0,0.75,0.5),float3(0.85,0.9,1.),rn*30.)*0.5 + 0.5);
			        p *= 0.9;
			    }

			    return c*c*1.5;
			}

			float4 renderSky(in float3 rd, in float3 light)
			{
			    const float3 up = float3(0, 1, 0);

			    // Some essential values
			    float vert = saturate(dot(rd, up)); 			// Verticality of this point
			    float svrt = dot(light, up);					// Verticality of the sun	
			    float ints = (dot(light, rd) + 1.0) * 0.5;		// (Intensity) How close the current point is to the sun
			    ints = exp(-1.0 * pow(1.0 - ints, 4.0));
			    
			    // Daytime Sky Colors
			    const float3 sc1 = float3(0.6, 0.8, 1.0);
			    const float3 sc2 = float3(0.3, 0.3, 1.0);

			    // Sunset Sky Colors
			    const float3 ss1 = float3(0.8, 0.6, 0.4);
			    const float3 ss2 = float3(0.6, 0.4, 1.0);

			    // Night Sky Colors
			    const float3 ns1 = float3(0.0 , 0.05, 0.09);
			    const float3 ns2 = float3(0.01, 0.01, 0.01);

			 	// Astral Body Colors
			    const float3 suncolor = float3(1.0, 1.0, 1.0);
			    const float3 mooncolor = float3(1.0, 1.0, 0.9);

			    
			    // Calculate the current color of the sky before sundown
			    float3 skycolor = lerp(sc1, sc2, vert);
			    float3 sunsetcolor = skycolor;//lerp(ss1, skycolor, smoothstep(0.0, 0.35, vert));

			    // Adjust to R, G, B values of this point according to its verticality
			    float ivert = 1.0 - vert;
			    sunsetcolor.r *= exp(-0.5 * pow(ivert, 3.0));
			    sunsetcolor.g *= exp(-1.4 * pow(ivert, 5.0));
			    sunsetcolor.b *= exp(-3.0 * pow(ivert, 4.0));
			    sunsetcolor += pow(1.0 - vert, 4.0) * 0.1;
			    float3 c = lerp(sunsetcolor * ints + pow(1.0 - vert, 2.0) * 0.3, skycolor, svrt) * smoothstep(-0.3, 0.1, svrt);;
			    
			    float pollution = saturate(_PollutionLevel);

			    // Calculate the current color of the night sky
			    float3 starcolor = stars(rd, 700.0) * length(hash33(rd + _Time.y * 0.01));
			    // Let's make the brightness of the stars proportional to the amount of pollution
			    starcolor *= (1.0 - smoothstep(0.0, 0.5, pollution));
			    starcolor += lerp(ns1, ns2, vert);

			    // Blend between the day and night skies based on the height of the sun in the sky
			    c = lerp(starcolor, c, smoothstep(-0.2, -0.1, svrt));

			    // sun color
			    float rdl = saturate(dot(rd, light));
			    float3 sun = float3(0.0, 0.0, 0.0);
			    sun += 0.35 * suncolor * pow(rdl, 10.0);
			    sun += 0.45 * suncolor * pow(rdl, 120.0) * smoothstep(0.35, 0.7, pollution);
			    sun += 0.6 * suncolor * pow(rdl, 512.0);
			    sun *= smoothstep(-0.2, -0.05, svrt);
			    c += sun;
			    
			    float3 moon = mooncolor;
			    float nrdl = saturate(dot(-rd, light));
			    moon *= step(0.999, nrdl);
			    c += mooncolor * pow(nrdl, 250.0) * pollution;
			    c = lerp(c, moon, moon.x);

			    float brightness = length(c);
			    c = (c * (1.0 - pollution) + _PollutionColor * max(_PollutionLevel, 0.0) * brightness) * lerp(1.0, 0.5, pollution);// * brightness;
			    
			    
			    return float4(c, 1.0);
			}

            fragmentInput vert(vertexInput i){
                fragmentInput o;
                o.position = mul (UNITY_MATRIX_MVP, i.vertex);
                o.texcoord0 = i.texcoord0;
                o.normal = i.vertex.xyz;//normalize(mul((float3x3)_Object2World, i.vertex.xyz));
                return o;
            }

            
            fixed4 frag(fragmentInput i) : SV_Target {

                //return float4((stars(i.normal, 500.0) + stars(i.normal, 880.0)) * length(hash33(i.normal + _Time * 0.01)), 1.0);
                return renderSky(normalize(mul((float3x3)_Object2World, i.normal)), _WorldSpaceLightPos0.xyz);
            }
   
            ENDCG
        }
    }
}