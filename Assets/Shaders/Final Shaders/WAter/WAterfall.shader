// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "WAterfall"
{
	Properties
	{
		_waterflow("waterflow", 2D) = "white" {}
		_TextureSample0("Texture Sample 0", 2D) = "white" {}
		_noiseScale("noiseScale", Range( 0 , 1)) = 0
		_minNew("minNew", Range( 0 , 1)) = 0
		_MaxNew("MaxNew", Range( 0 , 1)) = 0
		_PannerSpeed("PannerSpeed", Vector) = (0,0,0,0)
		_motion("motion", Float) = 0.87
		_maskmotion("maskmotion", Float) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Back
		CGINCLUDE
		#include "UnityShaderVariables.cginc"
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform sampler2D _waterflow;
		uniform float2 _PannerSpeed;
		uniform sampler2D _TextureSample0;
		uniform float4 _TextureSample0_ST;
		uniform float _motion;
		uniform float _noiseScale;
		uniform float _maskmotion;
		uniform float _minNew;
		uniform float _MaxNew;


		float3 mod2D289( float3 x ) { return x - floor( x * ( 1.0 / 289.0 ) ) * 289.0; }

		float2 mod2D289( float2 x ) { return x - floor( x * ( 1.0 / 289.0 ) ) * 289.0; }

		float3 permute( float3 x ) { return mod2D289( ( ( x * 34.0 ) + 1.0 ) * x ); }

		float snoise( float2 v )
		{
			const float4 C = float4( 0.211324865405187, 0.366025403784439, -0.577350269189626, 0.024390243902439 );
			float2 i = floor( v + dot( v, C.yy ) );
			float2 x0 = v - i + dot( i, C.xx );
			float2 i1;
			i1 = ( x0.x > x0.y ) ? float2( 1.0, 0.0 ) : float2( 0.0, 1.0 );
			float4 x12 = x0.xyxy + C.xxzz;
			x12.xy -= i1;
			i = mod2D289( i );
			float3 p = permute( permute( i.y + float3( 0.0, i1.y, 1.0 ) ) + i.x + float3( 0.0, i1.x, 1.0 ) );
			float3 m = max( 0.5 - float3( dot( x0, x0 ), dot( x12.xy, x12.xy ), dot( x12.zw, x12.zw ) ), 0.0 );
			m = m * m;
			m = m * m;
			float3 x = 2.0 * frac( p * C.www ) - 1.0;
			float3 h = abs( x ) - 0.5;
			float3 ox = floor( x + 0.5 );
			float3 a0 = x - ox;
			m *= 1.79284291400159 - 0.85373472095314 * ( a0 * a0 + h * h );
			float3 g;
			g.x = a0.x * x0.x + h.x * x0.y;
			g.yz = a0.yz * x12.xz + h.yz * x12.yw;
			return 130.0 * dot( m, g );
		}


		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float2 uv_TextureSample0 = v.texcoord * _TextureSample0_ST.xy + _TextureSample0_ST.zw;
			float4 lerpResult50 = lerp( tex2Dlod( _TextureSample0, float4( uv_TextureSample0, 0, 0.0) ) , float4( v.texcoord.xy, 0.0 , 0.0 ) , _motion);
			float2 panner37 = ( 1.0 * _Time.y * _PannerSpeed + lerpResult50.rg);
			float simplePerlin2D3 = snoise( tex2Dlod( _waterflow, float4( panner37, 0, 0.0) ).rg*_noiseScale );
			simplePerlin2D3 = simplePerlin2D3*0.5 + 0.5;
			float motionmask60 = ( 1.0 - ( v.texcoord.xy.y - _maskmotion ) );
			float vertexoffset85 = (0.0 + (( simplePerlin2D3 * motionmask60 ) - 0.0) * (0.3 - 0.0) / (1.0 - 0.0));
			float3 temp_cast_3 = (vertexoffset85).xxx;
			v.vertex.xyz += temp_cast_3;
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float4 color66 = IsGammaSpace() ? float4(1,1,1,0) : float4(1,1,1,0);
			float4 color8 = IsGammaSpace() ? float4(0.3537736,0.4724682,1,1) : float4(0.1027432,0.1894372,1,1);
			float2 uv_TextureSample0 = i.uv_texcoord * _TextureSample0_ST.xy + _TextureSample0_ST.zw;
			float4 lerpResult50 = lerp( tex2D( _TextureSample0, uv_TextureSample0 ) , float4( i.uv_texcoord, 0.0 , 0.0 ) , _motion);
			float2 panner37 = ( 1.0 * _Time.y * _PannerSpeed + lerpResult50.rg);
			float simplePerlin2D3 = snoise( tex2D( _waterflow, panner37 ).rg*_noiseScale );
			simplePerlin2D3 = simplePerlin2D3*0.5 + 0.5;
			float4 lerpResult65 = lerp( color66 , color8 , ( 1.0 - simplePerlin2D3 ));
			float4 emiss83 = lerpResult65;
			o.Emission = emiss83.rgb;
			float opacit87 = saturate( (_minNew + (simplePerlin2D3 - 0.0) * (_MaxNew - _minNew) / (1.24 - 0.0)) );
			o.Alpha = opacit87;
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Standard alpha:fade keepalpha fullforwardshadows exclude_path:deferred vertex:vertexDataFunc 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#pragma multi_compile_shadowcaster
			#pragma multi_compile UNITY_PASS_SHADOWCASTER
			#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
			#include "HLSLSupport.cginc"
			#if ( SHADER_API_D3D11 || SHADER_API_GLCORE || SHADER_API_GLES || SHADER_API_GLES3 || SHADER_API_METAL || SHADER_API_VULKAN )
				#define CAN_SKIP_VPOS
			#endif
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			sampler3D _DitherMaskLOD;
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float2 customPack1 : TEXCOORD1;
				float3 worldPos : TEXCOORD2;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};
			v2f vert( appdata_full v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_INITIALIZE_OUTPUT( v2f, o );
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				Input customInputData;
				vertexDataFunc( v, customInputData );
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				half3 worldNormal = UnityObjectToWorldNormal( v.normal );
				o.customPack1.xy = customInputData.uv_texcoord;
				o.customPack1.xy = v.texcoord;
				o.worldPos = worldPos;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
				return o;
			}
			half4 frag( v2f IN
			#if !defined( CAN_SKIP_VPOS )
			, UNITY_VPOS_TYPE vpos : VPOS
			#endif
			) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				Input surfIN;
				UNITY_INITIALIZE_OUTPUT( Input, surfIN );
				surfIN.uv_texcoord = IN.customPack1.xy;
				float3 worldPos = IN.worldPos;
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				SurfaceOutputStandard o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputStandard, o )
				surf( surfIN, o );
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
				half alphaRef = tex3D( _DitherMaskLOD, float3( vpos.xy * 0.25, o.Alpha * 0.9375 ) ).a;
				clip( alphaRef - 0.01 );
				SHADOW_CASTER_FRAGMENT( IN )
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=17800
705;75;843;757;1935.549;540.6381;2.508429;True;False
Node;AmplifyShaderEditor.TextureCoordinatesNode;36;-2045.36,373.6633;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;49;-1833.435,-44.12932;Inherit;True;Property;_TextureSample0;Texture Sample 0;1;0;Create;True;0;0;False;0;-1;1c0f28bc6d4a1df4c9e2c9aafa790657;1c0f28bc6d4a1df4c9e2c9aafa790657;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;77;-2199.295,689.2671;Inherit;False;Property;_maskmotion;maskmotion;7;0;Create;True;0;0;False;0;0;0.15;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;51;-1589.68,212.4218;Inherit;False;Property;_motion;motion;6;0;Create;True;0;0;False;0;0.87;0.97;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;50;-1460.428,59.50871;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.Vector2Node;38;-1628.59,672.7065;Inherit;False;Property;_PannerSpeed;PannerSpeed;5;0;Create;True;0;0;False;0;0,0;0,1;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleSubtractOpNode;61;-1986.846,561.3383;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0.1;False;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;37;-1361.72,461.6185;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.OneMinusNode;57;-1808.22,557.0909;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;1;-1129.943,467.422;Inherit;True;Property;_waterflow;waterflow;0;0;Create;True;0;0;False;0;-1;1c0f28bc6d4a1df4c9e2c9aafa790657;1c0f28bc6d4a1df4c9e2c9aafa790657;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;60;-1650.381,552.4479;Inherit;False;motionmask;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;26;-1190.246,741.3082;Inherit;False;Property;_noiseScale;noiseScale;2;0;Create;True;0;0;False;0;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;35;-978.5638,1059.529;Inherit;False;Property;_MaxNew;MaxNew;4;0;Create;True;0;0;False;0;0;0.471;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;75;-542.7633,771.8936;Inherit;False;60;motionmask;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;34;-1033.207,921.3721;Inherit;False;Property;_minNew;minNew;3;0;Create;True;0;0;False;0;0;0.3037815;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;3;-774.718,553.746;Inherit;True;Simplex2D;True;False;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;8;-475.6079,-665.3507;Inherit;False;Constant;_Color0;Color 0;2;0;Create;True;0;0;False;0;0.3537736,0.4724682,1,1;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;66;-464.0807,-434.8318;Inherit;False;Constant;_Color1;Color 1;7;0;Create;True;0;0;False;0;1,1,1,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.OneMinusNode;80;-516.9434,445.5511;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;76;-370.33,512.2036;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;33;-529.8554,889.5051;Inherit;True;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1.24;False;3;FLOAT;0.2;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;89;-131.6302,448.9663;Inherit;True;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;0.3;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;74;-141.6328,918.9891;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;65;-119.7616,-414.9214;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;87;181.1654,985.8167;Inherit;False;opacit;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;83;295.2087,-417.4758;Inherit;False;emiss;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;85;195.8456,637.1906;Inherit;False;vertexoffset;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.VoronoiNode;82;-723.145,375.2533;Inherit;False;0;0;1;0;1;False;1;False;False;4;0;FLOAT2;0,0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;2;FLOAT;0;FLOAT;1
Node;AmplifyShaderEditor.GetLocalVarNode;88;22.50171,152.1601;Inherit;False;87;opacit;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;86;-58.24947,243.2912;Inherit;False;85;vertexoffset;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;84;-29.21942,32.36169;Inherit;False;83;emiss;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;22;226.1901,-25.30839;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;WAterfall;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Transparent;0.5;True;True;0;False;Transparent;;Transparent;ForwardOnly;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;2;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;50;0;49;0
WireConnection;50;1;36;0
WireConnection;50;2;51;0
WireConnection;61;0;36;2
WireConnection;61;1;77;0
WireConnection;37;0;50;0
WireConnection;37;2;38;0
WireConnection;57;0;61;0
WireConnection;1;1;37;0
WireConnection;60;0;57;0
WireConnection;3;0;1;0
WireConnection;3;1;26;0
WireConnection;80;0;3;0
WireConnection;76;0;3;0
WireConnection;76;1;75;0
WireConnection;33;0;3;0
WireConnection;33;3;34;0
WireConnection;33;4;35;0
WireConnection;89;0;76;0
WireConnection;74;0;33;0
WireConnection;65;0;66;0
WireConnection;65;1;8;0
WireConnection;65;2;80;0
WireConnection;87;0;74;0
WireConnection;83;0;65;0
WireConnection;85;0;89;0
WireConnection;22;2;84;0
WireConnection;22;9;88;0
WireConnection;22;11;86;0
ASEEND*/
//CHKSM=E51524C821312EE06BFCA19FD76E6BC7F5484E0F