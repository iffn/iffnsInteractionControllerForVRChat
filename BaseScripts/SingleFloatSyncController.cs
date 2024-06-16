using iffnsStuff.iffnsVRCStuff.InteractionController;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class SingleFloatSyncController : UdonSharpBehaviour
{
    [UdonSynced] float syncedValue;
    [SerializeField] UdonBehaviour CurrentControlValueReceiver;
    
    //Runtime values
    public float MainValue { get; private set; }

    float valueVelocity;
    bool locallyOwned = true;
    bool smoothingActive;
    float nextSyncTime;
    bool serializationRequested = false;
    float smoothTimeFinish;

    //Fixed values
    const float smoothTime = 0.05f;
    const float timeBetweenSync = 1f / 6f;
    const char newLine = '\n';

    string debugState()
    {
        string returnString = $"{nameof(SingleFloatSyncController)}:" + newLine;

        returnString += $"{nameof(syncedValue)} = {syncedValue}" + newLine;
        returnString += $"{nameof(MainValue)} = {MainValue}" + newLine;

        return returnString;
    }

    //Internal functions
    void InformReceiver()
    {
        CurrentControlValueReceiver.SetProgramVariable($"ControlValueFromSync", MainValue);
        CurrentControlValueReceiver.SendCustomEvent("ControlValueFromSyncUpdated");
    }

    void SmoothValue()
    {
        if (!smoothingActive)
        {
            return;
        }

        if (Time.time < smoothTimeFinish)
        {
            MainValue = Mathf.SmoothDamp(MainValue, syncedValue, ref valueVelocity, smoothTime);
            InformReceiver();
        }
        else
        {
            smoothingActive = false;
            MainValue = syncedValue;
            InformReceiver();
        }
    }

    void SyncValue()
    {
        if (nextSyncTime < Time.time && syncedValue != MainValue && !serializationRequested)
        {
            serializationRequested = true;
            RequestSerialization();
            nextSyncTime = Time.time + timeBetweenSync;
        }
    }

    void EnsureOwnership()
    {
        if (!Networking.IsOwner(gameObject)) Networking.SetOwner(Networking.LocalPlayer, gameObject); //Not caching local player since the call could come before the assignment.
    }

    //Unity functions
    void Start()
    {
        locallyOwned = Networking.IsOwner(gameObject);
    }

    private void Update()
    {
        if (locallyOwned)
        {
            SyncValue();
        }
        else
        {
            SmoothValue();
        }
    }

    //VRChat functions
    public override void OnPreSerialization()
    {
        serializationRequested = false;
        base.OnPreSerialization();
        syncedValue = MainValue;
    }

    public override void OnDeserialization()
    {
        smoothingActive = true;
        smoothTimeFinish = Time.time + smoothTime * 10;
    }

    public override void OnOwnershipTransferred(VRCPlayerApi player)
    {
        base.OnOwnershipTransferred(player);

        locallyOwned = player.isLocal;
    }

    //Public access
    public void SetValueAndSync(float value)
    {
        MainValue = value;

        EnsureOwnership();
    }
}
