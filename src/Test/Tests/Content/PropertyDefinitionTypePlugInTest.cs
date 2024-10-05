using Microsoft.VisualStudio.TestTools.UnitTesting;
using Stekeblad.Optimizely.Analyzers.Analyzers.Content;
using Stekeblad.Optimizely.Analyzers.Test.Util;
using System.Threading.Tasks;
//using VerifyCS = Stekeblad.Optimizely.Analyzers.Test.CSharpAnalyzerVerifier<
//	Stekeblad.Optimizely.Analyzers.Analyzers.Content.PropertyDefinitionTypePlugInAnalyzer>;

using VerifyCS = Stekeblad.Optimizely.Analyzers.Test.CSharpCodeFixVerifier<
	Stekeblad.Optimizely.Analyzers.Analyzers.Content.PropertyDefinitionTypePlugInAnalyzer,
	 Stekeblad.Optimizely.Analyzers.CodeFixes.Content.PropertyDefinitionTypePlugInCodeFixProvider>;

namespace Stekeblad.Optimizely.Analyzers.Test.Tests.Content
{
	[TestClass]
	public class PropertyDefinitionTypePlugInTest
	{
		[TestMethod]
		public async Task PropDefType_MinimalValid_NoMatch()
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

			// Core_11 uses a version of Optimizely from before the GUID parameter was added
			//await VerifyCS.VerifyAnalyzerAsync(test, PackageCollections.Core_11);
			await VerifyCS.VerifyAnalyzerAsync(test, PackageCollections.Core_12);
		}

		[TestMethod]
		public async Task PropDefType_AttributeOnBadType_Match()
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

			// Core_11 uses a version of Optimizely from before the GUID parameter was added
			//await VerifyCS.VerifyAnalyzerAsync(test, PackageCollections.Core_11, expected0);
			await VerifyCS.VerifyAnalyzerAsync(test, PackageCollections.Core_12, expected0);
		}

		[TestMethod]
		public async Task PropDefType_Abstract_MissingAttribute_NoMatch()
		{
			const string test = @"
using EPiServer.Core;

namespace Tests
{
    public abstract class C : PropertyLongString
    {
    }
}";

			await VerifyCS.VerifyAnalyzerAsync(test, PackageCollections.Core_11);
			await VerifyCS.VerifyAnalyzerAsync(test, PackageCollections.Core_12);
		}

		[TestMethod]
		public async Task PropDefType_MissingAttribute_Match()
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

			await VerifyCS.VerifyAnalyzerAsync(test, PackageCollections.Core_11, expected0);
			await VerifyCS.VerifyAnalyzerAsync(test, PackageCollections.Core_12, expected0);
		}

		[TestMethod]
		public async Task PropDefType_MissingGuid_Match()
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

			// Core_11 uses a version of Optimizely from before the GUID parameter was added
			await VerifyCS.VerifyAnalyzerAsync(test, PackageCollections.Core_11);
			await VerifyCS.VerifyAnalyzerAsync(test, PackageCollections.Core_12, expected0);
		}

		[TestMethod]
		public async Task PropDefType_EmptyGuid_Match()
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

			// Core_11 uses a version of Optimizely from before the GUID parameter was added
			//await VerifyCS.VerifyAnalyzerAsync(test, PackageCollections.Core_11, expected0);
			await VerifyCS.VerifyAnalyzerAsync(test, PackageCollections.Core_12, expected0);
		}

		[TestMethod]
		public async Task PropDefType_InvalidGuid_Match()
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
			await VerifyCS.VerifyAnalyzerAsync(test, PackageCollections.Core_12, expected0);
		}

		[TestMethod]
		public async Task PropDefType_ReusedGuid_Match()
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

			// Core_11 uses a version of Optimizely from before the GUID parameter was added
			//await VerifyCS.VerifyAnalyzerAsync(test, PackageCollections.Core_11, expected0, expected1);
			await VerifyCS.VerifyAnalyzerAsync(test, PackageCollections.Core_12, expected0, expected1);
		}

		[TestMethod]
		public async Task PropDefType_AddAttribute_CodeFix()
		{
			const string test = @"
using EPiServer.Core;

namespace Tests
{
    public class {|#0:C|} : PropertyLongString
    {
    }
}";

			const string fix = @"
using EPiServer.Core;
using EPiServer.PlugIn;

namespace Tests
{
    [PropertyDefinitionTypePlugIn]
    public class C : PropertyLongString
    {
    }
}";

			var expected = VerifyCS.Diagnostic(PropertyDefinitionTypePlugInAnalyzer.MissingAttributeDiagnosticId)
				.WithLocation(0)
				.WithArguments("C");

			await VerifyCS.VerifyCodeFixAsync(test, PackageCollections.Core_11, expected, fix);
			// Do not run for CMS 12, test will fail with follow-up diagnostic for attribute has no GUID
			//await VerifyCS.VerifyCodeFixAsync(test, PackageCollections.Core_12, expected, fix);
		}
	}
}
