using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Stekeblad.Optimizely.Analyzers.Analyzers.Content;
using System.Collections.Generic;
using System.Threading.Tasks;
using VerifyCS = Stekeblad.Optimizely.Analyzers.Test.CSharpCodeFixVerifier<
	Stekeblad.Optimizely.Analyzers.Analyzers.Content.PropertyDefinitionTypePlugInAnalyzer,
	Stekeblad.Optimizely.Analyzers.CodeFixes.Content.PropertyDefinitionTypePlugInCodeFixProvider>;

namespace Stekeblad.Optimizely.Analyzers.Test.Tests.Content
{
	[TestClass]
	public class PropertyDefinitionTypePlugInTest : MyTestClassBase
	{
		public static readonly IEnumerable<TestDataRow<ReferenceAssemblies>> VersionsSupportingPropDefGuid =
			[Epi11_High, Opti12, Opti12_High, Opti13];

		[TestMethod]
		[DynamicData(nameof(VersionsSupportingPropDefGuid))]
		public async Task PropDefType_MinimalValid_NoMatch(ReferenceAssemblies assemblies)
		{
			const string test = @"
using EPiServer.Core;
using EPiServer.PlugIn;

namespace Tests
{
    [PropertyDefinitionTypePlugIn(GUID = ""87ca4b10-dbc5-4e0a-ae23-c79a113ef00a"")]
    public class C : PropertyLongString
    {
    }
}";

			await VerifyCS.VerifyAnalyzerAsync(test, assemblies);
		}

