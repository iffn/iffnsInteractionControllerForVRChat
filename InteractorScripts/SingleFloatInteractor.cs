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

        public static float Remap(float minInput, float maxInput, float minOutput, float maxOutput, float currentInput)
        {
            float t = Mathf.InverseLerp(minInput, maxInput, currentInput);
            return Mathf.Lerp(minOutput, maxOutput, t);
        }

        protected abstract void SetUnityValue(float value);

        float controlValue;
        protected float currentControlValue
        {
            get
            {
                return controlValue;
            }
            set
            {
                if (clampValue) controlValue = Mathf.Clamp(value, minControlValue, maxControlValue);
                else controlValue = value;
            }
        }

        protected float CurrentUnityValue
        {
            get
            {
                return Remap(minControlValue, maxControlValue, minUnityValue, maxUnityValue, currentControlValue);
            }
            set
            {
                if (clampValue) SetUnityValue(Mathf.Clamp(value, minUnityValue, maxUnityValue));
                else SetUnityValue(value);
            }
        }

        public float CurrentControlValue
        {
            get
            {
                return Remap(minUnityValue, maxUnityValue, minControlValue, maxControlValue, currentControlValue);
            }
            set
            {
                float unityValue = Remap(minControlValue, maxControlValue, minUnityValue, maxUnityValue, value);

                if(clampValue) SetUnityValue(Mathf.Clamp(unityValue, minUnityValue, maxUnityValue));
                else SetUnityValue(unityValue);
            }
        }
    }
}