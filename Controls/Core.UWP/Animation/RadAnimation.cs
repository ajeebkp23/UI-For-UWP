﻿using System;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Animation;

namespace Telerik.Core
{
    /// <summary>
    /// Base class for dynamic control animations.
    /// </summary>
    public abstract class RadAnimation
    {
        /// <summary>
        /// The Empty animation does nothing and must be used instead of null if no animation should be played.
        /// </summary>
        public static readonly RadAnimation Empty = new RadEmptyAnimation();

        private EasingFunctionBase easing;

        /// <summary>
        /// Initializes a new instance of the <see cref="RadAnimation"/> class.
        /// </summary>
        protected RadAnimation()
        {
            this.Duration = new Duration(TimeSpan.FromSeconds(.4));
            this.FillBehavior = AnimationFillBehavior.Inherit;
            this.AnimationOrigin = new Point(0.5, 0.5);
        }

        /// <summary>
        /// Occurs when animation has stopped running.
        /// </summary>
        public event EventHandler<AnimationEndedEventArgs> Ended;

        /// <summary>
        /// Gets or sets a value describing the easing function to be used for the animation.
        /// </summary>
        public EasingFunctionBase Easing
        {
            get
            {
                return this.ComposeEasingFunction();
            }

            set
            {
                this.easing = value;
            }
        }

        /// <summary>
        /// Gets or sets the duration of the animation. Defaults to (0:0:.4) - 400 milliseconds.
        /// </summary>
        public virtual Duration Duration { get; set; }

        /// <summary>
        /// Gets or sets an initial delay that will be applied before the animation starts.
        /// </summary>
        public TimeSpan InitialDelay { get; set; }

        /// <summary>
        /// Gets or sets the name of the animation.
        /// </summary>
        /// <remarks>
        ///        <para>
        ///            This property is used by the NamedAnimationSelector to identify the
        ///            correct animation to return.
        ///        </para>
        ///        <para>
        ///            It is not used outside the NamedAnimationSelector.
        ///        </para>
        /// </remarks>
        public string AnimationName { get; set; }

        /// <summary>
        /// Gets or sets the FillBehavior property of the internally created storyboard, associated with this animation.
        /// </summary>
        public AnimationFillBehavior FillBehavior { get; set; }

        /// <summary>
        /// Gets or sets the value for the SpeedRatio of the Storyboard generated by this animation.
        /// </summary>
        public double? SpeedRatio { get; set; }

        /// <summary>
        /// Gets or sets the AutoReverse property of the internally created storyboard associated with this animation.
        /// </summary>
        public bool? AutoReverse { get; set; }

        /// <summary>
        /// Gets or sets the repeat behavior of this RadAnimation instance.
        /// </summary>
        public RepeatBehavior? RepeatBehavior { get; set; }

        /// <summary>
        /// Gets or sets the render transform origin of the animated element.
        /// </summary>
        public virtual Point AnimationOrigin { get; set; }

        /// <summary>
        /// Gets or sets the parent group that owns this animation.
        /// </summary>
        internal RadAnimationGroup Parent { get; set; }

        /// <summary>
        /// Creates a clone animation of this instance.
        /// </summary>
        /// <returns>Returns a clone animation of this instance.</returns>
        public RadAnimation Clone()
        {
            return this.CloneCore();
        }

        /// <summary>
        /// Sets the initial animation values to the provided target element.
        /// </summary>
        /// <param name="target">The target.</param>
        public virtual void ApplyInitialValues(UIElement target)
        {
        }

        /// <summary>
        /// Removes any property modifications, applied to the specified element by this instance.
        /// </summary>
        /// <param name="target">The element which property values are to be cleared.</param>
        /// <remarks>
        /// It is assumed that the element has been previously animated by this animation.
        /// </remarks>
        public virtual void ClearAnimation(UIElement target)
        {
        }

        /// <summary>
        /// Creates a new instance of this animation that is the reverse of this instance.
        /// </summary>
        /// <returns>A new instance of this animation that is the reverse of this instance.</returns>
        public virtual RadAnimation CreateOpposite()
        {
            RadAnimation reversedAnimation = this.Clone();
            reversedAnimation.AnimationName = null;
            return reversedAnimation;
        }

        internal Storyboard CreateStoryboard(UIElement target)
        {
            return this.CreateStoryboardOverride(target);
        }

        /// <summary>
        ///        When overridden in a derived class this method updates the animation
        ///        before it is played.
        /// </summary>
        /// <param name="target">The control for which the animation needs to be updated.</param>
        /// <param name="storyboard">Storyboard that needs to be updated.</param>
        /// <param name="args">A set of arguments used for animation creation.</param>
        /// <remarks>
        ///        <para>
        ///            Currently the method sets the <see cref="SpeedRatio"/> of the storyboard to
        ///            the global <strong>AnimationSpeedRatio</strong> if the local <see cref="SpeedRatio"/> is null.
        ///            If the local <see cref="SpeedRatio"/> value is set, it will be used.
        ///        </para>
        /// </remarks>
        internal void UpdateAnimation(UIElement target, Storyboard storyboard, params object[] args)
        {
            storyboard.BeginTime = this.InitialDelay;
            AnimationContext context = new AnimationContext(target, storyboard, args);
            this.UpdateAnimationOverride(context);
            storyboard.SpeedRatio = this.GetSpeedRatio();
            storyboard.AutoReverse = this.AutoReverse.GetValueOrDefault();
        }

