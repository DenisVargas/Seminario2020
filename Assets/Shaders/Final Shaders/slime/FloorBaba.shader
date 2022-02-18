// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "FloorBaba"
{
	Properties
	{
		_Cutoff( "Mask Clip Value", Float ) = 0.5
		_Transparency("Transparency", 2D) = "white" {}
		_slimetexture("slime-texture", 2D) = "white" {}
		_flow2("flow2", 2D) = "white" {}
		_motion("motion", Float) = 0
		_pannerSpeed("pannerSpeed", Vector) = (0,0,0,0)
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma surface surf Standard keepalpha noshadow vertex:vertexDataFunc 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform sampler2D _slimetexture;
		uniform float2 _pannerSpeed;
		uniform sampler2D _flow2;
		uniform float4 _flow2_ST;
		uniform float _motion;
		uniform sampler2D _Transparency;
		uniform float4 _Transparency_ST;
		uniform float _Cutoff = 0.5;

		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float2 temp_cast_0 = (_pannerSpeed.x).xx;
			float2 uv_flow2 = v.texcoord * _flow2_ST.xy + _flow2_ST.zw;
			float4 lerpResult19 = lerp( tex2Dlod( _flow2, float4( uv_flow2, 0, 0.0) ) , float4( v.texcoord.xy, 0.0 , 0.0 ) , _motion);
			float2 panner23 = ( 1.0 * _Time.y * temp_cast_0 + lerpResult19.rg);
			float grayscale15 = Luminance(tex2Dlod( _slimetexture, float4( panner23, 0, 0.0) ).rgb);
			float3 temp_cast_4 = ((0.0 + (grayscale15 - 0.0) * (0.05 - 0.0) / (1.0 - 0.0))).xxx;
			v.vertex.xyz += temp_cast_4;
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 temp_cast_0 = (_pannerSpeed.x).xx;
			float2 uv_flow2 = i.uv_texcoord * _flow2_ST.xy + _flow2_ST.zw;
			float4 lerpResult19 = lerp( tex2D( _flow2, uv_flow2 ) , float4( i.uv_texcoord, 0.0 , 0.0 ) , _motion);
			float2 panner23 = ( 1.0 * _Time.y * temp_cast_0 + lerpResult19.rg);
			float grayscale15 = Luminance(tex2D( _slimetexture, panner23 ).rgb);
			float4 color13 = IsGammaSpace() ? float4(0.1280935,0.6226415,0.1262905,0) : float4(0.01494889,0.3456162,0.01459803,0);
			float2 uv_Transparency = i.uv_texcoord * _Transparency_ST.xy + _Transparency_ST.zw;
			float4 tex2DNode2 = tex2D( _Transparency, uv_Transparency );
			o.Emission = ( ( grayscale15 * color13 ) * tex2DNode2 ).rgb;
			o.Alpha = 1;
			clip( tex2DNode2.r - _Cutoff );
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=17800
614;227;843;792;764.4366;61.00967;1;True;False
Node;AmplifyShaderEditor.SamplerNode;21;-1851.898,-542.3652;Inherit;True;Property;_flow2;flow2;3;0;Create;True;0;0;False;0;-1;bb8b3ab835b3f43409dfcf1693c0145f;bb8b3ab835b3f43409dfcf1693c0145f;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;22;-1757.997,-312.2912;Inherit;False;Property;_motion;motion;4;0;Create;True;0;0;False;0;0;0.2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;18;-1788.201,-189.687;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;19;-1515.724,-480.4741;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.Vector2Node;24;-1478.941,-704.0251;Inherit;False;Property;_pannerSpeed;pannerSpeed;5;0;Create;True;0;0;False;0;0,0;0.02,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.PannerNode;23;-1319.43,-565.1194;Inherit;True;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;14;-985.6357,-537.149;Inherit;True;Property;_slimetexture;slime-texture;2;0;Create;True;0;0;False;0;-1;99886758b340ec3489fd041100f00f8c;99886758b340ec3489fd041100f00f8c;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TFHCGrayscale;15;-670.0299,-369.8174;Inherit;True;0;1;0;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;13;-949.991,-145.9633;Inherit;False;Constant;_MainColor;MainColor;3;0;Create;True;0;0;False;0;0.1280935,0.6226415,0.1262905,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;16;-409.3969,-199.5201;Inherit;True;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;2;-357.6221,195.4927;Inherit;True;Property;_Transparency;Transparency;1;0;Create;True;0;0;False;0;-1;78bf855bc52c048499b9e5d30c635af9;78bf855bc52c048499b9e5d30c635af9;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;17;-20.92911,-6.236169;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;28;-90.76127,421.6503;Inherit;False;Constant;_Color0;Color 0;6;0;Create;True;0;0;False;0;1,1,1,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TFHCRemapNode;25;-567.8567,80.56621;Inherit;True;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;0.05;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;8;209.1859,-30.40763;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;FloorBaba;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Custom;0.5;True;False;0;True;Transparent;;Geometry;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;False;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;0;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;19;0;21;0
WireConnection;19;1;18;0
WireConnection;19;2;22;0
WireConnection;23;0;19;0
WireConnection;23;2;24;1
WireConnection;14;1;23;0
WireConnection;15;0;14;0
WireConnection;16;0;15;0
WireConnection;16;1;13;0
WireConnection;17;0;16;0
WireConnection;17;1;2;0
WireConnection;25;0;15;0
WireConnection;8;2;17;0
WireConnection;8;10;2;0
WireConnection;8;11;25;0
ASEEND*/
//CHKSM=51D8ECA448DEB34AC7C5853795255D88B88F43E7