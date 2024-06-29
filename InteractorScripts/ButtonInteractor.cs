using UdonSharp;
using UnityEngine;

namespace iffnsStuff.iffnsVRCStuff.InteractionController
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class ButtonInteractor : InteractionElement
    {
        bool pressed;

        [SerializeField] UdonSharpBehaviour[] linkedInteractionReceiverElements;
        [SerializeField] string interactionStartMessage;
        [SerializeField] string interactionStopMessage;

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

        void SendMessageToReceivers(string message)
        {
            foreach(UdonSharpBehaviour linkedReceiverElement in linkedInteractionReceiverElements)
            {
                linkedReceiverElement.SendCustomEvent(message);
            }
        }

        void StartFunciton()
        {
            pressed = true;
            SendMessageToReceivers(interactionStartMessage);
        }

        public override void InteractionStart(Vector3 rayWorldOrigin, Vector3 rayWorldDirection)
        {
            StartFunciton();
        }

        public override void InteractionStart(Vector3 worldPosition)
        {
            StartFunciton();
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
            SendMessageToReceivers(interactionStopMessage);
        }
    }
}