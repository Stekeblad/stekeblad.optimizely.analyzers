using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Stekeblad.Optimizely.Analyzers.Analyzers.BadMethods;
using System.Threading.Tasks;
using VerifyCS = Stekeblad.Optimizely.Analyzers.Test.CSharpAnalyzerVerifier<
	Stekeblad.Optimizely.Analyzers.Analyzers.BadMethods.BlockToContentCastAnalyzer>;

namespace Stekeblad.Optimizely.Analyzers.Test.Tests.BadMethods
{
	[TestClass]
	public class BlockToContentCastTest : MyTestClassBase
	{
		[TestMethod]
		[DynamicData(nameof(AllOptimizelyTargets))]
		public async Task CastingBlockToContent_HardCast_Match(ReferenceAssemblies assemblies)
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

			await VerifyCS.VerifyAnalyzerAsync(test, assemblies, expected0, expected1);
		}

		[TestMethod]
		[DynamicData(nameof(AllOptimizelyTargets))]
		public async Task CastingBlockToContent_AsCast_NoMatch(ReferenceAssemblies assemblies)
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

			await VerifyCS.VerifyAnalyzerAsync(test, assemblies);
		}

		[TestMethod]
		[DynamicData(nameof(AllOptimizelyTargets))]
		public async Task CastingBlockToContent_IsCast_NoMatch(ReferenceAssemblies assemblies)
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

			await VerifyCS.VerifyAnalyzerAsync(test, assemblies);
		}
	}
}
