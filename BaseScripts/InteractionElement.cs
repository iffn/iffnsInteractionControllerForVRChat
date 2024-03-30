using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace iffnsStuff.iffnsVRCStuff.InteractionController
{
    public enum interactionSources
    {
        desktop,
        leftPalm,
        rightPalm,
        leftIndex,
        rightIndex
    }

    [DefaultExecutionOrder(ExecutionOrder)]
    public abstract class InteractionElement : UdonSharpBehaviour
    {
        public const int ExecutionOrder = InteractionController.ExecutionOrder + 1;

        [SerializeField] GameObject highlightObject;
        [SerializeField] Collider[] linkedColliders;

        public bool InteractionCollidersEnabled
        {
            set
            {
                foreach (Collider collider in linkedColliders)
                {
                    collider.enabled = value;
                }
            }
        }

        protected bool inVR;
        protected VRCPlayerApi localPlayer;

        protected virtual void Setup()
        {
            inVR = Networking.LocalPlayer.IsUserInVR();
            localPlayer = Networking.LocalPlayer;
        }

        private void Start()
        {
            Setup();
        }

        public abstract void InteractionStart(Vector3 rayWorldOrigin, Vector3 rayWorldDirection);
        public abstract void InteractionStart(Vector3 worldPosition);
        public abstract void InteractionStop();

        public abstract void UpdateElement(Vector3 rayWorldOrigin, Vector3 rayWorldDirection);
        public abstract void UpdateElement(Vector3 worldPosition);

        public bool Highlight
        {
            set
            {
                InputManager.EnableObjectHighlight(highlightObject, value);
            }
        }
    }
}