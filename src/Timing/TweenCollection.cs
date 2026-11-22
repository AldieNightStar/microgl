using System.Drawing;
using System.Numerics;

namespace microgl;

public static class TweenCollection
{
    public static TweenPlanner DamageShake(GameObject obj, float intensity = 8f, float duration = 0.35f)
    {
        float originX = obj.Position.X;
        float step = duration / 6f;

        return Tween.For(obj)
            .Call(originX, originX - intensity, step, x => _setX(obj, x), Easing.EaseOutQuad)
            .Call(originX - intensity, originX + intensity, step, x => _setX(obj, x), Easing.EaseInOutQuad)
            .Call(originX + intensity, originX - intensity * 0.6f, step, x => _setX(obj, x), Easing.EaseInOutQuad)
            .Call(originX - intensity * 0.6f, originX + intensity * 0.6f, step, x => _setX(obj, x), Easing.EaseInOutQuad)
            .Call(originX + intensity * 0.6f, originX - intensity * 0.25f, step, x => _setX(obj, x), Easing.EaseInOutQuad)
            .Call(originX - intensity * 0.25f, originX, step, x => _setX(obj, x), Easing.EaseOutQuad);
    }

    public static TweenPlanner DamageFlash(GameObject obj, float duration = 0.3f)
    {
        var original = obj.BlendColor;
        var hitColor = Color.FromArgb(original.A, 255, 60, 60);
        float quarter = duration / 4f;

        return Tween.For(obj)
            .CallFunc(() => obj.BlendColor = hitColor)
            .Wait(quarter)
            .CallFunc(() => obj.BlendColor = original)
            .Wait(quarter)
            .CallFunc(() => obj.BlendColor = hitColor)
            .Wait(quarter)
            .CallFunc(() => obj.BlendColor = original)
            .Wait(quarter);
    }

    public static TweenPlanner Knockback(GameObject obj, Vector2 impulse, float duration = 0.4f)
    {
        var start = obj.Position;
        var peak = start + impulse;

        return Tween.For(obj)
            .Call(0f, 1f, duration * 0.3f, t => obj.Position = Vector2.Lerp(start, peak, t), Easing.EaseOutCubic)
            .Call(0f, 1f, duration * 0.7f, t => obj.Position = Vector2.Lerp(peak, start, t), Easing.EaseInOutQuad);
    }

    public static TweenPlanner Squish(GameObject obj, float amount = 0.3f, float duration = 0.45f)
    {
        var baseScale = obj.Scale;

        return Tween.For(obj)
            .Call(0f, amount, duration * 0.35f, a => _setSquash(obj, baseScale, a), Easing.EaseOutQuad)
            .Call(amount, 0f, duration * 0.65f, a => _setSquash(obj, baseScale, a), Easing.EaseOutElastic);
    }

    public static TweenPlanner RiseFromSqueezedJelly(GameObject obj, float startSquish = 0.55f, float duration = 0.85f)
    {
        var target = obj.Scale;

        return Tween.For(obj)
            .CallFunc(() => _setSquash(obj, target, startSquish))
            .Call(startSquish, 0f, duration, a => _setSquash(obj, target, a), Easing.EaseOutElastic);
    }

    public static TweenPlanner StretchVertical(GameObject obj, float amount = 0.35f, float duration = 0.45f)
    {
        var baseScale = obj.Scale;

        return Tween.For(obj)
            .Call(0f, amount, duration * 0.35f, a => _setStretch(obj, baseScale, a), Easing.EaseOutQuad)
            .Call(amount, 0f, duration * 0.65f, a => _setStretch(obj, baseScale, a), Easing.EaseOutElastic);
    }

    public static TweenPlanner JellyWobble(GameObject obj, float intensity = 0.25f, float duration = 0.7f)
    {
        var baseScale = obj.Scale;

        return Tween.For(obj)
            .Call(0f, intensity, duration * 0.25f, a => _setSquash(obj, baseScale, a), Easing.EaseOutQuad)
            .Call(intensity, -intensity * 0.6f, duration * 0.35f, a => _setSquash(obj, baseScale, a), Easing.EaseInOutSine)
            .Call(-intensity * 0.6f, 0f, duration * 0.4f, a => _setSquash(obj, baseScale, a), Easing.EaseOutElastic);
    }