        /// <summary>
        /// Called by the animation manager before an associated storyboard is stopped.
        /// Allows inheritors to store any animated values to be restored later, when the storyboard is already stopped.
        /// </summary>
        /// <param name="info">The info.</param>
        internal void OnStopping(PlayAnimationInfo info)
        {
            if (this.FillBehavior != AnimationFillBehavior.Stop && info.Target != null)
            {
                this.CopyAnimationValues(info);
            }
        }

        /// <summary>
        /// Called by the animation manager after an associated storyboard has been stopped.
        /// Allows inheritors to apply previously stored (if any) animated values.
        /// </summary>
        /// <param name="info">The info.</param>
        internal void OnStopped(PlayAnimationInfo info)
        {
            if (this.FillBehavior != AnimationFillBehavior.Stop && info.Target != null)
            {
                this.ApplyAnimationValues(info);
            }

            this.OnEnded(info);
        }

        internal bool GetAutoReverse()
        {
            if (this.AutoReverse.HasValue)
            {
                return this.AutoReverse.Value;
            }

            if (this.Parent != null && this.Parent.AutoReverse.HasValue)
            {
                return this.Parent.AutoReverse.Value;
            }

            return false;
        }

        internal double GetSpeedRatio()
        {
            if (this.SpeedRatio.HasValue)
            {
                return this.SpeedRatio.Value;
            }

            if (this.Parent != null && this.Parent.SpeedRatio.HasValue)
            {
                return this.Parent.SpeedRatio.Value;
            }

            return RadAnimationManager.SpeedRatio;
        }

        internal FillBehavior GetFillBehavior()
        {
            if (this.FillBehavior != AnimationFillBehavior.Inherit)
            {
                return (FillBehavior)(this.FillBehavior + 1);
            }

            if (this.Parent != null && this.Parent.FillBehavior != AnimationFillBehavior.Inherit)
            {
                return (FillBehavior)(this.Parent.FillBehavior + 1);
            }

            return Windows.UI.Xaml.Media.Animation.FillBehavior.HoldEnd;
        }

        internal virtual EasingFunctionBase ComposeEasingFunction()
        {
            if (this.easing == null)
            {
                if (this.Parent != null)
                {
                    return this.Parent.Easing;
                }
            }

            return this.easing;
        }

        /// <summary>
        /// Applies already stored (if any) animated values.
        /// </summary>
        /// <param name="info">The animation info.</param>
        protected internal virtual void ApplyAnimationValues(PlayAnimationInfo info)
        {
        }

        /// <summary>
        /// Called by the animation manager when the storyboard has been started.
        /// </summary>
        /// <param name="info">The info.</param>
        protected internal virtual void OnStarted(PlayAnimationInfo info)
        {
        }

        /// <summary>
        /// Creates a storyboard for this animation.
        /// </summary>
        /// <param name="target">The target which the storyboard will animate.</param>
        /// <returns>Returns a new storyboard instance.</returns>
        protected virtual Storyboard CreateStoryboardOverride(UIElement target)
        {
            Storyboard result = new Storyboard();

            if (this.RepeatBehavior.HasValue)
            {
                result.RepeatBehavior = this.RepeatBehavior.Value;
            }

            return result;
        }

        /// <summary>
        /// Creates a clone animation of this instance.
        /// </summary>
        /// <returns>Returns a clone of this animation.</returns>
        protected virtual RadAnimation CloneCore()
        {
            return this.MemberwiseClone() as RadAnimation;
        }

        /// <summary>
        /// Allows inheritors to store the animated values.
        /// This is useful if the animation wants to keep the final values upon storyboard stopping.
        /// </summary>
        /// <param name="info">The info.</param>
        protected virtual void CopyAnimationValues(PlayAnimationInfo info)
        {
        }

        /// <summary>
        /// Core update routine.
        /// </summary>
        /// <param name="context">The context that holds information about the animation.</param>
        protected virtual void UpdateAnimationOverride(AnimationContext context)
        {
            if (context == null)
            {
                return;
            }

            context.EaseAll(this.Easing);
            context.Target.RenderTransformOrigin = this.AnimationOrigin;
        }

        /// <summary>
        /// Fires the <see cref="RadAnimation.Ended" /> event for the specific target provided
        /// in the <see cref="PlayAnimationInfo" /> object.
        /// </summary>
        /// <param name="info">The info.</param>
        protected virtual void OnEnded(PlayAnimationInfo info)
        {
            if (this.Ended == null)
            {
                System.Diagnostics.Debug.WriteLine("no ended handler");
            }

            if (this.Ended != null)
            {
                this.Ended(this, new AnimationEndedEventArgs(info));
            }
        }

        /// <summary>
        /// Raises the <see cref="Ended"/> event.
        /// </summary>
        [Obsolete("This method will be remoevd in Q3 2013. Please use the OnEnded(PlayAnimationInfo info) instead.")]
        protected virtual void OnEnded()
        {
            this.OnEnded(null);
        }
    }
}