namespace Editor.ShaderGraph.Nodes;

[Title( "SGE - Random Range" ), Category( "Unary" ), Icon("shuffle")]
public sealed class SGERandomRangeNode : ShaderNode
{
	[Hide]
	public string RandomRange => @"
		float RandomRange(float2 seed, float min, float max)
		{
			return lerp(min, max, frac(sin(dot(seed, float2(12.9898, 78.233)))*43758.5453));
		}
		";

	[Input( typeof( Vector2 ) )]
	[Hide]
	public NodeInput Seed {get; set;}
	
	[Input( typeof( float ) )]
	[Hide]
	public NodeInput Min {get; set;}
	
	[Input( typeof( float ) )]
	[Hide]
	public NodeInput Max {get; set;}
	
	[InputDefault (nameof( Seed ))]
	public Vector2 DefaultSeed {get; set;} = new Vector2 (0f, 0f);

	[InputDefault (nameof( Min ))]
	public float DefaultMin {get; set;} = 0f;

	[InputDefault (nameof( Max ))]
	public float DefaultMax {get; set;} = 1f;
	
	[Output( typeof( float ) )]
	[Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		var seed = compiler.ResultOrDefault(Seed, DefaultSeed).Cast(2);
		var min = compiler.ResultOrDefault(Min, DefaultMin);
		var max = compiler.ResultOrDefault(Max, DefaultMax);
		
		return new NodeResult( NodeResultType.Float, compiler.ResultFunction( compiler.RegisterFunction( RandomRange ), $"{seed}, {min}, {max}" ) );
	};
}