// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "FloorBaba"
{
	Properties
	{
		_Cutoff( "Mask Clip Value", Float ) = 0.5
		_Transparency("Transparency", 2D) = "white" {}
		_EmissionPower("EmissionPower", Range( 0 , 1)) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "TransparentCutout"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Standard keepalpha noshadow 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform sampler2D _Transparency;
		uniform float4 _Transparency_ST;
		uniform float _EmissionPower;
		uniform float _Cutoff = 0.5;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float4 color13 = IsGammaSpace() ? float4(0.1280935,0.6226415,0.1262905,0) : float4(0.01494889,0.3456162,0.01459803,0);
			float2 uv_Transparency = i.uv_texcoord * _Transparency_ST.xy + _Transparency_ST.zw;
			float4 tex2DNode2 = tex2D( _Transparency, uv_Transparency );
			o.Emission = ( ( color13 * tex2DNode2.r ) * _EmissionPower ).rgb;
			o.Alpha = 1;
			clip( tex2DNode2.r - _Cutoff );
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=17800
-1914;9;1906;1045;1478.398;450.6946;1.29126;True;False
Node;AmplifyShaderEditor.SamplerNode;2;-755,-14;Inherit;True;Property;_Transparency;Transparency;1;0;Create;True;0;0;False;0;-1;78bf855bc52c048499b9e5d30c635af9;bd63c59298326a543a3917aa28fd3594;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;13;-698.4768,-178.2387;Inherit;False;Constant;_MainColor;MainColor;3;0;Create;True;0;0;False;0;0.1280935,0.6226415,0.1262905,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;5;-419,-46;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;4;-481,383;Inherit;False;Property;_EmissionPower;EmissionPower;2;0;Create;True;0;0;False;0;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;6;-194,179;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;8;113,1;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;FloorBaba;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Custom;0.5;True;False;0;True;TransparentCutout;;Geometry;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;False;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;0;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;5;0;13;0
WireConnection;5;1;2;1
WireConnection;6;0;5;0
WireConnection;6;1;4;0
WireConnection;8;2;6;0
WireConnection;8;10;2;1
ASEEND*/
//CHKSM=9B2180ED4785E634F7276F32A0DCB9BB15074844