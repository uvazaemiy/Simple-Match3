using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;


public class SoundManager : Singleton<SoundManager>
{
    [SerializeField] private AudioClip[] musicClips;
    [SerializeField] private AudioClip[] winClips;
    [SerializeField] private AudioClip[] loseClips;
    [SerializeField] private AudioClip[] bonusClips;
    [Space]
    [Range(0, 1)] 
    [SerializeField] private float musicVolume = 0.5f;
    [Range(0, 1)] 
    public float fxVolume = 1;
    [Range(0.7f, 1)]
    [SerializeField] private float lowPitch = 0.8f;
    [Range(1, 1.3f)]
    [SerializeField] private float highPitch = 1.2f;

    
    
    

    private void Start()
    {
        StartCoroutine(PlayRandomMusicRoutine());
    }

    public AudioSource PlayClipAtPoint(AudioClip clip, Vector3 position, float volume = 1)
    {
        if (clip != null)
        {
            GameObject go = new GameObject("SoundFX " + clip.name);
            go.transform.position = position;

            AudioSource source = go.AddComponent<AudioSource>();
            source.clip = clip;

            float randomPitch = Random.Range(lowPitch, highPitch);
            source.pitch = randomPitch;
            source.volume = volume;

            source.Play();
            Destroy(go, clip.length);
            return source;
        }

        return null;
    }

    private AudioSource PlayRandom(AudioClip[] clips, Vector3 position, float volume = 1)
    {
        if (clips != null)
        {
            if (clips.Length != 0)
            {
                int randomIndex = Random.Range(0, clips.Length);

                if (clips[randomIndex] != null)
                {
                    AudioSource source = PlayClipAtPoint(clips[randomIndex], position, volume);
                    return source;
                }
            }
        }

        return null;
    }

    
    
    
    
    private IEnumerator PlayRandomMusicRoutine()
    {
        AudioSource music = PlayRandom(musicClips, Vector3.zero, musicVolume);
        yield return new WaitForSeconds(music.clip.length);
        StartCoroutine(PlayRandomMusicRoutine());
    }

    public void PlayWinSound()
    {
        PlayRandom(winClips, Vector3.zero, fxVolume);
    }

    public void PlayLoseSound()
    {
        PlayRandom(loseClips, Vector3.zero, fxVolume * 0.1f);
    }

    public void PlayBonusSound()
    {
        PlayRandom(bonusClips, Vector3.zero, fxVolume);
    }
}
