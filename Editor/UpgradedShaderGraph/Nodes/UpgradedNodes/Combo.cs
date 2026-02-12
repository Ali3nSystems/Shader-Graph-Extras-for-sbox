namespace Editor.ShaderGraphExtras.Nodes;

[Title( "SGE - Combo" ), Category( "Shader Graph Extras - Upgraded" ), Icon( "compare_arrows" )]
public sealed class SGEComboNode : ShaderNode
{
	[Title( "Combo Type" )]
	[Description( "Bool: 2 states (0/1). Enum: Multiple states (0..Range)." )]
	public ComboType ComboType { get; set; } = ComboType.Bool;

	[Title( "Combo Style" )]
	[Description( "Static: Compile-time shader variants. Dynamic: Runtime switching." )]
	public ComboStyle ComboStyle { get; set; } = ComboStyle.Static;

	[Title( "Value" )]
	[Description( "Default state when not set by material (1-based: 1, 2, 3...)." )]
	public int DefaultValue
	{
		get => _defaultValue;
		set => _defaultValue = Math.Clamp( value, 1, IsEnumMode ? StateCount : 2 );
	}
	[Hide, JsonIgnore]
	private int _defaultValue = 1;

	[Title( "Combo Name" )]
	[Description( "Display name for Material Editor (e.g., Types of Blend). Spaces will be converted to underscores for HLSL." )]
	public ComboNameGroup ComboName { get; set; } = "My Combo";

	[Title( "UI Group" )]
	[Description( "Category in Material Editor (e.g., Advanced, Rendering)." )]
	public ComboUIGroup UIGroup { get; set; } = "Advanced";

	[Title( "State Count" )]
	[Description( "Number of states for Enum mode (2-8)." )]
	[ShowIf( nameof( IsEnumMode ), true )]
	public int StateCount
	{
		get => _stateCount;
		set => _stateCount = Math.Clamp( value, 2, 8 );
	}
	[Hide, JsonIgnore]
	private int _stateCount = 2;

	[Hide, JsonIgnore]
	int _lastHashCode = 0;

	public override void OnFrame()
	{
		base.OnFrame();

		// Sync with other combo nodes that share the same identity
		SyncWithMatchingCombos();

		var hashCode = new HashCode();
		hashCode.Add( ComboType );
		hashCode.Add( StateCount );
		hashCode.Add( State1Name );
		hashCode.Add( State2Name );
		hashCode.Add( State3Name );
		hashCode.Add( State4Name );
		hashCode.Add( State5Name );
		hashCode.Add( State6Name );
		hashCode.Add( State7Name );
		hashCode.Add( State8Name );
		var hc = hashCode.ToHashCode();
		if ( hc != _lastHashCode )
		{
			_lastHashCode = hc;
			CreateInputs();
			Update();
		}
	}

	private void CreateInputs()
	{
		var plugs = new List<IPlugIn>();
		var serialized = this.GetSerialized();
		foreach ( var property in serialized )
		{
			if ( property.TryGetAttribute<InputAttribute>( out var inputAttr ) )
			{
				if ( property.TryGetAttribute<ConditionalVisibilityAttribute>( out var conditionalVisibilityAttr ) )
				{
					if ( conditionalVisibilityAttr.TestCondition( this.GetSerialized() ) )
					{
						continue;
					}
				}
				var propertyInfo = typeof( SGEComboNode ).GetProperty( property.Name );
				if ( propertyInfo is null ) continue;
				var info = new PlugInfo( propertyInfo );
				var displayInfo = info.DisplayInfo;

				// Set custom display names based on state names
				// For Bool mode, use "True"/"False" as defaults if state names are still default
				displayInfo.Name = property.Name switch
				{
					nameof( InputTrue ) => State1Name == "State 1" ? "True" : State1Name,
					nameof( InputFalse ) => State2Name == "State 2" ? "False" : State2Name,
					nameof( InputState1 ) => State1Name,
					nameof( InputState2 ) => State2Name,
					nameof( InputState3 ) => State3Name,
					nameof( InputState4 ) => State4Name,
					nameof( InputState5 ) => State5Name,
					nameof( InputState6 ) => State6Name,
					nameof( InputState7 ) => State7Name,
					nameof( InputState8 ) => State8Name,
					nameof( InputValue ) => "Value",
					_ => property.DisplayName
				};

				info.DisplayInfo = displayInfo;

				var oldPlug = Inputs.FirstOrDefault( x => x is BasePlugIn plugIn && plugIn.Info.Name == property.Name ) as BasePlugIn;
				if ( oldPlug is not null )
				{
					oldPlug.Info.Name = info.Name;
					oldPlug.Info.Type = info.Type;
					oldPlug.Info.DisplayInfo = info.DisplayInfo;
					plugs.Add( oldPlug );
				}
				else
				{
					var plug = new BasePlugIn( this, info, info.Type );
					plugs.Add( plug );
				}
			}
		}
		Inputs = plugs;
	}