		[TestMethod]
		[DynamicData(nameof(VersionsSupportingPropDefGuid))]
		public async Task PropDefType_AttributeOnBadType_Match(ReferenceAssemblies assemblies)
		{
			const string test = @"
using EPiServer.Core;
using EPiServer.PlugIn;

namespace Tests
{
    [PropertyDefinitionTypePlugIn(GUID = ""87ca4b10-dbc5-4e0a-ae23-c79a113ef00a"")]
    public class {|#0:C|}
    {
    }
}";

			var expected0 = VerifyCS.Diagnostic(PropertyDefinitionTypePlugInAnalyzer.BadBaseClassDiagnosticId)
				.WithLocation(0)
				.WithArguments("C");

			await VerifyCS.VerifyAnalyzerAsync(test, assemblies, expected0);
		}

		[TestMethod]
		[DynamicData(nameof(AllOptimizelyTargets))]
		public async Task PropDefType_Abstract_MissingAttribute_NoMatch(ReferenceAssemblies assemblies)
		{
			const string test = @"
using EPiServer.Core;

namespace Tests
{
    public abstract class C : PropertyLongString
    {
    }
}";

			await VerifyCS.VerifyAnalyzerAsync(test, assemblies);
		}

		[TestMethod]
		[DynamicData(nameof(AllOptimizelyTargets))]
		public async Task PropDefType_Abstract_WithAttribute_Match(ReferenceAssemblies assemblies)
		{
			const string test = @"
using EPiServer.Core;
using EPiServer.PlugIn;

namespace Tests
{
    [PropertyDefinitionTypePlugIn]
    public {|#0:abstract|} class C : PropertyLongString
    {
    }
}";

			var expected0 = VerifyCS.Diagnostic(PropertyDefinitionTypePlugInAnalyzer.AttributeOnAbstractDiagnosticsId)
				.WithLocation(0)
				.WithArguments("C");

			await VerifyCS.VerifyAnalyzerAsync(test, assemblies, expected0);
		}

		[TestMethod]
		[DynamicData(nameof(AllOptimizelyTargets))]
		public async Task PropDefType_MissingAttribute_Match(ReferenceAssemblies assemblies)
		{
			const string test = @"
using EPiServer.Core;

namespace Tests
{
    public class {|#0:C|} : PropertyLongString
    {
    }
}";

			var expected0 = VerifyCS.Diagnostic(PropertyDefinitionTypePlugInAnalyzer.MissingAttributeDiagnosticId)
				.WithLocation(0)
				.WithArguments("C");

			await VerifyCS.VerifyAnalyzerAsync(test, assemblies, expected0);
		}

		[TestMethod]
		[DataRow(OptiVersion.v10, false)]
		[DataRow(OptiVersion.v11, false)]
		[DataRow(OptiVersion.v11_High, true)]
		[DataRow(OptiVersion.v12, true)]
		[DataRow(OptiVersion.v12_High, true)]
		[DataRow(OptiVersion.v13, true)]
		public async Task PropDefType_MissingGuid_Match(OptiVersion version, bool meetsMinVersionCriteria)
		{
			const string test = @"
using EPiServer.Core;
using EPiServer.PlugIn;

namespace Tests
{
    [{|#0:PropertyDefinitionTypePlugIn|}]
    public class C : PropertyLongString
    {
    }
}";

			var expected0 = VerifyCS.Diagnostic(PropertyDefinitionTypePlugInAnalyzer.NoGuidDiagnosticId)
				.WithLocation(0)
				.WithArguments("C");

			// Guid support was added during the version 11 cycle
			if (meetsMinVersionCriteria)
				await VerifyCS.VerifyAnalyzerAsync(test, RefAssembliesForVersion(version), expected0);
			else
				await VerifyCS.VerifyAnalyzerAsync(test, RefAssembliesForVersion(version));
		}

		[TestMethod]
		[DynamicData(nameof(VersionsSupportingPropDefGuid))]
		public async Task PropDefType_EmptyGuid_Match(ReferenceAssemblies assemblies)
		{
			const string test = @"
using EPiServer.Core;
using EPiServer.PlugIn;

namespace Tests
{
    [{|#0:PropertyDefinitionTypePlugIn(GUID = """")|}]
    public class C : PropertyLongString
    {
    }
}";

			var expected0 = VerifyCS.Diagnostic(PropertyDefinitionTypePlugInAnalyzer.InvalidGuidDiagnosticId)
				.WithLocation(0)
				.WithArguments("C");

			await VerifyCS.VerifyAnalyzerAsync(test, assemblies, expected0);
		}

		[TestMethod]
		[DynamicData(nameof(VersionsSupportingPropDefGuid))]
		public async Task PropDefType_InvalidGuid_Match(ReferenceAssemblies assemblies)
		{
			const string test = @"
using EPiServer.Core;
using EPiServer.PlugIn;

namespace Tests
{
    [{|#0:PropertyDefinitionTypePlugIn(GUID = ""guidguid-guid-guid-guid-guidguidguid"")|}]
    public class C : PropertyLongString
    {
    }
}";

			var expected0 = VerifyCS.Diagnostic(PropertyDefinitionTypePlugInAnalyzer.InvalidGuidDiagnosticId)
				.WithLocation(0)
				.WithArguments("C");

			// Core_11 uses a version of Optimizely from before the GUID parameter was added
			//await VerifyCS.VerifyAnalyzerAsync(test, PackageCollections.Core_11, expected0);
			await VerifyCS.VerifyAnalyzerAsync(test, assemblies, expected0);
		}

		[TestMethod]
		[DynamicData(nameof(VersionsSupportingPropDefGuid))]
		public async Task PropDefType_ReusedGuid_Match(ReferenceAssemblies assemblies)
		{
			const string test = @"
using EPiServer.Core;
using EPiServer.PlugIn;

namespace Tests
{
    [{|#0:PropertyDefinitionTypePlugIn(GUID = ""fca6e3bd-15e4-4cf4-ae62-07ee7c62d2c1"")|}]
    public class C1 : PropertyLongString
    {
    }

[{|#1:PropertyDefinitionTypePlugIn(GUID = ""fca6e3bd-15e4-4cf4-ae62-07ee7c62d2c1"")|}]
    public class C2 : PropertyLongString
    {
    }
}";

			var expected0 = VerifyCS.Diagnostic(PropertyDefinitionTypePlugInAnalyzer.GuidReusedDiagnosticId)
				.WithLocation(0)
				.WithArguments("C1", "C1, C2");

			var expected1 = VerifyCS.Diagnostic(PropertyDefinitionTypePlugInAnalyzer.GuidReusedDiagnosticId)
				.WithLocation(1)
				.WithArguments("C2", "C1, C2");

			await VerifyCS.VerifyAnalyzerAsync(test, assemblies, expected0, expected1);
		}

		[TestMethod]
		[DynamicData(nameof(AllOptimizelyTargets))]
		public async Task PropDefType_AddAttribute_CodeFix(ReferenceAssemblies assemblies)
		{
			// This test disables SOA1032, it's a follow-up error after the fix has been applied.
			// Code fix test does not like follow-up errors and this error only applies for certain Optimizely versions
			const string test = @"
using EPiServer.Core;

#pragma warning disable SOA1032
namespace Tests
{
    public class {|#0:C|} : PropertyLongString
    {
    }
#pragma warning restore SOA1032
}";

			const string fix = @"
using EPiServer.Core;
using EPiServer.PlugIn;

#pragma warning disable SOA1032
namespace Tests
{
    [PropertyDefinitionTypePlugIn]
    public class C : PropertyLongString
    {
    }
#pragma warning restore SOA1032
}";

			var expected = VerifyCS.Diagnostic(PropertyDefinitionTypePlugInAnalyzer.MissingAttributeDiagnosticId)
				.WithLocation(0)
				.WithArguments("C");

			await VerifyCS.VerifyCodeFixAsync(test, assemblies, expected, fix);
		}
	}
}
