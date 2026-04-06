using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Stekeblad.Optimizely.Analyzers.Analyzers.BadMethods;
using System.Threading.Tasks;
using VerifyCS = Stekeblad.Optimizely.Analyzers.Test.CSharpAnalyzerVerifier<
	Stekeblad.Optimizely.Analyzers.Analyzers.BadMethods.ContentAreaItemAnalyzer>;

namespace Stekeblad.Optimizely.Analyzers.Test.Tests.BadMethods
{
	[TestClass]
	public class ContentAreaItemTest : MyTestClassBase
	{
		[TestMethod]
		// Inline blocks feature was added in 12.15/12.21, don't warn on versions before that, see the analyzer code for details
		[DataRow(OptiVersion.v10, false)]
		[DataRow(OptiVersion.v11, false)]
		[DataRow(OptiVersion.v11_High, false)]
		[DataRow(OptiVersion.v12, false)]
		[DataRow(OptiVersion.v12_High, true)]
		[DataRow(OptiVersion.v13, true)]
		public async Task GuidAndLink_Match(OptiVersion version, bool meetsMinVersionCriteria)
		{
			const string test = @"
using EPiServer.Core;

namespace Tests
{
    public class C
    {
        public object DoContentAreaItemThing(ContentAreaItem caItem)
        {
            return new
            {
                Guid = caItem.{|#0:ContentGuid|},
                Link = caItem.{|#1:ContentLink|},
                Roles = caItem.AllowedRoles
            };
        }
    }
}";

			var expected0 = VerifyCS.Diagnostic(ContentAreaItemAnalyzer.ContentAreaItemDiagnosticId)
				.WithLocation(0);

			var expected1 = VerifyCS.Diagnostic(ContentAreaItemAnalyzer.ContentAreaItemDiagnosticId)
				.WithLocation(1);

			// The code should be free from errors if criteria is not met
			if (meetsMinVersionCriteria)
				await VerifyCS.VerifyAnalyzerAsync(test, version, expected0, expected1);
			else
				await VerifyCS.VerifyAnalyzerAsync(test, version);

		}

		[TestMethod]
		// Inline blocks feature was added in 12.15/12.21, see the analyzer code for details
		[DataRow(OptiVersion.v12_High)]
		[DataRow(OptiVersion.v13)]
		public async Task InlineAndLoad_Match(OptiVersion version)
		{
			const string test = @"
using EPiServer.Core;

namespace Tests
{
    public class C
    {
        public object DoContentAreaItemThing(ContentAreaItem caItem)
        {
            return new
            {
                Data = caItem.{|#0:InlineBlock|},
                Content = caItem.LoadContent(),
            };
        }
    }
}";

			var expected = VerifyCS.Diagnostic(ContentAreaItemAnalyzer.ContentAreaItemDiagnosticId)
				.WithLocation(0);

			await VerifyCS.VerifyAnalyzerAsync(test, version, expected);
		}

		[TestMethod]
		[DynamicData(nameof(AllOptimizelyTargets))]
		public async Task AssigningToTestedProperty_NoMatch(ReferenceAssemblies assemblies)
		{
			const string test = @"
using EPiServer.Core;

namespace Tests
{
    public class C
    {
        public ContentAreaItem CreateContentAreaItem(ContentReference contentLink)
        {
            return new ContentAreaItem
            {
                ContentLink = contentLink
            };
        }
    }
}";
			await VerifyCS.VerifyAnalyzerAsync(test, assemblies);
		}
	}
}
