namespace Editor.ShaderGraph.Nodes;

[Title( "SGE - Random Range" ), Category( "Shader Graph Extras" ), Icon("shuffle")]
public sealed class SGERandomRangeNode : ShaderNode
{
	[Hide]
	public static string SGERandomRange => @"
		float SGERandomRange(float2 input, float min, float max)
		{
			return lerp(min, max, frac(sin(dot(input, float2(12.9898, 78.233)))*43758.5453));
		}
		";

	[Input( typeof( Vector2 ) )]
	[Hide]
	public NodeInput Input {get; set;}
	
	[Input( typeof( float ) )]
	[Hide]
	public NodeInput Min {get; set;}
	
	[Input( typeof( float ) )]
	[Hide]
	public NodeInput Max {get; set;}

	[InputDefault (nameof( Min ))]
	public float DefaultMin {get; set;} = 0f;

	[InputDefault (nameof( Max ))]
	public float DefaultMax {get; set;} = 1f;
	
	[Output( typeof( float ) )]
	[Hide]
	public NodeResult.Func Output => ( GraphCompiler compiler ) =>
	{
		var input = compiler.Result(Input).Cast(2);
		var min = compiler.ResultOrDefault(Min, DefaultMin);
		var max = compiler.ResultOrDefault(Max, DefaultMax);
		
		return new NodeResult( NodeResultType.Float, compiler.ResultFunction( compiler.RegisterFunction( SGERandomRange ), $"{input}, {min}, {max}" ) );
	};
}