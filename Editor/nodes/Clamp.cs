namespace Editor.ShaderGraph.Nodes;

[Title( "SGE - Clamp" ), Category( "Shader Graph Extras" ), Icon( "plumbing" )]
public sealed class SGEClampNode : ShaderNode
{
	[Hide]
	public string SGEClamp => @"
		float SGEClamp( float input, float min, float max )
		{
			return clamp(input, min, max);
		}
		";

	[Input (typeof( float ))]
	[Hide]
	public NodeInput Input {get; set;}
	
	[Input (typeof( float ))]
	[Hide]
	public NodeInput Min {get; set;}
	
	[Input (typeof( float ))]
	[Hide]
	public NodeInput Max {get; set;}
	
	[InputDefault( nameof( Min ) )]
	public float DefaultMin {get; set;} = 0f;
	
    [InputDefault( nameof( Max ) )]
	public float DefaultMax {get; set;} = 1f;

	[Output( typeof( float ) )]
	[Hide]
	public NodeResult.Func Output => ( GraphCompiler compiler ) =>
	{
		var input = compiler.Result(Input);
		var min = compiler.ResultOrDefault(Min, DefaultMin);
		var max = compiler.ResultOrDefault(Max, DefaultMax);
		
		return new NodeResult( NodeResultType.Float, compiler.ResultFunction( compiler.RegisterFunction ( SGEClamp ), $"{input}, {min}, {max}" ) );
	};
}