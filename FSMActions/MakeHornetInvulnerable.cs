using HutongGames.PlayMaker;

namespace Rebalance.FSMActions;

public class MakeHornetInvulnerable : FsmStateAction
{
    public FsmFloat? duration;

    public override void Reset()
    {
        duration = null;
    }

    public override void OnEnter()
    {
        if (duration == null)
            return;
        var instance = HeroController.instance;
        if (instance == null || duration.Value < 0.0001f)
            return;
        instance.StartInvulnerable(duration.Value);
        Finish();
    }
}