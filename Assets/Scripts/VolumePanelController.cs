using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VolumePanelController : MonoBehaviour, VolumeRemoteControl
{
   public Image sound_image, noSound_image;

    public Image target_image;
    public Sprite sound_sprite, noSound_sprite;

    public bool spriteChange = false;

    void Awake()
    {
        AudioController.Instance.RegisterRemoteVolume(this);
        ChangeState(AudioController.Instance.IsVoiceEnabled());
    }

    public void ChangeState(bool enable){

        if (spriteChange)
        {
            target_image.sprite = enable ? sound_sprite : noSound_sprite; 
            return;
        }

       sound_image.enabled = enable;
       noSound_image.enabled = !enable;
   }
       
    public void UpdateVolumeState(bool state)
    {
        ChangeState(state);
    }
}
