
using iffnsStuff.iffnsVRCStuff.InteractionController;
using JetBrains.Annotations;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class RotationInteractor : InteractionElement
{
    [PublicAPI] public const int ExecutionOrder = InteractionController.ExecutionOrder + 1;

    [UdonSynced] float syncedAngleRad;

    bool inputActive;

    InteractionController linkedInteractionController;
    float defaultAngleRad;

    [SerializeField] Transform movingElement;


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
            defaultAngleRad = GetCurrentDesktopAngleRad() - syncedAngleRad;
        }
    }

    float GetCurrentDesktopAngleRad()
    {
        Vector3 worldRayOrigin = linkedInteractionController.WorldRayOrigin;
        Vector3 worldRayDirection = linkedInteractionController.WorldRayDirection;

        Plane plane = new Plane(transform.forward, transform.position);

        Ray selectionRay = new Ray(worldRayOrigin, worldRayDirection);

        if (!plane.Raycast(selectionRay, out float rayLength)) return syncedAngleRad;

        Vector3 worldInteractionPoint = worldRayOrigin + worldRayDirection.normalized * rayLength;

        Vector3 localInteractionPoint = transform.InverseTransformPoint(worldInteractionPoint);

        return Mathf.Atan2(localInteractionPoint.y, localInteractionPoint.x);
    }

    void FixedUpdate()
    {
        if (!inputActive) return;

        syncedAngleRad = GetCurrentDesktopAngleRad() - defaultAngleRad;

        movingElement.localRotation = Quaternion.Euler(syncedAngleRad * Mathf.Rad2Deg * Vector3.forward);
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
