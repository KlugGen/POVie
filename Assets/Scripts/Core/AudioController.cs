using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class AudioController : Singleton<AudioController>
{
    public AudioSource audioSource;
    public AudioSource effectsSource;
    public AudioSource ambientSource;

    public AudioMixer mixer;
    public AudioMixerSnapshot mainSnapshot, effectsSnapshot;
    public AudioMixerSnapshot ambientSnapshot;

    public AudioClip ambientClip;
    public AudioClip engagingAmbientClip;
    public AudioClip entertainingAmbientClip;
    public AudioClip relaxingAmbientClip;

    public AudioClip lockingClip;

    public AudioClip vicotryClip, looseClip;

    public AudioMixerGroup masterGroup;
    public AudioMixer masterMixer;

    public List<AudioClip> ambientSounds = new List<AudioClip>();

    private Coroutine lastAudioCoroutine = null;

    public List<VolumeRemoteControl> registeredVolumes = new List<VolumeRemoteControl>();

    private bool isEnabled = true;

    void Start()
    {
        ChangeAmbientSoundtrack();

        int audio = PlayerPrefs.GetInt("audio", 1);

        // reverse values for next method which reverse it again
        if(audio == 1)
        {
            isEnabled = false;
        }
        else isEnabled = true;

        ChangeState();
    }

    public void InitializeAudio()
    {

    }

    private int lastEnabledAmbient = -1;

    public void ChangeAmbientSoundtrack()
    {
        int gameMode = UserController.Instance.GetGameModeID();

        if(lastEnabledAmbient == gameMode)
        {
            return;
        }

        switch (gameMode)
        {
            case 0:
                audioSource.clip = ambientClip;
                break;
            case 1:
                //engaging
                audioSource.clip = engagingAmbientClip;
                break;
            case 2:
                // entertainging
                audioSource.clip = entertainingAmbientClip;
                break;
            case 3:
                // relaxing
                audioSource.clip = relaxingAmbientClip;
                break;
        }

       
        audioSource.Play();
    }

    public void SetAudioState(bool audioEnabled)
    {
        isEnabled = audioEnabled;
        UpdateAudioState();
    }

    public bool IsVoiceEnabled()
    {
        return isEnabled;
    }

    public Sprite noSound_sprite, sound_image;

    public void ChangeState()
    {
        isEnabled = !isEnabled;
        UpdateAudioState();
    }

    public void Mute()
    {
        isEnabled = false;
        UpdateAudioState();
    }

    public void UnMute()
    {
        isEnabled = true;
        UpdateAudioState();
    }

    private void UpdateAudioState()
    {
        //volumePanels.ForEach(o => o.ChangeState(isEnabled));

        if (isEnabled)
        {
            //audioSource.volume = 1.0f;
            // masterGroup.
            masterGroup.audioMixer.SetFloat("masterVolume", 1f);
            //masterMixer.SetFloat("volume", 1f);
        }
        else
        {
            //audioSource.volume = 0.0f;
            masterGroup.audioMixer.SetFloat("masterVolume", -80f);
            //masterMixer.SetFloat("volume", -80f);
        }

        if (isEnabled)
            PlayerPrefs.SetInt("audio", 1);
        else
            PlayerPrefs.SetInt("audio", 0);

        registeredVolumes.ForEach(o => o.UpdateVolumeState(isEnabled));
    }

    public void RegisterRemoteVolume(VolumeRemoteControl vControl)
    {
        if (!registeredVolumes.Contains(vControl))
            registeredVolumes.Add(vControl);
    }

    private Elements.ElementsDatabase.AmbientSound lastAmbientSound = Elements.ElementsDatabase.AmbientSound.UNDEFINED;
    private Elements.ElementsDatabase.AmbientSound lastDefinedAmbientSound = Elements.ElementsDatabase.AmbientSound.UNDEFINED;
    

    public void ChangeAmbient(Elements.ElementsDatabase.AmbientSound sound = Elements.ElementsDatabase.AmbientSound.UNDEFINED, float audioOffset = 0f)
    {
        //"Change ambient game".LogDev();
        if (sound == Elements.ElementsDatabase.AmbientSound.UNDEFINED)
        {           
            //audioSource.clip = ambientClip;
          
            afterTransitionAction = delegate ()
            {
                ambientSource.Stop();
            };
            //effectsSnapshot.TransitionTo(.01f);
            if (lastAudioCoroutine != null)
                StopCoroutine(lastAudioCoroutine);

            lastAudioCoroutine= StartCoroutine(PlaySnapshotDelayed(.1f, mainSnapshot, 0.5f));  
            
            if(lastAmbientSound != sound)
                audioSource.Play();
        }
        else
        {
            //"Ambient defined".LogDev();
           // lastDefinedAmbientSound.LogDev("Last ambient: ");
            //sound.LogDev("New ambient: ");

            afterTransitionAction = delegate ()
            {
                audioSource.Pause();
            };

            if (lastDefinedAmbientSound != sound)
            {
                ambientSource.clip = ambientSounds[(int)sound];
            }

            mainSnapshot.TransitionTo(.01f);

            if (lastAudioCoroutine != null)
                StopCoroutine(lastAudioCoroutine);

            lastAudioCoroutine = StartCoroutine(PlaySnapshotDelayed(.1f, ambientSnapshot, 0.5f));

            if (audioOffset != 0)
            {
                if (ambientSource.clip.length > audioOffset)
                    ambientSource.time = 15f;
            }
            ambientSource.Play();
            lastDefinedAmbientSound = sound;
        }

        lastAmbientSound = sound;
    }

    public void PlayVictory()
    {
        if (!isEnabled)

            return;

        effectsSnapshot.TransitionTo(1f);
        effectsSource.clip = vicotryClip;
        effectsSource.Play();

        if (lastAudioCoroutine != null)
            StopCoroutine(lastAudioCoroutine);

        lastAudioCoroutine = StartCoroutine(PlaySnapshotDelayed(effectsSource.clip.length, mainSnapshot,.5f));

        afterTransitionAction = delegate ()
        {
            //"Victory delegate".LogDev();
            ChangeAmbient(Elements.ElementsDatabase.AmbientSound.UNDEFINED);
        };
    }

    public void PlayLockingTone()
    {
        if (!isEnabled)
            return;

        //effectsSnapshot.TransitionTo(1f);
        effectsSource.clip = lockingClip;        
        effectsSource.Play();

        //if (lastAudioCoroutine != null)
        //    StopCoroutine(lastAudioCoroutine);

        //lastAudioCoroutine = StartCoroutine(PlaySnapshotDelayed(effectsSource.clip.length, mainSnapshot, .5f));

        //afterTransitionAction = delegate ()
        //{
        //    //"Victory delegate".LogDev();
        //    ChangeAmbient(Elements.ElementsDatabase.AmbientSound.UNDEFINED);
        //};
    }

    public void PlayLoose()
    {
        if (!isEnabled)
            return;

        effectsSnapshot.TransitionTo(1f);
        effectsSource.clip = looseClip;        
        effectsSource.Play();        
        //StartCoroutine(PlaySnapshotDelayed(effectsSource.clip.length, mainSnapshot));
    }

    private UnityEngine.Events.UnityAction afterTransitionAction = null;

    private IEnumerator PlaySnapshotDelayed(float sec, AudioMixerSnapshot snapshot, float transitionTime = 1f)
    {
        yield return new WaitForSeconds(sec);
        snapshot.TransitionTo(transitionTime);
        yield return new WaitForSeconds(transitionTime);
        if(afterTransitionAction != null)
        {
            afterTransitionAction.Invoke();
            afterTransitionAction = null;
        }
        //"Audio coroutine end".LogDev();
    }
}

public interface VolumeRemoteControl
{
    void UpdateVolumeState(bool state);
}
