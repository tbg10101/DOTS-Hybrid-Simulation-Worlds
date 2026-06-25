# DOTS-Hybrid-Simulation-Worlds

A framework for using FixedUpdate in a simulation world which is linked to a GameObject-based presentation layer.

> ## ⚠️ DO NOT USE THIS BRANCH
>
> This branch contains an **incomplete, parked** rewrite of the system-group editor onto Unity 6.5's GraphToolkit
> module (`com.unity.graphtoolkit`). It is blocked on missing public APIs in the GraphToolkit version that ships with
> Unity 6.5 (`0.5.0-exp.1`) and is **not safe to use in production**. The work is being shelved until **Unity 6.6**,
> which makes the two required APIs public (confirmed in the 6.6 alpha — see
> https://discussions.unity.com/t/graph-toolkit-update-in-unity-6-6-alpha/1721970). Use a release branch instead.
>
> ### What works
> - The runtime data model: per-group `SystemGroupGraphAsset` ScriptableObjects referenced by `WorldBehaviour`, baked
>   from the authoring graph and read at runtime via the unchanged topological `GetExecutionOrder()`.
> - The GraphToolkit authoring graph, the bake bridge, and the one-time migration of pre-6.5 embedded
>   `GraphSystemGroupData` into the new assets.
>
> ### What is broken / blocked on missing GraphToolkit APIs
>
> 1. **Graphs can only form linear (in-tree) sequences — a node cannot have multiple dependencies.**
>    The "Depends On" input port is single-capacity, so only one wire can land on it. GraphToolkit `0.5.0-exp.1`
>    exposes **no public way to set a port's capacity to `Multi`** (the setter exists only on the internal
>    `Unity.GraphToolkit.Editor.PortModel.Capacity`). A general dependency DAG requires multi-capacity inputs.
>    - *Fixed in Unity 6.6:* the new public `IPortBuilder.WithCapacity(PortCapacity capacity)` API. Call
>      `.WithCapacity(PortCapacity.Multi)` on the "Depends On" input port in `OnDefinePorts` of `SystemReferenceNode`
>      and `EndOfGroupNode`; the bake (`IPort.GetConnectedPorts`) and runtime `GetExecutionOrder()` already handle
>      arbitrary multi-dependency DAGs, so nothing downstream changes.
>
> 2. **Migration cannot rebuild the visual authoring graph — it only bakes the runtime asset.**
>    Each `SystemReferenceNode` stores its `SystemTypeReference` in a GraphToolkit **node option**, and `INodeOption`
>    exposes only `TryGetValue` (read) — there is **no public setter for a node option's value** (contrast `IPort`,
>    which has `TrySetValue`). So the migration utility can build the node/wire structure but cannot populate each
>    node's assigned system, which would produce a graph full of unassigned nodes that would overwrite the migrated
>    runtime data on first edit. As a result, migrated groups get correct runtime data but **no `.sysgroup` authoring
>    graph**; re-authoring one visually means rebuilding its nodes by hand.
>    - *Fixed in Unity 6.6:* `INodeOption.TrySetValue` is now public. With it, the migration utility can write each
>      `SystemReferenceNode`'s `SystemTypeReference` option and fully reconstruct the `.sysgroup` authoring graph from
>      the baked `GraphSystemGroupData`, instead of baking runtime data only.
>
> 3. **The "End of Group" sink node cannot be hidden from the Add Node menu.**
>    GraphToolkit ties menu visibility to add-compatibility (a node that can be added is always listed), so duplicate
>    End of Group nodes can be created. They are flagged with an in-graph warning marker and ignored at runtime (the
>    first is kept), matching the GraphToolkit Texture Maker sample's pattern. This is a deliberate workaround, not a
>    blocker, and is **not** changed by the 6.6 update — the marker-and-ignore approach remains the intended design.
>
> 4. **Preset nodes can't be added to the graph.**
>    The graph editor currently requires that an empty system reference node be added then the user add the system
>    reference into a field on the node. This is more clicks than the previous graph editor. An updated workflow would
>    require an API to edit the right-click menu or add node menu.

# How To
Each instance of the Bootstrapper class contains it's own world.

This handles simulation and presentation a little differently than the default Unity world:
* Simulation systems are run during Unity's FixedUpdate. Presentation still runs during Update so there are utilities and examples that demonstrate how to render an interpolated simulation state.
* The top level system groups are not sorted using UpdateAfter/UpdateBefore annotations. Instead they execute in the order configured in the inspector. Other groups can be sorted using those annotations if you want.

Systems are set up to do presentation using GameObjects. Your custom Components should inherit from EntityMonoBehaviour instead of MonoBehaviour.

Prefabs and EntityArchetypes are managed in the Bootstrapper class as well. Utility methods for creation and destruction are provided to ease the management of the simulation and presentation sides of an object.

Add a GameObject with ResetDotsOnDestroy into your scene to clean things up when the scene transitions.

Remember to add `UNITY_DISABLE_AUTOMATIC_SYSTEM_BOOTSTRAP` to the Scripting Define Symbols in Project Settings > Player if you want to turn off the default world that Unity creates. This doesn't stop you from using any of the default systems but you will need to set them up in a Bootstrapper.

See the Example scene for more information.

# Development Environment

Turn on `.csproj` generation for local packages to enable IDE analysis of package files.
