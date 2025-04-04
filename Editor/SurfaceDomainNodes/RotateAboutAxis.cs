namespace Editor.ShaderGraph.Nodes;

public enum SGERotateAboutAxisMode
{
	Radians,
	Degrees,
}

[Title( "SGE - Rotate About Axis" ), Category( "Transform" ), Icon( "360" )]
public sealed class SGERotateAboutAxisNode : ShaderNode
{
	[Hide]
	public string Radians => @"
		float3 Radians( float3 input, float3 axis, float rotation )
		{
			float s = sin(rotation);
			float c = cos(rotation);
			float one_minus_c = 1.0 - c;

			axis = normalize(axis);
			float3x3 rot_mat = 
			{   one_minus_c * axis.x * axis.x + c, one_minus_c * axis.x * axis.y - axis.z * s, one_minus_c * axis.z * axis.x + axis.y * s,
				one_minus_c * axis.x * axis.y + axis.z * s, one_minus_c * axis.y * axis.y + c, one_minus_c * axis.y * axis.z - axis.x * s,
				one_minus_c * axis.z * axis.x - axis.y * s, one_minus_c * axis.y * axis.z + axis.x * s, one_minus_c * axis.z * axis.z + c
			};
			return float3(mul(rot_mat, input));
		}
		";

	[Hide]
	public string Degrees => @"
		float3 Degrees( float3 input, float3 axis, float rotation )
		{
			rotation *= 3.1415926f/180.0f;
			float s = sin(rotation);
			float c = cos(rotation);
			float one_minus_c = 1.0 - c;

			axis = normalize(axis);
			float3x3 rot_mat = 
			{   one_minus_c * axis.x * axis.x + c, one_minus_c * axis.x * axis.y - axis.z * s, one_minus_c * axis.z * axis.x + axis.y * s,
				one_minus_c * axis.x * axis.y + axis.z * s, one_minus_c * axis.y * axis.y + c, one_minus_c * axis.y * axis.z - axis.x * s,
				one_minus_c * axis.z * axis.x - axis.y * s, one_minus_c * axis.y * axis.z + axis.x * s, one_minus_c * axis.z * axis.z + c
			};
			return float3(mul(rot_mat, input));
		}
		";

	[Input (typeof( Vector3))]
	[Hide]
	public NodeInput Input {get; set;}
	
	[Input (typeof(Vector3))]
	[Hide]
	public NodeInput Axis {get; set;}
	
	[Input (typeof(float))]
	[Hide]
	public NodeInput Rotation {get; set;}

	[InputDefault( nameof( Input ) )]
	public Vector3 DefaultInput {get; set;} = new Vector3(0, 0, 0);
	
	[InputDefault( nameof( Axis ) )]
	public Vector3 DefaultAxis {get; set;} = new Vector3(0, 0, 1);

	[MinMax(0, 360)]
	[InputDefault( nameof( Rotation ) )]
	public float DefaultRotation {get; set;} = 0f;
	public SGERotateAboutAxisMode Mode {get; set;} = SGERotateAboutAxisMode.Radians;

	[Output( typeof( Vector3 ) )]
	[Hide]
	public NodeResult.Func Result => ( GraphCompiler compiler ) =>
	{
		var input = compiler.ResultOrDefault(Input, DefaultInput);
		var axis = compiler.ResultOrDefault(Axis, DefaultAxis);
		var rotation = compiler.ResultOrDefault(Rotation, DefaultRotation);
		
		NodeResult nodeResult = new NodeResult();

		switch (Mode)
		{
			case SGERotateAboutAxisMode.Radians:
				nodeResult = new NodeResult( NodeResultType.Vector3 , compiler.ResultFunction( compiler.RegisterFunction(Radians), $"{input}, {axis}, {rotation}" ) );
				break;
			
			case SGERotateAboutAxisMode.Degrees:
				nodeResult = new NodeResult( NodeResultType.Vector3 , compiler.ResultFunction( compiler.RegisterFunction(Degrees), $"{input}, {axis}, {rotation}" ) );
				break;
		}
		
		return nodeResult;
	};
}