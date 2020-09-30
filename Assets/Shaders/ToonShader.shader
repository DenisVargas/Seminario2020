// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Stylized/Toon_ColorMask"
{
	Properties
	{
		_Color1("Color1", Color) = (0.2400546,0.4150943,0,0)
		_Color2("Color2", Color) = (0.3396226,0.3396226,0.3396226,0)
		_Color3("Color3", Color) = (0.7075472,0.3701982,0.07676221,0)
		_Color4("Color4", Color) = (0.7830189,0.5836802,0.2179156,0)
		_ColorMask1("ColorMask1", 2D) = "white" {}
		_Bias("Bias", Float) = 0
		_Scale("Scale", Float) = 0
		_change("change", Float) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		CGINCLUDE
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		struct Input
		{
			float2 uv_texcoord;
			float3 worldPos;
			float3 worldNormal;
		};

		uniform float4 _Color2;
		uniform float4 _Color1;
		uniform sampler2D _ColorMask1;
		uniform float4 _ColorMask1_ST;
		uniform float4 _Color4;
		uniform float4 _Color3;
		uniform float _Bias;
		uniform float _Scale;
		uniform float _change;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_ColorMask1 = i.uv_texcoord * _ColorMask1_ST.xy + _ColorMask1_ST.zw;
			float4 tex2DNode193 = tex2D( _ColorMask1, uv_ColorMask1 );
			float4 lerpResult214 = lerp( _Color2 , _Color1 , ceil( tex2DNode193.g ));
			float4 lerpResult222 = lerp( _Color4 , _Color3 , ceil( tex2DNode193.b ));
			float4 lerpResult211 = lerp( lerpResult214 , lerpResult222 , ( 1.0 - saturate( ceil( ( tex2DNode193.r + tex2DNode193.g ) ) ) ));
			o.Albedo = lerpResult211.rgb;
			float4 color252 = IsGammaSpace() ? float4(0,0,0,0) : float4(0,0,0,0);
			float3 ase_worldPos = i.worldPos;
			float3 ase_worldViewDir = normalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float3 ase_worldNormal = i.worldNormal;
			float fresnelNdotV243 = dot( ase_worldNormal, ase_worldViewDir );
			float fresnelNode243 = ( _Bias + _Scale * pow( 1.0 - fresnelNdotV243, 0.0 ) );
			float4 temp_cast_1 = (fresnelNode243).xxxx;
			float4 lerpResult250 = lerp( color252 , temp_cast_1 , _change);
			o.Emission = lerpResult250.rgb;
			o.Alpha = 1;
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Standard keepalpha fullforwardshadows exclude_path:deferred 

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
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float2 customPack1 : TEXCOORD1;
				float3 worldPos : TEXCOORD2;
				float3 worldNormal : TEXCOORD3;
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
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				half3 worldNormal = UnityObjectToWorldNormal( v.normal );
				o.worldNormal = worldNormal;
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
				surfIN.worldPos = worldPos;
				surfIN.worldNormal = IN.worldNormal;
				SurfaceOutputStandard o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputStandard, o )
				surf( surfIN, o );
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
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
0;0;1920;1019;395.9536;385.0251;1.3;True;False
Node;AmplifyShaderEditor.SamplerNode;193;-588.5005,-42.78608;Inherit;True;Property;_ColorMask1;ColorMask1;4;0;Create;True;0;0;False;0;-1;1a6a0ceb6364b264d9558e96417f9c95;1a6a0ceb6364b264d9558e96417f9c95;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;223;-176,-16;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CeilOpNode;225;32,-16;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;245;473.7924,380.8434;Inherit;False;Property;_Scale;Scale;6;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;246;495.7925,506.8434;Inherit;False;Constant;_Power;Power;5;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;197;-553.7708,510.0634;Inherit;False;Property;_Color3;Color3;2;0;Create;True;0;0;False;0;0.7075472,0.3701982,0.07676221,0;0.8584906,0.5078996,0.1903257,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;198;-548.7708,342.063;Inherit;False;Property;_Color4;Color4;3;0;Create;True;0;0;False;0;0.7830189,0.5836802,0.2179156,0;0.2169811,0.2141343,0.2118636,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;244;478.1926,282.4431;Inherit;False;Property;_Bias;Bias;5;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.CeilOpNode;233;-261.9803,273.8998;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;196;-552.182,-566.9075;Inherit;False;Property;_Color2;Color2;1;0;Create;True;0;0;False;0;0.3396226,0.3396226,0.3396226,0;0.7735849,0.5771754,0.3393556,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;70;-556.7977,-375.8811;Inherit;False;Property;_Color1;Color1;0;0;Create;True;0;0;False;0;0.2400546,0.4150943,0,0;0.7264151,0.4019549,0.1062211,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CeilOpNode;235;-256.8388,-277.793;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;226;195,-16;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;252;680.4465,68.67479;Inherit;False;Constant;_Color0;Color 0;8;0;Create;True;0;0;False;0;0,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.OneMinusNode;242;357.7501,-15.37781;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;222;-53.04064,348.6456;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.FresnelNode;243;746.136,395.1739;Inherit;False;Standard;WorldNormal;ViewDir;False;False;5;0;FLOAT3;0,0,1;False;4;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;5;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;251;526.5335,182.2362;Inherit;False;Property;_change;change;7;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;214;4.585773,-566.0484;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;211;554.5174,-249.2405;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;250;1039.833,139.8362;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;1277.355,-29.6968;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;Stylized/Toon_ColorMask;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;ForwardOnly;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0.01;0.2595275,0.764151,0.08290318,1;VertexScale;False;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;223;0;193;1
WireConnection;223;1;193;2
WireConnection;225;0;223;0
WireConnection;233;0;193;3
WireConnection;235;0;193;2
WireConnection;226;0;225;0
WireConnection;242;0;226;0
WireConnection;222;0;198;0
WireConnection;222;1;197;0
WireConnection;222;2;233;0
WireConnection;243;1;244;0
WireConnection;243;2;245;0
WireConnection;243;3;246;0
WireConnection;214;0;196;0
WireConnection;214;1;70;0
WireConnection;214;2;235;0
WireConnection;211;0;214;0
WireConnection;211;1;222;0
WireConnection;211;2;242;0
WireConnection;250;0;252;0
WireConnection;250;1;243;0
WireConnection;250;2;251;0
WireConnection;0;0;211;0
WireConnection;0;2;250;0
ASEEND*/
//CHKSM=2F2C94F3144B8C92D55FEFBF1AD721F36B5D0837