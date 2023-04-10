using Microsoft.JSInterop;

namespace Timecop.Services
{
    public static class AxExtensions
    {
        public static Task Play(this BlazorElement element, params Func<BlazorElement, Animation>[] actions)
        {
            return Task.WhenAll(actions.Select(e => e.Invoke(element).Play()).ToArray());
        }

        public static Task Play(this BlazorElement element, int delay, params Func<BlazorElement, Animation>[] actions)
        {
            return Task.WhenAll(actions.Select(e => e.Invoke(element).Play(delay)).ToArray());
        }

        public static Animation FadeIn(this BlazorElement element, int duration = 1000) => element
            .New(duration, "ease")
               .Fade(0)
            .Then()
               .Fade(1);

        public static Animation FadeOut(this BlazorElement element, int duration = 1000) => element
            .New(duration, "ease")
               .Fade(1)
            .Then()
               .Fade(0);

        public static Animation FadeOutLeft(this BlazorElement element, int offset = 200, int duration = 1000) => element
            .New(duration, "ease-in")
               .Fade(1)
               .Translate()
            .Then()
               .Fade(0)
               .Translate(-offset);

        public static Animation FadeOutRight(this BlazorElement element, int offset = 200, int duration = 1000) => element
            .New(duration, "ease-in")
                .Fade(1)
                .Translate()
            .Then()
                .Fade(0)
                .Translate(offset);

        public static Animation MoveIn(this BlazorElement element, int duration = 500) => element
            .New(duration, "cubic-bezier(0.035, 1.305, 0.270, 1.000)")
                .Translate(0, 100)
            .Then()
                .Translate(0, 0);

        public static Animation Show(this BlazorElement element) => element
            .New(0, "linear").Fade(1).Translate(0);
    }

    public class BlazorElement
    {
        private readonly string element;

        public string Element => element;

        public IAnimationExtensions AnimationExtensions => animationExtensions;

        private readonly IAnimationExtensions animationExtensions;

        public BlazorElement(IAnimationExtensions animationExtensions)
        {
            this.animationExtensions = animationExtensions;
            this.element = $"ax-{Guid.NewGuid()}";
        }

        public Animation New(int duration = 1000, string easing = "ease") => animationExtensions.New(duration, easing).Element(this.element);

        public static implicit operator string(BlazorElement a) => a.Element;
    }

    public interface IEffect
    {
        public string Property { get; }

        public string Value { get; }
    }

    public class FadeEffect : IEffect
    {
        public FadeEffect(float opacity)
        {
            Opacity = opacity;
        }
        public float Opacity { get; set; }

        public string Property => "opacity";
        public string Value => Opacity.ToString();
    }

    public class ScaleEffect : IEffect
    {
        public ScaleEffect(float factor)
        {
            Factor = factor;
        }
        public float Factor { get; set; }

        public string Property => "transform";
        public string Value => $"scale({Factor})";
    }

    public class TranslateEffect : IEffect
    {
        public TranslateEffect(int x = 0, int y = 0)
        {
            X = x;
            Y = y;
        }

        public string Property => "transform";

        public int X { get; set; }
        public int Y { get; set; }

        public string Value => $"translateX({X}px) translateY({Y}px)";
    }

    public class KeyFrame
    {
        readonly IList<IEffect> effects = new List<IEffect>();

        public IEffect[] Effects => effects.ToArray();

        public KeyFrame AddEffect(IEffect effect)
        {
            effects.Add(effect);
            return this;
        }
    }

    public class Animation
    {
        IAnimationExtensions animationExtensions;

        protected IList<KeyFrame> keyFrames = new List<KeyFrame>();
        protected KeyFrame keyFrame = new KeyFrame();
        protected int duration = 1000;
        protected int delay = 0;
        protected string easing = "linear";
        protected string element;

        public string Id { get; set; }

        public KeyFrame[] KeyFrames => keyFrames.ToArray();

        public int Duration => duration;

        public int DelayValue => delay;

        public string Easing => easing;

        public Animation(IAnimationExtensions animationExtensions)
        {
            this.animationExtensions = animationExtensions;
            keyFrames.Add(keyFrame);
        }

        public Animation(IAnimationExtensions animationExtensions, int duration, string easing)
        {
            this.animationExtensions = animationExtensions;
            keyFrames.Add(keyFrame);

            this.duration = duration;
            this.easing = easing;
        }

        public Animation Fade(float opacity)
        {
            keyFrame.AddEffect(new FadeEffect(opacity));
            return this;
        }

        public Animation Scale(float factor)
        {
            keyFrame.AddEffect(new ScaleEffect(factor));
            return this;
        }

        public Animation Translate(int x = 0, int y = 0)
        {
            keyFrame.AddEffect(new TranslateEffect(x, y));
            return this;
        }

        public Animation Then()
        {
            keyFrame = new KeyFrame();
            keyFrames.Add(keyFrame);
            return this;
        }

        public Animation Element(string element)
        {
            this.element = element;
            return this;
        }

        public Animation For(int duration)
        {
            this.duration = duration;
            return this;
        }

        public Animation Ease(string easing)
        {
            this.easing = easing;
            return this;
        }

        public Animation Delay(int delay)
        {
            this.delay = delay;
            return this;
        }

        public Animation AddDelay(int delay)
        {
            this.delay += delay;
            return this;
        }

        public virtual Task Play(int delay = 0)
        {
            this.delay = delay;
            return animationExtensions.Play(element, this);
        }
    }

    public class AnimationExtensions : IAnimationExtensions
    {
        private readonly IJSRuntime jSRuntime;

        public AnimationExtensions(IJSRuntime jSRuntime)
        {
            this.jSRuntime = jSRuntime;
        }

        public async Task Play(string objectId, Animation animation, int delay = 0)
        {
            animation.Id = await jSRuntime.InvokeAsync<string>("AnimationExtensions.Start", objectId, animation, delay);
        }

        public Animation New(int duration = 1000, string easing = "ease")
        {
            return new Animation(this, duration, easing);
        }
    }

    public interface IAnimationExtensions
    {
        Task Play(string objectId, Animation animationReference, int delay = 0);

        Animation New(int duration = 1000, string easing = "ease");
    }
}