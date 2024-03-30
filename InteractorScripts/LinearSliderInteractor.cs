using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace iffnsStuff.iffnsVRCStuff.InteractionController
{
    public class LinearSliderInteractor : SingleFloatInteractor
    {
        [SerializeField] Transform movingElement;

        protected override float GetCurrentUnityValueFromUnity()
        {
            return movingElement.transform.localPosition.z;
        }

        protected override void ApplyUnityValue(float value)
        {
            movingElement.transform.localPosition = value * Vector3.forward;
        }

        float defaultUnityOffset;
        public override void InteractionStart(Vector3 rayWorldOrigin, Vector3 rayWorldDirection)
        {
            if (!Networking.IsOwner(gameObject)) Networking.SetOwner(localPlayer, gameObject);

            float unityOffset = GetUnityValueFromRay(rayWorldOrigin, rayWorldDirection);

            if (float.IsNaN(unityOffset))
            {
                defaultUnityOffset = CurrentUnityValue; //ToDo: Find better solution. Fail if needed. This will jump and introduce weird offset.
            }
            else
            {
                defaultUnityOffset = unityOffset - CurrentUnityValue;
            }
        }

        public override void InteractionStart(Vector3 worldPosition)
        {
            if (!Networking.IsOwner(gameObject)) Networking.SetOwner(localPlayer, gameObject);

            float unityOffset = GetUnityValueFromPoint(worldPosition);

            if (float.IsNaN(unityOffset))
            {
                defaultUnityOffset = CurrentUnityValue; //ToDo: Find better solution. Fail if needed. This will jump and introduce weird offset.
            }
            else
            {
                defaultUnityOffset = unityOffset - CurrentUnityValue;
            }
        }

        float GetUnityValueFromRay(Vector3 rayWorldOrigin, Vector3 rayWorldDirection)
        {
            Vector3 planeSideDirection = Vector3.Cross(transform.forward, rayWorldDirection);

            Vector3 planeNormal = Vector3.Cross(transform.forward, planeSideDirection);

            Plane plane = new Plane(planeNormal, transform.position);

            Ray selectionRay = new Ray(rayWorldOrigin, rayWorldDirection);

            if (!plane.Raycast(selectionRay, out float rayLength))
            {
                //Debug.LogWarning($"Plane raycast failed in {nameof(LinearSliderInteractor)} for some reason"); //-> Likely pointing away from the plane
                return -float.NaN; //No idea why, but I think this sometimes fails
            }

            Vector3 worldInteractionPoint = rayWorldOrigin + rayWorldDirection.normalized * rayLength;

            return GetUnityValueFromPoint(worldInteractionPoint);
        }

        float GetUnityValueFromPoint(Vector3 worldInteractionPoint)
        {
            Vector3 localInteractionPoint = transform.InverseTransformPoint(worldInteractionPoint);

            return localInteractionPoint.z;
        }

        //Mark
        public override void UpdateElement(Vector3 rayWorldOrigin, Vector3 rayWorldDirection)
        {
            float rawUnityValue = GetUnityValueFromRay(rayWorldOrigin, rayWorldDirection);

            if (!float.IsNaN(rawUnityValue))
            {
                CurrentUnityValue = rawUnityValue - defaultUnityOffset;
            }
        }

        public override void UpdateElement(Vector3 worldInteractionPoint)
        {
            float rawUnityValue = GetUnityValueFromPoint(worldInteractionPoint);

            if (!float.IsNaN(rawUnityValue))
            {
                CurrentUnityValue = rawUnityValue - defaultUnityOffset;
            }
        }

        public override void InteractionStop()
        {

        }
    }
}