namespace RueI.API;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using LabApi.Features.Wrappers;
using RoundRestarting;
using RueI.API.Elements;
using RueI.API.Parsing;
using RueI.Utils.Collections;
using RueI.Utils.Extensions;

using UnityEngine;

/// <summary>
/// Represents a display for a specific player.
/// </summary>
/// <remarks>
/// Every player has a <see cref="Display"/>, which is used by RueI to keep track of their elements and send them hints.
/// </remarks>
public sealed class Display
{
    private static readonly Dictionary<ReferenceHub, Display> Displays = new();

    private readonly ValueSortedDictionary<Tag, StoredElement> elements = new();
    private readonly MinHeap<Tag, float> expirationHeap = new();
    private readonly ReferenceHub hub;

    private bool updateNextFrame = false;
    private float forcedUpdate = float.PositiveInfinity;

    private Display(ReferenceHub hub)
    {
        this.hub = hub;
    }

    /// <summary>
    /// Gets a display for a specific <see cref="ReferenceHub"/>.
    /// </summary>
    /// <param name="hub">The <see cref="ReferenceHub"/> to get the display of.</param>
    /// <returns>The player's corresponding display.</returns>
    public static Display Get(ReferenceHub hub)
    {
        if (hub == null)
        {
            throw new ArgumentNullException(nameof(hub));
        }

        return Displays.GetOrAdd(hub, () => new Display(hub));
    }

    /// <summary>
    /// Gets a display for a specific <see cref="Player"/>.
    /// </summary>
    /// <param name="player">The <see cref="ReferenceHub"/> to get the display of.</param>
    /// <returns>The player's corresponding display.</returns>
    public static Display Get(Player player)
    {
        if (player == null)
        {
            throw new ArgumentNullException(nameof(player));
        }

        return Get(player.ReferenceHub);
    }

    /// <summary>
    /// Adds an <see cref="Element"/> with a unique <see cref="Tag"/> to this <see cref="Display"/>.
    /// </summary>
    /// <param name="tag">A <see cref="Tag"/> to use. If an <see cref="Element"/> already has this tag, it will be replaced.</param>
    /// <param name="element">The <see cref="Element"/> to add.</param>
    public void Add(Tag tag, Element element) => this.Add(
        tag ?? throw new ArgumentNullException(nameof(tag)),
        element ?? throw new ArgumentNullException(nameof(element)),
        float.NaN);

    /// <summary>
    /// Adds an <see cref="Element"/> with a unique <see cref="Tag"/> to this <see cref="Display"/> for a certain period of time.
    /// </summary>
    /// <param name="tag">A <see cref="Tag"/> to use. If an <see cref="Element"/> already has this tag, it will be replaced.</param>
    /// <param name="element">The <see cref="Element"/> to add.</param>
    /// <param name="duration">A <see cref="TimeSpan"/> indicating how long to show the <see cref="Element"/> for.</param>
    public void Add(Tag tag, Element element, TimeSpan duration) => this.Add(
        tag ?? throw new ArgumentNullException(nameof(tag)),
        element ?? throw new ArgumentNullException(nameof(element)),
        (float)duration.TotalSeconds + Time.time);

    /// <summary>
    /// Adds an <see cref="Element"/> to this <see cref="Display"/> for a certain period of time.
    /// </summary>
    /// <param name="element">The <see cref="Element"/> to add.</param>
    /// <param name="duration">A <see cref="TimeSpan"/> indicating how long to show the <see cref="Element"/> for.</param>
    /// <remarks>
    /// When adding an element using this method, it will not have a <see cref="Tag"/>. Therefore, there is no way to remove the element later.
    /// </remarks>
    public void Add(Element element, TimeSpan duration) => this.Add(
        new(), // new() results in a tag that is only equal to itself
        element ?? throw new ArgumentNullException(nameof(element)),
        Time.time + (float)duration.TotalSeconds);

