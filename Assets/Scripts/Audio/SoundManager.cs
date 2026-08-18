using System;
using System.Collections.Generic;
using UnityEngine;

public enum SoundId
{
    Walking,
    Bomb,
    ClickButton,
    Hit,
    LevelUp,
    Victory,
    Lose
}

[DisallowMultipleComponent]
public class SoundManager : MonoBehaviour
{
    [Serializable]
    private sealed class SoundEntry
    {
        [SerializeField] private SoundId id;
        [SerializeField] private AudioClip[] clips;
        [SerializeField, Range(0f, 1f)] private float volume = 1f;
        [SerializeField, Range(0.1f, 3f)] private float minPitch = 0.95f;
        [SerializeField, Range(0.1f, 3f)] private float maxPitch = 1.05f;
        [SerializeField, Range(0f, 1f)] private float spatialBlend;
        [SerializeField, Min(0f)] private float minDistance = 1f;
        [SerializeField, Min(0.1f)] private float maxDistance = 20f;

        [NonSerialized] private int lastClipIndex = -1;

        public SoundId Id => id;
        public float Volume => volume;
        public float Pitch => UnityEngine.Random.Range(
            Mathf.Min(minPitch, maxPitch),
            Mathf.Max(minPitch, maxPitch));
        public float SpatialBlend => spatialBlend;
        public float MinDistance => minDistance;
        public float MaxDistance => Mathf.Max(minDistance, maxDistance);

        public AudioClip GetClip()
        {
            if (clips == null || clips.Length == 0)
            {
                return null;
            }

            if (clips.Length == 1)
            {
                lastClipIndex = 0;
                return clips[0];
            }

            int index = UnityEngine.Random.Range(0, clips.Length);
            if (index == lastClipIndex)
            {
                index = (index + 1) % clips.Length;
            }

            lastClipIndex = index;
            return clips[index];
        }
    }

    public static SoundManager Instance { get; private set; }

    [SerializeField] private SoundEntry[] sounds;
    [SerializeField, Min(1)] private int initialSourceCount = 8;
    [SerializeField, Min(1)] private int maxSourceCount = 24;
    [SerializeField, Range(0f, 1f)] private float masterVolume = 1f;

    private readonly Dictionary<SoundId, SoundEntry> soundLookup = new();
    private readonly Dictionary<SoundId, AudioSource> loopingSources = new();
    private readonly List<AudioSource> sources = new();

    public float MasterVolume => masterVolume;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
        BuildLookup();

        int sourceCount = Mathf.Clamp(initialSourceCount, 1, maxSourceCount);
        for (int i = 0; i < sourceCount; i++)
        {
            CreateSource();
        }
    }

    public static bool Play(SoundId id)
    {
        return Instance != null && Instance.PlayInternal(id, Vector3.zero, true);
    }

    public static bool PlayAt(SoundId id, Vector3 position)
    {
        return Instance != null && Instance.PlayInternal(id, position, false);
    }

    public static bool StartLoop(SoundId id)
    {
        return Instance != null && Instance.StartLoopInternal(id);
    }

    public static void StopLoop(SoundId id)
    {
        Instance?.StopLoopInternal(id);
    }

    public void SetMasterVolume(float value)
    {
        masterVolume = Mathf.Clamp01(value);
    }

    private bool PlayInternal(SoundId id, Vector3 position, bool force2D)
    {
        if (!soundLookup.TryGetValue(id, out SoundEntry entry))
        {
            return false;
        }

        AudioClip clip = entry.GetClip();
        AudioSource source = GetAvailableSource();

        if (clip == null || source == null)
        {
            return false;
        }

        ConfigureSource(source, entry, clip, position, force2D, false);
        source.Play();
        return true;
    }

    private bool StartLoopInternal(SoundId id)
    {
        if (loopingSources.TryGetValue(id, out AudioSource activeSource))
        {
            if (activeSource != null && activeSource.isPlaying)
            {
                return true;
            }

            loopingSources.Remove(id);
        }

        if (!soundLookup.TryGetValue(id, out SoundEntry entry))
        {
            return false;
        }

        AudioClip clip = entry.GetClip();
        AudioSource source = GetAvailableSource();
        if (clip == null || source == null)
        {
            return false;
        }

        ConfigureSource(source, entry, clip, Vector3.zero, true, true);
        loopingSources.Add(id, source);
        source.Play();
        return true;
    }

    private void StopLoopInternal(SoundId id)
    {
        if (!loopingSources.Remove(id, out AudioSource source) || source == null)
        {
            return;
        }

        source.Stop();
        source.loop = false;
        source.clip = null;
    }

    private void ConfigureSource(
        AudioSource source,
        SoundEntry entry,
        AudioClip clip,
        Vector3 position,
        bool force2D,
        bool loop)
    {
        source.transform.position = position;
        source.clip = clip;
        source.loop = loop;
        source.volume = entry.Volume * masterVolume;
        source.pitch = entry.Pitch;
        source.spatialBlend = force2D ? 0f : entry.SpatialBlend;
        source.minDistance = entry.MinDistance;
        source.maxDistance = entry.MaxDistance;
    }

    private void BuildLookup()
    {
        soundLookup.Clear();

        if (sounds == null)
        {
            return;
        }

        foreach (SoundEntry sound in sounds)
        {
            if (sound == null)
            {
                continue;
            }

            if (!soundLookup.TryAdd(sound.Id, sound))
            {
                Debug.LogWarning($"Duplicate SoundId: {sound.Id}.", this);
            }
        }
    }

    private AudioSource GetAvailableSource()
    {
        foreach (AudioSource source in sources)
        {
            if (!source.isPlaying)
            {
                return source;
            }
        }

        return sources.Count < maxSourceCount ? CreateSource() : null;
    }

    private AudioSource CreateSource()
    {
        GameObject sourceObject = new GameObject($"SFX Source {sources.Count + 1}");
        sourceObject.transform.SetParent(transform, false);

        AudioSource source = sourceObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = false;
        source.rolloffMode = AudioRolloffMode.Linear;
        sources.Add(source);
        return source;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }

        foreach (AudioSource source in loopingSources.Values)
        {
            source?.Stop();
        }

        loopingSources.Clear();
        soundLookup.Clear();
        sources.Clear();
    }
}
