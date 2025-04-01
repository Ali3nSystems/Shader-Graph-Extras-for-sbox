namespace Editor.ShaderGraph.Nodes;

public enum SGEBlendNormalsMode
{
	Whiteout,
	Reoriented,
}

[Title( "SGE - Blend Normals" ), Category( "Transform" ), Icon( "blender" )]

public sealed class SGEBlendNormalsNode : ShaderNode
{	
	[Hide]
	public string Whiteout => @"
		float3 Whiteout(float3 a, float3 b)
		{
			return normalize(float3(a.rg + b.rg, a.b * b.b));
		}
		";
	
	[Hide]
	public string Reoriented => @"					
		float3 Reoriented(float3 a, float3 b)
		{
			float3 t = a.xyz + float3(0.0, 0.0, 1.0);
			float3 u = b.xyz * float3(-1.0, -1.0, 1.0);
			return float3((t / t.z) * dot(t, u) - u);
		}
		";
		
	[Input( typeof( Vector3 ) )]
	[Hide]
	public NodeInput A { get; set; }
	
	[Input( typeof( Vector3 ) )]
	[Hide]
	public NodeInput B { get; set; }

	public SGEBlendNormalsMode Mode { get; set; } = SGEBlendNormalsMode.Whiteout;

	[Output( typeof( Vector3 ))]
	[Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		var resultA = compiler.ResultOrDefault(A, Color.Blue).Cast(3);
		var resultB = compiler.ResultOrDefault(B, Color.Blue).Cast(3);

		NodeResult nodeResult = new NodeResult();
		
		switch (Mode)
		{
			case SGEBlendNormalsMode.Whiteout:
				nodeResult = new NodeResult(NodeResultType.Vector3 , compiler.ResultFunction(compiler.RegisterFunction( Whiteout ), $"{resultA}, {resultB}"));
				break;
			
			case SGEBlendNormalsMode.Reoriented:
				nodeResult = new NodeResult(NodeResultType.Vector3 , compiler.ResultFunction(compiler.RegisterFunction( Reoriented ), $"{resultA}, {resultB}"));
				break;
		}

		return nodeResult;
	};
}