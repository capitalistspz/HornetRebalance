using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;

namespace Rebalance.FSMActions;

public class Tk2dPlayAnimationWithEventsAtRate : Tk2dPlayAnimationWithEvents
{
    public FsmFloat? playRate;

    public override void Reset()
    {
        base.Reset();
        playRate = null;
    }
    
    public override void OnEnter()
    {
        hasExpectedClip = false;
        _getSprite();
        DoPlayAnimationWithEvents();
    }

    private new void DoPlayAnimationWithEvents()
    {
        if (_sprite == null)
        {
            LogWarning("Missing tk2dSpriteAnimator component");
            return;
        }

        var actualPlayRate = NamedVariable.IsNullOrNone(playRate) ? 1.0f : playRate!.Value;
        
        var component = _sprite.GetComponent<IHeroAnimationController>();
        expectedClip = component != null ? component.GetClip(clipName.Value) : _sprite.GetClipByName(clipName.Value);
        
        _sprite.Play(expectedClip, 0.0f, expectedClip.fps * actualPlayRate);
        hasExpectedClip = expectedClip != null;
        
        var hasEvent = false;
        if (animationTriggerEvent != null)
        {
            _sprite.AnimationEventTriggered = AnimationEventDelegate;
            hasEvent = true;
        }
        if (animationCompleteEvent != null)
        {
            _sprite.AnimationCompleted = AnimationCompleteDelegate;
            hasEvent = true;
        }
        if (!hasExpectedClip && hasEvent)
        {
            Fsm.Event(animationTriggerEvent);
            Fsm.Event(animationCompleteEvent);
        }
        if (!hasEvent || !hasExpectedClip)
        {
            Finish();
        }
    }
}