using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace iffnsStuff.iffnsVRCStuff.InteractionController
{
    public class InteractionCollider : UdonSharpBehaviour
    {
        [SerializeField] InteractionElement linkedInteractionElement;

        public bool SetupIsValid
        {
            get
            {
                if (!linkedInteractionElement) return false;

                return true;
            }
        }

        void Start()
        {
            if (!SetupIsValid)
            {
                Debug.LogWarning($"Error: Setup of {transform.name} is incorrect. Disabling");
                gameObject.SetActive(false);
                return;
            }
        }

        public InteractionElement LinkedInteractionElement
        {
            get
            {
                return linkedInteractionElement;
            }
        }
    }
}