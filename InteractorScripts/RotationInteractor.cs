
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

    [UdonSynced] float syncedAngleDeg;

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
            syncedAngleDeg = 0;
            defaultAngleDeg = 0;

            defaultAngleDeg = GetCurrentDesktopAngleDeg() - syncedAngleDeg; //Note: Will return 0 if raycast fails
        }
    }

    float GetCurrentDesktopAngleDeg()
    {
        Vector3 worldRayOrigin = linkedInteractionController.WorldRayOrigin;
        Vector3 worldRayDirection = linkedInteractionController.WorldRayDirection;

        Plane plane = new Plane(transform.forward, transform.position);

        Ray selectionRay = new Ray(worldRayOrigin, worldRayDirection);

        if (!plane.Raycast(selectionRay, out float rayLength)) return syncedAngleDeg + defaultAngleDeg; //No idea why this sometimes fails

        Vector3 worldInteractionPoint = worldRayOrigin + worldRayDirection.normalized * rayLength;

        Vector3 localInteractionPoint = transform.InverseTransformPoint(worldInteractionPoint);

        //float angleRad = Mathf.Atan2(localInteractionPoint.y, localInteractionPoint.x); //Would also work but returns Rad instead of needed Deg
        float angleDeg = Vector3.SignedAngle(Vector3.right, localInteractionPoint, Vector3.forward);

        return angleDeg;
    }

    void LateUpdate()
    {
        if (!inputActive) return;

        syncedAngleDeg = GetCurrentDesktopAngleDeg() - defaultAngleDeg;

        movingElement.localRotation = Quaternion.Euler(syncedAngleDeg * Vector3.forward);
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
