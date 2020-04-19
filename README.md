# DOTS-Hybrid-Simulation-Worlds

A framework for using FixedUpdate in a simulation world which is linked to a GameObject-based presentation layer.

# How To
Each instance of the Bootstrapper class contains it's own world.

This handles simulation and presentation a little differently than the default Unity world:
* Simulation systems are run during Unity's FixedUpdate. Presentation still runs during Update so there are utilities and examples that demonstrate how to render an interpolated simulation state.
* The top level system groups are not sorted using UpdateAfter/UpdateBefore annotations. Instead they execute in the order configured in the inspector. Other groups can be sorted using those annotations if you want.

Systems are set up to do presentation using GameObjects. Your custom Components should inherit from EntityMonoBehaviour instead of MonoBehaviour.

Prefabs and EntityArchetypes are managed in the Bootstrapper class as well. Utility methods for creation and destruction are provided to ease the management of the simulation and presentation sides of an object.

Add a GameObject with ResetDotsOnDestroy into your scene to clean things up when the scene transitions.

See the Example scene for more information.
