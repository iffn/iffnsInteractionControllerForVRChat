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

        public override void InteractionStart(InteractionController linkedInteractionController)
        {
            pressed = true;
            SendMessage();
        }

        public override void InteractionStop()
        {
            pressed = false;
            SendMessage();
        }
    }
}