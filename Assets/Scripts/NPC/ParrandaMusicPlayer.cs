using System.Collections.Generic;
using UnityEngine;

/// <summary>One clock and one shuffled playlist for every speaker in this scene.</summary>
public sealed class ParrandaMusicPlayer : MonoBehaviour
{
    [SerializeField] private AudioClip[] songs;
    [SerializeField] private AudioSource[] speakers;
    [SerializeField, Min(0.1f)] private float schedulingLead = 1f;

    private AudioSource[][] banks;
    private readonly List<int> bag = new List<int>();
    private int previousSong = -1;
    private int nextBank;
    private double nextStart;

    private void OnEnable()
    {
        if (songs == null || songs.Length == 0 || speakers == null || speakers.Length == 0)
        {
            Debug.LogError("Parranda: faltan canciones o baffles asignados.", this);
            enabled = false;
            return;
        }

        foreach (var song in songs)
            if (song == null || song.samples <= 0 || song.frequency <= 0)
            {
                Debug.LogError("Parranda: la lista contiene una cancion invalida.", this);
                enabled = false;
                return;
            }

        foreach (var speaker in speakers)
            if (speaker == null)
            {
                Debug.LogError("Parranda: falta una fuente de audio.", this);
                enabled = false;
                return;
            }

        if (banks == null)
        {
            banks = new[] { speakers, new AudioSource[speakers.Length] };
            for (int i = 0; i < speakers.Length; i++)
            {
                var source = speakers[i];
                source.playOnAwake = false;
                source.loop = false;
                source.pitch = 1f;
                source.dopplerLevel = 0f;
                var standby = source.gameObject.AddComponent<AudioSource>();
                standby.playOnAwake = false;
                standby.loop = false;
                standby.outputAudioMixerGroup = source.outputAudioMixerGroup;
                standby.volume = source.volume;
                standby.spatialBlend = source.spatialBlend;
                standby.rolloffMode = source.rolloffMode;
                standby.minDistance = source.minDistance;
                standby.maxDistance = source.maxDistance;
                standby.spread = source.spread;
                standby.dopplerLevel = 0f;
                standby.priority = source.priority;
                banks[1][i] = standby;
            }
        }
        RestartSchedule();
    }

    private void Update()
    {
        if (AudioListener.pause) return;
        double now = AudioSettings.dspTime;
        // Recover together if a long editor/application suspension missed a transition.
        if (now >= nextStart)
            RestartSchedule();
        else if (now + schedulingLead >= nextStart)
            ScheduleNext();
    }

    private void RestartSchedule()
    {
        StopSources();
        nextBank = 0;
        nextStart = AudioSettings.dspTime + schedulingLead;
        ScheduleNext();
    }

    private void ScheduleNext()
    {
        if (bag.Count == 0)
            for (int i = 0; i < songs.Length; i++) bag.Add(i);

        // Every song plays once per round; also avoid a repeat across round boundaries.
        int choice = Random.Range(0, bag.Count);
        if (bag.Count > 1 && bag[choice] == previousSong)
            choice = (choice + Random.Range(1, bag.Count)) % bag.Count;
        previousSong = bag[choice];
        bag.RemoveAt(choice);
        AudioClip clip = songs[previousSong];
        double end = nextStart + (double)clip.samples / clip.frequency;
        foreach (var source in banks[nextBank])
        {
            source.clip = clip;
            source.PlayScheduled(nextStart);
            source.SetScheduledEndTime(end);
        }
        nextStart = end;
        nextBank = 1 - nextBank;
    }

    [ContextMenu("Siguiente cancion (solo en Play)")]
    private void SkipSong()
    {
        if (Application.isPlaying && isActiveAndEnabled && banks != null)
            RestartSchedule();
    }

    private void StopSources()
    {
        if (banks == null) return;
        foreach (var bank in banks)
            foreach (var source in bank)
                if (source != null) source.Stop();
    }

    private void OnDisable() => StopSources();

    private void OnDestroy()
    {
        if (banks == null) return;
        foreach (var source in banks[1])
            if (source != null) Destroy(source);
    }
}
