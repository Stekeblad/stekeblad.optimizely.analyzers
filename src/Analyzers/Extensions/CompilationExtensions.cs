using Microsoft.CodeAnalysis;
using System;
using System.Linq;

namespace Stekeblad.Optimizely.Analyzers.Extensions
{
	public static class CompilationExtensions
	{
		public static bool HasReferenceAssembly(this Compilation compilation,
			string assemblyName, Version minVersion = null, Version maxVersion = null)
		{
			if (compilation == null)
				throw new ArgumentNullException(nameof(compilation));

			if (assemblyName == null)
				throw new ArgumentNullException(nameof(assemblyName));

			AssemblyIdentity assembly = compilation.ReferencedAssemblyNames
				.FirstOrDefault(assem => assem.Name.Equals(assemblyName));

			if (assembly == null)
				return false;

			if (minVersion != null && assembly.Version < minVersion)
				return false;

			if (maxVersion != null && assembly.Version > maxVersion)
				return false;

			return true;
		}
	}
}
