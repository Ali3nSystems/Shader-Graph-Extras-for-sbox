namespace Editor.ShaderGraph.Nodes;

[Title("SGE - Grayscale"), Category("Transform"), Icon("filter_b_and_w")]
public sealed class SGEGrayscaleNode : ShaderNode
{
	[Hide]
	public string Grayscale => @"
		float Grayscale(float3 input)
		{
			return float(dot(input, float3(0.299, 0.587, 0.114)));
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

		return new NodeResult(NodeResultType.Float, compiler.ResultFunction( compiler.RegisterFunction( Grayscale ), $"{input}"));
	};
}