    public static TweenPlanner JumpInPlace(GameObject obj, float height = 45f, float duration = 0.55f)
    {
        float groundY = obj.Position.Y;

        return Tween.For(obj)
            .Call(groundY, groundY - height, duration * 0.48f, y => _setY(obj, y), Easing.EaseOutQuad)
            .Call(groundY - height, groundY, duration * 0.52f, y => _setY(obj, y), Easing.EaseInQuad);
    }

    public static TweenPlanner CartoonJump(GameObject obj, float height = 55f, float duration = 0.75f)
    {
        float groundY = obj.Position.Y;
        var baseScale = obj.Scale;

        return Tween.For(obj)
            .Call(0f, 0.22f, duration * 0.16f, a => _setSquash(obj, baseScale, a), Easing.EaseOutQuad)
            .Call(0f, 1f, duration * 0.34f, t =>
            {
                _setY(obj, groundY - height * t);
                obj.Scale = new Vector2(baseScale.X * 0.88f, baseScale.Y * 1.15f);
            }, Easing.EaseOutQuad)
            .Call(0f, 1f, duration * 0.32f, t => _setY(obj, (groundY - height) + height * t), Easing.EaseInQuad)
            .Call(0.25f, 0f, duration * 0.18f, a => _setSquash(obj, baseScale, a), Easing.EaseOutBack);
    }

    public static TweenPlanner BumpUp(GameObject obj, float distance = 18f, float duration = 0.25f)
    {
        float startY = obj.Position.Y;

        return Tween.For(obj)
            .Call(startY, startY - distance, duration * 0.4f, y => _setY(obj, y), Easing.EaseOutQuad)
            .Call(startY - distance, startY, duration * 0.6f, y => _setY(obj, y), Easing.EaseOutBounce);
    }

    public static TweenPlanner BumpDirection(GameObject obj, Vector2 offset, float duration = 0.25f)
    {
        var origin = obj.Position;
        var target = origin + offset;

        return Tween.For(obj)
            .Call(0f, 1f, duration * 0.4f, t => obj.Position = Vector2.Lerp(origin, target, t), Easing.EaseOutQuad)
            .Call(1f, 0f, duration * 0.6f, t => obj.Position = Vector2.Lerp(origin, target, t), Easing.EaseOutBack);
    }

    public static TweenPlanner Wiggle(GameObject obj, float degrees = 15f, float duration = 0.5f)
    {
        float baseRotation = obj.Rotation;
        float radians = float.DegreesToRadians(degrees);
        float quarter = duration / 4f;

        return Tween.For(obj)
            .Call(baseRotation, baseRotation + radians, quarter, r => obj.Rotation = r, Easing.EaseOutSine)
            .Call(baseRotation + radians, baseRotation - radians, quarter, r => obj.Rotation = r, Easing.EaseInOutSine)
            .Call(baseRotation - radians, baseRotation + radians * 0.5f, quarter, r => obj.Rotation = r, Easing.EaseInOutSine)
            .Call(baseRotation + radians * 0.5f, baseRotation, quarter, r => obj.Rotation = r, Easing.EaseOutQuad);
    }

    public static TweenPlanner ShakeNo(GameObject obj, float distance = 12f, float duration = 0.4f)
    {
        float startX = obj.Position.X;
        float step = duration / 4f;

        return Tween.For(obj)
            .Call(startX, startX - distance, step, x => _setX(obj, x), Easing.EaseOutQuad)
            .Call(startX - distance, startX + distance, step, x => _setX(obj, x), Easing.EaseInOutQuad)
            .Call(startX + distance, startX - distance * 0.5f, step, x => _setX(obj, x), Easing.EaseInOutQuad)
            .Call(startX - distance * 0.5f, startX, step, x => _setX(obj, x), Easing.EaseOutQuad);
    }

