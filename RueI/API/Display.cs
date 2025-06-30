namespace RueI.API;

using System;
using System.Collections.Generic;
using System.Linq;
using LabApi.Features.Wrappers;

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

    private readonly ValueSortedDictionary<Tag, StoredElement> elements = new(ValueSortedDictionary<Tag, StoredElement>.InsertionBehavior.InsertAfterEqual);
    private readonly ReferenceHub hub;

    private bool updateNextFrame = false;

    static Display()
    {
        StaticUnityMethods.OnUpdate += () =>
        {
            foreach (Display display in Displays.Values)
            {
                if (display.updateNextFrame)
                {
                    display.updateNextFrame = false;

                    display.Update();
                }
            }
        };
    }

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
        float.PositiveInfinity);

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

    private void Add(Tag tag, Element element, float expireAt)
    {
        this.elements[tag] = new()
        {
            Element = element,
            ExpireAt = expireAt,
        };

        this.updateNextFrame = true;
    }

    private void Update()
    {
        ElementCombiner.Combine(this.hub, this.elements.FilterOut(x => x.Value.ExpireAt < Time.time).Select(x => x.Element));
    }

    private struct StoredElement : IComparable<StoredElement>
    {
        public float ExpireAt;

        public Element Element;

        public Tag Tag;

        public override readonly int GetHashCode() => this.Tag.GetHashCode();

        public override readonly bool Equals(object obj) => obj is StoredElement stored && this.Tag.Equals(stored.Tag);

        // sort ascending
        public readonly int CompareTo(StoredElement other) => this.Element.ZIndex.CompareTo(other.Element.ZIndex); // we use CompareTo for overflow support
    }
}