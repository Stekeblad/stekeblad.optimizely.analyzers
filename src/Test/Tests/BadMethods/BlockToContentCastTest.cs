using Microsoft.VisualStudio.TestTools.UnitTesting;
using Stekeblad.Optimizely.Analyzers.Analyzers.BadMethods;
using Stekeblad.Optimizely.Analyzers.Test.Util;
using System.Threading.Tasks;
using VerifyCS = Stekeblad.Optimizely.Analyzers.Test.CSharpAnalyzerVerifier<
	Stekeblad.Optimizely.Analyzers.Analyzers.BadMethods.BlockToContentCastAnalyzer>;

namespace Stekeblad.Optimizely.Analyzers.Test.Tests.BadMethods
{
	[TestClass]
	public class BlockToContentCastTest
	{
		[TestMethod]
		public async Task CastingBlockToContent_HardCast_Match()
		{
			const string test =
				@"using EPiServer.Core;
namespace test
{
	public class EditorialBlock : BlockData {}

    public class C
    {
        public int GetBlockContentId(BlockData block)
        {
            var content = {|#0:(IContent)block|};
            return content.ContentLink.ID;
        }

        public int GetEditorialId(EditorialBlock block)
        {
            var content = {|#1:(IContent)block|};
            return content.ContentLink.ID;
        }
    }
}";

			var expected0 = VerifyCS.Diagnostic(BlockToContentCastAnalyzer.BlockToContentCastDiagnosticId)
				.WithLocation(0);

			var expected1 = VerifyCS.Diagnostic(BlockToContentCastAnalyzer.BlockToContentCastDiagnosticId)
				.WithLocation(1);

			await VerifyCS.VerifyAnalyzerAsync(test, PackageCollections.Core_12_High, expected0, expected1);
		}

		[TestMethod]
		public async Task CastingBlockToContent_AsCast_NoMatch()
		{
			const string test =
				@"using EPiServer.Core;
namespace test
{
    public class C
    {
        public int GetBlockContentId(BlockData block)
        {
            var content = block as IContent;
            return content?.ContentLink.ID ?? -1;
        }
    }
}";

			await VerifyCS.VerifyAnalyzerAsync(test, PackageCollections.Core_12_High);
		}

		[TestMethod]
		public async Task CastingBlockToContent_IsCast_NoMatch()
		{
			const string test =
				@"using EPiServer.Core;
namespace test
{
    public class C
    {
        public int GetBlockContentId(BlockData block)
        {
            if (block is IContent content)
                return content.ContentLink.ID;
            return -1;
        }
    }
}";

			await VerifyCS.VerifyAnalyzerAsync(test, PackageCollections.Core_12_High);
		}
	}
}