    public static TweenPlanner NodYes(GameObject obj, float distance = 10f, float duration = 0.4f)
    {
        float startY = obj.Position.Y;
        float step = duration / 4f;

        return Tween.For(obj)
            .Call(startY, startY + distance, step, y => _setY(obj, y), Easing.EaseOutQuad)
            .Call(startY + distance, startY - distance * 0.4f, step, y => _setY(obj, y), Easing.EaseInOutQuad)
            .Call(startY - distance * 0.4f, startY + distance * 0.6f, step, y => _setY(obj, y), Easing.EaseInOutQuad)
            .Call(startY + distance * 0.6f, startY, step, y => _setY(obj, y), Easing.EaseOutQuad);
    }

    public static TweenPlanner Spin360(GameObject obj, bool clockwise = true, float duration = 0.75f)
    {
        float start = obj.Rotation;
        float delta = float.DegreesToRadians(clockwise ? 360f : -360f);

        return Tween.For(obj).Call(start, start + delta, duration, r => obj.Rotation = r, Easing.EaseInOutBack);
    }

    public static TweenPlanner Pulse(GameObject obj, float scaleMultiplier = 1.2f, float duration = 0.4f)
    {
        var baseScale = obj.Scale;
        var peakScale = baseScale * scaleMultiplier;

        return Tween.For(obj)
            .Call(0f, 1f, duration * 0.45f, t => obj.Scale = Vector2.Lerp(baseScale, peakScale, t), Easing.EaseOutQuad)
            .Call(1f, 0f, duration * 0.55f, t => obj.Scale = Vector2.Lerp(baseScale, peakScale, t), Easing.EaseInOutSine);
    }

    public static TweenPlanner Breathe(GameObject obj, float amount = 0.06f, float duration = 1.6f)
    {
        var baseScale = obj.Scale;
        var expanded = baseScale * (1f + amount);

        return Tween.For(obj)
            .Call(0f, 1f, duration * 0.5f, t => obj.Scale = Vector2.Lerp(baseScale, expanded, t), Easing.EaseInOutSine)
            .Call(1f, 0f, duration * 0.5f, t => obj.Scale = Vector2.Lerp(baseScale, expanded, t), Easing.EaseInOutSine);
    }

    public static TweenPlanner HoverFloat(GameObject obj, float amplitude = 8f, float duration = 1.8f)
    {
        float baseY = obj.Position.Y;

        return Tween.For(obj)
            .Call(baseY, baseY - amplitude, duration * 0.5f, y => _setY(obj, y), Easing.EaseInOutSine)
            .Call(baseY - amplitude, baseY, duration * 0.5f, y => _setY(obj, y), Easing.EaseInOutSine);
    }

    public static TweenPlanner PopIn(GameObject obj, float duration = 0.5f)
    {
        var targetScale = obj.Scale == Vector2.Zero ? Vector2.One : obj.Scale;

        return Tween.For(obj)
            .CallFunc(() =>
            {
                obj.Scale = Vector2.Zero;
                obj.BlendColor = Color.FromArgb(255, obj.BlendColor.R, obj.BlendColor.G, obj.BlendColor.B);
            })
            .Call(0f, 1f, duration, t => obj.Scale = targetScale * t, Easing.EaseOutBack);
    }

    public static TweenPlanner PopOut(GameObject obj, bool deleteOnComplete = false, float duration = 0.35f)
    {
        var startScale = obj.Scale;
        var startColor = obj.BlendColor;

        var planner = Tween.For(obj)
            .Call(1f, 0f, duration, t =>
            {
                obj.Scale = startScale * t;
                obj.BlendColor = Color.FromArgb((byte)(startColor.A * t), startColor.R, startColor.G, startColor.B);
            }, Easing.EaseInBack);

        return _appendDeleteIf(planner, obj, deleteOnComplete);
    }

