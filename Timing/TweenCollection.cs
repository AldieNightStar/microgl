using System.Drawing;
using System.Numerics;

namespace microgl;

public static class TweenCollection
{
    // =========================================================================
    // 1. IMPACT & DAMAGE REACTIONS
    // =========================================================================

    /// <summary>Rapid horizontal jitter as if taking damage.</summary>
    public static TweenPlanner DamageShake(GameObject obj, float intensity = 8f, float duration = 0.35f)
    {
        float originX = obj.Position.X;
        float step = duration / 6f;

        return Tween
            .Call(originX, originX - intensity, step, x => obj.Position = new Vector2(x, obj.Position.Y), Easing.EaseOutQuad)
            .Call(originX - intensity, originX + intensity, step, x => obj.Position = new Vector2(x, obj.Position.Y), Easing.EaseInOutQuad)
            .Call(originX + intensity, originX - intensity * 0.6f, step, x => obj.Position = new Vector2(x, obj.Position.Y), Easing.EaseInOutQuad)
            .Call(originX - intensity * 0.6f, originX + intensity * 0.6f, step, x => obj.Position = new Vector2(x, obj.Position.Y), Easing.EaseInOutQuad)
            .Call(originX + intensity * 0.6f, originX - intensity * 0.25f, step, x => obj.Position = new Vector2(x, obj.Position.Y), Easing.EaseInOutQuad)
            .Call(originX - intensity * 0.25f, originX, step, x => obj.Position = new Vector2(x, obj.Position.Y), Easing.EaseOutQuad);
    }

    /// <summary>Flashes red twice and restores original BlendColor.</summary>
    public static TweenPlanner DamageFlash(GameObject obj, float duration = 0.3f)
    {
        var original = obj.BlendColor;
        var hitColor = Color.FromArgb(original.A, 255, 60, 60);
        float quarter = duration / 4f;

        return Tween
            .CallFunc(() => obj.BlendColor = hitColor)
            .Wait(quarter)
            .CallFunc(() => obj.BlendColor = original)
            .Wait(quarter)
            .CallFunc(() => obj.BlendColor = hitColor)
            .Wait(quarter)
            .CallFunc(() => obj.BlendColor = original)
            .Wait(quarter);
    }

    /// <summary>Pushes object back in a direction and smoothly recovers.</summary>
    public static TweenPlanner Knockback(GameObject obj, Vector2 impulse, float duration = 0.4f)
    {
        var start = obj.Position;
        var peak = start + impulse;

        return Tween
            .Call(0f, 1f, duration * 0.3f, t => obj.Position = Vector2.Lerp(start, peak, t), Easing.EaseOutCubic)
            .Call(0f, 1f, duration * 0.7f, t => obj.Position = Vector2.Lerp(peak, start, t), Easing.EaseInOutQuad);
    }

    // =========================================================================
    // 2. SQUASH, STRETCH & JELLY PHYSICS
    // =========================================================================

    /// <summary>Flattens vertically and widens horizontally, then springs back.</summary>
    public static TweenPlanner Squish(GameObject obj, float amount = 0.3f, float duration = 0.45f)
    {
        var baseScale = obj.Scale;

        return Tween
            .Call(0f, amount, duration * 0.35f, a =>
                obj.Scale = new Vector2(baseScale.X * (1f + a), baseScale.Y * (1f - a)), Easing.EaseOutQuad)
            .Call(amount, 0f, duration * 0.65f, a =>
                obj.Scale = new Vector2(baseScale.X * (1f + a), baseScale.Y * (1f - a)), Easing.EaseOutElastic);
    }

    /// <summary>Rises from a flattened/squeezed state back to normal with jelly wobble.</summary>
    public static TweenPlanner RiseFromSqueezedJelly(GameObject obj, float startSquish = 0.55f, float duration = 0.85f)
    {
        var target = obj.Scale;

        return Tween
            .CallFunc(() => obj.Scale = new Vector2(target.X * (1f + startSquish), target.Y * (1f - startSquish)))
            .Call(startSquish, 0f, duration, a =>
                obj.Scale = new Vector2(target.X * (1f + a), target.Y * (1f - a)), Easing.EaseOutElastic);
    }

    /// <summary>Stretches tall and thin, then settles back to normal.</summary>
    public static TweenPlanner StretchVertical(GameObject obj, float amount = 0.35f, float duration = 0.45f)
    {
        var baseScale = obj.Scale;

        return Tween
            .Call(0f, amount, duration * 0.35f, a =>
                obj.Scale = new Vector2(baseScale.X * (1f - a * 0.6f), baseScale.Y * (1f + a)), Easing.EaseOutQuad)
            .Call(amount, 0f, duration * 0.65f, a =>
                obj.Scale = new Vector2(baseScale.X * (1f - a * 0.6f), baseScale.Y * (1f + a)), Easing.EaseOutElastic);
    }

