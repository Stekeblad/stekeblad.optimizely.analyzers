using Microsoft.VisualStudio.TestTools.UnitTesting;
using Stekeblad.Optimizely.Analyzers.Analyzers;
using Stekeblad.Optimizely.Analyzers.Test.Util;
using System.Threading.Tasks;
using VerifyCS = Stekeblad.Optimizely.Analyzers.Test.CSharpAnalyzerVerifier<
	Stekeblad.Optimizely.Analyzers.Analyzers.InitializableModuleMissingInterfaceAnalyzer>;

namespace Stekeblad.Optimizely.Analyzers.Test
{
	[TestClass]
	public class InitializableModuleMissingInterfaceTest
	{
		// The nomatch-test cases is already covered by InitializableModuleMissingAttributeTest

		[TestMethod]
		public async Task InitializbleAttributeNoInterface_Match()
		{
			const string test = @"
				using EPiServer.Framework;
				using EPiServer.Framework.Initialization;

				namespace tests
				{
					[InitializableModule]
					public class {|#0:TestModule|}
					{
						public void Initialize(InitializationEngine context) {}
						public void Uninitialize(InitializationEngine context) {}
					}
				}";

			var expected = VerifyCS.Diagnostic(InitializableModuleMissingInterfaceAnalyzer.DiagnosticId)
				.WithLocation(0)
				.WithArguments("TestModule", "InitializableModuleAttribute");
			await VerifyCS.VerifyAnalyzerAsync(test, PackageCollections.Core_11, expected);
		}

		[TestMethod]
		public async Task DependencyAttributeNoInterface_Match()
		{
			const string test = @"
				using EPiServer.Framework;
				using EPiServer.Framework.Initialization;
				using EPiServer.ServiceLocation;

				namespace tests
				{
					[ModuleDependency(typeof(ServiceContainerInitialization))]
					public class {|#0:TestModule|}
					{
						public void Initialize(InitializationEngine context) {}
						public void Uninitialize(InitializationEngine context) {}
						public void ConfigureContainer(ServiceConfigurationContext context) {}
					}
				}";

			var expected = VerifyCS.Diagnostic(InitializableModuleMissingInterfaceAnalyzer.DiagnosticId)
				.WithLocation(0)
				.WithArguments("TestModule", "ModuleDependencyAttribute");
			await VerifyCS.VerifyAnalyzerAsync(test, PackageCollections.Core_11, expected);
		}
	}
}
