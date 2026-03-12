using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Stekeblad.Optimizely.Analyzers.Analyzers.Content;
using System.Threading.Tasks;
using VerifyCS = Stekeblad.Optimizely.Analyzers.Test.CSharpCodeFixVerifier<
	Stekeblad.Optimizely.Analyzers.Analyzers.Content.ContentPropertyMustBeVirtualAnalyzer,
	Stekeblad.Optimizely.Analyzers.CodeFixes.Content.ContentPropertyMustBeVirtualCodeFixProvider>;

namespace Stekeblad.Optimizely.Analyzers.Test.Tests.Content
{
	[TestClass]
	public class ContentPropertyMustBeVirtualTest : MyTestClassBase
	{
		[TestMethod]
		[DynamicData(nameof(AllOptimizelyTargets))]
		public async Task PageTypeWithNoProperties_NoMatch(ReferenceAssemblies assemblies)
		{
			const string test = @"
				namespace tests
				{
					public class ArticlePage : EPiServer.Core.PageData {}
				}";

			await VerifyCS.VerifyAnalyzerAsync(test, assemblies);
		}

		[TestMethod]
		[DynamicData(nameof(AllOptimizelyTargets))]
		public async Task PageTypeWithCorrectProperty_NoMatch(ReferenceAssemblies assemblies)
		{
			const string test = @"
				namespace tests
				{
					public class ArticlePage : EPiServer.Core.PageData
					{
						public virtual string Heading { get; set; }
					}
				}";

			await VerifyCS.VerifyAnalyzerAsync(test, assemblies);
		}

		[TestMethod]
		[DynamicData(nameof(AllOptimizelyTargets))]
		public async Task PageTypeWithPropertyMissingVirtual_Match(ReferenceAssemblies assemblies)
		{
			const string test = @"
				namespace tests
				{
					public class ArticlePage : EPiServer.Core.PageData
					{
						public string {|#0:Heading|} { get; set; }
					}
				}";

			const string fixTest = @"
				namespace tests
				{
					public class ArticlePage : EPiServer.Core.PageData
					{
						public virtual string Heading { get; set; }
					}
				}";

			var expected = VerifyCS.Diagnostic(ContentPropertyMustBeVirtualAnalyzer.DiagnosticId)
				.WithLocation(0)
				.WithArguments("Heading");

			await VerifyCS.VerifyCodeFixAsync(test, assemblies, expected, fixTest);
		}

		[TestMethod]
		[DynamicData(nameof(AllOptimizelyTargets))]
		public async Task PageTypeWithOverriddenVirtualProperty_NoMatch(ReferenceAssemblies assemblies)
		{
			const string test = @"
				namespace tests
				{
					public class MyBasePage : EPiServer.Core.PageData
					{
						public virtual string Heading { get; set; }
					}
					public class ArticlePage : MyBasePage
					{
						public override string Heading { get; set; }
					}
				}";

			await VerifyCS.VerifyAnalyzerAsync(test, assemblies);
		}

		[TestMethod]
		[DynamicData(nameof(AllOptimizelyTargets))]
		public async Task PrivateProperty_NoMatch(ReferenceAssemblies assemblies)
		{
			const string test = @"
				namespace tests
				{
					public class ArticlePage : EPiServer.Core.PageData
					{
						private string Heading { get; set; }
					}
				}";

			await VerifyCS.VerifyAnalyzerAsync(test, assemblies);
		}

		[TestMethod]
		[DynamicData(nameof(AllOptimizelyTargets))]
		public async Task StaticProperty_NoMatch(ReferenceAssemblies assemblies)
		{
			const string test = @"
				namespace tests
				{
					public class ArticlePage : EPiServer.Core.PageData
					{
						public static string Heading { get; set; }
					}
				}";

			await VerifyCS.VerifyAnalyzerAsync(test, assemblies);
		}

		[TestMethod]
		[DynamicData(nameof(AllOptimizelyTargets))]
		public async Task AbstractPageTypesIsNotExcluded_Match(ReferenceAssemblies assemblies)
		{
			const string test = @"
				namespace tests
				{
					public abstract class ArticlePage : EPiServer.Core.PageData
					{
						public string {|#0:Heading|} { get; set; }
					}
				}";

			const string fixTest = @"
				namespace tests
				{
					public abstract class ArticlePage : EPiServer.Core.PageData
					{
						public virtual string Heading { get; set; }
					}
				}";

			var expected = VerifyCS.Diagnostic(ContentPropertyMustBeVirtualAnalyzer.DiagnosticId)
				.WithLocation(0)
				.WithArguments("Heading");
			await VerifyCS.VerifyCodeFixAsync(test, assemblies, expected, fixTest);
		}

		[TestMethod]
		[DynamicData(nameof(AllOptimizelyTargets))]
		public async Task ExcludePropertyWithoutSetter_NoMatch(ReferenceAssemblies assemblies)
		{
			const string test = @"
				namespace tests
				{
					public class ArticlePage : EPiServer.Core.PageData
					{
						public string Heading => ""Random page title"";
					}
				}";

			await VerifyCS.VerifyAnalyzerAsync(test, assemblies);
		}

		[TestMethod]
		[DynamicData(nameof(AllOptimizelyTargets))]
		public async Task ExcludePropertyWithIgnoreAttribute_NoMatch(ReferenceAssemblies assemblies)
		{
			const string test = @"
				namespace tests
				{
					public class ArticlePage : EPiServer.Core.PageData
					{
						[EPiServer.DataAnnotations.Ignore]
						public /*virtual*/ string Heading { get; set; }
					}
				}";

			await VerifyCS.VerifyAnalyzerAsync(test, assemblies);
		}

		[TestMethod]
		[DynamicData(nameof(AllOptimizelyTargets))]
		public async Task NonPublicAccessors_NoMatch(ReferenceAssemblies assemblies)
		{
			const string test = @"
				namespace tests
				{
					public class ArticlePage : EPiServer.Core.PageData
					{
						public /*virtual*/ string InternalGetter { internal get; set; }
						public /*virtual*/ string PrivateGetter { private get; set; }
						public /*virtual*/ string InternalSetter { get; internal set; }
						public /*virtual*/ string PrivateSetter { get; private set; }

#if NET5_0_OR_GREATER
						public /*virtual*/ string InitSetter { get; init; }
#endif
					}
				}";

			await VerifyCS.VerifyAnalyzerAsync(test, assemblies);
		}
	}
}
