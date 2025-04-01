namespace Editor.ShaderGraph.Nodes;

[Title("SGE - Negate"), Category("Unary"), Icon("exposure_neg_1")]
public sealed class SGENegateNode : ShaderNode
{
	[Hide]
	public string Negate => @"
		float Negate(float3 input)
		{
			return -1 * input;
		}
		";

	[Input( typeof( Vector3 ) )] 
	[Hide]
	public NodeInput Input { get; set; }

	[Output( typeof( float ) )] 
	[Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		var input = compiler.ResultOrDefault(Input, Vector3.One).Cast(3);

		return new NodeResult(NodeResultType.Float, compiler.ResultFunction( compiler.RegisterFunction( Negate ), $"{input}"));
	};
}