// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Slug"
{
	Properties
	{
		_images("images", 2D) = "white" {}
		_slugColorMask("slugColorMask", 2D) = "white" {}
		_slimetexture("slime-texture", 2D) = "white" {}
		_images_NORM("images_NORM", 2D) = "bump" {}
		_TextureSample0("Texture Sample 0", 2D) = "white" {}
		_Skincolor("Skincolor", Color) = (0.0676592,0.9622642,0,0)
		_Teeth("Teeth", 2D) = "white" {}
		_frecuencia("frecuencia", Float) = 0
		_speed("speed", Float) = 0
		_intensidad("intensidad", Float) = 8
		_slugColorMask2("slugColorMask2", 2D) = "white" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IgnoreProjector" = "True" }
		Cull Back
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows vertex:vertexDataFunc 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform sampler2D _slugColorMask2;
		uniform float4 _slugColorMask2_ST;
		uniform float _speed;
		uniform float _frecuencia;
		uniform float _intensidad;
		uniform sampler2D _images_NORM;
		uniform float4 _images_NORM_ST;
		uniform sampler2D _Teeth;
		uniform float4 _Teeth_ST;
		uniform sampler2D _slugColorMask;
		uniform float4 _slugColorMask_ST;
		uniform sampler2D _slimetexture;
		uniform float4 _slimetexture_ST;
		uniform float4 _Skincolor;
		uniform sampler2D _images;
		uniform float4 _images_ST;
		uniform sampler2D _TextureSample0;
		uniform float4 _TextureSample0_ST;

		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float2 uv_slugColorMask2 = v.texcoord * _slugColorMask2_ST.xy + _slugColorMask2_ST.zw;
			float mulTime79 = _Time.y * _speed;
			float3 ase_vertex3Pos = v.vertex.xyz;
			float temp_output_86_0 = ( tex2Dlod( _slugColorMask2, float4( uv_slugColorMask2, 0, 0.0) ).r * ( sin( ( mulTime79 + ( ase_vertex3Pos.x * _frecuencia ) ) ) * _intensidad ) );
			float vertoff44 = temp_output_86_0;
			float3 temp_cast_0 = (vertoff44).xxx;
			v.vertex.xyz += temp_cast_0;
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_images_NORM = i.uv_texcoord * _images_NORM_ST.xy + _images_NORM_ST.zw;
			o.Normal = UnpackNormal( tex2D( _images_NORM, uv_images_NORM ) );
			float2 uv_Teeth = i.uv_texcoord * _Teeth_ST.xy + _Teeth_ST.zw;
			float2 uv_slugColorMask = i.uv_texcoord * _slugColorMask_ST.xy + _slugColorMask_ST.zw;
			float4 tex2DNode11 = tex2D( _slugColorMask, uv_slugColorMask );
			float2 uv_slimetexture = i.uv_texcoord * _slimetexture_ST.xy + _slimetexture_ST.zw;
			float4 color32 = IsGammaSpace() ? float4(1,1,1,0) : float4(1,1,1,0);
			float2 uv_images = i.uv_texcoord * _images_ST.xy + _images_ST.zw;
			float4 color9 = IsGammaSpace() ? float4(0.1936899,0.5188679,0.1492969,0) : float4(0.03117264,0.2319225,0.01944564,0);
			float2 uv_TextureSample0 = i.uv_texcoord * _TextureSample0_ST.xy + _TextureSample0_ST.zw;
			float4 lerpResult27 = lerp( ( tex2DNode11.g * ( tex2D( _images, uv_images ).r * color9 ) ) , ( ( tex2DNode11.g * tex2D( _TextureSample0, uv_TextureSample0 ) ) * _Skincolor ) , 0.51);
			float4 albedog14 = ( tex2D( _Teeth, uv_Teeth ) + ( ( tex2DNode11.r * tex2D( _slimetexture, uv_slimetexture ) ) * _Skincolor ) + ( tex2DNode11.b * color32 ) + lerpResult27 );
			o.Albedo = albedog14.rgb;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=17800
888;186;859;751;3038.547;1617.831;2.82369;True;False
Node;AmplifyShaderEditor.PosVertexDataNode;72;-2226.225,997.7297;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;74;-2182.368,1307.783;Inherit;False;Property;_frecuencia;frecuencia;7;0;Create;True;0;0;False;0;0;38.5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;81;-1961.914,974.1213;Inherit;False;Property;_speed;speed;8;0;Create;True;0;0;False;0;0;3;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;2;-2576.495,-613.1272;Inherit;True;Property;_images;images;0;0;Create;True;0;0;False;0;-1;09d0f3a5305f44d43b3ebb55298e1e4f;09d0f3a5305f44d43b3ebb55298e1e4f;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;24;-1923.145,104.5389;Inherit;True;Property;_TextureSample0;Texture Sample 0;4;0;Create;True;0;0;False;0;-1;99886758b340ec3489fd041100f00f8c;99886758b340ec3489fd041100f00f8c;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;11;-2594.664,-1158.9;Inherit;True;Property;_slugColorMask;slugColorMask;1;0;Create;True;0;0;False;0;-1;737fb77f8d120064b94788a684d22b22;737fb77f8d120064b94788a684d22b22;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleTimeNode;79;-1798.062,954.757;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;73;-1967.871,1178.191;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;9;-2583.982,-337.0755;Inherit;False;Constant;_Color0;Color 0;3;0;Create;True;0;0;False;0;0.1936899,0.5188679,0.1492969,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;80;-1637.189,1072.432;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;36;-1561.345,-123.3516;Inherit;True;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;19;-1718.355,-945.7203;Inherit;False;Property;_Skincolor;Skincolor;5;0;Create;True;0;0;False;0;0.0676592,0.9622642,0,0;0.0676592,0.9622642,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;8;-2191.857,-401.9902;Inherit;True;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;13;-1809.015,-1427.543;Inherit;True;Property;_slimetexture;slime-texture;2;0;Create;True;0;0;False;0;-1;99886758b340ec3489fd041100f00f8c;99886758b340ec3489fd041100f00f8c;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;16;-1910.95,-415.222;Inherit;True;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SinOpNode;76;-1464.232,1096.161;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;84;-1512.891,1402.576;Inherit;False;Property;_intensidad;intensidad;9;0;Create;True;0;0;False;0;8;10;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;35;-1317.026,78.47025;Inherit;False;Constant;_Float0;Float 0;7;0;Create;True;0;0;False;0;0.51;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;32;-1939.216,-709.2601;Inherit;False;Constant;_Color2;Color 2;7;0;Create;True;0;0;False;0;1,1,1,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;12;-1426.499,-1286.724;Inherit;True;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;37;-1308.516,-140.5543;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;83;-1253.283,1085.327;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;18;-1215.305,-981.7699;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;31;-1651.47,-732.3019;Inherit;True;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;42;-1036.819,-1310.88;Inherit;True;Property;_Teeth;Teeth;6;0;Create;True;0;0;False;0;-1;1c048beda85e64a47a6cad1aae0379b0;1c048beda85e64a47a6cad1aae0379b0;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;87;-1792.874,638.4811;Inherit;True;Property;_slugColorMask2;slugColorMask2;10;0;Create;True;0;0;False;0;-1;e9a11b3863156304d845956ef0242edb;e9a11b3863156304d845956ef0242edb;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;27;-1022.404,-180.8263;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;17;-709.1353,-675.647;Inherit;True;4;4;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;86;-597.1326,757.3576;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;44;-366.269,1027.796;Inherit;False;vertoff;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;14;-466.7018,-647.212;Inherit;False;albedog;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;88;-146.6194,751.4619;Inherit;False;movimientoSin;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;45;-228.8712,276.9892;Inherit;True;44;vertoff;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;15;-254.6505,-31.33134;Inherit;False;14;albedog;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;10;-398.4388,72.39994;Inherit;True;Property;_images_NORM;images_NORM;3;0;Create;True;0;0;False;0;-1;ba47bae6ac75ca64a8f09333a4b86184;ba47bae6ac75ca64a8f09333a4b86184;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;0,0;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;Slug;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;5;True;True;0;False;Opaque;;Geometry;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;5;True;0;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;79;0;81;0
WireConnection;73;0;72;1
WireConnection;73;1;74;0
WireConnection;80;0;79;0
WireConnection;80;1;73;0
WireConnection;36;0;11;2
WireConnection;36;1;24;0
WireConnection;8;0;2;1
WireConnection;8;1;9;0
WireConnection;16;0;11;2
WireConnection;16;1;8;0
WireConnection;76;0;80;0
WireConnection;12;0;11;1
WireConnection;12;1;13;0
WireConnection;37;0;36;0
WireConnection;37;1;19;0
WireConnection;83;0;76;0
WireConnection;83;1;84;0
WireConnection;18;0;12;0
WireConnection;18;1;19;0
WireConnection;31;0;11;3
WireConnection;31;1;32;0
WireConnection;27;0;16;0
WireConnection;27;1;37;0
WireConnection;27;2;35;0
WireConnection;17;0;42;0
WireConnection;17;1;18;0
WireConnection;17;2;31;0
WireConnection;17;3;27;0
WireConnection;86;0;87;1
WireConnection;86;1;83;0
WireConnection;44;0;86;0
WireConnection;14;0;17;0
WireConnection;88;0;86;0
WireConnection;0;0;15;0
WireConnection;0;1;10;0
WireConnection;0;11;45;0
ASEEND*/
//CHKSM=535E566ABB8B604C5338BB5B24C01700DFF6C4C4