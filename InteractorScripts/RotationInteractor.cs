using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace iffnsStuff.iffnsVRCStuff.InteractionController
{
    public class RotationInteractor : SingleFloatInteractor
    {
        [SerializeField] Transform movingElement;

        protected override void ApplyUnityValue(float value)
        {
            Debug.Log($"Unity value = {value}");

            movingElement.localRotation = Quaternion.Euler(value * Mathf.Rad2Deg * Vector3.forward);
        }

        float defaultAngleRadOffset;

        public override void InteractionStart(Vector3 rayWorldOrigin, Vector3 rayWorldDirection)
        {
            if (!Networking.IsOwner(gameObject)) Networking.SetOwner(localPlayer, gameObject);

            float unityOffsetRad = GetAngleRadFromRay(rayWorldOrigin, rayWorldDirection);

            if (float.IsNaN(unityOffsetRad))
            {
                defaultAngleRadOffset = CurrentUnityValue; //ToDo: Find better solution. Fail if needed. This will jump and introduce weird offset.
            }
            else
            {
                defaultAngleRadOffset = unityOffsetRad - CurrentUnityValue;
            }
        }

        public override void InteractionStart(Vector3 worldPosition)
        {
            if (!Networking.IsOwner(gameObject)) Networking.SetOwner(localPlayer, gameObject);

            float unityOffsetRad = GetAngleFromWorldPoint(worldPosition);

            if (float.IsNaN(unityOffsetRad))
            {
                defaultAngleRadOffset = CurrentUnityValue; //ToDo: Find better solution. Fail if needed. This will jump and introduce weird offset.
            }
            else
            {
                defaultAngleRadOffset = unityOffsetRad - CurrentUnityValue;
            }
        }

        float GetAngleRadFromRay(Vector3 rayWorldOrigin, Vector3 rayWorldDirection)
        {
            Plane plane = new Plane(transform.forward, transform.position);

            Ray selectionRay = new Ray(rayWorldOrigin, rayWorldDirection);

            if (!plane.Raycast(selectionRay, out float rayLength))
            {
                Debug.LogWarning($"Plane raycast failed in {nameof(RotationInteractor)} for some reason");
                return float.NaN;
            }

            Vector3 worldInteractionPoint = rayWorldOrigin + rayWorldDirection.normalized * rayLength;

            return GetAngleFromWorldPoint(worldInteractionPoint);
        }

        float GetAngleFromWorldPoint(Vector3 worldInteractionPoint)
        {
            Vector3 localInteractionPoint = transform.InverseTransformPoint(worldInteractionPoint);

            return Mathf.Atan2(localInteractionPoint.y, localInteractionPoint.x);
            //return Vector3.SignedAngle(Vector3.right, localInteractionPoint, Vector3.forward) * Mathf.Deg2Rad; //Would also work but returns Deg instead of needed Rad
        }

        //Mark
        public override void UpdateElement(Vector3 rayWorldOrigin, Vector3 rayWorldDirection)
        {
            float rawUnityValue = GetAngleRadFromRay(rayWorldOrigin, rayWorldDirection);

            if (!float.IsNaN(rawUnityValue))
            {
                CurrentUnityValue = rawUnityValue - defaultAngleRadOffset;
            }
        }

        public override void UpdateElement(Vector3 worldInteractionPoint)
        {
            float rawUnityValue = GetAngleFromWorldPoint(worldInteractionPoint);

            if (!float.IsNaN(rawUnityValue))
            {
                CurrentUnityValue = rawUnityValue - defaultAngleRadOffset;
            }
        }


        public override void InteractionStop()
        {

        }
    }
}