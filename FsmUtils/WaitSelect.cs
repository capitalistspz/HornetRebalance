using HutongGames.PlayMaker;
using UnityEngine;

namespace Rebalance.FsmUtils;
// Based on Wait from AssemblyCSharp
public class WaitSelect : FsmStateAction
{
    [RequiredField] 
    public FsmBool condition;
    
    [RequiredField]
    [HutongGames.PlayMaker.Tooltip("Time to wait in seconds if condition was true.")]
    public FsmFloat trueTime;
    
    [RequiredField]
    [HutongGames.PlayMaker.Tooltip("Time to wait in seconds if condition was true.")]
    public FsmFloat falseTime;

    [HutongGames.PlayMaker.Tooltip("Event to send after the specified time.")]
    public FsmEvent? finishEvent;

    [HutongGames.PlayMaker.Tooltip("Ignore TimeScale. E.g., if the game is paused using Scale Time.")]
    public bool realTime;

    private float startTime;

    private float timer;

    private float targetTime;
    
    public override void Reset()
    {
        trueTime = 1f;
        falseTime = 1f;
        finishEvent = null;
        realTime = false;
    }

    public override void OnEnter()
    {
        var time = condition.Value ? trueTime.Value : falseTime.Value;
        if (time <= 0f)
        {
            Fsm.Event(finishEvent);
            Finish();
        }
        else
        {
            startTime = FsmTime.RealtimeSinceStartup;
            timer = 0f;
            targetTime = time;
        }
    }

    public override void OnUpdate()
    {
        if (realTime)
        {
            timer = FsmTime.RealtimeSinceStartup - startTime;
        }
        else
        {
            timer += Time.deltaTime;
        }
        if (timer >= targetTime)
        {
            Finish();
            if (finishEvent != null)
            {
                Fsm.Event(finishEvent);
            }
        }
    }
}