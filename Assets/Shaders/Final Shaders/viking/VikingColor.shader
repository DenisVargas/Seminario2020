// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "VikingColor"
{
	Properties
	{
		_colormask("colormask", 2D) = "white" {}
		_skin("skin", Color) = (1,0.4933104,0,0)
		_pants("pants", Color) = (0.3396226,0.1227859,0,0)
		_armor("armor", Color) = (0.5283019,0.5283019,0.5283019,0)
		_vikingTexture2("vikingTexture2", 2D) = "white" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" }
		Cull Back
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform sampler2D _colormask;
		uniform float4 _colormask_ST;
		uniform float4 _skin;
		uniform float4 _armor;
		uniform float4 _pants;
		uniform sampler2D _vikingTexture2;
		uniform float4 _vikingTexture2_ST;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_colormask = i.uv_texcoord * _colormask_ST.xy + _colormask_ST.zw;
			float4 tex2DNode5 = tex2D( _colormask, uv_colormask );
			float2 uv_vikingTexture2 = i.uv_texcoord * _vikingTexture2_ST.xy + _vikingTexture2_ST.zw;
			o.Albedo = ( ( tex2DNode5.r * _skin ) + ( tex2DNode5.g * _armor ) + ( tex2DNode5.b * _pants ) + tex2D( _vikingTexture2, uv_vikingTexture2 ) ).rgb;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=17800
705;80;842;752;2327.955;1069.524;2.855134;True;False
Node;AmplifyShaderEditor.SamplerNode;5;-1413.255,142.3768;Inherit;True;Property;_colormask;colormask;0;0;Create;True;0;0;False;0;-1;caceba2ef439b0a4fb007a58ef021a1a;caceba2ef439b0a4fb007a58ef021a1a;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;9;-1255.555,-460.2691;Inherit;False;Property;_skin;skin;1;0;Create;True;0;0;False;0;1,0.4933104,0,0;0.3867925,0.1755504,0.03101638,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;10;-1279.196,-266.3478;Inherit;False;Property;_armor;armor;3;0;Create;True;0;0;False;0;0.5283019,0.5283019,0.5283019,0;0.2830189,0.09178752,0.009344969,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;11;-1278.675,-67.93114;Inherit;False;Property;_pants;pants;2;0;Create;True;0;0;False;0;0.3396226,0.1227859,0,0;0.2830189,0.2562163,0.244304,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;8;-995.1066,305.3976;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;1,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;6;-929.6521,-160.4574;Inherit;True;2;2;0;FLOAT;0;False;1;COLOR;1,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;7;-888.2199,129.0476;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;1,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;13;-871.8367,609.295;Inherit;True;Property;_vikingTexture2;vikingTexture2;4;0;Create;True;0;0;False;0;-1;2f35993f40f150c459b2e93f0189fafe;2f35993f40f150c459b2e93f0189fafe;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;12;-550.2207,100.3645;Inherit;True;4;4;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;0,0;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;VikingColor;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;5;True;True;0;False;Opaque;;Geometry;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;8;0;5;3
WireConnection;8;1;11;0
WireConnection;6;0;5;1
WireConnection;6;1;9;0
WireConnection;7;0;5;2
WireConnection;7;1;10;0
WireConnection;12;0;6;0
WireConnection;12;1;7;0
WireConnection;12;2;8;0
WireConnection;12;3;13;0
WireConnection;0;0;12;0
ASEEND*/
//CHKSM=EB1B91030F3EBE6533797F86AB5A1A7460231771