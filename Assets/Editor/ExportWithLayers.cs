using UnityEngine;
using UnityEditor;
using System.Collections;

namespace Spewnity
{
	public static class ExportPackagePlus
	{
		[MenuItem("Spewnity/Export Package Plus")]
		public static void ExportPackage()
		{
			string[] projectContent = new string[] 
			{
				"Assets",  // exports your prefabs
				"ProjectSettings/TagManager.asset", // exports tags, layers, and sorting layers
				"ProjectSettings/Physics2DSettings.asset" // exports 2D layer collision matrix
			};
			string outName = PlayerSettings.productName + ".unitypackage";
			AssetDatabase.ExportPackage(projectContent, outName,
            ExportPackageOptions.Interactive | ExportPackageOptions.Recurse | ExportPackageOptions.IncludeDependencies);
			Debug.Log("Project Exported to " + outName);
		}
	
	}
}