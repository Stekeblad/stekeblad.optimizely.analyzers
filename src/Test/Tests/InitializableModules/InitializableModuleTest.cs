using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Stekeblad.Optimizely.Analyzers.Analyzers.InitializableModules;
using System.Threading.Tasks;
using VerifyAttributeCS = Stekeblad.Optimizely.Analyzers.Test.CSharpCodeFixVerifier<
	Stekeblad.Optimizely.Analyzers.Analyzers.InitializableModules.InitializableModuleMissingAttributeAnalyzer,
	Stekeblad.Optimizely.Analyzers.CodeFixes.InitializableModules.InitializableModuleMissingAttributeAnalyzerCodeFixProvider>;
using VerifyInterfaceCS = Stekeblad.Optimizely.Analyzers.Test.CSharpAnalyzerVerifier<
	Stekeblad.Optimizely.Analyzers.Analyzers.InitializableModules.InitializableModuleMissingInterfaceAnalyzer>;

namespace Stekeblad.Optimizely.Analyzers.Test.Tests.InitializableModules
{
	[TestClass]
	public class InitializableModuleTest : MyTestClassBase
	{
		[TestMethod]
		[DynamicData(nameof(AllOptimizelyTargets))]
		public async Task InitializableAttributeNoInterface_Match(ReferenceAssemblies assemblies)
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
			await VerifyInterfaceCS.VerifyAnalyzerAsync(test, assemblies, expected);
		}

