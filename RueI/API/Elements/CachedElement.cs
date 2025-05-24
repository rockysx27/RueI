namespace RueI.API.Elements;

using System;

using RueI.API.Parsing;

using UnityEngine;

/// <summary>
/// Represents a <see cref="DynamicElement"/> that caches text for a certain period of time.
/// </summary>
internal class CachedElement : DynamicElement
{
    private float timeLeft = -1;
    private ParsedData cachedParsedData;

    /// <summary>
    /// Initializes a new instance of the <see cref="CachedElement"/> class.
    /// </summary>
    /// <param name="contentGetter"><inheritdoc cref="DynamicElement(Func{string}, float)" path="/param[@name='contentGetter']"/></param>
    /// <param name="cacheTime">A <see cref="TimeSpan"/> indicating how long to store
    /// cached text before regenerating.
    /// </param>
    /// <param name="position"><inheritdoc cref="DynamicElement(Func{string}, float)" path="/param[@name='position']"/></param>
    public CachedElement(Func<string> contentGetter, TimeSpan cacheTime, float position)
        : base(contentGetter, position)
    {
    }

    /// <summary>
    /// Gets a <see cref="TimeSpan"/> indicating how long data should be cached for before it is regenerated.
    /// </summary>
    public TimeSpan CacheTime { get; }

    /// <inheritdoc/>
    protected internal override ParsedData GetParsedData(ReferenceHub hub)
    {
        this.timeLeft -= Time.deltaTime;

        if (this.timeLeft < 0)
        {
            this.timeLeft = (float)this.CacheTime.TotalSeconds;

            return this.cachedParsedData = Parser.Parse(this.ContentGetter(hub));
        }
        else
        {
            return this.cachedParsedData;
        }
    }
}