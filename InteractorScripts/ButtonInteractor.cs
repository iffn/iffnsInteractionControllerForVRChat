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

        public override void InteractionStart(InteractionController linkedInteractionController)
        {
            pressed = true;
        }

        public override void InteractionStop()
        {
            pressed = false;
        }
    }
}