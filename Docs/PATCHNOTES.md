# 1.0.0 - Initial Release
* **Initial Project Architecture**:
  * Established modular BepInEx plugin architecture utilizing `Vapok.Common` 3.21.1015 and `JotunnLib` 2.30.2.
  * Implemented synchronized configuration pipeline inheriting from `ConfigSyncBase`.
  * Configured `ILRepack` MSBuild task to internalize `Vapok.Valheim.Common.dll` into target assembly `WardMeBruh.dll`.
* **Dedicated Server Safety**:
  * Isolated client UI and game startup hooks using `GUIManager.IsHeadless()`.
* **Game Reference Alignment**:
  * Built against Valheim 1.0.16 publicized assemblies and Unity 6 (6000.0.75f1) engine runtime.
