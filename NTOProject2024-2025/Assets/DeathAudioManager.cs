using UnityEngine;

public class DeathAudioManager : MonoBehaviour
{
    public GameEvent StartExtremeConditionsEvent; 
    public GameEvent EndExtremeConditionsEvent;
    public GameEvent SafeZoneConditions;
    [SerializeField] AudioSource Sound;
    [SerializeField] AudioSource Music;
    void Start()
    {
        
    }

    public void DeathSound()
    {
        Music.Stop();
        Sound.Play();
    }
    public void DeathMusic()
    {
        if(!Music.isPlaying){
            Music.Play();
        }
    }
    public void SafeZoneConditionsFunc(){
        Music.Stop();  
        Sound.Stop();
    }
}
