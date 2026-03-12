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

		public static readonly ReferenceAssemblies Core_11_High = ReferenceAssemblies.NetFramework.Net48.Default
			.WithNuGetConfigFilePath(GetNugetConfigPath())
			.AddPackages(ImmutableArray.Create(
				new PackageIdentity("EPiServer.CMS.Core", "11.21.5"),
				new PackageIdentity("EPiServer.CMS.UI.Core", "11.37.5")));

		public static readonly ReferenceAssemblies Core_12 = ReferenceAssemblies.Net.Net60
			.WithNuGetConfigFilePath(GetNugetConfigPath())
			.AddPackages(ImmutableArray.Create(
				new PackageIdentity("EPiServer.CMS.Core", "12.1.0"),
				new PackageIdentity("EPiServer.CMS.UI.Core", "12.1.0")));

		public static readonly ReferenceAssemblies Core_12_High = ReferenceAssemblies.Net.Net80
			.WithNuGetConfigFilePath(GetNugetConfigPath())
			.AddPackages(ImmutableArray.Create(
				new PackageIdentity("EPiServer.CMS.Core", "12.23.1"),
				new PackageIdentity("EPiServer.CMS.UI.Core", "12.34.2")));

		public static readonly ReferenceAssemblies Core_13 = ReferenceAssemblies.Net.Net100
			.WithNuGetConfigFilePath(GetNugetConfigPath())
			.AddPackages(ImmutableArray.Create(
				new PackageIdentity("EPiServer.CMS.Core", "13.0.0-preview3"),
				new PackageIdentity("EPiServer.CMS.UI.Core", "13.0.0-preview3")));

		private static string GetNugetConfigPath()
		{
			return Path.Combine(Directory.GetParent(ThisFile())!.Parent!.FullName, "NuGet.config");
		}

		private static string ThisFile([CallerFilePath] string path = "") => path;
	}
}