	// State name properties (one for each potential state)
	// Only show in Enum mode - Bool mode uses "True"/"False" by default
	[Title( "State 1 Name" )]
	[ShowIf( nameof( IsEnumMode ), true )]
	public string State1Name { get; set; } = "State 1";

	[Title( "State 2 Name" )]
	[ShowIf( nameof( IsEnumMode ), true )]
	public string State2Name { get; set; } = "State 2";

	[Title( "State 3 Name" )]
	[ShowIf( nameof( HasState3 ), true )]
	public string State3Name { get; set; } = "State 3";

	[Title( "State 4 Name" )]
	[ShowIf( nameof( HasState4 ), true )]
	public string State4Name { get; set; } = "State 4";

	[Title( "State 5 Name" )]
	[ShowIf( nameof( HasState5 ), true )]
	public string State5Name { get; set; } = "State 5";

	[Title( "State 6 Name" )]
	[ShowIf( nameof( HasState6 ), true )]
	public string State6Name { get; set; } = "State 6";

	[Title( "State 7 Name" )]
	[ShowIf( nameof( HasState7 ), true )]
	public string State7Name { get; set; } = "State 7";

	[Title( "State 8 Name" )]
	[ShowIf( nameof( HasState8 ), true )]
	public string State8Name { get; set; } = "State 8";

	// Helper properties for ShowIf
	[Hide]
	private bool IsEnumMode => ComboType == ComboType.Enum;

	[Hide]
	private bool IsBoolMode => ComboType == ComboType.Bool;

	[Hide]
	private bool HasState3 => IsEnumMode && StateCount >= 3;

	[Hide]
	private bool HasState4 => IsEnumMode && StateCount >= 4;

	[Hide]
	private bool HasState5 => IsEnumMode && StateCount >= 5;

	[Hide]
	private bool HasState6 => IsEnumMode && StateCount >= 6;

	[Hide]
	private bool HasState7 => IsEnumMode && StateCount >= 7;

	[Hide]
	private bool HasState8 => IsEnumMode && StateCount >= 8;

	// ========== Combo Node Syncing ==========
	// When multiple combo nodes share the same identity (ComboType + ComboStyle + ComboName),
	// their syncable properties are automatically kept in sync.

	/// <summary>
	/// Static tracking of which node was last modified per combo identity
	/// </summary>
	[Hide, JsonIgnore]
	private static Dictionary<string, (string nodeId, int hashCode)> _lastModifiedByIdentity = new();

	/// <summary>
	/// Gets a unique identity key for this combo based on Name only
	/// </summary>
	[Hide, JsonIgnore]
	private string ComboIdentity => ComboName.Name;

	/// <summary>
	/// Computes a hash of all syncable properties for change detection
	/// </summary>
	private int GetSyncablePropertiesHash()
	{
		var hash = new HashCode();
		hash.Add( ComboType );
		hash.Add( ComboStyle );
		hash.Add( UIGroup.Name );
		hash.Add( StateCount );
		hash.Add( DefaultValue );
		hash.Add( State1Name );
		hash.Add( State2Name );
		hash.Add( State3Name );
		hash.Add( State4Name );
		hash.Add( State5Name );
		hash.Add( State6Name );
		hash.Add( State7Name );
		hash.Add( State8Name );
		return hash.ToHashCode();
	}

