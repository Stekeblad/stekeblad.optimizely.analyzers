using Microsoft.CodeAnalysis.Testing;
using System;
using System.Collections.Generic;
using static Stekeblad.Optimizely.Analyzers.Test.Util.PackageCollections;
using TestRow = Microsoft.VisualStudio.TestTools.UnitTesting.TestDataRow<Microsoft.CodeAnalysis.Testing.ReferenceAssemblies>;

namespace Stekeblad.Optimizely.Analyzers.Test.Tests
{
	/// <summary>
	/// Base for all classes with tests giving easy access to both individual
	/// <c>ReferenceAssemblies</c> and a list with all the common once.
	/// </summary>
	public class MyTestClassBase
	{
		public enum OptiVersion { v10, v11, v11_High, v12, v12_High, v13 }

		public static readonly TestRow Epi10 = new(Core_10) { DisplayName = nameof(Epi10) };
		public static readonly TestRow Epi11 = new(Core_11) { DisplayName = nameof(Epi11) };
		public static readonly TestRow Epi11_High = new(Core_11_High) { DisplayName = nameof(Epi11_High) };
		public static readonly TestRow Opti12 = new(Core_12) { DisplayName = nameof(Opti12) };
		public static readonly TestRow Opti12_High = new(Core_12_High) { DisplayName = nameof(Opti12_High) };
		public static readonly TestRow Opti13 = new(Core_13) { DisplayName = nameof(Opti13) };

		public static readonly IEnumerable<TestRow> AllOptimizelyTargets =
			[Epi10, Epi11, Epi11_High, Opti12, Opti12_High, Opti13];

		public static ReferenceAssemblies RefAssembliesForVersion(OptiVersion target)
		{
			return target switch
			{
				OptiVersion.v10 => Core_10,
				OptiVersion.v11 => Core_11,
				OptiVersion.v11_High => Core_11_High,
				OptiVersion.v12 => Core_12,
				OptiVersion.v12_High => Core_12_High,
				OptiVersion.v13 => Core_13,
				_ => throw new ArgumentOutOfRangeException(nameof(target), target, "OptiVersion has not been mapped to any reference assemblies")
			};
		}
	}
}
