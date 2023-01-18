// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'
// https://github.com/losuffi/Unity-JellyBody.git
// buildIn 2 urp:
// https://cuihongzhi1991.github.io/blog/2020/05/27/builtinttourp/
Shader "EffectL/SphereModifyTrans"
{
	
	Properties
	{
		_MainColor("Main Color",Color)=(1,1,1,1)
		_Force("Force",Float)=0.1

	
	}
	SubShader 
	{
		Tags { "RenderPipeline" = "UniversalRenderPipeline" "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
		
		// 添加额外的Pass，仅用于渲染到深度缓冲区
		Pass
		{
			// 开启深度写入
			ZWrite On
			// 用于控制Pass不写入任何颜色通道
			ColorMask 0
		}
		
		Pass
		{
			Name "CrazyShaderTrans"
			Tags
			{
				"LightMode"="UniversalForward"
				"Queue"="Overlay+1" // 最上层
			}
			Cull Off
			ZWrite On
			Blend SrcAlpha OneMinusSrcAlpha
			
      		ZTest Always
			
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
			    o.pos = TransformObjectToHClip(v.pos);
			    o.nor=w.resNormal;
			    return o;
			}
			float4 frag(FInput i):SV_Target
			{
				float3 albedo = _MainColor.rgb;
				half alphaScale = _MainColor.a;
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
				
				
				return float4(finalcolor,alphaScale);
			}
		

			ENDHLSL
		}
		
	}
}