	/// <summary>
	/// Syncs this node with other combo nodes that share the same identity
	/// </summary>
	private void SyncWithMatchingCombos()
	{
		if ( Graph == null ) return;

		var identity = ComboIdentity;
		var currentHash = GetSyncablePropertiesHash();

		// Check if we have a stored hash for this node in this identity
		var myPreviousHash = GetStoredHashForNode( identity, Identifier );
		var isNewToIdentity = myPreviousHash == 0;

		// Check if this identity already has a master
		if ( _lastModifiedByIdentity.TryGetValue( identity, out var last ) )
		{
			if ( isNewToIdentity )
			{
				// We just joined this identity (e.g., changed our name to match)
				// Pull from the existing master instead of pushing our values
				var master = Graph.Nodes.OfType<SGEComboNode>()
					.FirstOrDefault( n => n.Identifier == last.nodeId && n.ComboIdentity == identity );
				if ( master != null && master != this )
				{
					PullSyncFrom( master );
					StoreHashForNode( identity, Identifier, GetSyncablePropertiesHash() );
				}
				else
				{
					// Master no longer exists, register ourselves
					_lastModifiedByIdentity[identity] = (Identifier, currentHash);
					StoreHashForNode( identity, Identifier, currentHash );
				}
				return;
			}

			if ( last.nodeId == Identifier )
			{
				// We were the last modifier - check if we changed again
				if ( last.hashCode != currentHash )
				{
					// We changed - push to others
					PushSyncToOthers( identity );
					_lastModifiedByIdentity[identity] = (Identifier, currentHash);
				}
				return;
			}
			else
			{
				// Another node was last modifier - check if we need to pull or if we changed
				if ( myPreviousHash != currentHash )
				{
					// We changed locally - become the new master and push
					PushSyncToOthers( identity );
					_lastModifiedByIdentity[identity] = (Identifier, currentHash);
					return;
				}

				// We didn't change - pull from the master
				var master = Graph.Nodes.OfType<SGEComboNode>()
					.FirstOrDefault( n => n.Identifier == last.nodeId && n.ComboIdentity == identity );
				if ( master != null && master != this )
				{
					PullSyncFrom( master );
				}
			}
		}
		else
		{
			// First time seeing this identity - check if other nodes already have this identity
			var existingNode = Graph.Nodes.OfType<SGEComboNode>()
				.FirstOrDefault( n => n != this && n.ComboIdentity == identity );

			if ( existingNode != null )
			{
				// Another node already has this identity, pull from it
				PullSyncFrom( existingNode );
				_lastModifiedByIdentity[identity] = (existingNode.Identifier, existingNode.GetSyncablePropertiesHash());
				StoreHashForNode( identity, Identifier, GetSyncablePropertiesHash() );
			}
			else
			{
				// We're the first with this identity
				_lastModifiedByIdentity[identity] = (Identifier, currentHash);
				StoreHashForNode( identity, Identifier, currentHash );
			}
		}
	}

	/// <summary>
	/// Static cache of previous hash values per node per identity
	/// </summary>
	[Hide, JsonIgnore]
	private static Dictionary<string, Dictionary<string, int>> _nodeHashCache = new();

	private int GetStoredHashForNode( string identity, string nodeId )
	{
		if ( _nodeHashCache.TryGetValue( identity, out var nodeHashes ) )
		{
			if ( nodeHashes.TryGetValue( nodeId, out var hash ) )
				return hash;
		}
		return 0;
	}

	private void StoreHashForNode( string identity, string nodeId, int hash )
	{
		if ( !_nodeHashCache.ContainsKey( identity ) )
			_nodeHashCache[identity] = new Dictionary<string, int>();
		_nodeHashCache[identity][nodeId] = hash;
	}

	/// <summary>
	/// Pushes this node's syncable properties to all other nodes with the same identity
	/// </summary>
	private void PushSyncToOthers( string identity )
	{
		if ( Graph == null ) return;

		var others = Graph.Nodes.OfType<SGEComboNode>()
			.Where( n => n != this && n.ComboIdentity == identity );

		foreach ( var other in others )
		{
			other.ComboType = this.ComboType;
			other.ComboStyle = this.ComboStyle;
			other.UIGroup = this.UIGroup;
			other.StateCount = this.StateCount;
			other.DefaultValue = this.DefaultValue;
			other.State1Name = this.State1Name;
			other.State2Name = this.State2Name;
			other.State3Name = this.State3Name;
			other.State4Name = this.State4Name;
			other.State5Name = this.State5Name;
			other.State6Name = this.State6Name;
			other.State7Name = this.State7Name;
			other.State8Name = this.State8Name;

			// Store the hash for the other node so it knows it didn't change locally
			StoreHashForNode( identity, other.Identifier, GetSyncablePropertiesHash() );

			// Mark as dirty to trigger viewport refresh
			other.IsDirty = true;
			other.Update();
		}

		// Store our own hash
		StoreHashForNode( identity, Identifier, GetSyncablePropertiesHash() );
	}

