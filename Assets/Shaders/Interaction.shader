// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Interaction"
{
	Properties
	{
		_MainColor("MainColor", Color) = (0.5490196,0.4509804,0.4117647,0)
		_EmissionColor("EmissionColor", Color) = (0,0,0,0)
		[HideInInspector]_mouseOver("mouseOver", Float) = 0
		_smoothness("smoothness", Range( 0 , 1)) = 0
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		struct Input
		{
			half filler;
		};

		uniform float4 _MainColor;
		uniform float4 _EmissionColor;
		uniform float _mouseOver;
		uniform float _smoothness;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			o.Albedo = _MainColor.rgb;
			float4 color4 = IsGammaSpace() ? float4(0,0,0,0) : float4(0,0,0,0);
			float4 lerpResult7 = lerp( color4 , _EmissionColor , _mouseOver);
			o.Emission = lerpResult7.rgb;
			o.Smoothness = _smoothness;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=17800
-1664;187;1529;825;1368.366;262.3552;1.3;True;False
Node;AmplifyShaderEditor.CommentaryNode;11;-683.6122,8.164619;Inherit;False;593.3865;488.5614;Interaction;4;4;10;6;7;;0.5754247,1,0.4575472,1;0;0
Node;AmplifyShaderEditor.ColorNode;4;-633.6122,58.16462;Inherit;False;Constant;_Color1;Color 0;8;0;Create;True;0;0;False;0;0,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;10;-630.7001,221.1954;Inherit;False;Property;_EmissionColor;EmissionColor;1;0;Create;True;0;0;False;0;0,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;6;-569.5251,381.726;Inherit;False;Property;_mouseOver;mouseOver;2;1;[HideInInspector];Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;9;-327.1497,542.05;Inherit;False;Property;_smoothness;smoothness;3;0;Create;True;0;0;False;0;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;7;-274.2257,129.326;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;8;-310.6021,-159.2973;Inherit;False;Property;_MainColor;MainColor;0;0;Create;True;0;0;False;0;0.5490196,0.4509804,0.4117647,0;0.4901961,0.2509804,0.145098,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;0,0;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;Interaction;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;5;True;True;0;False;Opaque;;Geometry;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;7;0;4;0
WireConnection;7;1;10;0
WireConnection;7;2;6;0
WireConnection;0;0;8;0
WireConnection;0;2;7;0
WireConnection;0;4;9;0
ASEEND*/
//CHKSM=D84B9E2ECD41F1852A8A68E739E7C3478BFA7ED0