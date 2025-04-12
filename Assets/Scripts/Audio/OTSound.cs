using Projectiles;
using UnityEngine;

public class OTSound : MonoBehaviour
{

    // references to audio source and music managers
    [SerializeField] AudioSource sound;
    [SerializeField] MusicManager musicManager;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // when the game object is enabled, set the sudio to overtime and play the music
    private void OnEnable()
    {
        musicManager.startOvertimeMusic();
        sound.Play();
    }
}