	/// <summary>
	/// Pulls syncable properties from another node (the master)
	/// </summary>
	private void PullSyncFrom( SGEComboNode master )
	{
		ComboType = master.ComboType;
		ComboStyle = master.ComboStyle;
		UIGroup = master.UIGroup;
		StateCount = master.StateCount;
		DefaultValue = master.DefaultValue;
		State1Name = master.State1Name;
		State2Name = master.State2Name;
		State3Name = master.State3Name;
		State4Name = master.State4Name;
		State5Name = master.State5Name;
		State6Name = master.State6Name;
		State7Name = master.State7Name;
		State8Name = master.State8Name;

		StoreHashForNode( ComboIdentity, Identifier, GetSyncablePropertiesHash() );

		// Mark as dirty to trigger viewport refresh
		IsDirty = true;
		Update();
	}

	// ========== End Combo Node Syncing ==========

	[Input( typeof( Vector4 ) )]
	[Hide]
	[ShowIf( nameof( IsBoolMode ), true )]
	[Title( "True" )]
	public NodeInput InputTrue { get; set; }

	// Input properties for Bool mode
	[Input( typeof( Vector4 ) )]
	[Hide]
	[ShowIf( nameof( IsBoolMode ), true )]
	[Title( "False" )]
	public NodeInput InputFalse { get; set; }

	// Input properties for Enum mode (up to 8 states)
	[Input( typeof( Vector4 ) )]
	[Hide]
	[ShowIf( nameof( IsEnumModeState1 ), true )]
	[Title( "State 1" )]
	public NodeInput InputState1 { get; set; }

	[Input( typeof( Vector4 ) )]
	[Hide]
	[ShowIf( nameof( IsEnumModeState2 ), true )]
	[Title( "State 2" )]
	public NodeInput InputState2 { get; set; }

	[Input( typeof( Vector4 ) )]
	[Hide]
	[ShowIf( nameof( IsEnumModeState3 ), true )]
	[Title( "State 3" )]
	public NodeInput InputState3 { get; set; }

	[Input( typeof( Vector4 ) )]
	[Hide]
	[ShowIf( nameof( IsEnumModeState4 ), true )]
	[Title( "State 4" )]
	public NodeInput InputState4 { get; set; }

	[Input( typeof( Vector4 ) )]
	[Hide]
	[ShowIf( nameof( IsEnumModeState5 ), true )]
	[Title( "State 5" )]
	public NodeInput InputState5 { get; set; }

	[Input( typeof( Vector4 ) )]
	[Hide]
	[ShowIf( nameof( IsEnumModeState6 ), true )]
	[Title( "State 6" )]
	public NodeInput InputState6 { get; set; }

	[Input( typeof( Vector4 ) )]
	[Hide]
	[ShowIf( nameof( IsEnumModeState7 ), true )]
	[Title( "State 7" )]
	public NodeInput InputState7 { get; set; }

	[Input( typeof( Vector4 ) )]
	[Hide]
	[ShowIf( nameof( IsEnumModeState8 ), true )]
	[Title( "State 8" )]
	public NodeInput InputState8 { get; set; }

	[Input( typeof( float ) )]
	[Hide]
	[Title( "Value" )]
	public NodeInput InputValue { get; set; }

	[InputDefault( nameof( InputValue ) )]
	[Hide]
	public float DefaultInputValue => DefaultValue;

	// Helper properties for enum input visibility
	// In Enum mode, StateCount is always >= 2, so State1 and State2 are always visible
	[Hide]
	private bool IsEnumModeState1 => IsEnumMode;

	[Hide]
	private bool IsEnumModeState2 => IsEnumMode;

	[Hide]
	private bool IsEnumModeState3 => IsEnumMode && StateCount >= 3;

	[Hide]
	private bool IsEnumModeState4 => IsEnumMode && StateCount >= 4;

	[Hide]
	private bool IsEnumModeState5 => IsEnumMode && StateCount >= 5;

	[Hide]
	private bool IsEnumModeState6 => IsEnumMode && StateCount >= 6;

	[Hide]
	private bool IsEnumModeState7 => IsEnumMode && StateCount >= 7;

	[Hide]
	private bool IsEnumModeState8 => IsEnumMode && StateCount >= 8;

