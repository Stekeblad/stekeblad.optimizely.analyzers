using Microsoft.CodeAnalysis.Testing;
using System.Collections.Immutable;
using System.IO;
using System.Runtime.CompilerServices;

namespace Stekeblad.Optimizely.Analyzers.Test.Util
{
	public static class PackageCollections
	{
		public static readonly ReferenceAssemblies Core_10 = ReferenceAssemblies.NetFramework.Net472.Default
			.WithNuGetConfigFilePath(GetNugetConfigPath())
			.AddPackages(ImmutableArray.Create(
				new PackageIdentity("EPiServer.CMS.Core", "10.1.0"),
				new PackageIdentity("EPiServer.CMS.UI.Core", "10.1.0")));

		public static readonly ReferenceAssemblies Core_11 = ReferenceAssemblies.NetFramework.Net48.Default
			.WithNuGetConfigFilePath(GetNugetConfigPath())
			.AddPackages(ImmutableArray.Create(
				new PackageIdentity("EPiServer.CMS.Core", "11.1.0"),
				new PackageIdentity("EPiServer.CMS.UI.Core", "11.1.0")));

		public static readonly ReferenceAssemblies Core_12 = ReferenceAssemblies.Net.Net60
			.WithNuGetConfigFilePath(GetNugetConfigPath())
			.AddPackages(ImmutableArray.Create(
				new PackageIdentity("EPiServer.CMS.Core", "12.1.0"),
				new PackageIdentity("EPiServer.CMS.UI.Core", "12.1.0")));

		public static readonly ReferenceAssemblies Core_12_High = ReferenceAssemblies.Net.Net60
			.WithNuGetConfigFilePath(GetNugetConfigPath())
			.AddPackages(ImmutableArray.Create(
				new PackageIdentity("EPiServer.CMS.Core", "12.21.6"),
				new PackageIdentity("EPiServer.CMS.UI.Core", "12.30.0")));

		private static string GetNugetConfigPath()
		{
			return Path.Combine(Directory.GetParent(ThisFile())!.Parent!.FullName, "NuGet.config");
		}

		private static string ThisFile([CallerFilePath] string path = "") => path;
	}
}
