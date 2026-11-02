using System;
using UnityEngine.TextCore;

namespace BingusNametagsPlusPlus.Utilities;

internal static class IconGlyphLayout
{
    internal static GlyphMetrics ScaleMetrics(GlyphMetrics reference, float height)
    {
        if (reference.height <= 0 || height <= 0)
            throw new ArgumentOutOfRangeException(nameof(height));
        float ratio = height / reference.height;
        return new GlyphMetrics(reference.width * ratio, height, reference.horizontalBearingX * ratio, reference.horizontalBearingY * ratio, reference.horizontalAdvance * ratio);
    }
}
