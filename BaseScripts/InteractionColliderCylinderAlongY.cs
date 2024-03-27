using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace iffnsStuff.iffnsVRCStuff.InteractionController
{
    public class InteractionColliderCylinderAlongY : InteractionCollider
    {
        float radius;

        public override bool WorldCollisionPointIsValid(Vector3 worldPosition)
        {
            Vector3 localPosition = transform.InverseTransformPoint(worldPosition);

            localPosition.y = 0;

            Debug.Log(localPosition);

            return (radius < localPosition.magnitude * 1.001f);
        }

        protected override void Setup()
        {
            base.Setup();

            Vector3 size = transform.lossyScale;

            size.y = 0;

            radius = size.magnitude * 0.5f;
        }
    }
}