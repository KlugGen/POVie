// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/Blur" {
	Properties{
			_Size("Blur", Range(0, 30)) = 1

			[PerRendererData] _MainTex("Base (RGB)", 2D) = "white" {}
			_StencilComp("Stencil Comparison", Float) = 8
			_Stencil("Stencil ID", Float) = 0
			_StencilOp("Stencil Operation", Float) = 0
			_StencilWriteMask("Stencil Write Mask", Float) = 255
			_StencilReadMask("Stencil Read Mask", Float) = 255

			_ColorMask("Color Mask", Float) = 15

			_Tex("Base (RGB)", 2D) = "white" {}

			[Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip("Use Alpha Clip", Float) = 0
	}

		SubShader
			{
				// Draw ourselves after all opaque geometry
				Tags
				{
					"Queue" = "Transparent"
					"IgnoreProjector" = "True"
					"RenderType" = "Transparent"
					"PreviewType" = "Plane"
					"CanUseSpriteAtlas" = "True"
				}

				Stencil
				{
					Ref[_Stencil]
					Comp[_StencilComp]
					Pass[_StencilOp]
					ReadMask[_StencilReadMask]
					WriteMask[_StencilWriteMask]
				}

				Cull Off
				Lighting Off
				ZWrite Off
				ZTest[unity_GUIZTestMode]
				Blend SrcAlpha OneMinusSrcAlpha
				ColorMask[_ColorMask]

				// Grab the screen behind the object into _BackgroundTexture
				GrabPass
				{
					"_BackgroundTexture"
				}

				// Render the object with the texture generated above, and invert the colors
				Pass
				{
					CGPROGRAM
					#pragma vertex vert
					#pragma fragment frag

					#pragma fragmentoption ARB_precision_hint_fastest

					#include "UnityCG.cginc"
					#include "UnityUI.cginc"

					#pragma multi_compile __ UNITY_UI_ALPHACLIP

					struct appdata_t
					{
						float4 vertex   : POSITION;
						float4 color    : COLOR;
						float2 texcoord : TEXCOORD0;
					};

					struct v2f
					{
						float4 grabPos : TEXCOORD0;
						float4 pos : SV_POSITION;
						float4 color    : COLOR;
					};

					v2f vert(appdata_t v) {
						//v2f o;
						//// use UnityObjectToClipPos from UnityCG.cginc to calculate 
						//// the clip-space of the vertex
						//o.pos = UnityObjectToClipPos(v.vertex);
						//// use ComputeGrabScreenPos function from UnityCG.cginc
						//// to get the correct texture coordinate
						//o.grabPos = ComputeGrabScreenPos(o.pos);

						//return o;

						v2f o;
						o.pos = UnityObjectToClipPos(v.vertex);
						#if UNITY_UV_STARTS_AT_TOP
						float scale = -1.0;
						#else
						float scale = 1.0;
						#endif
						o.grabPos.xy = (float2(o.pos.x, o.pos.y * scale) + o.pos.w) * 0.5;
						o.grabPos.zw = o.pos.zw;

						o.color = v.color;

						return o;
					}

					sampler2D _BackgroundTexture;
					float4 _BackgroundTexture_TexelSize;

					//sampler2D _HBlur;
					//float4 _HBlur_TexelSize;
					float _Size;

					half4 frag(v2f i) : SV_Target
					{
						//half4 bgcolor = tex2Dproj(_BackgroundTexture, UNITY_PROJ_COORD(i.grabPos));
						//half4 color = 1 - bgcolor;
						//color.a = 1;
						//return color;


						float alpha = tex2D(_BackgroundTexture, i.grabPos).a;
						half4 sum = half4(0,0,0,0);

						#define GRABPIXEL_H(weight,kernelx) tex2Dproj( _BackgroundTexture, UNITY_PROJ_COORD(float4(i.grabPos.x + _BackgroundTexture_TexelSize.x * kernelx * _Size * alpha, i.grabPos.y, i.grabPos.z, i.grabPos.w))) * weight

						sum += GRABPIXEL_H(0.05, -4.0);
						sum += GRABPIXEL_H(0.09, -3.0);
						sum += GRABPIXEL_H(0.12, -2.0);
						sum += GRABPIXEL_H(0.15, -1.0);
						sum += GRABPIXEL_H(0.18,  0.0);
						sum += GRABPIXEL_H(0.15, +1.0);
						sum += GRABPIXEL_H(0.12, +2.0);
						sum += GRABPIXEL_H(0.09, +3.0);
						sum += GRABPIXEL_H(0.05, +4.0);

						#define GRABPIXEL_V(weight,kernely) tex2Dproj( _BackgroundTexture, UNITY_PROJ_COORD(float4(i.grabPos.x, i.grabPos.y + _BackgroundTexture_TexelSize.y * kernely * _Size * alpha, i.grabPos.z, i.grabPos.w))) * weight

						sum += GRABPIXEL_V(0.05, -4.0);
						sum += GRABPIXEL_V(0.09, -3.0);
						sum += GRABPIXEL_V(0.12, -2.0);
						sum += GRABPIXEL_V(0.15, -1.0);
						sum += GRABPIXEL_V(0.18, 0.0);
						sum += GRABPIXEL_V(0.15, +1.0);
						sum += GRABPIXEL_V(0.12, +2.0);
						sum += GRABPIXEL_V(0.09, +3.0);
						sum += GRABPIXEL_V(0.05, +4.0);

						sum.rgb /= 2;
						return sum * i.color;




						//float alpha = tex2D(_BackgroundTexture, i.grabPos).a;
						//float alpha = tex2D(_BackgroundTexture, i.grabPos).a;
						//half4 sum = half4(0,0,0,0);
					}
					ENDCG
				}

			}
}
