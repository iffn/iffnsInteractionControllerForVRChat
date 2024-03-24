
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

    InteractionController linkedInteractionController;
    float defaultOffset;

    public override void InteractionStart(Vector3 rayWorldOrigin, Vector3 rayWorldDirection)
    {
        if (!Networking.IsOwner(gameObject)) Networking.SetOwner(localPlayer, gameObject);

        float offset = GetValueFromRay(rayWorldOrigin, rayWorldDirection);

        if (float.IsNaN(offset))
        {
            defaultOffset = currentValue / unityToValueScaler; //ToDo: Find better solution. Fail if needed. This will jump and introduce weird offset.
        }
        else
        {
            defaultOffset = offset - currentValue / unityToValueScaler;
        }
    }

    public override void InteractionStart(Vector3 worldPosition)
    {
        if (!Networking.IsOwner(gameObject)) Networking.SetOwner(localPlayer, gameObject);

        float offset = GetValueFromPoint(worldPosition);

        if (float.IsNaN(offset))
        {
            defaultOffset = currentValue / unityToValueScaler; //ToDo: Find better solution. Fail if needed. This will jump and introduce weird offset.
        }
        else
        {
            defaultOffset = offset - currentValue / unityToValueScaler;
        }
    }

    float GetValueFromRay(Vector3 rayWorldOrigin, Vector3 rayWorldDirection)
    {
        Vector3 planeSideDirection = Vector3.Cross(transform.forward, rayWorldDirection);

        Vector3 planeNormal = Vector3.Cross(transform.forward, planeSideDirection);

        Plane plane = new Plane(planeNormal, transform.position);

        Ray selectionRay = new Ray(rayWorldOrigin, rayWorldDirection);

        if (!plane.Raycast(selectionRay, out float rayLength))
        {
            Debug.LogWarning($"Plane raycast failed in {nameof(LinearSliderInteractor)} for some reason");
            return -float.NaN; //No idea why, but I think this sometimes fails
        }

        Vector3 worldInteractionPoint = rayWorldOrigin + rayWorldDirection.normalized * rayLength;

        return GetValueFromPoint(worldInteractionPoint);
    }

    float GetValueFromPoint(Vector3 worldInteractionPoint)
    {
        Vector3 localInteractionPoint = transform.InverseTransformPoint(worldInteractionPoint);

        return localInteractionPoint.z;
    }

    public override void UpdateElement(Vector3 rayWorldOrigin, Vector3 rayWorldDirection)
    {
        float rawValue = GetValueFromRay(rayWorldOrigin, rayWorldDirection);

        if (!float.IsNaN(rawValue))
        {
            float offset = rawValue - defaultOffset;

            CurrentValue = offset * unityToValueScaler;
        }
    }

    public override void UpdateElement(Vector3 worldInteractionPoint)
    {
        float rawValue = GetValueFromPoint(worldInteractionPoint);

        if (!float.IsNaN(rawValue))
        {
            float offset = rawValue - defaultOffset;

            CurrentValue = offset * unityToValueScaler;
        }
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
