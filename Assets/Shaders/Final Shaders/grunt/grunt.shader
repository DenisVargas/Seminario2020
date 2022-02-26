// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "grunt"
{
	Properties
	{
		_gruntText("gruntText", 2D) = "white" {}
		_slimetexture("slime-texture", 2D) = "white" {}
		_flow2("flow2", 2D) = "white" {}
		_motion("motion", Float) = 0.12
		_pannerSpeed("pannerSpeed", Vector) = (0.02,0.02,0,0)
		_drips("drips", 2D) = "white" {}
		_stained("stained", Range( 0 , 1)) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" }
		Cull Back
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform sampler2D _gruntText;
		uniform float4 _gruntText_ST;
		uniform sampler2D _slimetexture;
		uniform float2 _pannerSpeed;
		uniform sampler2D _flow2;
		uniform float4 _flow2_ST;
		uniform float _motion;
		uniform sampler2D _drips;
		uniform float _stained;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_gruntText = i.uv_texcoord * _gruntText_ST.xy + _gruntText_ST.zw;
			float4 tex2DNode4 = tex2D( _gruntText, uv_gruntText );
			float2 temp_cast_0 = (_pannerSpeed.x).xx;
			float2 uv_flow2 = i.uv_texcoord * _flow2_ST.xy + _flow2_ST.zw;
			float4 lerpResult8 = lerp( tex2D( _flow2, uv_flow2 ) , float4( i.uv_texcoord, 0.0 , 0.0 ) , _motion);
			float2 panner10 = ( 1.0 * _Time.y * temp_cast_0 + lerpResult8.rg);
			float grayscale12 = Luminance(tex2D( _slimetexture, panner10 ).rgb);
			float4 color13 = IsGammaSpace() ? float4(0.1280935,0.6226415,0.1262905,0) : float4(0.01494889,0.3456162,0.01459803,0);
			float2 panner25 = ( 1.0 * _Time.y * float2( 0,0.01 ) + i.uv_texcoord);
			float2 lerpResult21 = lerp( i.uv_texcoord , panner25 , 0.97);
			float4 lerpResult18 = lerp( tex2DNode4 , ( grayscale12 * color13 ) , tex2D( _drips, lerpResult21 ));
			float4 lerpResult32 = lerp( tex2DNode4 , lerpResult18 , _stained);
			float4 albedo15 = lerpResult32;
			o.Albedo = albedo15.rgb;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=17800
365;134;1448;845;749.4261;873.6649;1;True;False
Node;AmplifyShaderEditor.SamplerNode;5;-2163.024,-1396.436;Inherit;True;Property;_flow2;flow2;2;0;Create;True;0;0;False;0;-1;bb8b3ab835b3f43409dfcf1693c0145f;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;6;-2069.123,-1166.364;Inherit;False;Property;_motion;motion;3;0;Create;True;0;0;False;0;0.12;0.12;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;7;-2099.327,-1043.76;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector2Node;9;-1861.104,-1596.659;Inherit;False;Property;_pannerSpeed;pannerSpeed;4;0;Create;True;0;0;False;0;0.02,0.02;0.02,0.02;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.LerpOp;8;-1826.85,-1334.546;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.PannerNode;10;-1630.556,-1419.191;Inherit;True;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;19;-1472.283,-553.2772;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;11;-1296.761,-1391.22;Inherit;True;Property;_slimetexture;slime-texture;1;0;Create;True;0;0;False;0;-1;99886758b340ec3489fd041100f00f8c;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PannerNode;25;-1178.567,-489.3334;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0.01;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;22;-1464.341,-681.9721;Inherit;False;Constant;_Float0;Float 0;6;0;Create;True;0;0;False;0;0.97;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;21;-1071.578,-714.5356;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TFHCGrayscale;12;-981.1554,-1223.89;Inherit;True;0;1;0;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;13;-1261.116,-1000.036;Inherit;False;Constant;_MainColor;MainColor;3;0;Create;True;0;0;False;0;0.1280935,0.6226415,0.1262905,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;4;-772.0287,-410.0052;Inherit;True;Property;_gruntText;gruntText;0;0;Create;True;0;0;False;0;-1;4c7269a967cf9ac4f98bdde969bc72f6;4c7269a967cf9ac4f98bdde969bc72f6;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;14;-654.9384,-1132.63;Inherit;True;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;17;-865.04,-747.8162;Inherit;True;Property;_drips;drips;5;0;Create;True;0;0;False;0;-1;016359dada0f1f44ab2ba2c494593c23;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;18;-251.4171,-815.7903;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;31;-319.9445,-383.7892;Inherit;False;Property;_stained;stained;7;0;Create;True;0;0;False;0;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;32;32.65625,-584.2435;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;15;296.0053,-576.6307;Inherit;False;albedo;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;16;-233.3158,-8.785123;Inherit;False;15;albedo;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;0,0;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;grunt;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;5;True;True;0;False;Opaque;;Geometry;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;8;0;5;0
WireConnection;8;1;7;0
WireConnection;8;2;6;0
WireConnection;10;0;8;0
WireConnection;10;2;9;1
WireConnection;11;1;10;0
WireConnection;25;0;19;0
WireConnection;21;0;19;0
WireConnection;21;1;25;0
WireConnection;21;2;22;0
WireConnection;12;0;11;0
WireConnection;14;0;12;0
WireConnection;14;1;13;0
WireConnection;17;1;21;0
WireConnection;18;0;4;0
WireConnection;18;1;14;0
WireConnection;18;2;17;0
WireConnection;32;0;4;0
WireConnection;32;1;18;0
WireConnection;32;2;31;0
WireConnection;15;0;32;0
WireConnection;0;0;16;0
ASEEND*/
//CHKSM=13522B2B53B78D3FAD3A4185E9144E39FE85D996