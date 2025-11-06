namespace ShaderGraphExtras;

public static class ShaderGraphUpgrader
{
	private const string BaseGraphCompiler = "addons/tools/Code/ShaderGraph/Compiler/GraphCompiler.cs";
	private const string UpgradedGraphCompiler = "Editor/upgrader/files/GraphCompiler.cs.txt";

	private const string BaseNodeResult = "addons/tools/Code/ShaderGraph/Compiler/NodeResult.cs";
	private const string UpgradedNodeResult = "Editor/upgrader/files/NodeResult.cs.txt";

	private const string BaseSubgraphInput = "addons/tools/Code/ShaderGraph/SubgraphInput.cs";
	private const string UpgradedSubgraphInput = "Editor/upgrader/files/SubgraphInput.cs.txt";

	private const string BaseSubgraphNode = "addons/tools/Code/ShaderGraph/SubgraphNode.cs";
	private const string UpgradedSubgraphNode = "Editor/upgrader/files/SubgraphNode.cs.txt";

	private const string BaseTexture = "addons/tools/Code/ShaderGraph/Texture.cs";
	private const string UpgradedTexture = "Editor/upgrader/files/Texture.cs.txt";

	private static readonly (string BaseFile, string UpgradeFile)[] ShaderGraphFiles =
	[
		(BaseGraphCompiler, UpgradedGraphCompiler),
		(BaseNodeResult, UpgradedNodeResult),
		(BaseSubgraphInput, UpgradedSubgraphInput),
		(BaseSubgraphNode, UpgradedSubgraphNode),
		(BaseTexture, UpgradedTexture)
	];

	private static readonly string[] TextureObjectNodeFiles =
	[
		"Flipbook.cs",
		"ParallaxOcclusionMapping.cs",
		"TextureSample.cs",
		"TextureObject.cs"
	];

	/// <summary>
	/// Checks if the shader graph files have been upgraded with TextureObject support
	/// </summary>
	public static bool IsUpgraded()
	{
		string baseTexturePath = Path.Combine( Environment.CurrentDirectory, BaseTexture );

		if ( File.Exists( baseTexturePath ) )
		{
			string content = File.ReadAllText( baseTexturePath );
			return content.Contains( "public struct TextureObject" );
		}

		return false;
	}

	/// <summary>
	/// Copies .cs.txt upgraded files to .cs files to enable TextureObject nodes
	/// </summary>
	private static bool EnableTextureObjectNodes( [CallerFilePath] string callerPath = "" )
	{
		string nodesDirectory = Path.Combine( GetAddonRootDirectory( callerPath ), "Editor", "nodes" );
		int successCount = 0;

		foreach ( string fileName in TextureObjectNodeFiles )
		{
			string upgradedFilePath = Path.Combine( nodesDirectory, $"{fileName}.txt" );
			string baseFilePath = Path.Combine( nodesDirectory, fileName );

			try
			{
				if ( !File.Exists( upgradedFilePath ) )
				{
					Log.Warning( $"⚠ Upgraded file not found: {fileName}.txt" );
					continue;
				}

				File.Copy( upgradedFilePath, baseFilePath, overwrite: true );
				Log.Info( $"✓ Enabled TextureObject support in: {fileName}" );
				successCount++;
			}
			catch ( Exception ex )
			{
				Log.Error( $"✗ Failed to enable {fileName}: {ex.Message}" );
			}
		}

		bool allSucceeded = successCount == TextureObjectNodeFiles.Length;
		if ( allSucceeded )
		{
			Log.Info( "✓ All TextureObject nodes are now enabled" );
		}
		else
		{
			Log.Warning( $"⚠ Only {successCount}/{TextureObjectNodeFiles.Length} TextureObject nodes were enabled" );
		}

		return allSucceeded;
	}

	/// <summary>
	/// Gets the library root directory from the caller's file path
	/// </summary>
	private static string GetAddonRootDirectory( string callerPath )
	{
		string upgraderDirectory = Path.GetDirectoryName( callerPath );
		string editorDirectory = Path.GetDirectoryName( upgraderDirectory );
		return Path.GetDirectoryName( editorDirectory );
	}

	[Menu( "Editor", "Shader Graph Extras/Upgrade Files" )]
	public static void UpgradeFiles()
	{
		if ( IsUpgraded() )
		{
			Log.Warning( "Shader Graph files appear to already be upgraded. Re-running upgrade..." );
		}
		else
		{
			Log.Info( "Starting Shader Graph Extras upgrade process..." );
		}

		bool upgradeSucceeded = true;
		foreach ( var (baseFile, upgradedFile) in ShaderGraphFiles )
		{
			if ( !UpgradeShaderGraphFile( baseFile, upgradedFile ) )
			{
				upgradeSucceeded = false;
			}
		}

		if ( upgradeSucceeded )
		{
			if ( !EnableTextureObjectNodes() )
			{
				Log.Warning( "⚠ Shader Graph files were upgraded, but some TextureObject nodes failed to enable" );
			}
		}
		else
		{
			Log.Error( "✗ Upgrade process completed with errors. Some files may not have been upgraded successfully." );
		}
	}

	[Menu( "Editor", "Shader Graph Extras/Check Upgrade Status" )]
	public static void CheckUpgradeStatus()
	{
		if ( IsUpgraded() )
		{
			Log.Info( "✓ Shader Graph files are upgraded. All TextureObject nodes should work correctly." );
		}
		else
		{
			Log.Warning( "⚠ Shader Graph files have NOT been upgraded yet." );
			Log.Warning( "  TextureObject-based nodes will not function correctly until you run:" );
			Log.Warning( "  Editor → Shader Graph Extras → Upgrade Files" );
		}
	}

	/// <summary>
	/// Upgrades a single shader graph file by copying the upgraded file to the installation directory
	/// </summary>
	private static bool UpgradeShaderGraphFile( string baseFile, string upgradedFile, [CallerFilePath] string callerPath = "" )
	{
		string libraryRoot = GetAddonRootDirectory( callerPath );
		string upgradedFilePath = Path.Combine( libraryRoot, upgradedFile );
		string baseFilePath = Path.Combine( Environment.CurrentDirectory, baseFile );

		if ( !File.Exists( upgradedFilePath ) )
		{
			Log.Error( $"✗ Upgraded file not found: {upgradedFilePath}" );
			return false;
		}

		if ( !File.Exists( baseFilePath ) )
		{
			Log.Error( $"✗ Base file not found: {baseFilePath}" );
			Log.Error( $"  Make sure s&box is properly installed and this library is in the correct location." );
			return false;
		}

		try
		{
			File.WriteAllText( baseFilePath, File.ReadAllText( upgradedFilePath ) );
			Log.Info( $"✓ Successfully upgraded: {Path.GetFileName( baseFilePath )}" );
			return true;
		}
		catch ( Exception ex )
		{
			Log.Error( $"✗ Failed to upgrade {Path.GetFileName( baseFilePath )}: {ex.Message}" );
			return false;
		}
	}
}
