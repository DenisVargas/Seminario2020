// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Stylized/ColorMask_Toon"
{
	Properties
	{
		_Color1("Color1", Color) = (0.2400546,0.4150943,0,0)
		_Color2("Color2", Color) = (0.3396226,0.3396226,0.3396226,0)
		_Color3("Color3", Color) = (0.7075472,0.3701982,0.07676221,0)
		_Color4("Color4", Color) = (0.7830189,0.5836802,0.2179156,0)
		_ColorMask1("ColorMask1", 2D) = "white" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" }
		Cull Back
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows exclude_path:deferred 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform float4 _Color1;
		uniform sampler2D _ColorMask1;
		uniform float4 _ColorMask1_ST;
		uniform float4 _Color2;
		uniform float4 _Color3;
		uniform float4 _Color4;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_ColorMask1 = i.uv_texcoord * _ColorMask1_ST.xy + _ColorMask1_ST.zw;
			float4 tex2DNode193 = tex2D( _ColorMask1, uv_ColorMask1 );
			float4 lerpResult195 = lerp( _Color1 , float4( 0,0,0,0 ) , tex2DNode193.r);
			float4 lerpResult199 = lerp( _Color2 , float4( 0,0,0,0 ) , tex2DNode193.g);
			float4 lerpResult200 = lerp( _Color3 , float4( 0,0,0,0 ) , tex2DNode193.b);
			float4 lerpResult201 = lerp( _Color4 , float4( 0,0,0,0 ) , tex2DNode193.a);
			float4 lerpResult211 = lerp( ( lerpResult195 + lerpResult199 ) , ( lerpResult200 + lerpResult201 ) , ( tex2DNode193.b + tex2DNode193.a ));
			o.Albedo = lerpResult211.rgb;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=17200
-1913;1;1906;1050;2958.314;1277.294;2.797354;True;False
Node;AmplifyShaderEditor.CommentaryNode;212;-710.5009,-391.554;Inherit;False;1668.342;963.1978;Painting With ColorMask;13;193;70;198;197;196;195;199;204;205;208;211;200;201;;0.4423016,0.8396226,0.4000089,1;0;0
Node;AmplifyShaderEditor.ColorNode;198;-223.6442,308.5649;Inherit;False;Property;_Color4;Color4;3;0;Create;True;0;0;False;0;0.7830189,0.5836802,0.2179156,0;0,1,0.002258778,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;197;-238.4806,91.02505;Inherit;False;Property;_Color3;Color3;2;0;Create;True;0;0;False;0;0.7075472,0.3701982,0.07676221,0;1,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;196;-205.7126,-124.946;Inherit;False;Property;_Color2;Color2;1;0;Create;True;0;0;False;0;0.3396226,0.3396226,0.3396226,0;0.6698113,0.628738,0.628738,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;70;-241.1149,-341.554;Inherit;False;Property;_Color1;Color1;0;0;Create;True;0;0;False;0;0.2400546,0.4150943,0,0;0.5566038,0.4293447,0.2861784,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;193;-660.5009,-38.76392;Inherit;True;Property;_ColorMask1;ColorMask1;4;0;Create;True;0;0;False;0;-1;1a6a0ceb6364b264d9558e96417f9c95;102e1a18cf054594a80c941b596fed1a;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;199;137.435,-119.9277;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;200;117.1097,96.1235;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;201;120.3642,318.6438;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;195;87.89336,-336.0135;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;205;446.8415,280.9298;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;204;444.8415,-220.0702;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;208;424.8415,47.92977;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;211;692.8415,-33.07026;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;1277.355,-29.6968;Float;False;True;2;ASEMaterialInspector;0;0;Standard;Stylized/ColorMask_Toon;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;ForwardOnly;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0.01;0.2595275,0.764151,0.08290318,1;VertexScale;False;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;199;0;196;0
WireConnection;199;2;193;2
WireConnection;200;0;197;0
WireConnection;200;2;193;3
WireConnection;201;0;198;0
WireConnection;201;2;193;4
WireConnection;195;0;70;0
WireConnection;195;2;193;1
WireConnection;205;0;200;0
WireConnection;205;1;201;0
WireConnection;204;0;195;0
WireConnection;204;1;199;0
WireConnection;208;0;193;3
WireConnection;208;1;193;4
WireConnection;211;0;204;0
WireConnection;211;1;205;0
WireConnection;211;2;208;0
WireConnection;0;0;211;0
ASEEND*/
//CHKSM=3B77664376190FF614287CA2964B4BAA3ED04D42