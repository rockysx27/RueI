namespace RueI;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

using HarmonyLib;

using LabApi.Features.Wrappers;
using LabApi.Loader.Features.Plugins;
using RueI.API;

[SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "The plugin class does not need documentation, as it is not used by consumers")]
internal class RueIPlugin : Plugin
{
    private static readonly Assembly Assembly = typeof(RueIPlugin).Assembly;

    private readonly Harmony harmony = new("RueI");

    public override string Name { get; } = Assembly.GetName().Name;

    public override string Description { get; } = "universal hint framework";

    public override string Author { get; } = "Rue <3";

    public override Version Version { get; } = Assembly.GetName().Version;

    public override Version RequiredApiVersion { get; } = typeof(Player).Assembly.GetName().Version;

    public override void Enable()
    {
        this.harmony.PatchAll();

        RueDisplay.RegisterEvents();
    }

    public override void Disable()
    {
        this.harmony.UnpatchAll();

        RueDisplay.UnregisterEvents();
    }
}