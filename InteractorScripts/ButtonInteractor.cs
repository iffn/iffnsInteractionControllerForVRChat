using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace iffnsStuff.iffnsVRCStuff.InteractionController
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class ButtonInteractor : InteractionElement
    {
        bool pressed;

        [SerializeField] UdonSharpBehaviour[] linkedInteractionReceiverElements;
        [SerializeField] string stateChangeMessage;

        public bool Pressed
        {
            get
            {
                return pressed;
            }
            set
            {
                pressed = value;
            }
        }

        void SendMessage()
        {
            foreach(UdonSharpBehaviour linkedReceiverElement in linkedInteractionReceiverElements)
            {
                linkedReceiverElement.SendCustomEvent(stateChangeMessage);
            }
        }

        void startFunciton()
        {
            pressed = true;
            SendMessage();
        }

        public override void InteractionStart(Vector3 rayWorldOrigin, Vector3 rayWorldDirection)
        {
            startFunciton();
        }

        public override void InteractionStart(Vector3 worldPosition)
        {
            startFunciton();
        }

        public override void UpdateElement(Vector3 rayWorldOrigin, Vector3 rayWorldDirection)
        {
            //No update needed
        }

        public override void UpdateElement(Vector3 worldInteractionPoint)
        {
            //No update needed
        }

        public override void InteractionStop()
        {
            pressed = false;
            SendMessage();
        }
    }
}