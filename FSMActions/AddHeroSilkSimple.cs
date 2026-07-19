using HutongGames.PlayMaker;

namespace Rebalance.FSMActions;

public class AddHeroSilkSimple : FsmStateAction
{
    public FsmInt? amount;

    public override void Reset()
    {
        amount = null;
    }

    public override void OnEnter()
    {
        if (!NamedVariable.IsNullOrNone(amount))
            HeroController.instance.AddSilk(amount!.Value, false);
        Finish();
    }
}