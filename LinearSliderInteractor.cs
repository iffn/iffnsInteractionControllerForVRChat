
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
            defaultOffset = GetCurrentDeskopValue();
        }
    }

    float GetCurrentDeskopValue()
    {
        Vector3 worldRayOrigin = linkedInteractionController.WorldRayOrigin;
        Vector3 worldRayDirection = linkedInteractionController.WorldRayDirection;

        Vector3 planeNormal = Vector3.Cross(transform.forward, localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Origin).rotation * Vector3.right);

        Plane plane = new Plane(planeNormal, transform.position);

        Ray selectionRay = new Ray(worldRayOrigin, worldRayDirection);

        plane.Raycast(selectionRay, out float rayLength);

        Vector3 worldInteractionPoint = worldRayOrigin + worldRayDirection.normalized * rayLength;

        Vector3 localInteractionPoint = transform.InverseTransformPoint(worldInteractionPoint);

        return localInteractionPoint.z;
    }

    private void FixedUpdate()
    {
        if (!inputActive) return;

        float rawValue = GetCurrentDeskopValue();

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
