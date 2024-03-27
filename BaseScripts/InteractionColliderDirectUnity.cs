using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace iffnsStuff.iffnsVRCStuff.InteractionController
{
    public class InteractionColliderDirectUnity : InteractionCollider
    {
        public override bool WorldCollisionPointIsValid(Vector3 worldPosition)
        {
            Debug.Log("Just checking");

            return true;
        }
    }
}