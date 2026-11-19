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

    // =========================================================================
    // 6. COMBAT & ATTACK MOTIONS
    // =========================================================================

    /// <summary>Pulls back slightly for anticipation, dashes toward target, and returns.</summary>
    public static TweenPlanner LungeAttack(GameObject obj, Vector2 targetOffset, float duration = 0.45f)
    {
        var origin = obj.Position;
        var windup = origin - targetOffset * 0.2f;
        var strike = origin + targetOffset;

        return Tween
            .Call(0f, 1f, duration * 0.25f, t => obj.Position = Vector2.Lerp(origin, windup, t), Easing.EaseOutQuad)
            .Call(0f, 1f, duration * 0.25f, t => obj.Position = Vector2.Lerp(windup, strike, t), Easing.EaseInExpo)
            .Call(0f, 1f, duration * 0.50f, t => obj.Position = Vector2.Lerp(strike, origin, t), Easing.EaseOutCubic);
    }

    /// <summary>Jerks backward sharply along a direction vector and eases back into place.</summary>
    public static TweenPlanner RecoilKick(GameObject obj, Vector2 kickbackOffset, float duration = 0.3f)
    {
        var origin = obj.Position;
        var kicked = origin + kickbackOffset;

        return Tween
            .Call(0f, 1f, duration * 0.2f, t => obj.Position = Vector2.Lerp(origin, kicked, t), Easing.EaseOutQuad)
            .Call(0f, 1f, duration * 0.8f, t => obj.Position = Vector2.Lerp(kicked, origin, t), Easing.EaseOutBack);
    }

    /// <summary>Brief hit-stop freeze, bright cyan-white flash, and quick scale pop.</summary>
    public static TweenPlanner ParryBounce(GameObject obj, float duration = 0.35f)
    {
        var baseScale = obj.Scale;
        var originalColor = obj.BlendColor;
        var parryColor = Color.FromArgb(originalColor.A, 140, 255, 255);

        return Tween
            .CallFunc(() =>
            {
                obj.BlendColor = parryColor;
                obj.Scale = baseScale * 1.25f;
            })
            .Wait(duration * 0.3f)
            .CallFunc(() => obj.BlendColor = originalColor)
            .Call(1.25f, 1f, duration * 0.7f, s => obj.Scale = baseScale * s, Easing.EaseOutElastic);
    }

    /// <summary>Wobbly sway and erratic tilting like a stunned character.</summary>
    public static TweenPlanner DizzyStagger(GameObject obj, float swayDistance = 14f, float tiltDegrees = 18f, float duration = 1.2f)
    {
        var origin = obj.Position;
        float baseRot = obj.Rotation;
        float rad = float.DegreesToRadians(tiltDegrees);
        float quarter = duration / 4f;

        return Tween
            .Call(0f, 1f, quarter, t =>
            {
                obj.Position = origin + new Vector2(-swayDistance * t, -4f * t);
                obj.Rotation = baseRot - rad * t;
            }, Easing.EaseInOutSine)
            .Call(0f, 1f, quarter, t =>
            {
                obj.Position = origin + new Vector2(float.Lerp(-swayDistance, swayDistance, t), -4f * (1f - t));
                obj.Rotation = float.Lerp(baseRot - rad, baseRot + rad, t);
            }, Easing.EaseInOutSine)
            .Call(0f, 1f, quarter, t =>
            {
                obj.Position = origin + new Vector2(float.Lerp(swayDistance, -swayDistance * 0.5f, t), 3f * t);
                obj.Rotation = float.Lerp(baseRot + rad, baseRot - rad * 0.5f, t);
            }, Easing.EaseInOutSine)
            .Call(0f, 1f, quarter, t =>
            {
                obj.Position = Vector2.Lerp(origin + new Vector2(-swayDistance * 0.5f, 3f), origin, t);
                obj.Rotation = float.Lerp(baseRot - rad * 0.5f, baseRot, t);
            }, Easing.EaseOutQuad);
    }

    // =========================================================================
    // 7. COLLECTIBLE, COIN & REWARD JUICE
    // =========================================================================

    /// <summary>Fakes a 3D coin or card spin by flipping Scale.X from 1 to -1 and back.</summary>
    public static TweenPlanner CoinSpin(GameObject obj, float duration = 0.6f)
    {
        var baseScale = obj.Scale;

        return Tween
            .Call(1f, -1f, duration * 0.5f, sx => obj.Scale = new Vector2(baseScale.X * sx, baseScale.Y), Easing.EaseInOutSine)
            .Call(-1f, 1f, duration * 0.5f, sx => obj.Scale = new Vector2(baseScale.X * sx, baseScale.Y), Easing.EaseInOutSine);
    }

    /// <summary>Tugs slightly backward before accelerating rapidly into a target point.</summary>
    public static TweenPlanner MagnetPull(GameObject obj, Vector2 targetPos, float duration = 0.55f)
    {
        var start = obj.Position;
        var dir = Vector2.Normalize(targetPos - start);
        if (float.IsNaN(dir.X)) dir = Vector2.Zero;
        var tugBack = start - dir * 18f;

        return Tween
            .Call(0f, 1f, duration * 0.28f, t => obj.Position = Vector2.Lerp(start, tugBack, t), Easing.EaseOutQuad)
            .Call(0f, 1f, duration * 0.72f, t => obj.Position = Vector2.Lerp(tugBack, targetPos, t), Easing.EaseInBack);
    }

    /// <summary>Launches upward while spinning 360 degrees, then lands with a bounce.</summary>
    public static TweenPlanner TreasurePop(GameObject obj, float height = 65f, float duration = 0.85f)
    {
        float groundY = obj.Position.Y;
        float startRot = obj.Rotation;
        float fullSpin = float.DegreesToRadians(360f);

        return Tween
            .Call(0f, 1f, duration * 0.45f, t =>
            {
                obj.Position = new Vector2(obj.Position.X, groundY - height * t);
                obj.Rotation = startRot + fullSpin * t;
            }, Easing.EaseOutQuad)
            .Call(0f, 1f, duration * 0.55f, t =>
                obj.Position = new Vector2(obj.Position.X, (groundY - height) + height * t), Easing.EaseOutBounce);
    }

    /// <summary>Expands with a golden flash, holds at peak scale, then settles smoothly.</summary>
    public static TweenPlanner LevelUpBurst(GameObject obj, float peakMultiplier = 1.4f, float duration = 0.8f)
    {
        var baseScale = obj.Scale;
        var originalColor = obj.BlendColor;
        var goldColor = Color.FromArgb(originalColor.A, 255, 220, 80);

        return Tween
            .CallFunc(() => obj.BlendColor = goldColor)
            .Call(1f, peakMultiplier, duration * 0.35f, s => obj.Scale = baseScale * s, Easing.EaseOutBack)
            .Wait(duration * 0.25f)
            .CallFunc(() => obj.BlendColor = originalColor)
            .Call(peakMultiplier, 1f, duration * 0.40f, s => obj.Scale = baseScale * s, Easing.EaseInOutSine);
    }

    // =========================================================================
    // 8. UI, BUTTONS & ATTENTION GRABBERS
    // =========================================================================

    /// <summary>Squishes inward like a button click and springs back out.</summary>
    public static TweenPlanner ButtonPress(GameObject obj, float pressScale = 0.86f, float duration = 0.25f)
    {
        var baseScale = obj.Scale;

        return Tween
            .Call(1f, pressScale, duration * 0.35f, s => obj.Scale = baseScale * s, Easing.EaseOutQuad)
            .Call(pressScale, 1f, duration * 0.65f, s => obj.Scale = baseScale * s, Easing.EaseOutBack);
    }

    /// <summary>Rapidly rings left and right like an alarm bell or notification icon.</summary>
    public static TweenPlanner AttentionRing(GameObject obj, float degrees = 20f, float duration = 0.65f)
    {
        float baseRot = obj.Rotation;
        float rad = float.DegreesToRadians(degrees);
        float step = duration / 6f;

        return Tween
            .Call(baseRot, baseRot + rad, step, r => obj.Rotation = r, Easing.EaseOutSine)
            .Call(baseRot + rad, baseRot - rad, step, r => obj.Rotation = r, Easing.EaseInOutSine)
            .Call(baseRot - rad, baseRot + rad * 0.8f, step, r => obj.Rotation = r, Easing.EaseInOutSine)
            .Call(baseRot + rad * 0.8f, baseRot - rad * 0.8f, step, r => obj.Rotation = r, Easing.EaseInOutSine)
            .Call(baseRot - rad * 0.8f, baseRot + rad * 0.4f, step, r => obj.Rotation = r, Easing.EaseInOutSine)
            .Call(baseRot + rad * 0.4f, baseRot, step, r => obj.Rotation = r, Easing.EaseOutQuad);
    }

    /// <summary>Stretches wide horizontally while pulling slightly, then snaps back like rubber.</summary>
    public static TweenPlanner RubberSnap(GameObject obj, float pullX = 24f, float duration = 0.5f)
    {
        float startX = obj.Position.X;
        var baseScale = obj.Scale;

        return Tween
            .Call(0f, 1f, duration * 0.35f, t =>
            {
                obj.Position = new Vector2(startX - pullX * t, obj.Position.Y);
                obj.Scale = new Vector2(baseScale.X * (1f + 0.35f * t), baseScale.Y * (1f - 0.2f * t));
            }, Easing.EaseOutQuad)
            .Call(1f, 0f, duration * 0.65f, t =>
            {
                obj.Position = new Vector2(startX - pullX * t, obj.Position.Y);
                obj.Scale = new Vector2(baseScale.X * (1f + 0.35f * t), baseScale.Y * (1f - 0.2f * t));
            }, Easing.EaseOutElastic);
    }

    /// <summary>Drops in from slightly above while fading alpha from 0 to 255.</summary>
    public static TweenPlanner TypewriterPop(GameObject obj, float dropDistance = 14f, float duration = 0.3f)
    {
        var targetPos = obj.Position;
        var startPos = new Vector2(targetPos.X, targetPos.Y - dropDistance);
        var baseColor = obj.BlendColor;

        return Tween
            .CallFunc(() =>
            {
                obj.Position = startPos;
                obj.BlendColor = Color.FromArgb(0, baseColor.R, baseColor.G, baseColor.B);
            })
            .Call(0f, 1f, duration, t =>
            {
                obj.Position = Vector2.Lerp(startPos, targetPos, t);
                obj.BlendColor = Color.FromArgb((byte)(baseColor.A * t), baseColor.R, baseColor.G, baseColor.B);
            }, Easing.EaseOutBack);
    }

    // =========================================================================
    // 9. DRAMATIC ENTRANCES & EXITS
    // =========================================================================
    /// Falls fast from high above, slams with a wide squash, and rebounds.
    public static TweenPlanner DropSlam(GameObject obj, float dropHeight = 160f, float duration = 0.65f)
    {
        var groundPos = obj.Position;
        var skyPos = new Vector2(groundPos.X, groundPos.Y - dropHeight);
        var baseScale = obj.Scale;
        return Tween
            .CallFunc(() => obj.Position = skyPos)
            .Call(0f, 1f, duration * 0.45f, t => obj.Position = Vector2.Lerp(skyPos, groundPos, t), Easing.EaseInQuad)
            .Call(0f, 0.35f, duration * 0.20f, a =>
        obj.Scale = new Vector2(baseScale.X * (1f + a), baseScale.Y * (1f - a)), Easing.EaseOutQuad)
            .Call(0.35f, 0f, duration * 0.35f, a =>
        obj.Scale = new Vector2(baseScale.X * (1f + a), baseScale.Y * (1f - a)), Easing.EaseOutElastic);
    }
    /// Spins faster while shrinking to 0 and tinting violet into a portal.
    public static TweenPlanner PortalSwirl(GameObject obj, bool deleteOnComplete = false, float duration = 0.85f)
    {
        var startScale = obj.Scale;
        float startRot = obj.Rotation;
        var startColor = obj.BlendColor;
        float totalSpin = float.DegreesToRadians(720f);
        var planner = Tween.Call(0f, 1f, duration, t =>
        {
            obj.Scale = startScale * (1f - t);
            obj.Rotation = startRot + totalSpin * t;
            obj.BlendColor = Color.FromArgb(
                (byte)(startColor.A * (1f - t)),
                (byte)float.Lerp(startColor.R, 170f, t),
                (byte)float.Lerp(startColor.G, 70f, t),
                255
            );
        }, Easing.EaseInExpo);
        if (deleteOnComplete)
            planner.CallFunc(obj.Delete);
        return planner;
    }
    /// Floats slowly upward while tinting pale blue and fading alpha to 0.
    public static TweenPlanner GhostFadeRise(GameObject obj, float riseDistance = 55f, bool deleteOnComplete = false, float duration = 1.1f)
    {
        var startPos = obj.Position;
        var endPos = new Vector2(startPos.X, startPos.Y - riseDistance);
        var startColor = obj.BlendColor;
        var planner = Tween.Call(0f, 1f, duration, t =>
        {
            obj.Position = Vector2.Lerp(startPos, endPos, t);
            obj.BlendColor = Color.FromArgb(
                (byte)(startColor.A * (1f - t)),
                (byte)float.Lerp(startColor.R, 160f, t),
                (byte)float.Lerp(startColor.G, 220f, t),
                255
            );
        }, Easing.EaseOutSine);
        if (deleteOnComplete)
            planner.CallFunc(obj.Delete);
        return planner;
    }
    /// Unfolds horizontally from Scale.X = 0 to full width like flipping a card.
    public static TweenPlanner FlipReveal(GameObject obj, float duration = 0.45f)
    {
        var targetScale = obj.Scale == Vector2.Zero ? Vector2.One : obj.Scale;
        return Tween
        .CallFunc(() => obj.Scale = new Vector2(0f, targetScale.Y))
        .Call(0f, 1f, duration, t => obj.Scale = new Vector2(targetScale.X * t, targetScale.Y), Easing.EaseOutBack);
    }
    // =========================================================================
    // 10. ENVIRONMENT, CREATURES & PHYSICS
    // =========================================================================
    /// Swings smoothly back and forth like a hanging lantern or sign.
    public static TweenPlanner PendulumSwing(GameObject obj, float maxDegrees = 25f, float duration = 1.6f)
    {
        float centerRot = obj.Rotation;
        float rad = float.DegreesToRadians(maxDegrees);
        return Tween
            .Call(centerRot, centerRot + rad, duration * 0.25f, r => obj.Rotation = r, Easing.EaseOutSine)
            .Call(centerRot + rad, centerRot - rad, duration * 0.50f, r => obj.Rotation = r, Easing.EaseInOutSine)
            .Call(centerRot - rad, centerRot, duration * 0.25f, r => obj.Rotation = r, Easing.EaseInSine);
    }
    /// Stretches forward, arches upward, and settles one step ahead like a slime.
    public static TweenPlanner SlimeCrawl(GameObject obj, float stepX = 35f, float duration = 0.7f)
    {
        var startPos = obj.Position;
        var baseScale = obj.Scale;
        return Tween
        // Stretch forward
        .Call(0f, 1f, duration * 0.35f, t =>
        {
            obj.Position = new Vector2(startPos.X + stepX * 0.4f * t, startPos.Y);
            obj.Scale = new Vector2(baseScale.X * (1f + 0.3f * t), baseScale.Y * (1f - 0.2f * t));
        }, Easing.EaseOutQuad)
        // Arch up and pull tail forward
        .Call(0f, 1f, duration * 0.35f, t =>
        {
            obj.Position = new Vector2(startPos.X + stepX * (0.4f + 0.6f * t), startPos.Y);
            obj.Scale = new Vector2(baseScale.X * (1.3f - 0.45f * t), baseScale.Y * (0.8f + 0.45f * t));
        }, Easing.EaseInOutSine)
        // Settle back to normal proportions
        .Call(0f, 1f, duration * 0.30f, t =>
        {
            obj.Scale = Vector2.Lerp(new Vector2(baseScale.X * 0.85f, baseScale.Y * 1.25f), baseScale, t);
        }, Easing.EaseOutBounce);
    }
    /// High-frequency micro-shakes in X and Y that gradually decay to zero.
    public static TweenPlanner EarthquakeRumble(GameObject obj, float intensity = 10f, float duration = 0.8f)
    {
        var origin = obj.Position;
        int steps = 10;
        float stepTime = duration / steps;
        var planner = Tween.CallFunc(() => { });
        for (int i = 0; i < steps; i++)
        {
            float decay = 1f - (i / (float)steps);
            float offsetX = (i % 2 == 0 ? 1f : -1f) * intensity * decay;
            float offsetY = ((i % 3) - 1) * intensity * 0.6f * decay;
            var target = origin + new Vector2(offsetX, offsetY);
            var current = obj.Position;
            planner.Call(0f, 1f, stepTime, t => obj.Position = Vector2.Lerp(obj.Position, target, t), Easing.Linear);
        }
        return planner.CallFunc(() => obj.Position = origin);
    }
    /// Two quick consecutive heartbeat pulses (lub-dub) followed by a rest.
    public static TweenPlanner HeartbeatDouble(GameObject obj, float beatScale = 1.22f, float duration = 0.9f)
    {
        var baseScale = obj.Scale;
        var bigBeat = baseScale * beatScale;
        var smallBeat = baseScale * (1f + (beatScale - 1f) * 0.65f);
        return Tween
            // First beat (Lub)
            .Call(0f, 1f, duration * 0.14f, t => obj.Scale = Vector2.Lerp(baseScale, bigBeat, t), Easing.EaseOutQuad)
            .Call(1f, 0f, duration * 0.16f, t => obj.Scale = Vector2.Lerp(baseScale, bigBeat, t), Easing.EaseInQuad)
            // Second beat (Dub)
            .Call(0f, 1f, duration * 0.12f, t => obj.Scale = Vector2.Lerp(baseScale, smallBeat, t), Easing.EaseOutQuad)
            .Call(1f, 0f, duration * 0.18f, t => obj.Scale = Vector2.Lerp(baseScale, smallBeat, t), Easing.EaseOutSine)
            // Rest pause
            .Wait(duration * 0.40f);
    }
}