using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace iffnsStuff.iffnsVRCStuff.InteractionController
{
    public abstract class InteractionElement : UdonSharpBehaviour
    {
        [SerializeField] GameObject highlightObject;

        protected bool inVR;
        protected VRCPlayerApi localPlayer;

        private void Start()
        {
            inVR = Networking.LocalPlayer.IsUserInVR();
            localPlayer = Networking.LocalPlayer;
        }

        public abstract void InteractionStart(InteractionController linkedInteractionController);
        public abstract void InteractionStop();

        public void Highlight(bool state)
        {
            InputManager.EnableObjectHighlight(highlightObject, state);
        }
        
    }
}