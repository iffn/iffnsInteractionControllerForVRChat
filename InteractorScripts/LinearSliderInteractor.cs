
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

    [SerializeField] Transform movingElement;
    [SerializeField] float unityToValueScaler = 1;
    [SerializeField] float maxOutput = 1;
    [SerializeField] float minOutput = -1;

    float currentValue;

    public float CurrentValue
    {
        get
        {
            return currentValue;
        }

        set
        {
            currentValue = Mathf.Clamp(value, minOutput, maxOutput);

            movingElement.transform.localPosition = currentValue / unityToValueScaler * Vector3.forward;
        }
    }

    bool inputActive;
    InteractionController linkedInteractionController;
    float defaultOffset;

    public override void InteractionStart(InteractionController linkedInteractionController, interactionSources interactionSource)
    {
        inputActive = true;

        if (!Networking.IsOwner(gameObject)) Networking.SetOwner(localPlayer, gameObject);

        this.linkedInteractionController = linkedInteractionController;

        if (inVR)
        {

        }
        else
        {
            float offset = GetCurrentDesktopValue();

            if (float.IsNaN(offset))
            {
                defaultOffset = currentValue / unityToValueScaler; //ToDo: Find better solution. Fail if needed. This will jump and introduce weird offset.
            }
            else
            {
                defaultOffset = offset - currentValue / unityToValueScaler;
            }

            
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

        if (!plane.Raycast(selectionRay, out float rayLength))
        {
            Debug.LogWarning($"Plane raycast failed in {nameof(LinearSliderInteractor)} for some reason");
            return -float.NaN; //No idea why, but I think this sometimes fails
        }

        Vector3 worldInteractionPoint = worldRayOrigin + worldRayDirection.normalized * rayLength;

        Vector3 localInteractionPoint = transform.InverseTransformPoint(worldInteractionPoint);

        return localInteractionPoint.z;
    }

    private void FixedUpdate()
    {
        if (!inputActive) return;

        if (inVR)
        {

        }
        else
        {
            float rawValue = GetCurrentDesktopValue();

            if (!float.IsNaN(rawValue))
            {
                float offset = rawValue - defaultOffset;

                CurrentValue = offset * unityToValueScaler;
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
