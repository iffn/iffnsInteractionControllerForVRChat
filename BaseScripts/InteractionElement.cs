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

    public abstract class InteractionElement : UdonSharpBehaviour
    {
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

        private void Start()
        {
            inVR = Networking.LocalPlayer.IsUserInVR();
            localPlayer = Networking.LocalPlayer;
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