    public static TweenPlanner LungeAttack(GameObject obj, Vector2 targetOffset, float duration = 0.45f)
    {
        var origin = obj.Position;
        var windup = origin - targetOffset * 0.2f;
        var strike = origin + targetOffset;

        return Tween.For(obj)
            .Call(0f, 1f, duration * 0.25f, t => obj.Position = Vector2.Lerp(origin, windup, t), Easing.EaseOutQuad)
            .Call(0f, 1f, duration * 0.25f, t => obj.Position = Vector2.Lerp(windup, strike, t), Easing.EaseInExpo)
            .Call(0f, 1f, duration * 0.50f, t => obj.Position = Vector2.Lerp(strike, origin, t), Easing.EaseOutCubic);
    }

    public static TweenPlanner RecoilKick(GameObject obj, Vector2 kickbackOffset, float duration = 0.3f)
    {
        var origin = obj.Position;
        var kicked = origin + kickbackOffset;

        return Tween.For(obj)
            .Call(0f, 1f, duration * 0.2f, t => obj.Position = Vector2.Lerp(origin, kicked, t), Easing.EaseOutQuad)
            .Call(0f, 1f, duration * 0.8f, t => obj.Position = Vector2.Lerp(kicked, origin, t), Easing.EaseOutBack);
    }

    public static TweenPlanner ParryBounce(GameObject obj, float duration = 0.35f)
    {
        var baseScale = obj.Scale;
        var originalColor = obj.BlendColor;
        var parryColor = Color.FromArgb(originalColor.A, 140, 255, 255);

        return Tween.For(obj)
            .CallFunc(() =>
            {
                obj.BlendColor = parryColor;
                obj.Scale = baseScale * 1.25f;
            })
            .Wait(duration * 0.3f)
            .CallFunc(() => obj.BlendColor = originalColor)
            .Call(1.25f, 1f, duration * 0.7f, s => obj.Scale = baseScale * s, Easing.EaseOutElastic);
    }

    public static TweenPlanner DizzyStagger(GameObject obj, float swayDistance = 14f, float tiltDegrees = 18f, float duration = 1.2f)
    {
        var origin = obj.Position;
        float baseRotation = obj.Rotation;
        float radians = float.DegreesToRadians(tiltDegrees);
        float quarter = duration / 4f;

        return Tween.For(obj)
            .Call(0f, 1f, quarter, t =>
            {
                obj.Position = origin + new Vector2(-swayDistance * t, -4f * t);
                obj.Rotation = baseRotation - radians * t;
            }, Easing.EaseInOutSine)
            .Call(0f, 1f, quarter, t =>
            {
                obj.Position = origin + new Vector2(float.Lerp(-swayDistance, swayDistance, t), -4f * (1f - t));
                obj.Rotation = float.Lerp(baseRotation - radians, baseRotation + radians, t);
            }, Easing.EaseInOutSine)
            .Call(0f, 1f, quarter, t =>
            {
                obj.Position = origin + new Vector2(float.Lerp(swayDistance, -swayDistance * 0.5f, t), 3f * t);
                obj.Rotation = float.Lerp(baseRotation + radians, baseRotation - radians * 0.5f, t);
            }, Easing.EaseInOutSine)
            .Call(0f, 1f, quarter, t =>
            {
                obj.Position = Vector2.Lerp(origin + new Vector2(-swayDistance * 0.5f, 3f), origin, t);
                obj.Rotation = float.Lerp(baseRotation - radians * 0.5f, baseRotation, t);
            }, Easing.EaseOutQuad);
    }

    public static TweenPlanner CoinSpin(GameObject obj, float duration = 0.6f)
    {
        var baseScale = obj.Scale;

        return Tween.For(obj)
            .Call(1f, -1f, duration * 0.5f, sx => obj.Scale = new Vector2(baseScale.X * sx, baseScale.Y), Easing.EaseInOutSine)
            .Call(-1f, 1f, duration * 0.5f, sx => obj.Scale = new Vector2(baseScale.X * sx, baseScale.Y), Easing.EaseInOutSine);
    }

    public static TweenPlanner MagnetPull(GameObject obj, Vector2 targetPos, float duration = 0.55f)
    {
        var start = obj.Position;
        var direction = Vector2.Normalize(targetPos - start);
        if (float.IsNaN(direction.X)) direction = Vector2.Zero;
        var tugBack = start - direction * 18f;

        return Tween.For(obj)
            .Call(0f, 1f, duration * 0.28f, t => obj.Position = Vector2.Lerp(start, tugBack, t), Easing.EaseOutQuad)
            .Call(0f, 1f, duration * 0.72f, t => obj.Position = Vector2.Lerp(tugBack, targetPos, t), Easing.EaseInBack);
    }

