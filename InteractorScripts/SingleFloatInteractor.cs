using BestHTTP.SecureProtocol.Org.BouncyCastle.Crypto;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace iffnsStuff.iffnsVRCStuff.InteractionController
{
    public abstract class SingleFloatInteractor : InteractionElement
    {
        [SerializeField] protected float minControlValue;
        [SerializeField] protected float maxControlValue;
        [SerializeField] protected float minUnityValue;
        [SerializeField] protected float maxUnityValue;
        [SerializeField] protected bool clampValue;

        protected abstract void ApplyUnityValue(float value);

        public float currentControlValue;
        
        protected float CurrentUnityValue
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
            }
        }

        public float CurrentControlValue
        {
            get
            {
                return currentControlValue;
            }
            set
            {
                float unityValue = GetUnityValueFromControlValue(value);

                if(clampValue) ApplyUnityValue(Mathf.Clamp(unityValue, minUnityValue, maxUnityValue));
                else ApplyUnityValue(unityValue);

                currentControlValue = value;
            }
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