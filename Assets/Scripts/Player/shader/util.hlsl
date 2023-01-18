                  
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS
           		#pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
           		#pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
           		#pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
           		#pragma multi_compile_fragment _ _SHADOWS_SOFT

			#pragma multi_compile_fog

			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile _IsWave _Explore
			#pragma multi_compile _Ground _Player _Ball
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			
			int _Count;
			float4 _pAfs[10];
			float4 _dAts[10];
			float4 _MainColor;
			float _MaxForce;
			float _Spring;
			float _Damping;
			float _Namida;
			#pragma multi_compile_fwdbase
			struct VInput
			{
				float4 pos:POSITION;
				float3 normalOS:NORMAL;
				half4 tangentOS     : TANGENT;
				float3 LightDir:TEXCOORD1;
				float4 texcoord:TEXCOORD0;  
				float4 wpos:TEXCOORD2;
			};
			struct FInput
			{
				float4 texcoord :TEXCOORD0;
				float3 LightDir:TEXCOORD1;
				float3 nor:NORMAL;
				float4 positionWSAndFogFactor   : TEXCOORD2;   // xyz: positionWS, w: vertex fog factor
				float4 pos:SV_POSITION;
		
			};
                  struct result
			{
				float4 resPos;
				float3 resNormal;
			};
			struct dataBuff
			{
				float _StartTime;
				float _Force;
				float _Namida;
				float3 _ForceDir;
				float4 _WorldForcePos;
			};
			// urp
				struct ToonSurfaceData
				{
					half3   albedo;
					half    alpha;
					half3   emission;
					half    occlusion;
				};
				struct ToonLightingData
				{
					half3   normalWS;
					float3  positionWS;
					half3   viewDirectionWS;
					float4  shadowCoord;
				};
				ToonSurfaceData InitializeSurfaceData(FInput input)
				{
					ToonSurfaceData output;

					// albedo & alpha
					float4 baseColorFinal = _MainColor;
					output.albedo = baseColorFinal.rgb;
					output.alpha = baseColorFinal.a;

					// emission
					output.emission = _MainColor;
					// SJX TODO
					half oc = 1;
					output.occlusion = oc;

					return output;
				}
				ToonLightingData InitializeLightingData(FInput input)
				{
					ToonLightingData lightingData;
				
					lightingData.viewDirectionWS = SafeNormalize(GetCameraPositionWS() - lightingData.positionWS);  
					lightingData.normalWS = normalize(input.nor); //interpolated normal is NOT unit vector, we need to normalize it
					lightingData.positionWS = input.positionWSAndFogFactor.xyz;

					return lightingData;
				}
				half3 ApplyFog(half3 color, FInput input)
				{
					half fogFactor = input.positionWSAndFogFactor.w;
					// Mix the pixel color with fogColor. You can optionaly use MixFogColor to override the fogColor
					// with a custom one.
					color = MixFog(color, fogFactor);

					return color;  
				}
				half3 ShadeGI(ToonSurfaceData surfaceData, ToonLightingData lightingData)
				{
					half3 averageSH = SampleSH(0);

					// can prevent result becomes completely black if lightprobe was not baked 
					// SJX TODO
					 float _IndirectLightMinColor = 10; 
					//averageSH = max(_IndirectLightMinColor,averageSH);

					// occlusion (maximum 50% darken for indirect to prevent result becomes completely black)
					half indirectOcclusion = lerp(1, surfaceData.occlusion, 0.5);
					return averageSH * indirectOcclusion;
				}
				// Most important part: lighting equation, edit it according to your needs, write whatever you want here, be creative!
				// This function will be used by all direct lights (directional/point/spot)
				half3 ShadeSingleLight(ToonSurfaceData surfaceData, ToonLightingData lightingData, Light light, bool isAdditionalLight)
				{
					half3 N = lightingData.normalWS;
					half3 L = light.direction;

					half NoL = dot(N,L);

					half lightAttenuation = 1;

					// light's distance & angle fade for point light & spot light (see GetAdditionalPerObjectLight(...) in Lighting.hlsl)
					// Lighting.hlsl -> https://github.com/Unity-Technologies/Graphics/blob/master/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl
					half distanceAttenuation = min(4,light.distanceAttenuation); //clamp to prevent light over bright if point/spot light too close to vertex

					// N dot L
					// simplest 1 line cel shade, you can always replace this line by your own method!
					// half litOrShadowArea = smoothstep(_CelShadeMidPoint-_CelShadeSoftness,_CelShadeMidPoint+_CelShadeSoftness, NoL);
					half litOrShadowArea = NoL;

					// occlusion
					litOrShadowArea *= surfaceData.occlusion;

					// face ignore celshade since it is usually very ugly using NoL method
					litOrShadowArea =  litOrShadowArea;

					// light's shadow map
					// SJX TODO
					float _ReceiveShadowMappingAmount = 0.5;
					litOrShadowArea *= lerp(1,light.shadowAttenuation,_ReceiveShadowMappingAmount);

					half3 litOrShadowColor = lerp( saturate( _MainColor-0.5),1, litOrShadowArea);

					half3 lightAttenuationRGB = litOrShadowColor * distanceAttenuation;

					// saturate() light.color to prevent over bright
					// additional light reduce intensity since it is additive
					return saturate(light.color) * lightAttenuationRGB * (isAdditionalLight ? 0.25 : 1);
				}
				half3 ShadeEmission(ToonSurfaceData surfaceData, ToonLightingData lightingData)
				{
					half3 emissionResult = lerp(surfaceData.emission, surfaceData.emission * surfaceData.albedo, _MainColor); // optional mul albedo
					return emissionResult;
				}
				half3 CompositeAllLightResults(half3 indirectResult, half3 mainLightResult, half3 additionalLightSumResult, half3 emissionResult, ToonSurfaceData surfaceData, ToonLightingData lightingData)
				{
					// [remember you can write anything here, this is just a simple tutorial method]
					// here we prevent light over bright,
					// while still want to preserve light color's hue
					half3 rawLightSum = max(indirectResult, mainLightResult + additionalLightSumResult); // pick the highest between indirect and direct light
					return surfaceData.albedo * rawLightSum + emissionResult;
				}
			half3 ShadeAllLights(ToonSurfaceData surfaceData, ToonLightingData lightingData)
			{
					// Indirect lighting
					half3 indirectResult = ShadeGI(surfaceData, lightingData);
					Light mainLight = GetMainLight();
					// SJX TODO
					float _ReceiveShadowMappingPosOffset = 0; 
					float3 shadowTestPosWS = lightingData.positionWS + mainLight.direction * (_ReceiveShadowMappingPosOffset);
				#ifdef _MAIN_LIGHT_SHADOWS
					float4 shadowCoord = TransformWorldToShadowCoord(shadowTestPosWS);
					mainLight.shadowAttenuation = MainLightRealtimeShadow(shadowCoord);
				#endif 
					// Main light
					half3 mainLightResult = ShadeSingleLight(surfaceData, lightingData, mainLight, false);

					//==============================================================================================
					// All additional lights

					half3 additionalLightSumResult = 0;

				#ifdef _ADDITIONAL_LIGHTS
					// Returns the amount of lights affecting the object being renderer.
					// These lights are culled per-object in the forward renderer of URP.
					int additionalLightsCount = GetAdditionalLightsCount();
					for (int i = 0; i < additionalLightsCount; ++i)
					{
						
						int perObjectLightIndex = GetPerObjectLightIndex(i);
						Light light = GetAdditionalPerObjectLight(perObjectLightIndex, lightingData.positionWS); // use original positionWS for lighting
						light.shadowAttenuation = AdditionalLightRealtimeShadow(perObjectLightIndex, shadowTestPosWS); // use offseted positionWS for shadow test

						// Different function used to shade additional lights.
						additionalLightSumResult += ShadeSingleLight(surfaceData, lightingData, light, true);
					}
				#endif
					//==============================================================================================

					// emission
					half3 emissionResult = ShadeEmission(surfaceData, lightingData);

					return CompositeAllLightResults(indirectResult, mainLightResult, additionalLightSumResult, emissionResult, surfaceData, lightingData);
			}
			// urp
			dataBuff getData(int i)
			{
				dataBuff o;
				o._StartTime=_dAts[i].w;
				o._Force=_pAfs[i].w;
				
				o._ForceDir=mul((float3x3)unity_WorldToObject,_dAts[i].xyz);
				o._WorldForcePos=mul(unity_WorldToObject,float4(_pAfs[i].xyz,1));
				o._Namida=_Namida;
				return o;
			}
			result ModifyPos(float4 pos,in float3 normal)
			{
				result r;
				float3 v;
				float3 n=normal;
				for(int i=0;i<_Count;i++)
				{
					dataBuff data=getData(i);
					float3 dir=pos.xyz-data._WorldForcePos.xyz;
					float3 wdir=mul((float3x3)unity_ObjectToWorld,dir);
					float distance=dot(dir,dir);
					float time=_Time.y-data._StartTime;
					float singleForce=data._Force/(1+distance);
					float A=lerp(singleForce,0,saturate((_Damping*time)/abs(singleForce)));
					//float speed=data._Namida* (4*3.14*3.14)/_Spring;
					//A=(speed*speed*time*time)>distance?A:0;	
					float offset=(cos(_Spring*time))*-A;			 	
					v+=dir*offset;
					float3 binormal=cross(normal,wdir);
					float3 tangent=cross(binormal,normal);
					n+=offset*tangent*5;
				}
				r.resPos=half4(pos.xyz+v,1);
				r.resNormal=normalize(n);
				return r;
			}

			result WaveModify(float4 pos,in float3 normal)
			{
				result r;
				float3 v;
				float3 n=normal;
				for(int i=0;i<_Count;i++){
					dataBuff data=getData(i);
					float3 dir=pos.xyz-data._WorldForcePos.xyz;
					float3 wdir=mul((float3x3)unity_ObjectToWorld,dir);
					float distance=dot(dir,dir);
					float time=_Time.y-data._StartTime;
					float singleForce=data._Force/(1+distance*5);
					float A=lerp(singleForce,0,saturate(((_Damping)*time)/abs(singleForce)));
					float x=time-distance/data._Namida;
					//x=x<0?0:x;
					float speed=data._Namida* (4*3.14*3.14)/_Spring;
					A=(speed*speed*time*time)>distance?A:0;
                              float h = -A*(cos(_Spring*(x)))*0.1;

                              #ifdef _Player	
					h = -A*(cos(_Spring*(x)))*0.6;
                              #endif
                            
                              #ifdef _Ball
                              h = A*(cos(_Spring*(x)))*0.1;
                              #endif

					v+=h*normal;
					float3 binormal=cross(normal,wdir);
					float3 tangent=cross(binormal,normal);
					n+=h*tangent*20;
				}
				n=normalize(n);
				r.resPos=float4(pos.xyz+v,1);
				r.resNormal=n;
				return r;
			}