	[Output( typeof( Vector4 ) )]
	[Hide]
	public NodeResult.Func Output => ( GraphCompiler compiler ) =>
	{
		// Build ComboDeclaration
		// Convert ComboName to valid HLSL identifier (replace spaces with underscores)
		string hlslName = ComboName.Name.Replace( ' ', '_' );

		var combo = new ComboDeclaration
		{
			Name = hlslName,
			DisplayName = ComboName.Name,
			UIGroup = UIGroup.Name,
			Type = ComboType,
			Style = ComboStyle,
			DefaultValue = DefaultValue - 1 // Convert from 1-based UI to 0-based HLSL
		};

		// Set Range and Labels based on ComboType
		// Note: Feature() uses 0-based ranges (0..1 for bool, 0..N for enum)
		// But our UI uses 1-based indexing, so we adjust when generating the ternary
		if ( ComboType == ComboType.Bool )
		{
			combo.Range = 1; // 0..1 range
			combo.Labels = new[] { State1Name, State2Name };
		}
		else // Enum
		{
			// Clamp StateCount to valid range
			int actualStateCount = System.Math.Max( 2, System.Math.Min( 8, StateCount ) );
			combo.Range = actualStateCount - 1; // 0..N range

			// Collect labels
			var stateNames = new[]
			{
				State1Name, State2Name, State3Name, State4Name,
				State5Name, State6Name, State7Name, State8Name
			};
			combo.Labels = stateNames.Take( actualStateCount ).ToArray();
		}

		// Validate combo
		if ( !combo.IsValid() )
		{
			return NodeResult.Error( $"Invalid combo configuration: {ComboName.Name}" );
		}

		// Register combo with compiler
		compiler.RegisterCombo( combo );

		// Get combo variable name
		string comboVar = combo.GetComboVarName();

		// Collect input results
		NodeResult[] results;
		int stateCount;

		if ( ComboType == ComboType.Bool )
		{
			var trueResult = compiler.Result( InputTrue );
			var falseResult = compiler.Result( InputFalse );
			

			if ( !falseResult.IsValid )
				return NodeResult.Error( "False input must be connected" );
			if ( !trueResult.IsValid )
				return NodeResult.Error( "True input must be connected" );

			results = new[] { trueResult, falseResult };
			stateCount = 2;
		}
		else // Enum
		{
			int actualStateCount = System.Math.Max( 2, System.Math.Min( 8, StateCount ) );
			var inputs = new[]
			{
				InputState1, InputState2, InputState3, InputState4,
				InputState5, InputState6, InputState7, InputState8
			};

			results = new NodeResult[actualStateCount];
			for ( int i = 0; i < actualStateCount; i++ )
			{
				var result = compiler.Result( inputs[i] );
				if ( !result.IsValid )
					return NodeResult.Error( $"State {i + 1} input must be connected" );
				results[i] = result;
			}
			stateCount = actualStateCount;
		}

		// Determine max component count
		int maxComponents = results.Max( r => r.Components );

		// Check if InputValue is connected
		var inputValueResult = compiler.Result( InputValue );

		// In preview mode, directly return the default input for immediate visual feedback
		if ( compiler.IsPreview )
		{
			int valueIndex = DefaultValue - 1; // Convert from 1-based UI to 0-based array index
			valueIndex = System.Math.Max( 0, System.Math.Min( valueIndex, stateCount - 1 ) ); // Clamp to valid range
			string cast = CastToComponents( results[valueIndex], maxComponents );
			return new NodeResult( maxComponents, cast );
		}

		// Generate ternary chain
		// If InputValue is connected, use it instead of combo variable
		// Format: (value == 0 ? val1 : value == 1 ? val2 : val3)
		string switchVar = inputValueResult.IsValid
			? $"(int)({inputValueResult.Code} - 1)" // Convert 1-based input to 0-based
			: comboVar;

		var sb = new StringBuilder();
		sb.Append( "(" );

		for ( int i = 0; i < stateCount; i++ )
		{
			string cast = CastToComponents( results[i], maxComponents );

			if ( i < stateCount - 1 )
			{
				sb.Append( $"{switchVar} == {i} ? {cast} : " );
			}
			else
			{
				// Final else value
				sb.Append( cast );
			}
		}

		sb.Append( ")" );

		return new NodeResult( maxComponents, sb.ToString() );
	};

	private string CastToComponents( NodeResult result, int targetComponents )
	{
		if ( result.Components == targetComponents )
			return result.Code;

		// Need to cast
		if ( targetComponents == 1 )
			return result.Code;
		else if ( targetComponents == 2 )
			return $"float2({result.Code})";
		else if ( targetComponents == 3 )
			return $"float3({result.Code})";
		else
			return $"float4({result.Code})";
	}
}
