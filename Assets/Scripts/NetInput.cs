using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public enum InputButton
{
    Jump,
    UseAbility,
    Grapple,
}

public struct NetInput : INetworkInput
{
    public NetworkButtons Buttons;
    public Vector2 Direction;
    public Vector2 LookDelta;
    public AbilityMode AbilityMode;
}
