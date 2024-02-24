
using iffnsStuff.iffnsVRCStuff.InteractionController;
using JetBrains.Annotations;
using UdonSharp;
using UnityEngine;
using UnityEngine.UIElements;
using VRC.SDKBase;
using VRC.Udon;

public class RotationInteractor : InteractionElement
{
    [PublicAPI] public const int ExecutionOrder = InteractionController.ExecutionOrder + 1;
    
    [SerializeField] Transform movingElement;

    float currentAngleDeg;

    public float CurrentAngleDeg
    {
        get
        {
            return currentAngleDeg;
        }
        set
        {
            currentAngleDeg = value;
            movingElement.localRotation = Quaternion.Euler(currentAngleDeg * Vector3.forward);
        }
    }

    bool inputActive;
    InteractionController linkedInteractionController;
    float defaultAngleDeg;

    public override void InteractionStart(InteractionController linkedInteractionController)
    {
        inputActive = true;

        if (!Networking.IsOwner(gameObject)) Networking.SetOwner(localPlayer, gameObject);

        this.linkedInteractionController = linkedInteractionController;

        if (inVR)
        {

        }
        else
        {
            currentAngleDeg = 0;
            defaultAngleDeg = 0;

            defaultAngleDeg = GetCurrentDesktopAngleDeg() - currentAngleDeg; //Note: Will return 0 if raycast fails
        }
    }

    float GetCurrentDesktopAngleDeg()
    {
        Vector3 worldRayOrigin = linkedInteractionController.WorldRayOrigin;
        Vector3 worldRayDirection = linkedInteractionController.WorldRayDirection;

        Plane plane = new Plane(transform.forward, transform.position);

        Ray selectionRay = new Ray(worldRayOrigin, worldRayDirection);

        if (!plane.Raycast(selectionRay, out float rayLength)) return float.NaN; //No idea why this sometimes fails

        Vector3 worldInteractionPoint = worldRayOrigin + worldRayDirection.normalized * rayLength;

        Vector3 localInteractionPoint = transform.InverseTransformPoint(worldInteractionPoint);

        //float angleRad = Mathf.Atan2(localInteractionPoint.y, localInteractionPoint.x); //Would also work but returns Rad instead of needed Deg
        float angleDeg = Vector3.SignedAngle(Vector3.right, localInteractionPoint, Vector3.forward);

        return angleDeg;
    }

    void LateUpdate()
    {
        if (!inputActive) return;

        if (inVR)
        {

        }
        else
        {
            float angle = GetCurrentDesktopAngleDeg();

            if (!float.IsNaN(angle))
            {
                CurrentAngleDeg = GetCurrentDesktopAngleDeg() - defaultAngleDeg;
            }
        }
    }

    public override void InteractionStop()
    {
        inputActive = false;
    }

    public override void OnOwnershipTransferred(VRCPlayerApi player)
    {
        base.OnOwnershipTransferred(player);

        if (player.isLocal)
        {

        }
        else
        {
            inputActive = false;
        }
    }
}
