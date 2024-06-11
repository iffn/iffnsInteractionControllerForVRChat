using Newtonsoft.Json.Linq;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace iffnsStuff.iffnsVRCStuff.InteractionController
{
    public abstract class SingleSyncedFloatInteractor : InteractionElement
    {
        [SerializeField] protected float minControlValue;
        [SerializeField] protected float maxControlValue;
        [SerializeField] protected float minUnityValue;
        [SerializeField] protected float maxUnityValue;
        [SerializeField] protected bool clampValue;
        [SerializeField] SingleFloatSyncController linkedSyncController;

        protected abstract void ApplyUnityValue(float value);
        protected abstract float GetCurrentUnityValueFromUnity();

        float currentControlValue;

        protected override void Setup()
        {
            base.Setup();

            //Setting default value
            if (Networking.IsOwner(linkedSyncController.gameObject))
            {
                CurrentSyncedUnityValue = GetCurrentUnityValueFromUnity();
            }
        }

        protected float CurrentSyncedUnityValue
        {
            get
            {
                return GetUnityValueFromControlValue(currentControlValue);
            }
            set
            {
                if (clampValue) ApplyUnityValue(Mathf.Clamp(value, minUnityValue, maxUnityValue));
                else ApplyUnityValue(value);

                currentControlValue = GetControlValueFromUnityValue(value);
                linkedSyncController.SetValueAndSync(currentControlValue);
            }
        }

        public float CurrentSyncedControlValue
        {
            get
            {
                return currentControlValue;
            }
            set
            {
                float unityValue = GetUnityValueFromControlValue(value);

                if (clampValue) ApplyUnityValue(Mathf.Clamp(unityValue, minUnityValue, maxUnityValue));
                else ApplyUnityValue(unityValue);

                currentControlValue = value;

                linkedSyncController.SetValueAndSync(value);
            }
        }

        public float ControlValueFromSync;
        public void ControlValueFromSyncUpdated()
        {
            float unityValue = GetUnityValueFromControlValue(ControlValueFromSync);

            if (clampValue) ApplyUnityValue(Mathf.Clamp(unityValue, minUnityValue, maxUnityValue));
            else ApplyUnityValue(unityValue);

            currentControlValue = ControlValueFromSync;
        }

        public static float Remap(float minInput, float maxInput, float minOutput, float maxOutput, float currentInput)
        {
            float t = Mathf.InverseLerp(minInput, maxInput, currentInput);
            return Mathf.Lerp(minOutput, maxOutput, t);
        }

        float GetUnityValueFromControlValue(float controlValue)
        {
            return Remap(minControlValue, maxControlValue, minUnityValue, maxUnityValue, controlValue);
        }

        float GetControlValueFromUnityValue(float unityValue)
        {
            return Remap(minUnityValue, maxUnityValue, minControlValue, maxControlValue, unityValue);
        }
    }
}