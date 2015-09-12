﻿Shader "Custom/Composite" 
{
	Properties {
		_MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
	}

	SubShader 
	{
		Tags { "RenderType"="Opaque" }
		
		// Blit with depth
		Pass 
		{
			ZWrite On
			ZTest Always

            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag

            #include "UnityCG.cginc"

			sampler2D _MainTex;
			sampler2D_float _CameraDepthTexture;

            void frag(v2f_img i,  out float4 color : COLOR0, out float depth : DEPTH) 
			{             
				color = tex2D(_MainTex, i.uv);       
				depth = tex2D(_CameraDepthTexture, i.uv);
            }
            ENDCG
        }

		Pass 
		{
			ZWrite On
			ZTest Lequal

            CGPROGRAM
			
            #pragma fragment frag
			#pragma vertex vert_img

            #include "UnityCG.cginc"

			sampler2D _MainTex;
			sampler2D_float _CameraDepthTexture;

			sampler2D _ColorTexture;
			sampler2D_float _DepthTexture;

            void frag(v2f_img i,  out float4 color : COLOR0, out float depth : depth) 
			{   
				float customDepth = (tex2D(_DepthTexture, i.uv));  		
				float cameraDepth = (tex2D(_CameraDepthTexture, i.uv));  	
					
				bool depthTest = customDepth < cameraDepth;
				color = depthTest ? tex2D(_ColorTexture, i.uv) : tex2D(_MainTex, i.uv);
				depth = depthTest ? customDepth : cameraDepth;
				    
            }
            ENDCG
        }

		Pass 
		{
			ZTest Always

            CGPROGRAM
            #pragma fragment frag
            #pragma vertex vert_img
						
			#pragma target 5.0	
			#pragma only_renderers d3d11			

            #include "UnityCG.cginc"
			
			Texture3D<float> _HiZMap;

            void frag(v2f_img i, out float4 color : COLOR0) 
			{       
				uint2 coord = uint2(i.uv.x * _ScreenParams.x, i.uv.y * _ScreenParams.y);   
				color.r = Linear01Depth(_HiZMap[uint3(coord, 4)]); 
            }
            
            ENDCG
        }

		Pass 
		{
			ZTest Always

            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag	
					
			#pragma target 5.0	
			#pragma only_renderers d3d11	

            #include "UnityCG.cginc"
			#include "Helper.cginc"	

			uniform Texture2D<int> _IdTexture; 
			StructuredBuffer<float4> _ProteinColors;
			StructuredBuffer<float4> _ProteinInstanceInfo;

            void frag(v2f_img i, out float4 color : COLOR0) 
			{   
				int2 uv = i.uv * _ScreenParams.xy; 
				int id = _IdTexture[uv];

				//if(id == -3)
				//{
				//	color = float4(1,0,0,0);
				//}
				//else if(id == -2)
				//{
				//	color = floatna4(1,240.0/255.0,114.0/255.0,0);
				//}
				
				if(id >= 0)
				{
					float4 proteinInfo = _ProteinInstanceInfo[id];					
					float4 proteinColor = _ProteinColors[proteinInfo.x];
					//float4 lowlight = float4(float3(dot( proteinColor.rgb, float3(0.22, 0.707, 0.071))),1);
					if (proteinInfo.y == 0)
					{
						color = float4(ColorCorrection(proteinColor.xyz), 1);
					}
					else if (proteinInfo.y == 1) 
					{
						color = float4(HighlightColor(proteinColor), 1);
					}
					else 
					{
						color = float4(DesaturateColor(proteinColor), 1);
					}
					//else if (proteinInfo.y == 3) {
					//	float f = 0.2;
					//	float r = proteinColor.r;
					//	float g = proteinColor.g;
					//	float b = proteinColor.b;
					//	//float3 lum = float3(r*0.22,g*0.707,b*0.071);//0.3,0.6,0.1?
					//	float lum = dot( proteinColor.rgb, float3(0.22, 0.707, 0.071));
					//	color = proteinColor*f+float4(lum,lum,lum,1);
					//	//float lum = dot( proteinColor.rgb, float3(0.1, 0.5, 0.07));//
					//	//float lum = dot( proteinColor.rgb, float3(0.22, 0.707, 0.071));//0.3,0.6,0.1?
					//	//color = float4(lum,lum,lum,1);
					//}
					//else {
					//	color  =float4(ColorCorrection(proteinColor.xyz), 1);
					//}
					//color = (proteinInfo.y == 0) ? float4(ColorCorrection(proteinColor.xyz), 1) : highlight;
					//color = float4(ColorCorrection(proteinColor.xyz), 1);
				}
				else
				{
					discard;
				}
            }
            
            ENDCG
        }

		Pass 
		{
			ZTest Always

            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag			
			#pragma target 5.0		

            #include "UnityCG.cginc"
			
			sampler2D_float _DepthTexture;

            void frag(v2f_img i, out float4 color : COLOR0) 
			{       
				//uint2 coord = uint2(i.uv.x * _ScreenParams.x, i.uv.y * _ScreenParams.y);   
				color.r = Linear01Depth(tex2D(_DepthTexture, i.uv)); 
            }
            
            ENDCG
        }

		// Blit with depth
		Pass
		{
			ZWrite Off
			ZTest Always

			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag

			#include "UnityCG.cginc"

			sampler2D _MainTex;

			void frag(v2f_img i,  out float4 color : COLOR0)
			{
				color = tex2D(_MainTex, i.uv);
			}
			ENDCG
		}
	}	

	FallBack "Diffuse"
}
