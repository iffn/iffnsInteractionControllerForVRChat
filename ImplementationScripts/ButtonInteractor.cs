using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace iffnsStuff.iffnsVRCStuff.InteractionController
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class ButtonInteractor : InteractionElement
    {
        public override void InteractionStart(InteractionController linkedInteractionController)
        {
            Debug.Log("Button Click Start");
        }

        public override void InteractionStop()
        {
            Debug.Log("Button Click Stop");
            
        }
    }
}