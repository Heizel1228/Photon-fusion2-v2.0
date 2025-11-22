using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion.Addons.KCC;

public class ImpulseProcessor : KCCProcessor
{
    public float ImpulseStrength;
    [SerializeField] private bool isSphere;

    public override void OnEnter(KCC kcc, KCCData data)
    {
        Vector3 impulseDirection;

        if (isSphere)
        {
            impulseDirection = Vector3.Normalize(kcc.Collider.ClosestPoint(transform.position) - transform.position);
        }
        else
        {
            impulseDirection = transform.forward;
        }

        //Clear dynamic velocity proortionally to impulse direction // in human word means clear the player impulse prevent stacking impulse, like flying like a super man
        kcc.SetDynamicVelocity(data.DynamicVelocity - Vector3.Scale(data.DynamicVelocity, impulseDirection.normalized));

        //add impulse
        kcc.AddExternalImpulse(impulseDirection * ImpulseStrength);
    }
}
