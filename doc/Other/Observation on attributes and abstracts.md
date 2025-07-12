# Observation on Attributes and Abstracts

I noticed inconsistent behavior surrounding attributes
on abstract types and thought I had to note it down somewhere.

## The experiment

Consider the following classes, what will happen if you add
them to a site and start it?

> This was tested on Episerver.CMS version 12.32.4

```C#
using EPiServer.PlugIn;
using EPiServer.Scheduler;

namespace Opti12Alloy
{
	[ScheduledPlugIn(GUID = "11111111-1111-1111-1111-111111111111")]
	public abstract class AbstractJob : ScheduledJobBase { }

	[ScheduledPlugIn(GUID = "11111111-1111-1111-1111-111111111111")]
	public class RealJob : ScheduledJobBase { public override string Execute() => throw new NotImplementedException(); }

	[ContentType(GUID = "11111111-1111-1111-1111-111111111111")]
	public abstract class AbstractContent : PageData { }

	[ContentType(GUID = "11111111-1111-1111-1111-111111111111")]
	public class RealContent : PageData { }

	[PropertyDefinitionTypePlugIn(GUID = "11111111-1111-1111-1111-111111111111")]
	public abstract class AbstractPluginType : PropertyLongString { }

	[PropertyDefinitionTypePlugIn(GUID = "11111111-1111-1111-1111-111111111111")]
	public class RealPluginType : PropertyLongString { }

	[InitializableModule]
	public abstract class AbstractInitializer : IInitializableModule
	{
		public abstract void Initialize(InitializationEngine context);
		public abstract void Uninitialize(InitializationEngine context);
	}

	[InitializableModule]
	public class RealInitializer : IInitializableModule
	{
		public void Initialize(InitializationEngine context) { return; }
		public void Uninitialize(InitializationEngine context) { return; }
	}
}
```

### Attempt 1

The site fails to start with the following exception:

`System.MissingMethodException: 'Constructor on type 'Opti12Alloy.AbstractInitializer' not found.'`

Solution: Remove `[InitializableModule]` from `AbstractInitializer`.

### Attempt 2

The site fails to start with the following exception:

`InvalidOperationException: Type 'Opti12Alloy.AbstractPluginType' does not have any public constructor`

Solution: Remove `[PropertyDefinitionTypePlugIn]` from `AbstractPluginType`.

### Attempt 3

The site fails to start with the following exception:

`SynchronizationException: Multiple scheduled jobs uses the same GUID '11111111-1111-1111-1111-111111111111': 'Opti12Alloy.AbstractJob,Opti12Alloy', 'Opti12Alloy.RealJob,Opti12Alloy'`

Solution: Change/remove the GUID, or preferably, remove the entire `[ScheduledPlugIn]` attribute from `AbstractJob`.

### Attempt 4

Now the site starts

## Summary

- The content scanner at startup seems to exclude abstract classes.
- The scheduled job scanner seems to validate GUIDs before filtering out abstract types (if filtering at all).
- The PlugIn scanner and initialization engine attempts to instantiate types without checking if they are abstract first.

## Conclusion

To avoid problems, avoid adding attributes to types if they are marked as abstract.