    /// <summary>Two-axis rubbery jelly jiggle.</summary>
    public static TweenPlanner JellyWobble(GameObject obj, float intensity = 0.25f, float duration = 0.7f)
    {
        var baseScale = obj.Scale;

        return Tween
            .Call(0f, intensity, duration * 0.25f, a =>
                obj.Scale = new Vector2(baseScale.X * (1f + a), baseScale.Y * (1f - a)), Easing.EaseOutQuad)
            .Call(intensity, -intensity * 0.6f, duration * 0.35f, a =>
                obj.Scale = new Vector2(baseScale.X * (1f + a), baseScale.Y * (1f - a)), Easing.EaseInOutSine)
            .Call(-intensity * 0.6f, 0f, duration * 0.4f, a =>
                obj.Scale = new Vector2(baseScale.X * (1f + a), baseScale.Y * (1f - a)), Easing.EaseOutElastic);
    }

    // =========================================================================
    // 3. JUMPS, BUMPS & HOPS
    // =========================================================================

    /// <summary>Jumps straight up in place and lands cleanly.</summary>
    public static TweenPlanner JumpInPlace(GameObject obj, float height = 45f, float duration = 0.55f)
    {
        float groundY = obj.Position.Y;

        return Tween
            .Call(groundY, groundY - height, duration * 0.48f, y => obj.Position = new Vector2(obj.Position.X, y), Easing.EaseOutQuad)
            .Call(groundY - height, groundY, duration * 0.52f, y => obj.Position = new Vector2(obj.Position.X, y), Easing.EaseInQuad);
    }

    /// <summary>Anticipation crouch, jump up, and squash landing.</summary>
    public static TweenPlanner CartoonJump(GameObject obj, float height = 55f, float duration = 0.75f)
    {
        float groundY = obj.Position.Y;
        var baseScale = obj.Scale;

        return Tween
            // Anticipation squash
            .Call(0f, 0.22f, duration * 0.16f, a =>
                obj.Scale = new Vector2(baseScale.X * (1f + a), baseScale.Y * (1f - a)), Easing.EaseOutQuad)
            // Launch upward
            .Call(0f, 1f, duration * 0.34f, t =>
            {
                obj.Position = new Vector2(obj.Position.X, groundY - height * t);
                obj.Scale = new Vector2(baseScale.X * 0.88f, baseScale.Y * 1.15f);
            }, Easing.EaseOutQuad)
            // Fall back down
            .Call(0f, 1f, duration * 0.32f, t =>
                obj.Position = new Vector2(obj.Position.X, (groundY - height) + height * t), Easing.EaseInQuad)
            // Landing recover
            .Call(0.25f, 0f, duration * 0.18f, a =>
                obj.Scale = new Vector2(baseScale.X * (1f + a), baseScale.Y * (1f - a)), Easing.EaseOutBack);
    }

    /// <summary>Quick upward UI/item bump (like hitting a block).</summary>
    public static TweenPlanner BumpUp(GameObject obj, float distance = 18f, float duration = 0.25f)
    {
        float startY = obj.Position.Y;

        return Tween
            .Call(startY, startY - distance, duration * 0.4f, y => obj.Position = new Vector2(obj.Position.X, y), Easing.EaseOutQuad)
            .Call(startY - distance, startY, duration * 0.6f, y => obj.Position = new Vector2(obj.Position.X, y), Easing.EaseOutBounce);
    }

    /// <summary>Quick forward nudge/bump in any direction.</summary>
    public static TweenPlanner BumpDirection(GameObject obj, Vector2 offset, float duration = 0.25f)
    {
        var origin = obj.Position;
        var target = origin + offset;

        return Tween
            .Call(0f, 1f, duration * 0.4f, t => obj.Position = Vector2.Lerp(origin, target, t), Easing.EaseOutQuad)
            .Call(1f, 0f, duration * 0.6f, t => obj.Position = Vector2.Lerp(origin, target, t), Easing.EaseOutBack);
    }

    // =========================================================================
    // 4. WIGGLES, SWAYS & ROTATIONS
    // =========================================================================

    /// <summary>Playful left-right rotational wiggle.</summary>
    public static TweenPlanner Wiggle(GameObject obj, float degrees = 15f, float duration = 0.5f)
    {
        float baseRot = obj.Rotation;
        float rad = float.DegreesToRadians(degrees);
        float quarter = duration / 4f;

        return Tween
            .Call(baseRot, baseRot + rad, quarter, r => obj.Rotation = r, Easing.EaseOutSine)
            .Call(baseRot + rad, baseRot - rad, quarter, r => obj.Rotation = r, Easing.EaseInOutSine)
            .Call(baseRot - rad, baseRot + rad * 0.5f, quarter, r => obj.Rotation = r, Easing.EaseInOutSine)
            .Call(baseRot + rad * 0.5f, baseRot, quarter, r => obj.Rotation = r, Easing.EaseOutQuad);
    }

