# Changelog

## [1.4.2] - 2026-06-22

- Added `JobHandleExtensions.GetManagedMonoBehaviourUpdateQuery` and a
  `CompleteBeforeManagedMonoBehaviourUpdates(JobHandle, EntityQuery)` overload so callers can cache the
  singleton query in `OnCreate` instead of building it every `OnUpdate`. Deprecated the
  `CompleteBeforeManagedMonoBehaviourUpdates(JobHandle, ref SystemState)` overload, which triggered Entities'
  "creates a query during OnUpdate" performance warning.

## [0.1.0] - 2020-04-19

- Initial release
