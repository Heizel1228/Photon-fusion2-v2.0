using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class SFX
{
    public string name;          // Name of the sound effect
    public AudioClip clip;       // Reference to the AudioClip
    [Range(0f, 1f)] public float volume = 1f; // Volume for this sound effect (default is 1.0)
}

public class SFXManager : MonoBehaviour
{
    public AudioSource sfxSource;       // AudioSource for playing SFX
    public AudioClip[] OnBeatSFX;        // Clips in SFX list 
    private AudioClip activeSound;      // Clips will be used for random play
    [Range(0f, 1f)] public float OnBeatvolume = 1f;
    public List<SFX> sfxList;           // List of SFX objects

    private Dictionary<string, SFX> sfxDictionary;  // Dictionary to look up SFX by name

    public static SFXManager Instance;

    private void Awake()
    {
        MakeInstance();
        // Initialize the dictionary and populate it with the SFX from the list
        sfxDictionary = new Dictionary<string, SFX>();
        foreach (SFX sfx in sfxList)
        {
            if (!sfxDictionary.ContainsKey(sfx.name) && sfx.clip != null)
            {
                sfxDictionary[sfx.name] = sfx;
            }
            else
            {
                Debug.LogWarning($"Duplicate or missing SFX name: {sfx.name}");
            }
        }

        // Ensure the AudioSource is assigned or create it if missing
        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
        }

        // SFX should not loop and should not play on awake
        sfxSource.loop = false;
        sfxSource.playOnAwake = false;
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

    // Function to play a specific SFX by name
    public void PlaySFX(string sfxName)
    {
        if (sfxDictionary.TryGetValue(sfxName, out SFX sfx))
        {
            // Play the sound with its specific volume using PlayOneShot with the volume parameter
            sfxSource.PlayOneShot(sfx.clip, sfx.volume);
        }
        else
        {
            Debug.LogWarning($"SFX '{sfxName}' not found!");
        }
    }

    public void playRandomSFX()
    {
        activeSound = OnBeatSFX[Random.Range(0, OnBeatSFX.Length)];
        // Play the sound with its specific volume using PlayOneShot with the volume parameter
        sfxSource.PlayOneShot(activeSound, OnBeatvolume);
    }

    // Function to set the volume for a specific SFX (if needed at runtime)
    public void SetSFXVolume(string sfxName, float volume)
    {
        if (sfxDictionary.TryGetValue(sfxName, out SFX sfx))
        {
            sfx.volume = Mathf.Clamp(volume, 0f, 1f);  // Ensure the volume is within range
        }
        else
        {
            Debug.LogWarning($"SFX '{sfxName}' not found!");
        }
    }

    // Optional: Debug method to list all SFX names
    public void ListAllSFX()
    {
        foreach (SFX sfx in sfxList)
        {
            Debug.Log($"SFX Name: {sfx.name}, Volume: {sfx.volume}");
        }
    }
}
