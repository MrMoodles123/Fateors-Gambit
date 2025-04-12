using UnityEngine;

public class AnnouncerAudio : MonoBehaviour
{
    [SerializeField] AudioSource clip;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnEnable()
    {
        clip.Play();
    }
}
