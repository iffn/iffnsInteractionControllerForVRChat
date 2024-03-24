
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

    InteractionController linkedInteractionController;
    float defaultAngleDeg;

    public override void InteractionStart(Vector3 rayWorldOrigin, Vector3 rayWorldDirection)
    {
        if (!Networking.IsOwner(gameObject)) Networking.SetOwner(localPlayer, gameObject);

        float angle = GetAngleFromRay(rayWorldOrigin, rayWorldDirection);

        if (!float.IsNaN(angle))
        {
            defaultAngleDeg = angle - currentAngleDeg;
            currentAngleDeg = defaultAngleDeg;
        }
    }

    public override void InteractionStart(Vector3 worldPosition)
    {
        if (!Networking.IsOwner(gameObject)) Networking.SetOwner(localPlayer, gameObject);

        float angle = GetAngleFromPoint(worldPosition);

        if (!float.IsNaN(angle))
        {
            defaultAngleDeg = angle - currentAngleDeg;
            currentAngleDeg = defaultAngleDeg;
        }
    }

    float GetAngleFromRay(Vector3 rayWorldOrigin, Vector3 rayWorldDirection)
    {
        Plane plane = new Plane(transform.forward, transform.position);

        Ray selectionRay = new Ray(rayWorldOrigin, rayWorldDirection);

        if (!plane.Raycast(selectionRay, out float rayLength))
        {
            Debug.LogWarning($"Plane raycast failed in {nameof(RotationInteractor)} for some reason");
            return float.NaN;
        }

        Vector3 worldInteractionPoint = rayWorldOrigin + rayWorldDirection.normalized * rayLength;

        return GetAngleFromPoint(worldInteractionPoint);
    }

    float GetAngleFromPoint(Vector3 worldInteractionPoint)
    {
        Vector3 localInteractionPoint = transform.InverseTransformPoint(worldInteractionPoint);

        //float angleRad = Mathf.Atan2(localInteractionPoint.y, localInteractionPoint.x); //Would also work but returns Rad instead of needed Deg
        return Vector3.SignedAngle(Vector3.right, localInteractionPoint, Vector3.forward);
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

        //float angleRad = Mathf.Atan2(localInteractionPoint.y, localInteractionPoint.x); //Would also work but returns Rad instead of needed Deg
        float angleDeg = Vector3.SignedAngle(Vector3.right, localInteractionPoint, Vector3.forward);

        CurrentAngleDeg = angleDeg - defaultAngleDeg;
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
