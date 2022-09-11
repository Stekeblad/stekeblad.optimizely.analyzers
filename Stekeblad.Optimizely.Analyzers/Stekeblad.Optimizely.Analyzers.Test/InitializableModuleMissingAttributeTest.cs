using Microsoft.VisualStudio.TestTools.UnitTesting;
using Stekeblad.Optimizely.Analyzers.Analyzers;
using System.Threading.Tasks;
using VerifyCS = Stekeblad.Optimizely.Analyzers.Test.CSharpAnalyzerVerifier<
	Stekeblad.Optimizely.Analyzers.Analyzers.InitializableModuleMissingAttributeAnalyzer>;

namespace Stekeblad.Optimizely.Analyzers.Test
{
	[TestClass]
	public class InitializableModuleMissingAttributeTest
	{
		[TestMethod]
		public async Task IInitInteraceInitModuleAttr_NoMatch()
		{
			const string test = @"
				using EPiServer.Framework;
				using EPiServer.Framework.Initialization;

				namespace tests
				{
					[InitializableModule]
					public class TestModule : IInitializableModule
					{
						public void Initialize(InitializationEngine context) {}
						public void Uninitialize(InitializationEngine context) {}
					}
				}";

			await VerifyCS.VerifyAnalyzerAsync(test);
		}

		[TestMethod]
		public async Task IConfigurableInteraceInitModuleAttr_NoMatch()
		{
			const string test = @"
				using EPiServer.Framework;
				using EPiServer.Framework.Initialization;
				using EPiServer.ServiceLocation;

				namespace tests
				{
					[InitializableModule]
					public class TestModule : IConfigurableModule
					{
						public void Initialize(InitializationEngine context) {}
						public void Uninitialize(InitializationEngine context) {}
						public void ConfigureContainer(ServiceConfigurationContext context) {}
					}
				}";

			await VerifyCS.VerifyAnalyzerAsync(test);
		}

		[TestMethod]
		public async Task IInitInteraceModuleDependAttr_NoMatch()
		{
			const string test = @"
				using EPiServer.Framework;
				using EPiServer.Framework.Initialization;
				using EPiServer.ServiceLocation;

				namespace tests
				{
					[ModuleDependency(typeof(ServiceContainerInitialization))]
					public class TestModule : IInitializableModule
					{
						public void Initialize(InitializationEngine context) {}
						public void Uninitialize(InitializationEngine context) {}
					}
				}";

			await VerifyCS.VerifyAnalyzerAsync(test);
		}

		[TestMethod]
		public async Task IConfigurableInteraceModuleDependAttr_NoMatch()
		{
			const string test = @"
				using EPiServer.Framework;
				using EPiServer.Framework.Initialization;
				using EPiServer.ServiceLocation;

				namespace tests
				{
					[ModuleDependency(typeof(ServiceContainerInitialization))]
					public class TestModule : IConfigurableModule
					{
						public void Initialize(InitializationEngine context) {}
						public void Uninitialize(InitializationEngine context) {}
						public void ConfigureContainer(ServiceConfigurationContext context) {}
					}
				}";

			await VerifyCS.VerifyAnalyzerAsync(test);
		}

		[TestMethod]
		public async Task IConfigurableInteraceNoAttr_Match()
		{
			const string test = @"
				using EPiServer.Framework;
				using EPiServer.Framework.Initialization;
				using EPiServer.ServiceLocation;

				namespace tests
				{
					public class {|#0:TestModule|} : IConfigurableModule
					{
						public void Initialize(InitializationEngine context) {}
						public void Uninitialize(InitializationEngine context) {}
						public void ConfigureContainer(ServiceConfigurationContext context) {}
					}
				}";

			var expected = VerifyCS.Diagnostic(InitializableModuleMissingAttributeAnalyzer.DiagnosticId)
				.WithLocation(0)
				.WithArguments("TestModule");
			await VerifyCS.VerifyAnalyzerAsync(test, expected);
		}

		[TestMethod]
		public async Task IInitInteraceNoAttr_Match()
		{
			const string test = @"
				using EPiServer.Framework;
				using EPiServer.Framework.Initialization;

				namespace tests
				{
					public class {|#0:TestModule|} : IInitializableModule
					{
						public void Initialize(InitializationEngine context) {}
						public void Uninitialize(InitializationEngine context) {}
					}
				}";

			var expected = VerifyCS.Diagnostic(InitializableModuleMissingAttributeAnalyzer.DiagnosticId)
				.WithLocation(0)
				.WithArguments("TestModule");
			await VerifyCS.VerifyAnalyzerAsync(test, expected);
		}
	}
}
