using UnityEngine;
namespace Projectiles
{
    public class MusicManager : MonoBehaviour
    {
        // array to hold different audios
        [SerializeField] private AudioSource[] affects;

        // int to know which audio is currently in use
        public int currentMusic;

        // defualt to playing the menu music
        void Start()
        {
            //setSoundLvl(0.3f);
            playMenuMusic();
        }

        // Update is called once per frame
        void Update()
        {

        }


        // plays the audio of the given int position
        public void playMusic(int map)
        {
            affects[currentMusic].FadeOut(this);
            // yield return new WaitForSeconds(affects[currentMusic].DefaultSetup.FadeOut + 0.1f); // Wait for fade-out
            affects[map + 1].FadeIn(this);
            
            currentMusic = map + 1;
        }

        // find the music needed for menu and set current index to play it
        public void playMenuMusic()
        {
            affects[0].FadeIn(this);
            currentMusic = 0;
        }

        // find the music needed for overtime and set the index to play it
        public void startOvertimeMusic()
        {
            affects[currentMusic].FadeOut(this);
            affects[affects.Length - 1].FadeIn(this);
            currentMusic = affects.Length - 1;
        }

        // modifies the volume level for all tracks
        public void setSoundLvl(float volume)
        {
            foreach (AudioSource a in affects)
            {
                a.volume = volume;
            }
        }
        
    }
}