		[TestMethod]
		[DynamicData(nameof(AllOptimizelyTargets))]
		public async Task DependencyAttributeNoInterface_Match(ReferenceAssemblies assemblies)
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
			await VerifyInterfaceCS.VerifyAnalyzerAsync(test, assemblies, expected);
		}

		[TestMethod]
		[DynamicData(nameof(AllOptimizelyTargets))]
		public async Task IInitInterfaceInitModuleAttr_NoMatch(ReferenceAssemblies assemblies)
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

			await VerifyAttributeCS.VerifyAnalyzerAsync(test, assemblies);
		}

		[TestMethod]
		[DynamicData(nameof(AllOptimizelyTargets))]
		public async Task IConfigurableInterfaceInitModuleAttr_NoMatch(ReferenceAssemblies assemblies)
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

			await VerifyAttributeCS.VerifyAnalyzerAsync(test, assemblies);
		}

		[TestMethod]
		[DynamicData(nameof(AllOptimizelyTargets))]
		public async Task IInitInterfaceModuleDependAttr_NoMatch(ReferenceAssemblies assemblies)
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

			await VerifyAttributeCS.VerifyAnalyzerAsync(test, assemblies);
		}

		[TestMethod]
		[DynamicData(nameof(AllOptimizelyTargets))]
		public async Task IConfigurableInterfaceModuleDependAttr_NoMatch(ReferenceAssemblies assemblies)
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

			await VerifyAttributeCS.VerifyAnalyzerAsync(test, assemblies);
		}

		[TestMethod]
		[DynamicData(nameof(AllOptimizelyTargets))]
		public async Task IInitInterfaceAbstract_NoMatch(ReferenceAssemblies assemblies)
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

			await VerifyAttributeCS.VerifyAnalyzerAsync(test, assemblies);
		}

		[TestMethod]
		[DynamicData(nameof(AllOptimizelyTargets))]
		public async Task IConfigurableInterfaceNoAttr_Match(ReferenceAssemblies assemblies)
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
			await VerifyAttributeCS.VerifyAnalyzerAsync(test, assemblies, expected);
		}

		[TestMethod]
		[DynamicData(nameof(AllOptimizelyTargets))]
		public async Task IInitInterfaceNoAttr_Match(ReferenceAssemblies assemblies)
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
			await VerifyAttributeCS.VerifyAnalyzerAsync(test, assemblies, expected);
		}

		[TestMethod]
		[DynamicData(nameof(AllOptimizelyTargets))]
		public async Task ModuleDependency_EmptyDependencyList_NoMatch(ReferenceAssemblies assemblies)
		{
			const string test = @"
				using EPiServer.Framework;
				using EPiServer.Framework.Initialization;
				using EPiServer.ServiceLocation;

				namespace tests
				{
					[ModuleDependency]
					public class TestModule : IInitializableModule
					{
						public void Initialize(InitializationEngine context) {}
						public void Uninitialize(InitializationEngine context) {}
					}
				}";

			await VerifyAttributeCS.VerifyAnalyzerAsync(test, assemblies);
		}

		[TestMethod]
		[DynamicData(nameof(AllOptimizelyTargets))]
		public async Task ModuleDependency_MultipleDependencies_NoMatch(ReferenceAssemblies assemblies)
		{
			const string test = @"
				using EPiServer.Framework;
				using EPiServer.Framework.Initialization;
				using EPiServer.ServiceLocation;

				namespace tests
				{
					[ModuleDependency(typeof(ServiceContainerInitialization), typeof(ServiceContainerInitialization))]
					public class TestModule : IInitializableModule
					{
						public void Initialize(InitializationEngine context) {}
						public void Uninitialize(InitializationEngine context) {}
					}
				}";

			await VerifyAttributeCS.VerifyAnalyzerAsync(test, assemblies);
		}

		[TestMethod]
		[DynamicData(nameof(AllOptimizelyTargets))]
		public async Task ModuleDependency_SingleBadDependency_Match(ReferenceAssemblies assemblies)
		{
			const string test = @"
				using EPiServer.Framework;
				using EPiServer.Framework.Initialization;
				using EPiServer.ServiceLocation;
               using EPiServer.Shell.ObjectEditing;

				namespace tests
				{
					[ModuleDependency({|#0:typeof(SelectOneAttribute)|})]
					public class TestModule : IInitializableModule
					{
						public void Initialize(InitializationEngine context) {}
						public void Uninitialize(InitializationEngine context) {}
					}
				}";

			var expected = VerifyAttributeCS.Diagnostic(InitializableModuleMissingAttributeAnalyzer.BadTypeDiagnosticId)
				.WithLocation(0)
				.WithArguments("TestModule", "SelectOneAttribute");
			await VerifyAttributeCS.VerifyAnalyzerAsync(test, assemblies, expected);
		}

		[TestMethod]
		[DynamicData(nameof(AllOptimizelyTargets))]
		public async Task ModuleDependency_TwoDependencyAttributesOneBeingBad_Match(ReferenceAssemblies assemblies)
		{
			const string test = @"
				using EPiServer.Framework;
				using EPiServer.Framework.Initialization;
				using EPiServer.ServiceLocation;
               using EPiServer.Shell.ObjectEditing;

				namespace tests
				{
					[ModuleDependency(typeof(ServiceContainerInitialization))]
					[ModuleDependency({|#0:typeof(SelectOneAttribute)|})]
					public class TestModule : IInitializableModule
					{
						public void Initialize(InitializationEngine context) {}
						public void Uninitialize(InitializationEngine context) {}
					}
				}";

			var expected = VerifyAttributeCS.Diagnostic(InitializableModuleMissingAttributeAnalyzer.BadTypeDiagnosticId)
				.WithLocation(0)
				.WithArguments("TestModule", "SelectOneAttribute");
			await VerifyAttributeCS.VerifyAnalyzerAsync(test, assemblies, expected);
		}

		[TestMethod]
		[DynamicData(nameof(AllOptimizelyTargets))]
		public async Task ModuleDependency_DependencyListContainsBadDependency_Match(ReferenceAssemblies assemblies)
		{
			const string test = @"
				using EPiServer.Framework;
				using EPiServer.Framework.Initialization;
				using EPiServer.ServiceLocation;
               using EPiServer.Shell.ObjectEditing;

				namespace tests
				{
					[ModuleDependency(typeof(ServiceContainerInitialization), {|#0:typeof(SelectOneAttribute)|})]
					public class TestModule : IInitializableModule
					{
						public void Initialize(InitializationEngine context) {}
						public void Uninitialize(InitializationEngine context) {}
					}
				}";

			var expected = VerifyAttributeCS.Diagnostic(InitializableModuleMissingAttributeAnalyzer.BadTypeDiagnosticId)
				.WithLocation(0)
				.WithArguments("TestModule", "SelectOneAttribute");
			await VerifyAttributeCS.VerifyAnalyzerAsync(test, assemblies, expected);
		}

		[TestMethod]
		[DynamicData(nameof(AllOptimizelyTargets))]
		public async Task AttributeOnAbstract_Match(ReferenceAssemblies assemblies)
		{
			const string test = @"
				using EPiServer.Framework;
				using EPiServer.Framework.Initialization;
				using EPiServer.ServiceLocation;
               using EPiServer.Shell.ObjectEditing;

				namespace tests
				{
					[ModuleDependency(typeof(ServiceContainerInitialization))]
					public {|#0:abstract|} class TestModule : IInitializableModule
					{
						public void Initialize(InitializationEngine context) {}
						public void Uninitialize(InitializationEngine context) {}
					}
				}";

			var expected = VerifyAttributeCS.Diagnostic(InitializableModuleMissingAttributeAnalyzer.AttributeOnAbstractDiagnosticId)
				.WithLocation(0)
				.WithArguments("TestModule");
			await VerifyAttributeCS.VerifyAnalyzerAsync(test, assemblies, expected);
		}

		[TestMethod]
		[DynamicData(nameof(AllOptimizelyTargets))]
		public async Task FixTest_AddAttribute_InitModule(ReferenceAssemblies assemblies)
		{
			const string test = @"
using EPiServer.Framework;
using EPiServer.Framework.Initialization;

namespace tests
{
    public class {|#0:TestModule|} : IInitializableModule
    {
        public void Initialize(InitializationEngine context) { }
        public void Uninitialize(InitializationEngine context) { }
    }
}";

			const string fixTest = @"
using EPiServer.Framework;
using EPiServer.Framework.Initialization;

namespace tests
{
    [InitializableModule]
    public class TestModule : IInitializableModule
    {
        public void Initialize(InitializationEngine context) { }
        public void Uninitialize(InitializationEngine context) { }
    }
}";

			var expected = VerifyAttributeCS.Diagnostic(InitializableModuleMissingAttributeAnalyzer.DiagnosticId)
				.WithLocation(0)
				.WithArguments("TestModule");
			await VerifyAttributeCS.VerifyCodeFixAsync(test, assemblies, expected, fixTest, expected.Id + "a");
		}

		[TestMethod]
		[DynamicData(nameof(AllOptimizelyTargets))]
		public async Task FixTest_AddAttribute_ModuleDependency(ReferenceAssemblies assemblies)
		{
			const string test = @"
using EPiServer.Framework;
using EPiServer.Framework.Initialization;

namespace tests
{
    public class {|#0:TestModule|} : IInitializableModule
    {
        public void Initialize(InitializationEngine context) { }
        public void Uninitialize(InitializationEngine context) { }
    }
}";

			const string fixTest = @"
using EPiServer.Framework;
using EPiServer.Framework.Initialization;

namespace tests
{
    [ModuleDependency(typeof(EPiServer.Web.InitializationModule))]
    public class TestModule : IInitializableModule
    {
        public void Initialize(InitializationEngine context) { }
        public void Uninitialize(InitializationEngine context) { }
    }
}";

			var expected = VerifyAttributeCS.Diagnostic(InitializableModuleMissingAttributeAnalyzer.DiagnosticId)
				.WithLocation(0)
				.WithArguments("TestModule");
			await VerifyAttributeCS.VerifyCodeFixAsync(test, assemblies, expected, fixTest, expected.Id + "b");
		}
	}
}
