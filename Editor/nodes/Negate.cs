namespace Editor.ShaderGraph.Nodes;

[Title( "SGE - Negate" ), Category( "Shader Graph Extras" ), Icon( "exposure_neg_1" )]
public sealed class SGENegateNode : ShaderNode
{
	[Hide]
	public static string SGENegate => @"
		float4 SGENegate(float4 input)
		{
			return -1 * input;
		}
		";

	[Input( typeof( Color ) )] 
	[Hide]
	public NodeInput Input { get; set; }

	[Output( typeof( Color ) )] 
	[Hide]
	public NodeResult.Func Output => ( GraphCompiler compiler ) =>
	{
		var input = compiler.Result(Input);

		return new NodeResult(NodeResultType.Color, compiler.ResultFunction( compiler.RegisterFunction( SGENegate ), $"{input}"));
	};
}