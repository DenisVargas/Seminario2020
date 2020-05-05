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
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Lambert keepalpha addshadow fullforwardshadows noshadow 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform float4 _Color1;
		uniform sampler2D _ColorMasks;
		uniform float4 _ColorMasks_ST;
		uniform float4 _Color2;
		uniform float _EmissionIntensity;

		void surf( Input i , inout SurfaceOutput o )
		{
			float2 uv_ColorMasks = i.uv_texcoord * _ColorMasks_ST.xy + _ColorMasks_ST.zw;
			float4 tex2DNode1 = tex2D( _ColorMasks, uv_ColorMasks );
			float4 lerpResult5 = lerp( _Color1 , float4( 0,0,0,0 ) , tex2DNode1.r);
			float4 lerpResult6 = lerp( _Color2 , float4( 0,0,0,0 ) , tex2DNode1.g);
			float4 temp_output_7_0 = ( lerpResult5 + lerpResult6 );
			o.Albedo = temp_output_7_0.rgb;
			float4 temp_cast_1 = (_EmissionIntensity).xxxx;
			o.Emission = ( temp_output_7_0 - temp_cast_1 ).rgb;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=17200
-1913;7;1906;1044;1461.73;307.0483;1.215873;True;False
Node;AmplifyShaderEditor.ColorNode;3;-549.3761,-55.67865;Inherit;False;Property;_Color1;Color 1;2;0;Create;True;0;0;False;0;0,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;4;-556.376,228.9213;Inherit;False;Property;_Color2;Color 2;1;0;Create;True;0;0;False;0;0,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;1;-1006.713,55.99257;Inherit;True;Property;_ColorMasks;Color Masks;0;0;Create;True;0;0;False;0;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;6;-308.4104,295.6264;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;5;-315.2762,33.72135;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;7;-17.20867,156.5265;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;8;-105.1079,536.5023;Inherit;False;Property;_EmissionIntensity;Emission Intensity;3;0;Create;True;0;0;False;0;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;9;205.6637,358.7056;Inherit;True;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;464.8,155.9;Float;False;True;2;ASEMaterialInspector;0;0;Lambert;Stylized/Toon_2Color;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;False;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;6;0;4;0
WireConnection;6;2;1;2
WireConnection;5;0;3;0
WireConnection;5;2;1;1
WireConnection;7;0;5;0
WireConnection;7;1;6;0
WireConnection;9;0;7;0
WireConnection;9;1;8;0
WireConnection;0;0;7;0
WireConnection;0;2;9;0
ASEEND*/
//CHKSM=5A3C15DD34BE5E75D3A3ACB93C199EEEBF281A04