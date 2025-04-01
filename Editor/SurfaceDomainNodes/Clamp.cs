namespace Editor.ShaderGraph.Nodes;

[Title( "SGE - Clamp" ), Category( "Unary" ), Icon( "plumbing" )]
public sealed class SGEClampNode : ShaderNode
{
	[Hide]
	public string Clamp => @"
		float Clamp( float input, float min, float max )
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

	[InputDefault( nameof( Input ) )]
	public float DefaultInput {get; set;} = 0f;
	
	[InputDefault( nameof( Min ) )]
	public float DefaultMin {get; set;} = 0f;
	
    [InputDefault( nameof( Max ) )]
	public float DefaultMax {get; set;} = 1f;

	[Output( typeof( float ) )]
	[Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		var input = compiler.ResultOrDefault(Input, DefaultInput);
		var min = compiler.ResultOrDefault(Min, DefaultMin);
		var max = compiler.ResultOrDefault(Max, DefaultMax);
		
		return new NodeResult( NodeResultType.Float, compiler.ResultFunction( compiler.RegisterFunction ( Clamp ), $"{input}, {min}, {max}" ) );
	};
}