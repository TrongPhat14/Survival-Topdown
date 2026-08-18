using System;
using UnityEngine;

[DisallowMultipleComponent]
public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [SerializeField] private AudioClip[] tracks;
    [SerializeField] private bool playOnStart = true;
    [SerializeField] private bool shuffle;
    [SerializeField] private bool loopPlaylist = true;
    [SerializeField, Range(0f, 1f)] private float volume = 0.6f;

    private AudioSource source;
    private int currentTrackIndex = -1;
    private bool shouldPlay;
    private bool isPaused;

    public event Action<AudioClip> TrackChanged;

    public AudioClip CurrentTrack => source != null ? source.clip : null;
    public float Volume => volume;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
        source = gameObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = false;
        source.spatialBlend = 0f;
        source.volume = volume;
    }

    private void Start()
    {
        if (playOnStart)
        {
            PlayNext();
        }
    }

    private void Update()
    {
        if (shouldPlay && !isPaused && source != null && !source.isPlaying)
        {
            PlayNext();
        }
    }

    public bool PlayTrack(int index)
    {
        if (tracks == null ||
            index < 0 ||
            index >= tracks.Length ||
            tracks[index] == null)
        {
            return false;
        }

        currentTrackIndex = index;
        shouldPlay = true;
        isPaused = false;
        source.clip = tracks[index];
        source.Play();
        TrackChanged?.Invoke(source.clip);
        return true;
    }

    public bool PlayNext()
    {
        if (tracks == null || tracks.Length == 0)
        {
            shouldPlay = false;
            return false;
        }

        int startIndex = currentTrackIndex;

        for (int attempt = 0; attempt < tracks.Length; attempt++)
        {
            int nextIndex = GetNextIndex();

            if (!loopPlaylist &&
                !shuffle &&
                startIndex >= 0 &&
                nextIndex <= startIndex)
            {
                Stop();
                return false;
            }

            if (PlayTrack(nextIndex))
            {
                return true;
            }
        }

        shouldPlay = false;
        return false;
    }

    public void Pause()
    {
        if (source == null || !source.isPlaying)
        {
            return;
        }

        source.Pause();
        isPaused = true;
    }

    public void Resume()
    {
        if (source == null || !isPaused)
        {
            return;
        }

        source.UnPause();
        isPaused = false;
        shouldPlay = true;
    }

    public void Stop()
    {
        shouldPlay = false;
        isPaused = false;
        source?.Stop();
    }

    public void SetVolume(float value)
    {
        volume = Mathf.Clamp01(value);

        if (source != null)
        {
            source.volume = volume;
        }
    }

    private int GetNextIndex()
    {
        if (!shuffle || tracks.Length == 1)
        {
            return (currentTrackIndex + 1) % tracks.Length;
        }

        int nextIndex = UnityEngine.Random.Range(0, tracks.Length);
        if (nextIndex == currentTrackIndex)
        {
            nextIndex = (nextIndex + 1) % tracks.Length;
        }

        return nextIndex;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
