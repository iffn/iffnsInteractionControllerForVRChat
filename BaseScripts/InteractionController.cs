using BestHTTP.SecureProtocol.Org.BouncyCastle.Utilities.IO.Pem;
using JetBrains.Annotations;
using System;
using UdonSharp;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.SDKBase.Editor;
using VRC.Udon.Common;

namespace iffnsStuff.iffnsVRCStuff.InteractionController
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [DefaultExecutionOrder(ExecutionOrder)]
    public class InteractionController : UdonSharpBehaviour
    {
        [PublicAPI] public const int ExecutionOrder = 0;

        /*
        Created with additional inputs from KitKat
        
        ToAdd:
        - Differentiate between mouse does or does not move when off screen
        - Recalibration key hint
        */

        //Assignments
        [SerializeField] FOVDetector linkedFOVDetector;
        [SerializeField] private RectTransform uiCursor;
        [SerializeField] private RectTransform uiCalibrator;
        [SerializeField] private float cursorSpeedUI;
        [SerializeField] Camera linkedGrabCamera;
        [SerializeField] Transform referenceTransform;
        [SerializeField] LayerMask interactionMask;

        //Fixed variables
        bool useIsGrab;
        bool inVR;
        VRCPlayerApi localPlayer;
        float vrInteractionDistance;
        
        Vector3 leftHandIndexInteractionOffset;
        Vector3 rightHandIndexInteractionOffset;
        Vector3 leftHandPalmInteractionOffset;
        Vector3 rightHandPalmInteractionOffset;
        bool triggerHoldBehavior = true;
        bool grabHoldBehavior = true;


        //Runtime variables General
        int fixedUpdateCounter = 0;
        int fixedUpdateClearanceByLateUpdate;

        //Runtime variables VR
        Vector3 leftHandIndexInteractionPosition;
        Vector3 rightHandIndexInteractionPosition;
        Vector3 leftHandPalmInteractionPosition;
        Vector3 rightHandPalmInteractionPosition;
        bool leftIndexActive = false;
        bool leftPalmActive = false;
        bool rightIndexActive = false;
        bool rightPalmActive = false;
        bool leftIndexChange = false;
        bool rightIndexChange = false;
        bool leftPalmChange = false;
        bool rightPalmChange = false;
        InteractionElement leftHandObject;
        InteractionElement rightHandObject;

        //Runtime variables Desktop
        public bool isCastingInDesktop = true;
        public InteractionElement previousDesktopElement;
        public InteractionElement newDesktopElement;
        public bool desktopInputActive = false;
        public bool desktopInputChange = false;
        Vector2 cursorPosition;
        const int canvasSizeY = 540;
        const float halfDeg2Rad = Mathf.Deg2Rad * 0.5f;
        bool calibrated = false;
        Vector3 localRayOrigin;
        Vector3 localRayDirection;

        public Vector3 WorldRayOrigin
        {
            get
            {
                return referenceTransform.InverseTransformPoint(localRayOrigin);
            }
        }

        public Vector3 WorldRayDirection
        {
            get
            {
                return referenceTransform.TransformDirection(localRayDirection);
            }
        }

        public Transform ReferenceTransform
        {
            set
            {
                referenceTransform = value;
            }
        }
        
        //Internal function

        //Unity functions
        private void Start()
        {
            localPlayer = Networking.LocalPlayer;
            inVR = localPlayer.IsUserInVR();

            if(referenceTransform == null) referenceTransform = transform;
        }

        void UpdateInteraction(InteractionElement previousElement, InteractionElement newElement, bool inputActive, bool inputChange)
        {
            if (!inputActive)
            {
                //ToDo: More complex highlight logic to prevent shutting off and on if needed
                
                if(previousDesktopElement != newElement)
                {
                    if (previousElement) previousElement.Highlight = false;
                    if (newElement) newElement.Highlight = true;
                }

                if (inputChange)
                {
                    if(previousElement) previousElement.InteractionStop();
                }
            }
            else
            {
                if (inputChange)
                {
                    if(previousElement) previousElement.InteractionStart(this);
                }
            }
        }

        private void Update()
        {
            if (inVR)
            {

            }
            else
            {
                UpdateInteraction(previousDesktopElement, newDesktopElement, desktopInputActive, desktopInputChange);

                isCastingInDesktop = !desktopInputActive;

                if(!desktopInputActive) previousDesktopElement = newDesktopElement;

                desktopInputChange = false;
            }
        }

        //public override void PostLateUpdate()
        void LateUpdate()
        {
            fixedUpdateClearanceByLateUpdate = fixedUpdateCounter;

            if (inVR)
            {
                VRCPlayerApi.TrackingData leftHand = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.LeftHand);
                VRCPlayerApi.TrackingData rightHand = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand);

                leftHandIndexInteractionPosition = leftHand.position + leftHand.rotation * leftHandPalmInteractionOffset;
                rightHandIndexInteractionPosition = rightHand.position + rightHand.rotation * rightHandPalmInteractionOffset;
            }
            else
            {
                if (Input.GetKeyDown(KeyCode.Tab))
                {
                    cursorPosition = Vector2.zero;
                    uiCursor.gameObject.SetActive(calibrated);

                    uiCalibrator.gameObject.SetActive(!calibrated);
                }
                else if (Input.GetKeyUp(KeyCode.Tab))
                {
                    uiCursor.gameObject.SetActive(false);
                    uiCalibrator.gameObject.SetActive(false);
                }

                if (Input.GetKey(KeyCode.Tab))
                {
                    cursorPosition.x += Input.GetAxisRaw("Mouse X");
                    cursorPosition.y += Input.GetAxisRaw("Mouse Y");

                    Vector2 cursorPositionActual = cursorPosition * cursorSpeedUI;

                    if (!calibrated)
                    {
                        isCastingInDesktop = false;
                        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Tilde))
                        {
                            float uiSensitivityX = uiCalibrator.localPosition.x / cursorPositionActual.x;
                            float uiSensitivityY = uiCalibrator.localPosition.y / cursorPositionActual.y;
                            cursorSpeedUI *= (uiSensitivityX + uiSensitivityY) / 2;
                            calibrated = true;
                            isCastingInDesktop = true;
                            uiCalibrator.gameObject.SetActive(false);
                            uiCursor.gameObject.SetActive(true);
                        }
                    }
                    else
                    {
                        float aspectRatio = 1f * linkedGrabCamera.pixelWidth / linkedGrabCamera.pixelHeight;

                        cursorPositionActual.x = Mathf.Clamp(cursorPositionActual.x, -aspectRatio * canvasSizeY, aspectRatio * canvasSizeY);
                        cursorPositionActual.y = Mathf.Clamp(cursorPositionActual.y, -canvasSizeY, canvasSizeY);

                        uiCursor.anchoredPosition = cursorPositionActual;

                        VRCPlayerApi.TrackingData head = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head);

                        Vector3 localHeading = new Vector3
                        (
                            cursorPositionActual.x,
                            cursorPositionActual.y,
                            540 / Mathf.Tan(linkedFOVDetector.DetectedFOV * halfDeg2Rad)
                        );

                        localRayOrigin = referenceTransform.InverseTransformPoint(head.position);
                        localRayDirection = referenceTransform.InverseTransformDirection(head.rotation * localHeading);

                        newDesktopElement = GetInteractedObjectInDesktop(head.position, head.rotation * localHeading);

                        if (Input.GetKeyDown(KeyCode.Q))
                        {
                            calibrated = false;
                            uiCursor.gameObject.SetActive(false);
                            uiCalibrator.gameObject.SetActive(true);
                        }
                    }
                }
                else
                {
                    isCastingInDesktop = true;

                    VRCPlayerApi.TrackingData head = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head);

                    localRayOrigin = referenceTransform.InverseTransformPoint(head.position);
                    localRayDirection = referenceTransform.InverseTransformDirection(head.rotation * Vector3.forward);
                    
                    newDesktopElement = GetInteractedObjectInDesktop(head.position, head.rotation * Vector3.forward);
                }
            }
        }

        void InteractWithObject(InteractionElement oldElement, InteractionElement newElement, bool oldState, bool newState)
        {
            newElement.Highlight = true;

            oldElement.Highlight = false;



            if (!oldState)
            {

            }

            if(oldElement == null)
            {

            }
            else
            {

            }
        }

        InteractionElement GetInteractedObjectInVR(Vector3 position)
        {
            Collider[] colliders = Physics.OverlapSphere(position, vrInteractionDistance, interactionMask);

            foreach(Collider collider in colliders)
            {

                InteractionCollider potentialCollider = collider.transform.GetComponent<InteractionCollider>();

                if (potentialCollider)
                {
                    return potentialCollider.LinkedInteractionElement;
                }

                /*
                //TryGetComponent not exposed in U# yet
                if (collider.transform.TryGetComponent(out InteractionCollider controller))
                    return controller.LinkedInteractionElement;
                */
            }

            return null;
        }

        public GameObject debugCollider;
        public Transform debugTransform;

        InteractionElement GetInteractedObjectInDesktop(Vector3 origin, Vector3 direction)
        {
            debugTransform.position = origin;
            debugTransform.LookAt(direction, Vector2.up);

            if (Physics.Raycast(new Ray(origin, direction), out RaycastHit hit, Mathf.Infinity, interactionMask)) //ToDo: Limit interaction distance
            {
                if (hit.collider != null) //At least VRChat client sim canvas hit collider somehow null
                {
                    debugCollider = hit.collider.gameObject;

                    InteractionCollider potentialCollider = hit.transform.GetComponent<InteractionCollider>();

                    if (potentialCollider)
                    {
                        return potentialCollider.LinkedInteractionElement;
                    }
                }
            }
            else debugCollider = null;

            return null;
        }

        void FixedUpdatePerLateUpdate()
        {
            if (inVR)
            {
                InteractionElement newLeftPalmCollider = GetInteractedObjectInVR(leftHandPalmInteractionPosition);

            }
            else
            {
                if (isCastingInDesktop)
                    newDesktopElement = GetInteractedObjectInDesktop(WorldRayOrigin, WorldRayDirection);
            }
        }

        private void FixedUpdate()
        {
            return;

            if(fixedUpdateClearanceByLateUpdate == fixedUpdateCounter)
            {
                FixedUpdatePerLateUpdate();
            }

            fixedUpdateCounter++;
        }

        //VRChat functions
        public override void InputUse(bool value, UdonInputEventArgs args)
        {
            if (inVR)
            {
                switch (args.handType)
                {
                    case HandType.RIGHT:
                        if (rightIndexActive != value) rightIndexChange = true;
                        rightIndexActive = value;
                        break;
                    case HandType.LEFT:
                        if (leftIndexActive != value) leftIndexChange = true;
                        leftIndexActive = value;
                        break;
                    default:
                        break;
                }
            }
            else
            {
                desktopInputChange = true;
                desktopInputActive = value;
            }
        }

        public override void InputGrab(bool value, UdonInputEventArgs args)
        {
            if (!inVR) return;

            if (!useIsGrab)
            {
                switch (args.handType)
                {
                    case HandType.RIGHT:
                        if (rightPalmActive != value) rightPalmChange = true;
                        rightPalmActive = value;
                        break;
                    case HandType.LEFT:
                        if (leftPalmActive != value) leftPalmChange = true;
                        leftPalmActive = value;
                        break;
                    default:
                        break;
                }
            }
        }

        public override void InputDrop(bool value, UdonInputEventArgs args)
        {
            if (!inVR) return;

            switch (args.handType)
            {
                case HandType.RIGHT:
                    if (rightPalmActive != value) rightPalmChange = true;
                    rightPalmActive = value;
                    break;
                case HandType.LEFT:
                    if (leftPalmActive != value) leftPalmChange = true;
                    leftPalmActive = value;
                    break;
                default:
                    break;
            }
        }
    }
}