    /// <summary>Horizontal "No" head-shake movement.</summary>
    public static TweenPlanner ShakeNo(GameObject obj, float distance = 12f, float duration = 0.4f)
    {
        float startX = obj.Position.X;
        float step = duration / 4f;

        return Tween
            .Call(startX, startX - distance, step, x => obj.Position = new Vector2(x, obj.Position.Y), Easing.EaseOutQuad)
            .Call(startX - distance, startX + distance, step, x => obj.Position = new Vector2(x, obj.Position.Y), Easing.EaseInOutQuad)
            .Call(startX + distance, startX - distance * 0.5f, step, x => obj.Position = new Vector2(x, obj.Position.Y), Easing.EaseInOutQuad)
            .Call(startX - distance * 0.5f, startX, step, x => obj.Position = new Vector2(x, obj.Position.Y), Easing.EaseOutQuad);
    }

    /// Vertical "Yes" nod movement.
    public static TweenPlanner NodYes(GameObject obj, float distance = 10f, float duration = 0.4f)
    {
        float startY = obj.Position.Y;
        float step = duration / 4f;
        return Tween
        .Call(startY, startY + distance, step, y => obj.Position = new Vector2(obj.Position.X, y), Easing.EaseOutQuad)
        .Call(startY + distance, startY - distance * 0.4f, step, y => obj.Position = new Vector2(obj.Position.X, y), Easing.EaseInOutQuad)
        .Call(startY - distance * 0.4f, startY + distance * 0.6f, step, y => obj.Position = new Vector2(obj.Position.X, y), Easing.EaseInOutQuad)
        .Call(startY + distance * 0.6f, startY, step, y => obj.Position = new Vector2(obj.Position.X, y), Easing.EaseOutQuad);
    }
    /// Full 360-degree spin with smooth deceleration.
    public static TweenPlanner Spin360(GameObject obj, bool clockwise = true, float duration = 0.75f)
    {
        float start = obj.Rotation;
        float delta = float.DegreesToRadians(clockwise ? 360f : -360f);
        return Tween.Call(start, start + delta, duration, r => obj.Rotation = r, Easing.EaseInOutBack);
    }
    // =========================================================================
    // 5. PULSES, IDLE & ENTRANCE / EXIT
    // =========================================================================
    /// Single heartbeat scale pulse.
    public static TweenPlanner Pulse(GameObject obj, float scaleMultiplier = 1.2f, float duration = 0.4f)
    {
        var baseScale = obj.Scale;
        var peakScale = baseScale * scaleMultiplier;
        return Tween
        .Call(0f, 1f, duration * 0.45f, t => obj.Scale = Vector2.Lerp(baseScale, peakScale, t), Easing.EaseOutQuad)
        .Call(1f, 0f, duration * 0.55f, t => obj.Scale = Vector2.Lerp(baseScale, peakScale, t), Easing.EaseInOutSine);
    }
    /// Gentle idle breathing cycle (expand and relax).
    public static TweenPlanner Breathe(GameObject obj, float amount = 0.06f, float duration = 1.6f)
    {
        var baseScale = obj.Scale;
        var expanded = baseScale * (1f + amount);
        return Tween
        .Call(0f, 1f, duration * 0.5f, t => obj.Scale = Vector2.Lerp(baseScale, expanded, t), Easing.EaseInOutSine)
        .Call(1f, 0f, duration * 0.5f, t => obj.Scale = Vector2.Lerp(baseScale, expanded, t), Easing.EaseInOutSine);
    }
    /// Gentle floating hover up and down.
    public static TweenPlanner HoverFloat(GameObject obj, float amplitude = 8f, float duration = 1.8f)
    {
        float baseY = obj.Position.Y;
        return Tween
        .Call(baseY, baseY - amplitude, duration * 0.5f, y => obj.Position = new Vector2(obj.Position.X, y), Easing.EaseInOutSine)
        .Call(baseY - amplitude, baseY, duration * 0.5f, y => obj.Position = new Vector2(obj.Position.X, y), Easing.EaseInOutSine);
    }
    /// Pops an object into view from scale 0 with an elastic bounce.
    public static TweenPlanner PopIn(GameObject obj, float duration = 0.5f)
    {
        var targetScale = obj.Scale == Vector2.Zero ? Vector2.One : obj.Scale;
        return Tween.CallFunc(() =>
        {
            obj.Scale = Vector2.Zero;
            obj.BlendColor = Color.FromArgb(255, obj.BlendColor.R, obj.BlendColor.G, obj.BlendColor.B);
        })
        .Call(0f, 1f, duration, t => obj.Scale = targetScale * t, Easing.EaseOutBack);
    }

    /// Shrinks and fades out, optionally marking the GameObject as deleted.
    public static TweenPlanner PopOut(GameObject obj, bool deleteOnComplete = false, float duration = 0.35f)
    {
        var startScale = obj.Scale;
        var startColor = obj.BlendColor;
        var planner = Tween
        .Call(1f, 0f, duration, t =>
        {
            obj.Scale = startScale * t;
            obj.BlendColor = Color.FromArgb((byte)(startColor.A * t), startColor.R, startColor.G, startColor.B);
        }, Easing.EaseInBack);
        if (deleteOnComplete)
            planner.CallFunc(obj.Delete);
        return planner;
    }
}