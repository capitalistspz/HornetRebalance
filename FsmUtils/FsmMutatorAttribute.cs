using System;

namespace Rebalance.FsmUtils;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class FsmMutatorAttribute : Attribute
{
    public string ObjectName { get; }
    public string FsmName { get; }

    public FsmMutatorAttribute(string objectName, string fsmName)
    {
        ObjectName = objectName;
        FsmName = fsmName;
    }
}