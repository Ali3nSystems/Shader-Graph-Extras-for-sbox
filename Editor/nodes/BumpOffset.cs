namespace Editor.ShaderGraph.Nodes;

[Title( "SGE - Bump Offset" ), Category( "Shader Graph Extras" ), Icon( "airline_stops" )]
public sealed class SGEBumpOffsetNode : ShaderNode
{

	[Hide]
	public static string SGEBumpOffset => @"
		float2 SGEBumpOffset( float2 coordinates, float input, float3 tangentSpaceViewDirection, float amplitude, float offset )
		{
			return coordinates.xy + tangentSpaceViewDirection.xy * (-offset + 1 - input) * amplitude;
		}
		";

	[Input( typeof( Vector2 ) )]
	[Hide]
	public NodeInput Coordinates { get; set; }

	[Input( typeof( float ) )]
	[Hide]
	public NodeInput Input { get; set; }
	
	[Title( "Tangent Space View Direction" )]
	[Input( typeof( Vector3 ) )]
	[Hide]
	public NodeInput TangentSpaceViewDirection { get; set; }

	[Hide]
	[Input( typeof( float ) )]
	public NodeInput Amplitude { get; set; }

	[Hide]
	[Input( typeof( float ) )]
	public NodeInput Offset { get; set; }

	public SGEBumpOffsetNode()
	{
		ExpandSize = new Vector2( 32, 0 );
	}

	[InputDefault(nameof(Amplitude))]
	public float DefaultAmplitude { get; set; } = 0.1f;

	[InputDefault( nameof( Offset ) )]
	public float DefaultOffset { get; set; } = 0.3f;

	[Output( typeof( Vector2 ) )]
	[Hide]
	public NodeResult.Func Output => ( GraphCompiler compiler ) =>
	{
		var coordinates = compiler.Result( Coordinates );
		var input = compiler.Result( Input );
		var tangentSpaceViewDirection = compiler.Result( TangentSpaceViewDirection );
		var amplitude = compiler.ResultOrDefault( Amplitude, DefaultAmplitude );
		var offset = compiler.ResultOrDefault( Offset, DefaultOffset );

		return new NodeResult( NodeResultType.Vector2, compiler.ResultFunction(compiler.RegisterFunction(SGEBumpOffset), $"{coordinates}, {input}, {tangentSpaceViewDirection}, {amplitude}, {offset}") );
	};
}