    public static TweenPlanner TreasurePop(GameObject obj, float height = 65f, float duration = 0.85f)
    {
        float groundY = obj.Position.Y;
        float startRotation = obj.Rotation;
        float fullSpin = float.DegreesToRadians(360f);

        return Tween.For(obj)
            .Call(0f, 1f, duration * 0.45f, t =>
            {
                _setY(obj, groundY - height * t);
                obj.Rotation = startRotation + fullSpin * t;
            }, Easing.EaseOutQuad)
            .Call(0f, 1f, duration * 0.55f, t => _setY(obj, (groundY - height) + height * t), Easing.EaseOutBounce);
    }

    public static TweenPlanner LevelUpBurst(GameObject obj, float peakMultiplier = 1.4f, float duration = 0.8f)
    {
        var baseScale = obj.Scale;
        var originalColor = obj.BlendColor;
        var goldColor = Color.FromArgb(originalColor.A, 255, 220, 80);

        return Tween.For(obj)
            .CallFunc(() => obj.BlendColor = goldColor)
            .Call(1f, peakMultiplier, duration * 0.35f, s => obj.Scale = baseScale * s, Easing.EaseOutBack)
            .Wait(duration * 0.25f)
            .CallFunc(() => obj.BlendColor = originalColor)
            .Call(peakMultiplier, 1f, duration * 0.40f, s => obj.Scale = baseScale * s, Easing.EaseInOutSine);
    }

    public static TweenPlanner ButtonPress(GameObject obj, float pressScale = 0.86f, float duration = 0.25f)
    {
        var baseScale = obj.Scale;

        return Tween.For(obj)
            .Call(1f, pressScale, duration * 0.35f, s => obj.Scale = baseScale * s, Easing.EaseOutQuad)
            .Call(pressScale, 1f, duration * 0.65f, s => obj.Scale = baseScale * s, Easing.EaseOutBack);
    }

    public static TweenPlanner AttentionRing(GameObject obj, float degrees = 20f, float duration = 0.65f)
    {
        float baseRotation = obj.Rotation;
        float radians = float.DegreesToRadians(degrees);
        float step = duration / 6f;

        return Tween.For(obj)
            .Call(baseRotation, baseRotation + radians, step, r => obj.Rotation = r, Easing.EaseOutSine)
            .Call(baseRotation + radians, baseRotation - radians, step, r => obj.Rotation = r, Easing.EaseInOutSine)
            .Call(baseRotation - radians, baseRotation + radians * 0.8f, step, r => obj.Rotation = r, Easing.EaseInOutSine)
            .Call(baseRotation + radians * 0.8f, baseRotation - radians * 0.8f, step, r => obj.Rotation = r, Easing.EaseInOutSine)
            .Call(baseRotation - radians * 0.8f, baseRotation + radians * 0.4f, step, r => obj.Rotation = r, Easing.EaseInOutSine)
            .Call(baseRotation + radians * 0.4f, baseRotation, step, r => obj.Rotation = r, Easing.EaseOutQuad);
    }

    public static TweenPlanner RubberSnap(GameObject obj, float pullX = 24f, float duration = 0.5f)
    {
        float startX = obj.Position.X;
        var baseScale = obj.Scale;

        return Tween.For(obj)
            .Call(0f, 1f, duration * 0.35f, t => _setPullAndStretch(obj, startX, baseScale, pullX, t), Easing.EaseOutQuad)
            .Call(1f, 0f, duration * 0.65f, t => _setPullAndStretch(obj, startX, baseScale, pullX, t), Easing.EaseOutElastic);
    }

