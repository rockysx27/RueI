# Migrating to v3
This is a guide on how to migrate from RueI v2 to v3. Not all changes are documented; this is just a basic introduction of the various changes.

## Key Changes
- **RueI is no longer a dependency. It is now a LabAPI plugin.** EXILED plugins that rely on it should still work, but you need to put it in the LabAPI plugin folder still.
- `Display`s have been removed. `DisplayCore` has been renamed to `RueDisplay`, and functions very similar.
- After adding an element, you no longer need to update the `RueDisplay`.
- Elements are now read-only: after creating an element, you can't edit its text.
