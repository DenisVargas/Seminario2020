// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Stylized/Toon_2Color"
{
	Properties
	{
		_ColorMasks("Color Masks", 2D) = "white" {}
		_Color2("Color 2", Color) = (0,0,0,0)
		_Color1("Color 1", Color) = (0,0,0,0)
		_EmissionIntensity("Emission Intensity", Range( 0 , 1)) = 0
		_metallic("metallic", Range( 0 , 1)) = 0
		_smooth("smooth", Range( 0 , 1)) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		Blend SrcAlpha OneMinusSrcAlpha
		
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows noshadow 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform float4 _Color1;
		uniform sampler2D _ColorMasks;
		uniform float4 _ColorMasks_ST;
		uniform float4 _Color2;
		uniform float _EmissionIntensity;
		uniform float _metallic;
		uniform float _smooth;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_ColorMasks = i.uv_texcoord * _ColorMasks_ST.xy + _ColorMasks_ST.zw;
			float4 tex2DNode1 = tex2D( _ColorMasks, uv_ColorMasks );
			float4 lerpResult5 = lerp( _Color1 , float4( 0,0,0,0 ) , tex2DNode1.r);
			float4 lerpResult6 = lerp( _Color2 , float4( 0,0,0,0 ) , tex2DNode1.g);
			float4 temp_output_7_0 = ( lerpResult5 + lerpResult6 );
			o.Albedo = temp_output_7_0.rgb;
			float4 temp_cast_1 = (_EmissionIntensity).xxxx;
			o.Emission = ( temp_output_7_0 - temp_cast_1 ).rgb;
			o.Metallic = ( tex2DNode1.r * _metallic );
			o.Smoothness = ( tex2DNode1.r * _smooth );
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=17800
174;230;1096;558;1258.532;-251.6118;1.613384;True;False
Node;AmplifyShaderEditor.ColorNode;3;-819.7675,-371.5826;Inherit;False;Property;_Color1;Color 1;2;0;Create;True;0;0;False;0;0,0,0,0;0.3301887,0.3192862,0.2974813,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;4;-826.7674,-86.9826;Inherit;False;Property;_Color2;Color 2;1;0;Create;True;0;0;False;0;0,0,0,0;1,0.9527312,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;1;-1096.095,405.868;Inherit;True;Property;_ColorMasks;Color Masks;0;0;Create;True;0;0;False;0;-1;None;dd43178558b0d99429b2a86da36a5688;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;6;-578.8026,-20.2775;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;5;-585.6683,-282.1826;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;7;-287.6011,-159.3774;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;8;-375.5004,220.5982;Inherit;False;Property;_EmissionIntensity;Emission Intensity;3;0;Create;True;0;0;False;0;0;0.345;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;15;-473.4053,953.0981;Float;False;Property;_smooth;smooth;5;0;Create;True;0;0;False;0;0;0.479;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;16;-350.4117,610.9904;Inherit;False;Property;_metallic;metallic;5;0;Create;True;0;0;False;0;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;9;-64.72865,42.80164;Inherit;True;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;10;-67.89954,462.4439;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;13;-81.77554,755.3864;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;464.8,155.9;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;Stylized/Toon_2Color;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;False;2;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;6;0;4;0
WireConnection;6;2;1;2
WireConnection;5;0;3;0
WireConnection;5;2;1;1
WireConnection;7;0;5;0
WireConnection;7;1;6;0
WireConnection;9;0;7;0
WireConnection;9;1;8;0
WireConnection;10;0;1;1
WireConnection;10;1;16;0
WireConnection;13;0;1;1
WireConnection;13;1;15;0
WireConnection;0;0;7;0
WireConnection;0;2;9;0
WireConnection;0;3;10;0
WireConnection;0;4;13;0
ASEEND*/
//CHKSM=D2FC9885725A1D61A101073C53A0B2250B493251