    public static TweenPlanner TypewriterPop(GameObject obj, float dropDistance = 14f, float duration = 0.3f)
    {
        var targetPos = obj.Position;
        var startPos = new Vector2(targetPos.X, targetPos.Y - dropDistance);
        var baseColor = obj.BlendColor;

        return Tween.For(obj)
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

    public static TweenPlanner DropSlam(GameObject obj, float dropHeight = 160f, float duration = 0.65f)
    {
        var groundPos = obj.Position;
        var skyPos = new Vector2(groundPos.X, groundPos.Y - dropHeight);
        var baseScale = obj.Scale;

        return Tween.For(obj)
            .CallFunc(() => obj.Position = skyPos)
            .Call(0f, 1f, duration * 0.45f, t => obj.Position = Vector2.Lerp(skyPos, groundPos, t), Easing.EaseInQuad)
            .Call(0f, 0.35f, duration * 0.20f, a => _setSquash(obj, baseScale, a), Easing.EaseOutQuad)
            .Call(0.35f, 0f, duration * 0.35f, a => _setSquash(obj, baseScale, a), Easing.EaseOutElastic);
    }

    public static TweenPlanner PortalSwirl(GameObject obj, bool deleteOnComplete = false, float duration = 0.85f)
    {
        var startScale = obj.Scale;
        float startRotation = obj.Rotation;
        var startColor = obj.BlendColor;
        float totalSpin = float.DegreesToRadians(720f);

        var planner = Tween.For(obj).Call(0f, 1f, duration, t =>
        {
            obj.Scale = startScale * (1f - t);
            obj.Rotation = startRotation + totalSpin * t;
            obj.BlendColor = Color.FromArgb(
                (byte)(startColor.A * (1f - t)),
                (byte)float.Lerp(startColor.R, 170f, t),
                (byte)float.Lerp(startColor.G, 70f, t),
                255
            );
        }, Easing.EaseInExpo);

        return _appendDeleteIf(planner, obj, deleteOnComplete);
    }

    public static TweenPlanner GhostFadeRise(GameObject obj, float riseDistance = 55f, bool deleteOnComplete = false, float duration = 1.1f)
    {
        var startPos = obj.Position;
        var endPos = new Vector2(startPos.X, startPos.Y - riseDistance);
        var startColor = obj.BlendColor;

        var planner = Tween.For(obj).Call(0f, 1f, duration, t =>
        {
            obj.Position = Vector2.Lerp(startPos, endPos, t);
            obj.BlendColor = Color.FromArgb(
                (byte)(startColor.A * (1f - t)),
                (byte)float.Lerp(startColor.R, 160f, t),
                (byte)float.Lerp(startColor.G, 220f, t),
                255
            );
        }, Easing.EaseOutSine);

        return _appendDeleteIf(planner, obj, deleteOnComplete);
    }

    public static TweenPlanner FlipReveal(GameObject obj, float duration = 0.45f)
    {
        var targetScale = obj.Scale == Vector2.Zero ? Vector2.One : obj.Scale;

        return Tween.For(obj)
            .CallFunc(() => obj.Scale = new Vector2(0f, targetScale.Y))
            .Call(0f, 1f, duration, t => obj.Scale = new Vector2(targetScale.X * t, targetScale.Y), Easing.EaseOutBack);
    }

    public static TweenPlanner PendulumSwing(GameObject obj, float maxDegrees = 25f, float duration = 1.6f)
    {
        float centerRotation = obj.Rotation;
        float radians = float.DegreesToRadians(maxDegrees);

        return Tween.For(obj)
            .Call(centerRotation, centerRotation + radians, duration * 0.25f, r => obj.Rotation = r, Easing.EaseOutSine)
            .Call(centerRotation + radians, centerRotation - radians, duration * 0.50f, r => obj.Rotation = r, Easing.EaseInOutSine)
            .Call(centerRotation - radians, centerRotation, duration * 0.25f, r => obj.Rotation = r, Easing.EaseInSine);
    }

    public static TweenPlanner SlimeCrawl(GameObject obj, float stepX = 35f, float duration = 0.7f)
    {
        var startPos = obj.Position;
        var baseScale = obj.Scale;

        return Tween.For(obj)
            .Call(0f, 1f, duration * 0.35f, t =>
            {
                obj.Position = new Vector2(startPos.X + stepX * 0.4f * t, startPos.Y);
                obj.Scale = new Vector2(baseScale.X * (1f + 0.3f * t), baseScale.Y * (1f - 0.2f * t));
            }, Easing.EaseOutQuad)
            .Call(0f, 1f, duration * 0.35f, t =>
            {
                obj.Position = new Vector2(startPos.X + stepX * (0.4f + 0.6f * t), startPos.Y);
                obj.Scale = new Vector2(baseScale.X * (1.3f - 0.45f * t), baseScale.Y * (0.8f + 0.45f * t));
            }, Easing.EaseInOutSine)
            .Call(0f, 1f, duration * 0.30f, t =>
                obj.Scale = Vector2.Lerp(new Vector2(baseScale.X * 0.85f, baseScale.Y * 1.25f), baseScale, t),
                Easing.EaseOutBounce);
    }

    public static TweenPlanner EarthquakeRumble(GameObject obj, float intensity = 10f, float duration = 0.8f)
    {
        var origin = obj.Position;
        const int steps = 10;
        float stepDuration = duration / steps;

        var planner = Tween.For(obj).CallFunc(() => { });
        for (int step = 0; step < steps; step++)
        {
            var target = origin + _rumbleOffset(step, steps, intensity);
            planner.Call(0f, 1f, stepDuration, t => obj.Position = Vector2.Lerp(obj.Position, target, t), Easing.Linear);
        }

        return planner.CallFunc(() => obj.Position = origin);
    }

    public static TweenPlanner HeartbeatDouble(GameObject obj, float beatScale = 1.22f, float duration = 0.9f)
    {
        var baseScale = obj.Scale;
        var bigBeat = baseScale * beatScale;
        var smallBeat = baseScale * (1f + (beatScale - 1f) * 0.65f);

        return Tween.For(obj)
            .Call(0f, 1f, duration * 0.14f, t => obj.Scale = Vector2.Lerp(baseScale, bigBeat, t), Easing.EaseOutQuad)
            .Call(1f, 0f, duration * 0.16f, t => obj.Scale = Vector2.Lerp(baseScale, bigBeat, t), Easing.EaseInQuad)
            .Call(0f, 1f, duration * 0.12f, t => obj.Scale = Vector2.Lerp(baseScale, smallBeat, t), Easing.EaseOutQuad)
            .Call(1f, 0f, duration * 0.18f, t => obj.Scale = Vector2.Lerp(baseScale, smallBeat, t), Easing.EaseOutSine)
            .Wait(duration * 0.40f);
    }

    private static void _setX(GameObject obj, float x) => obj.Position = new Vector2(x, obj.Position.Y);

    private static void _setY(GameObject obj, float y) => obj.Position = new Vector2(obj.Position.X, y);

    private static void _setSquash(GameObject obj, Vector2 baseScale, float amount) =>
        obj.Scale = new Vector2(baseScale.X * (1f + amount), baseScale.Y * (1f - amount));

    private static void _setStretch(GameObject obj, Vector2 baseScale, float amount) =>
        obj.Scale = new Vector2(baseScale.X * (1f - amount * 0.6f), baseScale.Y * (1f + amount));

    private static void _setPullAndStretch(GameObject obj, float startX, Vector2 baseScale, float pullX, float t)
    {
        obj.Position = new Vector2(startX - pullX * t, obj.Position.Y);
        obj.Scale = new Vector2(baseScale.X * (1f + 0.35f * t), baseScale.Y * (1f - 0.2f * t));
    }

    private static Vector2 _rumbleOffset(int step, int steps, float intensity)
    {
        float decay = 1f - step / (float)steps;
        float offsetX = (step % 2 == 0 ? 1f : -1f) * intensity * decay;
        float offsetY = ((step % 3) - 1) * intensity * 0.6f * decay;
        return new Vector2(offsetX, offsetY);
    }

    private static TweenPlanner _appendDeleteIf(TweenPlanner planner, GameObject obj, bool deleteOnComplete)
    {
        if (deleteOnComplete)
            planner.CallFunc(() => obj.IsAlive = false);
        return planner;
    }
}
