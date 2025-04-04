namespace Editor.ShaderGraph.Nodes;

public enum SGERotateUVMode
{
	Radians,
	Degrees,
}

[Title( "SGE - Rotate UV" ), Category( "Transform" ), Icon( "360" )]
public sealed class SGERotateUVNode : ShaderNode
{
	[Hide]
	public string Radians => @"
		float2 Radians( float2 input, float2 center, float rotation )
		{
			input -= center;
			float s = sin(rotation);
			float c = cos(rotation);
			float2x2 rMatrix = float2x2(c, -s, s, c);
			rMatrix *= 0.5;
			rMatrix += 0.5;
			rMatrix = rMatrix * 2 - 1;
			input.xy = mul(input.xy, rMatrix);
			input += center;
			return input;
		}
		";

	[Hide]
	public string Degrees => @"
		float2 Degrees( float2 input, float2 center, float rotation )
		{
			rotation = rotation * (3.1415926f/180.0f);
			input -= center;
			float s = sin(rotation);
			float c = cos(rotation);
			float2x2 rMatrix = float2x2(c, -s, s, c);
			rMatrix *= 0.5;
			rMatrix += 0.5;
			rMatrix = rMatrix * 2 - 1;
			input.xy = mul(input.xy, rMatrix);
			input += center;
			return input;
		}
		";

	[Input (typeof( Vector2 ))]
	[Hide]
	public NodeInput Input {get; set;}
	
	[Input (typeof( Vector2 ))]
	[Hide]
	public NodeInput Center {get; set;}
	
	[Input (typeof( float ))]
	[Hide]
	public NodeInput Rotation {get; set;}

	[InputDefault( nameof( Input ) )]
	public Vector2 DefaultInput {get; set;} = new Vector2(0, 0);
	
	[InputDefault( nameof( Center ) )]
	public Vector2 DefaultCenter {get; set;} = new Vector2(0.5f, 0.5f);

	[MinMax(0, 360)]
	[InputDefault( nameof( Rotation ) )]
	public float DefaultRotation {get; set;} = 0f;
	public SGERotateUVMode Mode {get; set;} = SGERotateUVMode.Radians;

	[Output( typeof( Vector2 ) )]
	[Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		var input = compiler.ResultOrDefault(Input, DefaultInput);
		var center = compiler.ResultOrDefault(Center, DefaultCenter);
		var rotation = compiler.ResultOrDefault(Rotation, DefaultRotation);
		
		NodeResult nodeResult = new NodeResult();

		switch (Mode)
		{
			case SGERotateUVMode.Radians:
				nodeResult = new NodeResult( NodeResultType.Vector2 , compiler.ResultFunction( compiler.RegisterFunction(Radians), $"{input}, {center}, {rotation}" ) );
				break;
			
			case SGERotateUVMode.Degrees:
				nodeResult = new NodeResult( NodeResultType.Vector2 , compiler.ResultFunction( compiler.RegisterFunction(Degrees), $"{input}, {center}, {rotation}" ) );
				break;
		}
		
		return nodeResult;
	};
}