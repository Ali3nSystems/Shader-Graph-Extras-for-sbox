namespace Editor.ShaderGraphExtras;
public static class SGESkyboxTemplate
{
	public static Dictionary<string, bool> Features => new()
	{
		{ "SupportsAlbedo", true},
		{ "SupportsEmission", false },
		{ "SupportsOpacity", false },
		{ "SupportsNormal", false },
		{ "SupportsRoughness", false },
		{ "SupportsMetalness", false },
		{ "SupportsAmbientOcclusion", false },
		{ "SupportsPositionOffset", false },
		{ "SupportsPixelDepthOffset", false},

		{ "SupportsLitShadingModel", false},
		{ "SupportsUnlitShadingModel", true},
		{ "SupportsCustomShadingModel", false},

		{ "SupportsOpaqueBlendMode", false},
		{ "SupportsMaskedBlendMode", false},
		{ "SupportsTranslucentBlendMode", false},
		{ "SupportsDynamicBlendMode", false},
		{ "SupportsCustomBlendMode", true}
	};

	public static string Code => @"
HEADER
{{
	Description = ""{0}"";
}}

FEATURES
{{
	#include ""common/features.hlsl""
{1}
}}

MODES
{{
	Forward();
}}

COMMON
{{
{2}
	#include ""common/shared.hlsl""
	#include ""procedural.hlsl""
}}

struct VertexInput
{{
	float4 vPositionOs : POSITION < Semantic( PosXyz ); >;
{3}
}};

struct PixelInput
{{
	// Graph nodes (World Position, view vector, triplanar, ...) emit
	// ""i.vPositionWithOffsetWs.xyz + g_vHighPrecisionLightingOffsetWs.xyz"",
	// so this field has to exist even though the sky builds its own position.
	float3 vPositionWithOffsetWs : TEXCOORD1;
	float3 vRayWs : TEXCOORD2;

	#if ( PROGRAM == VFX_PROGRAM_VS )
		float4 vPositionPs : SV_Position;
	#endif
	#if ( PROGRAM == VFX_PROGRAM_PS )
		float4 vPositionSs : SV_Position;
	#endif
{4}
}};

VS
{{
	// NOTE: no ""common/vertex.hlsl"" here -- it defines ProcessVertex/FinalizeVertex
	// in terms of VS_CommonProcessing, which only exists for the standard vertex
	// input path. The sky box builds its own position, so it isn't needed.
	#include ""system.fxc""
{5}{6}{7}
	PixelInput MainVs( VertexInput v )
	{{
		PixelInput i;

		// Push the box out to the far plane and keep it centred on the camera
		// so the sky never intersects world geometry.
		float flSkyboxScale = g_flNearPlane + g_flFarPlane;
		float3 vPositionWs = g_vCameraPositionWs.xyz + v.vPositionOs.xyz * flSkyboxScale;

		i.vPositionPs = Position3WsToPs( vPositionWs );

		// Camera-relative, matching the standard vertex path -- graph nodes add
		// g_vHighPrecisionLightingOffsetWs back on to recover absolute world space.
		i.vPositionWithOffsetWs = vPositionWs - g_vHighPrecisionLightingOffsetWs.xyz;
		i.vRayWs = normalize( v.vPositionOs.xyz );
{8}
		return i;
	}}
}}

PS
{{
	// Our PixelInput has none of the standard mesh fields (normals, tangents,
	// lightmap UVs), so Material::Init( PixelInput ) can't read them. This
	// switches it to the empty Material::Init() -- see common/material.hlsl.
	#define CUSTOM_MATERIAL_INPUTS 1

	#include ""common/pixel.hlsl""
{9}{10}{11}
	// Sky renders behind everything: no depth write, reversed-Z far plane test.
	RenderState( CullMode, NONE );
	RenderState( DepthWriteEnable, false );
	RenderState( DepthEnable, true );
	RenderState( DepthFunc, GREATER_EQUAL );

	BoolAttribute( sky, true );

	float4 MainPs( PixelInput i ) : SV_Target0
	{{
		Material m = Material::Init();
		m.Albedo = float3( 0, 0, 0 );
		m.Opacity = 1;
{12}
		m.Opacity = saturate( m.Opacity );
{13}
	}}
}}";
}
