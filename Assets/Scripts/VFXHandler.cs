using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class VFXhandler : MonoBehaviour
{
    public static VFXhandler Instance;

    private GameObject vfx;
    void MakeInstance()
    {
        if (Instance == null)
        {
            //DontDestroyOnLoad(gameObject);
            Instance = this;
        }
        else if (Instance != null)
        {
            //Destroy(gameObject);
        }
    }
    private void Awake()
    {
        MakeInstance();
    }

    //[Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public GameObject OutNetworkVFX(string vfxname)
    {
        vfx = PlayerVFX.Instance.OutVfx(vfxname);
        return vfx;
    }
}
