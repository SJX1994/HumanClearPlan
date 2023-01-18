// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'
// https://github.com/losuffi/Unity-JellyBody.git
// buildIn 2 urp:
// https://cuihongzhi1991.github.io/blog/2020/05/27/builtinttourp/
Shader "EffectL/SphereModify"
{
	
	Properties
	{
		_MainColor("Main Color",Color)=(1,1,1,1)
		_Force("Force",Float)=0.1

	
	}
	SubShader 
	{
		Tags { 
			"RenderPipeline" = "UniversalPipeline"

			// explict SubShader tag to avoid confusion
			"RenderType"="Opaque"
			"UniversalMaterialType" = "Lit"
			"Queue"="Geometry"
		 }
		
		Pass
		{
			Name "CrazyShader"
			Tags
			{
				"LightMode"="UniversalForward"
			}
			
			Cull Back
			ZTest LEqual
			ZWrite On
			Blend One Zero

			HLSLPROGRAM
			
			
			#include "util.hlsl"
			

			FInput vert(VInput v)
			{
				FInput o;
				o.texcoord=v.texcoord;
				// o.LightDir = _MainLightPosition.xyz - TransformObjectToWorld(v.pos.xyz);
				 o.LightDir = GetMainLight().direction;
				v.wpos=mul(unity_ObjectToWorld,v.pos);
				float3 normal=mul((float3x3)unity_ObjectToWorld,v.normalOS);
				#ifdef _IsWave
			    	result w= WaveModify(v.pos,normal);
				#elif _Explore
				result w= ModifyPos(v.pos,normal);
				#else
				result w= ModifyPos(v.pos,normal);
				#endif
			    o.pos=TransformObjectToHClip(w.resPos.xyz);
			    o.nor=w.resNormal;
			    return o;
			}
			float4 frag(FInput i):SV_Target
			{
				// 进urp
					ToonSurfaceData surfaceData = InitializeSurfaceData(i);
					ToonLightingData lightingData = InitializeLightingData(i);
					half3 urpColor = ShadeAllLights(surfaceData, lightingData);
				// 进urp
				float3 albedo = _MainColor.rgb;
				float3 ambient = UNITY_LIGHTMODEL_AMBIENT.xyz*albedo;
				float3 L=normalize(i.LightDir);
				float3 diffuse = float3(0,0,0); 
				
				#ifdef _Ground
					diffuse = lerp(_MainLightColor.rgb * albedo*0.6,_MainLightColor.rgb * albedo,saturate( dot(i.nor,L))  );
				#endif

				#ifdef _Player
					diffuse =_MainLightColor.rgb * albedo - 0.3*(1.0 -  saturate( max(0,step(0.05,dot(i.nor,L)))) ); 
				#endif

				#ifdef _Ball
					float3 player1Color = _MainLightColor.rgb * float3(1,0.42,0.46);
					float3 player2Color = _MainLightColor.rgb * float3(0.3,0.76,1);
					diffuse = lerp(player1Color,player2Color,saturate( sin(_Time.y*3.0))) -   0.5*(1.0 -  saturate( max(0,dot(i.nor,L))) ); 
				#endif
				
				float3 finalcolor = saturate(diffuse + ambient) ;
				
				finalcolor = ApplyFog(urpColor*diffuse + ambient, i); 

				return float4(finalcolor,1);
			}
		

			ENDHLSL
		}
		Pass
		{
			Name "ShadowCaster"
			Tags{"LightMode" = "ShadowCaster"}

			// more explict render state to avoid confusion
			ZWrite On // the only goal of this pass is to write depth!
			ZTest LEqual // early exit at Early-Z stage if possible            
			ColorMask 0 // we don't care about color, we just want to write depth, ColorMask 0 will save some write bandwidth
			Cull Back // support Cull[_Cull] requires "flip vertex normal" using VFACE in fragment shader, which is maybe beyond the scope of a simple tutorial shader

			HLSLPROGRAM

			// the only keywords we need in this pass = _UseAlphaClipping, which is already defined inside the HLSLINCLUDE block
			// (so no need to write any multi_compile or shader_feature in this pass)

			#pragma vertex VertexShaderWork
			#pragma fragment BaseColorAlphaClipTest // we only need to do Clip(), no need shading

			// because it is a ShadowCaster pass, define "ToonShaderApplyShadowBiasFix" to inject "remove shadow mapping artifact" code into VertexShaderWork()
			#define ToonShaderApplyShadowBiasFix

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			struct Attributes
			{
				float3 positionOS   : POSITION;
				half3 normalOS      : NORMAL;
				half4 tangentOS     : TANGENT;
				float2 uv           : TEXCOORD0;
			};
			struct Varyings
			{
				float2 uv                       : TEXCOORD0;
				float4 positionWSAndFogFactor   : TEXCOORD1; // xyz: positionWS, w: vertex fog factor
				half3 normalWS                  : TEXCOORD2;
				float4 positionCS               : SV_POSITION;
			};
			half4   _MainColor;
			Varyings VertexShaderWork(Attributes input)
			{
				Varyings output;
				VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS);
				VertexNormalInputs vertexNormalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);
				float3 positionWS = vertexInput.positionWS;
				float fogFactor = ComputeFogFactor(vertexInput.positionCS.z);
				output.positionWSAndFogFactor = float4(positionWS, fogFactor);
				output.normalWS = vertexNormalInput.normalWS;
				output.positionCS = TransformWorldToHClip(positionWS);
				return output;
			}			
			
			void BaseColorAlphaClipTest()
			{
				_MainColor.a;
			}

			ENDHLSL
		}
	}
}