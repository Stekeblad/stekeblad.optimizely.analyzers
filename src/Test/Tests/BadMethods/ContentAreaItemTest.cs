using Microsoft.VisualStudio.TestTools.UnitTesting;
using Stekeblad.Optimizely.Analyzers.Analyzers.BadMethods;
using Stekeblad.Optimizely.Analyzers.Test.Util;
using System.Threading.Tasks;
using VerifyCS = Stekeblad.Optimizely.Analyzers.Test.CSharpAnalyzerVerifier<
	Stekeblad.Optimizely.Analyzers.Analyzers.BadMethods.ContentAreaItemAnalyzer>;

namespace Stekeblad.Optimizely.Analyzers.Test.Tests.BadMethods
{
	[TestClass]
	public class ContentAreaItemTest
	{
		[TestMethod]
		public async Task Test_Match()
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
                Data = caItem.{|#2:InlineBlock|},
                Content = caItem.LoadContent(),
                Roles = caItem.AllowedRoles
            };
        }
    }
}";

			var expected0 = VerifyCS.Diagnostic(ContentAreaItemAnalyzer.ContentAreaItemDiagnosticId)
				.WithLocation(0);

			var expected1 = VerifyCS.Diagnostic(ContentAreaItemAnalyzer.ContentAreaItemDiagnosticId)
				.WithLocation(1);

			var expected2 = VerifyCS.Diagnostic(ContentAreaItemAnalyzer.ContentAreaItemDiagnosticId)
				.WithLocation(2);

			await VerifyCS.VerifyAnalyzerAsync(test, PackageCollections.Core_12_High, expected0, expected1, expected2);
		}

		[TestMethod]
		public async Task AssigningToTestedProperty_NoMatch()
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
		    await VerifyCS.VerifyAnalyzerAsync(test, PackageCollections.Core_12_High);
		}
	}
}
