namespace ShaderGraphExtras;

public static class ShaderGraphUpgrader
{
	private const string BaseGraphCompiler = "addons/tools/Code/ShaderGraph/Compiler/GraphCompiler.cs";
	private const string UpgradedGraphCompiler = "Editor/upgrader/files/GraphCompiler.cs.txt";

	private const string BaseNodeResult = "addons/tools/Code/ShaderGraph/Compiler/NodeResult.cs";
	private const string UpgradedNodeResult = "Editor/upgrader/files/NodeResult.cs.txt";

	private const string BaseShaderTemplate = "addons/tools/Code/ShaderGraph/Compiler/ShaderTemplate.cs";
	private const string UpgradedShaderTemplate = "Editor/upgrader/files/ShaderTemplate.cs.txt";

	private const string BaseResult = "addons/tools/Code/ShaderGraph/Result.cs";
	private const string UpgradedResult = "Editor/upgrader/files/Result.cs.txt";

	private const string BaseShaderGraph = "addons/tools/Code/ShaderGraph/ShaderGraph.cs";
	private const string UpgradedShaderGraph = "Editor/upgrader/files/ShaderGraph.cs.txt";

	private const string BaseSubgraphInput = "addons/tools/Code/ShaderGraph/SubgraphInput.cs";
	private const string UpgradedSubgraphInput = "Editor/upgrader/files/SubgraphInput.cs.txt";

	private const string BaseSubgraphNode = "addons/tools/Code/ShaderGraph/SubgraphNode.cs";
	private const string UpgradedSubgraphNode = "Editor/upgrader/files/SubgraphNode.cs.txt";

	private const string BaseTexture = "addons/tools/Code/ShaderGraph/Texture.cs";
	private const string UpgradedTexture = "Editor/upgrader/files/Texture.cs.txt";

	private const string BaseEnumerationEditor = "addons/tools/Code/ShaderGraph/EnumerationEditor.cs";
	private const string UpgradedEnumerationEditor = "Editor/upgrader/files/EnumerationEditor.cs.txt";

	private const string BaseUnlit = "addons/tools/Code/ShaderGraph/Compiler/ShadingModels/Unlit.cs";
	private const string UpgradedUnlit = "Editor/upgrader/files/Unlit.cs.txt";

	private const string BaseLit = "addons/tools/Code/ShaderGraph/Compiler/ShadingModels/Lit.cs";
	private const string UpgradedLit = "Editor/upgrader/files/Lit.cs.txt";

	private const string BaseSurface = "addons/tools/Code/ShaderGraph/Compiler/Templates/Surface.cs";
	private const string UpgradedSurface = "Editor/upgrader/files/Surface.cs.txt";
	
	private const string BasePostProcess = "addons/tools/Code/ShaderGraph/Compiler/Templates/PostProcess.cs";
	private const string UpgradedPostProcess = "Editor/upgrader/files/PostProcess.cs.txt";

	private static readonly (string BaseFile, string UpgradeFile)[] ShaderGraphFiles =
	[
		(BaseGraphCompiler, UpgradedGraphCompiler),
		(BaseNodeResult, UpgradedNodeResult),
		(BaseShaderTemplate, UpgradedShaderTemplate),
		(BaseResult, UpgradedResult),
		(BaseShaderGraph, UpgradedShaderGraph),
		(BaseSubgraphInput, UpgradedSubgraphInput),
		(BaseSubgraphNode, UpgradedSubgraphNode),
		(BaseTexture, UpgradedTexture),
		(BaseEnumerationEditor, UpgradedEnumerationEditor),
		(BaseLit, UpgradedLit),
		(BaseUnlit, UpgradedUnlit),
		(BaseSurface, UpgradedSurface),
		(BasePostProcess, UpgradedPostProcess)
	];

	private static readonly string[] TextureObjectNodeFiles =
	[
		"Blur.cs",
		"ChromaticAberration.cs",
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

		try
		{
			// Create directory if it doesn't exist
			string baseDirectory = Path.GetDirectoryName( baseFilePath );
			if ( !Directory.Exists( baseDirectory ) )
			{
				Directory.CreateDirectory( baseDirectory );
				Log.Info( $"✓ Created directory: {baseDirectory}" );
			}

			File.WriteAllText( baseFilePath, File.ReadAllText( upgradedFilePath ) );

			if ( File.Exists( baseFilePath ) )
			{
				Log.Info( $"✓ Successfully upgraded: {Path.GetFileName( baseFilePath )}" );
			}
			else
			{
				Log.Info( $"✓ Successfully created: {Path.GetFileName( baseFilePath )}" );
			}

			return true;
		}
		catch ( Exception ex )
		{
			Log.Error( $"✗ Failed to upgrade {Path.GetFileName( baseFilePath )}: {ex.Message}" );
			return false;
		}
	}
}