    /// <summary>
    /// Removes the element with the given tag from the <see cref="Display"/>, if there is one.
    /// </summary>
    /// <param name="tag">The tag for which remove the associated <see cref="Element"/>.</param>
    public void Remove(Tag tag)
    {
        if (this.elements.Remove(tag, out StoredElement element))
        {
            if (element.HeapNode != null)
            {
                this.expirationHeap.Remove(element.HeapNode);
            }

            // if there's a forced update (i.e. another hint is hiding RueI,
            // there's no reason to update the display just to remove one hint
            // (since it's already not visible)
            if (!float.IsPositiveInfinity(this.forcedUpdate))
            {
                this.updateNextFrame = true;
            }
        }

        // no need to update if we didn't actually remove anything
    }

    /// <summary>
    /// Updates the display, refreshing any <see cref="DynamicElement"/>.
    /// </summary>
    public void Update()
    {
        this.updateNextFrame = true;
    }

    /// <summary>
    /// Registers the <see cref="Display"/> events.
    /// </summary>
    internal static void RegisterEvents()
    {
        // i use the base game alternatives because these are
        // more lightweight
        StaticUnityMethods.OnUpdate += CheckDisplays;
        ReferenceHub.OnPlayerRemoved += RemoveHub;
        RoundRestart.OnRestartTriggered += Displays.Clear;
    }

    /// <summary>
    /// Unregisters the <see cref="Display"/> events.
    /// </summary>
    internal static void UnregisterEvents()
    {
        StaticUnityMethods.OnUpdate -= CheckDisplays;
        ReferenceHub.OnPlayerRemoved -= RemoveHub;
        RoundRestart.OnRestartTriggered -= Displays.Clear;
    }

    /// <summary>
    /// Updates the display after the given number of seconds.
    /// </summary>
    /// <param name="duration">The time to wait before updating.</param>
    internal void SetUpdateIn(float duration)
    {
        this.forcedUpdate = Time.time + duration;
    }

    // not a lambda so we can later unregister
    private static void RemoveHub(ReferenceHub hub) => Displays.Remove(hub);

    private static void CheckDisplays()
    {
        float time = Time.time;

        foreach (Display display in Displays.Values)
        {
            if (display.forcedUpdate < time)
            {
                display.updateNextFrame = true;
            }

            if (display.updateNextFrame)
            {
                display.FrameUpdate();

                display.updateNextFrame = false;
            }

            while (display.expirationHeap.TryPeek(out var node) && node.Priority < time)
            {
                display.expirationHeap.Pop();
                display.elements.Remove(node.Value);

                display.updateNextFrame = true;
            }
        }
    }

    private void Add(Tag tag, Element element, float expireAt)
    {
        if (float.IsNaN(expireAt))
        {
            var node = this.expirationHeap.Add(tag, expireAt);

            this.elements[tag] = new()
            {
                Element = element,
                Tag = tag,
                HeapNode = node,
            };
        }
        else
        {
            this.elements[tag] = new()
            {
                Element = element,
                Tag = tag,
            };
        }

        this.updateNextFrame = true;
    }

    private void FrameUpdate()
    {
        ElementCombiner.Combine(this.hub, this.elements.Select(x => x.Value.Element)); // we don't use elements.Value, since that boxes (no way around it)

        this.updateNextFrame = false;
        this.forcedUpdate = float.PositiveInfinity;
    }

    private struct StoredElement : IComparable<StoredElement>
    {
        public Element Element;

        public Tag Tag;

        public MinHeap<Tag, float>.Node? HeapNode;

        public override readonly int GetHashCode() => this.Tag.GetHashCode();

        public override readonly bool Equals(object obj) => obj is StoredElement stored && this.Tag.Equals(stored.Tag);

        // sort ascending
        public readonly int CompareTo(StoredElement other) => this.Element.ZIndex.CompareTo(other.Element.ZIndex); // we use CompareTo for overflow support
    }
}