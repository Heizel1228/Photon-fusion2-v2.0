using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class VFX
{
    public string name;         
    public GameObject vfx;       
}

public class PlayerVFX : MonoBehaviour
{
    public static PlayerVFX Instance;
    private Dictionary<string, VFX> vfxDictionary;
    public List<VFX> vfxList;

    private void Awake()
    {
        MakeInstance();
        vfxDictionary = new Dictionary<string, VFX>();
        foreach (VFX vfx in vfxList)
        {
            if (!vfxDictionary.ContainsKey(vfx.name) && vfx.vfx != null)
            {
                vfxDictionary[vfx.name] = vfx;
            }
            else
            {
                Debug.LogWarning($"Duplicate or missing VFX name: {vfx.name}");
            }
        }
    }

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

    public GameObject OutVfx(string vfxname)
    {
        if (vfxDictionary.TryGetValue(vfxname, out VFX Vfx))
        {
            return Vfx.vfx;
        }
        else
        {
            Debug.LogWarning($"SFX '{vfxname}' not found!");
            return null;
        }
    }
}
