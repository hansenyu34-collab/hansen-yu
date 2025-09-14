using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class SoundClips
{
    public AudioClip shootingSound;
    public AudioClip silentShootSound;
    public AudioClip reloadSound;
    public AudioClip reloadSoundNoAmmo;
    public AudioClip aimSound;
}

public abstract class Weapon : MonoBehaviour
{
    public abstract void GunFire();
    public abstract void Reload();
    public abstract void AimIn();
    public abstract void AimOut();
    public abstract void ExpandingCrossUpdate(float expandDegree);
    public abstract void DoReloadAnimation();

}
