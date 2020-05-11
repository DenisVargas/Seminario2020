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

		uniform float4 _Color2;
		uniform float4 _Color1;
		uniform sampler2D _ColorMask1;
		uniform float4 _ColorMask1_ST;
		uniform float4 _Color4;
		uniform float4 _Color3;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_ColorMask1 = i.uv_texcoord * _ColorMask1_ST.xy + _ColorMask1_ST.zw;
			float4 tex2DNode193 = tex2D( _ColorMask1, uv_ColorMask1 );
			float4 lerpResult214 = lerp( _Color2 , _Color1 , ceil( tex2DNode193.g ));
			float4 lerpResult222 = lerp( _Color4 , _Color3 , ceil( tex2DNode193.b ));
			float4 lerpResult211 = lerp( lerpResult214 , lerpResult222 , ( 1.0 - saturate( ceil( ( tex2DNode193.r + tex2DNode193.g ) ) ) ));
			o.Albedo = lerpResult211.rgb;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=17800
-1913;1;1906;1051;539.6318;482.0875;1;False;False
Node;AmplifyShaderEditor.SamplerNode;193;-588.5005,-42.78608;Inherit;True;Property;_ColorMask1;ColorMask1;4;0;Create;True;0;0;False;0;-1;1a6a0ceb6364b264d9558e96417f9c95;8fbc51d973347c44daa20e5cffb9790c;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;223;-176,-16;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CeilOpNode;225;32,-16;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CeilOpNode;235;-256.8388,-277.793;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;70;-556.7977,-375.8811;Inherit;False;Property;_Color1;Color1;0;0;Create;True;0;0;False;0;0.2400546,0.4150943,0,0;0.7169812,0.3935967,0.1251335,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;196;-552.182,-566.9075;Inherit;False;Property;_Color2;Color2;1;0;Create;True;0;0;False;0;0.3396226,0.3396226,0.3396226,0;0.7830189,0.4737797,0.1883677,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CeilOpNode;233;-261.9803,273.8998;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;226;195,-16;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;198;-548.7708,342.063;Inherit;False;Property;_Color4;Color4;3;0;Create;True;0;0;False;0;0.7830189,0.5836802,0.2179156,0;0.754717,0.4558695,0.1317195,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;197;-553.7708,510.0634;Inherit;False;Property;_Color3;Color3;2;0;Create;True;0;0;False;0;0.7075472,0.3701982,0.07676221,0;0.8113208,0.5679314,0.3252937,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;214;4.585773,-566.0484;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.OneMinusNode;242;357.7501,-15.37781;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;222;-53.04064,348.6456;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;211;783.5606,-57.54158;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;1277.355,-29.6968;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;Stylized/Toon_ColorMask;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;ForwardOnly;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0.01;0.2595275,0.764151,0.08290318,1;VertexScale;False;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;223;0;193;1
WireConnection;223;1;193;2
WireConnection;225;0;223;0
WireConnection;235;0;193;2
WireConnection;233;0;193;3
WireConnection;226;0;225;0
WireConnection;214;0;196;0
WireConnection;214;1;70;0
WireConnection;214;2;235;0
WireConnection;242;0;226;0
WireConnection;222;0;198;0
WireConnection;222;1;197;0
WireConnection;222;2;233;0
WireConnection;211;0;214;0
WireConnection;211;1;222;0
WireConnection;211;2;242;0
WireConnection;0;0;211;0
ASEEND*/
//CHKSM=0ECEAFEB4FB8B76306B03543C695017CB09AA131