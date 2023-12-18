using UdonSharp;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon.Common;

namespace iffnsStuff.iffnsVRCStuff.InteractionController
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class InteractionController : UdonSharpBehaviour
    {
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

        //Fixed variables
        bool useIsGrab;
        bool inVR;
        VRCPlayerApi localPlayer;
        
        Vector3 leftHandIndexInteractionOffset;
        Vector3 rightHandIndexInteractionOffset;
        Vector3 leftHandPalmInteractionOffset;
        Vector3 rightHandPalmInteractionOffset;
        bool triggerHoldBehavior = true;
        bool grabHoldBehavior = true;


        //Runtime variables General
        Transform previousObject;

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


        //Runtime variables Desktop
        Vector2 cursorPosition;
        const int canvasSizeY = 540;
        const float halfDeg2Rad = Mathf.Deg2Rad * 0.5f;
        bool calibrated = false;
        Vector3 localRayOrigin;
        Vector3 localRayDirection;
        bool isCasting = true;


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

        //public override void PostLateUpdate()
        void LateUpdate()
        {
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
                        isCasting = false;
                        if (Input.GetMouseButtonDown(0))
                        {
                            float uiSensitivityX = uiCalibrator.localPosition.x / cursorPositionActual.x;
                            float uiSensitivityY = uiCalibrator.localPosition.y / cursorPositionActual.y;
                            cursorSpeedUI *= (uiSensitivityX + uiSensitivityY) / 2;
                            calibrated = true;
                            isCasting = true;
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
                    isCasting = true;

                    VRCPlayerApi.TrackingData head = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head);

                    localRayOrigin = referenceTransform.InverseTransformPoint(head.position);
                    localRayDirection = referenceTransform.InverseTransformDirection(head.rotation * Vector3.forward);
                }
            }
        }

        private void FixedUpdate()
        {
            Transform newObject = null;

            if (inVR)
            {
                
            }
            else
            {
                if (isCasting)
                {
                    Ray ray = new Ray(referenceTransform.TransformPoint(localRayOrigin), referenceTransform.TransformDirection(localRayDirection));

                    if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
                    {
                        if (Input.GetMouseButtonDown(1))
                        {
                            Debug.Log(hit.collider.transform.name);
                        }

                        if (hit.collider != null) //At least VRChat client sim canvas hit collider somehow null
                        {
                            Transform hitObject = hit.collider.transform;

                            if (hitObject.transform.GetComponent<HighlightController>())
                            {
                                newObject = hitObject;
                            }
                        }
                    }
                }

                if (newObject != null)
                {
                    if (newObject != previousObject)
                    {
                        InputManager.EnableObjectHighlight(newObject.gameObject, true);
                        if (previousObject) InputManager.EnableObjectHighlight(previousObject.gameObject, false);
                    }
                }
                else
                {
                    if (previousObject)
                    {
                        InputManager.EnableObjectHighlight(previousObject.gameObject, false);
                    }
                }
            }

            previousObject = newObject;
        }

        //VRChat functions
        public override void InputUse(bool value, UdonInputEventArgs args)
        {
            if (!inVR) return;

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