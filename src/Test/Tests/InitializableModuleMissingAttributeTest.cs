using Microsoft.VisualStudio.TestTools.UnitTesting;
using Stekeblad.Optimizely.Analyzers.Analyzers.InitializableModules;
using Stekeblad.Optimizely.Analyzers.Test.Util;
using System.Threading.Tasks;
using VerifyAttributeCS = Stekeblad.Optimizely.Analyzers.Test.CSharpAnalyzerVerifier<
    Stekeblad.Optimizely.Analyzers.Analyzers.InitializableModules.InitializableModuleMissingAttributeAnalyzer>;
using VerifyInterfaceCS = Stekeblad.Optimizely.Analyzers.Test.CSharpAnalyzerVerifier<
    Stekeblad.Optimizely.Analyzers.Analyzers.InitializableModules.InitializableModuleMissingInterfaceAnalyzer>;

namespace Stekeblad.Optimizely.Analyzers.Test.Tests
{
    [TestClass]
    public class InitializableModuleMissingAttributeTest
    {
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

            var expected = VerifyInterfaceCS.Diagnostic(InitializableModuleMissingInterfaceAnalyzer.DiagnosticId)
                .WithLocation(0)
                .WithArguments("TestModule", "InitializableModuleAttribute");
            await VerifyInterfaceCS.VerifyAnalyzerAsync(test, PackageCollections.Core_11, expected);
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

            var expected = VerifyInterfaceCS.Diagnostic(InitializableModuleMissingInterfaceAnalyzer.DiagnosticId)
                .WithLocation(0)
                .WithArguments("TestModule", "ModuleDependencyAttribute");
            await VerifyInterfaceCS.VerifyAnalyzerAsync(test, PackageCollections.Core_11, expected);
        }

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

            await VerifyAttributeCS.VerifyAnalyzerAsync(test, PackageCollections.Core_11);
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

            await VerifyAttributeCS.VerifyAnalyzerAsync(test, PackageCollections.Core_11);
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

            await VerifyAttributeCS.VerifyAnalyzerAsync(test, PackageCollections.Core_11);
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

            await VerifyAttributeCS.VerifyAnalyzerAsync(test, PackageCollections.Core_11);
        }

        [TestMethod]
        public async Task IInitInteraceAbstract_NoMatch()
        {
            const string test = @"
				using EPiServer.Framework;
				using EPiServer.Framework.Initialization;
				using EPiServer.ServiceLocation;

				namespace tests
				{
					public abstract class TestModule : IInitializableModule
					{
						public void Initialize(InitializationEngine context) {}
						public void Uninitialize(InitializationEngine context) {}
						public void ConfigureContainer(ServiceConfigurationContext context) {}
					}
				}";

            await VerifyAttributeCS.VerifyAnalyzerAsync(test, PackageCollections.Core_11);
        }

        [TestMethod]
        public async Task IConfigurableInterfaceNoAttr_Match()
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

            var expected = VerifyAttributeCS.Diagnostic(InitializableModuleMissingAttributeAnalyzer.DiagnosticId)
                .WithLocation(0)
                .WithArguments("TestModule");
            await VerifyAttributeCS.VerifyAnalyzerAsync(test, PackageCollections.Core_11, expected);
        }

        [TestMethod]
        public async Task IInitInterfaceNoAttr_Match()
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

            var expected = VerifyAttributeCS.Diagnostic(InitializableModuleMissingAttributeAnalyzer.DiagnosticId)
                .WithLocation(0)
                .WithArguments("TestModule");
            await VerifyAttributeCS.VerifyAnalyzerAsync(test, PackageCollections.Core_11, expected);
        }
    }
}
