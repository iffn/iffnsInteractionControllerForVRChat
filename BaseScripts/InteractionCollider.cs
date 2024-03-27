using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace iffnsStuff.iffnsVRCStuff.InteractionController
{
    public abstract class InteractionCollider : UdonSharpBehaviour
    {
        [SerializeField] InteractionElement linkedInteractionElement;

        public abstract bool WorldCollisionPointIsValid(Vector3 worldPosition);

        public bool SetupIsValid
        {
            get
            {
                if (!linkedInteractionElement) return false;

                return true;
            }
        }

        protected virtual void Setup()
        {
            
        }

        void Start()
        {
            if (!SetupIsValid)
            {
                Debug.LogWarning($"Error: Setup of {transform.name} is incorrect. Disabling");
                gameObject.SetActive(false);
                return;
            }

            Setup();
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