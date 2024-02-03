
using iffnsStuff.iffnsVRCStuff.InteractionController;
using JetBrains.Annotations;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[DefaultExecutionOrder(ExecutionOrder)]
public class LinearSliderInteractor : InteractionElement
{
    [PublicAPI] public const int ExecutionOrder = InteractionController.ExecutionOrder + 1;

    [UdonSynced] float syncedValue;

    Plane plane;

    bool inputActive;

    InteractionController linkedInteractionController;

    float defaultOffset;

    [SerializeField] Transform movingElement;
    [SerializeField] float unityToValueScaler = 1;
    [SerializeField] float maxOutput = 1;
    [SerializeField] float minOutput = -1;


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
            defaultOffset = GetCurrentDesktopValue();
        }
    }

    float GetCurrentDesktopValue()
    {
        Vector3 worldRayOrigin = linkedInteractionController.WorldRayOrigin;
        Vector3 worldRayDirection = linkedInteractionController.WorldRayDirection;

        Vector3 planeSideDirection = Vector3.Cross(transform.forward, worldRayDirection);

        Vector3 planeNormal = Vector3.Cross(transform.forward, planeSideDirection);

        Plane plane = new Plane(planeNormal, transform.position);

        Ray selectionRay = new Ray(worldRayOrigin, worldRayDirection);

        if(!plane.Raycast(selectionRay, out float rayLength)) return syncedValue;

        Vector3 worldInteractionPoint = worldRayOrigin + worldRayDirection.normalized * rayLength;

        Vector3 localInteractionPoint = transform.InverseTransformPoint(worldInteractionPoint);

        return localInteractionPoint.z;
    }

    private void FixedUpdate()
    {
        if (!inputActive) return;

        float rawValue = GetCurrentDesktopValue();

        float offset = rawValue - defaultOffset;

        syncedValue = Mathf.Clamp(offset * unityToValueScaler, minOutput, maxOutput);

        movingElement.transform.localPosition = Vector3.forward * syncedValue;
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
