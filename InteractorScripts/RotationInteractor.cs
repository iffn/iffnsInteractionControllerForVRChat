using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace iffnsStuff.iffnsVRCStuff.InteractionController
{
    public class RotationInteractor : SingleFloatInteractor
    {
        [SerializeField] Transform movingElement;

        protected override void SetUnityValue(float value)
        {
            movingElement.localRotation = Quaternion.Euler(value * Mathf.Rad2Deg * Vector3.forward);
        }

        InteractionController linkedInteractionController;
        float defaultAngleRad;

        public override void InteractionStart(Vector3 rayWorldOrigin, Vector3 rayWorldDirection)
        {
            if (!Networking.IsOwner(gameObject)) Networking.SetOwner(localPlayer, gameObject);

            float angleRad = GetAngleRadFromRay(rayWorldOrigin, rayWorldDirection);

            if (!float.IsNaN(angleRad))
            {
                defaultAngleRad = angleRad - currentControlValue;
                currentControlValue = defaultAngleRad;
            }
        }

        public override void InteractionStart(Vector3 worldPosition)
        {
            if (!Networking.IsOwner(gameObject)) Networking.SetOwner(localPlayer, gameObject);

            float angleRad = GetAngleFromWorldPoint(worldPosition);

            if (!float.IsNaN(angleRad))
            {
                defaultAngleRad = angleRad - currentControlValue;
                currentControlValue = defaultAngleRad;
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

        public override void UpdateElement(Vector3 rayWorldOrigin, Vector3 rayWorldDirection)
        {
            Plane plane = new Plane(transform.forward, transform.position);

            Ray selectionRay = new Ray(rayWorldOrigin, rayWorldDirection);

            if (!plane.Raycast(selectionRay, out float rayLength))
            {
                Debug.LogWarning($"Plane raycast failed in {nameof(RotationInteractor)} for some reason");
                return;
            }

            Vector3 worldInteractionPoint = rayWorldOrigin + rayWorldDirection.normalized * rayLength;

            UpdateElement(worldInteractionPoint);
        }

        public override void UpdateElement(Vector3 worldInteractionPoint)
        {
            Vector3 localInteractionPoint = transform.InverseTransformPoint(worldInteractionPoint);

            float angleRad = Mathf.Atan2(localInteractionPoint.y, localInteractionPoint.x); //Would also work but returns Rad instead of needed Deg
            //float angleRad = Vector3.SignedAngle(Vector3.right, localInteractionPoint, Vector3.forward) * Mathf.Deg2Rad;

            currentControlValue = angleRad - defaultAngleRad;
        }


        public override void InteractionStop()
        {

        }

        public override void OnOwnershipTransferred(VRCPlayerApi player)
        {
            base.OnOwnershipTransferred(player);

            if (player.isLocal)
            {

            }
            else
            {

            }
        }
    }
}