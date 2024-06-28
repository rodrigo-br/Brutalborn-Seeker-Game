using UnityEngine;

[CreateAssetMenu()]
public class SoundsCollectionSO : ScriptableObject
{
    [Header("Music")]
    public SoundSO[] GamePlayMusic;
    public SoundSO[] UIMusic;
    public SoundSO[] StartScreenMusic;
    [Header("SFX")]
    public SoundSO[] Jump;
    public SoundSO[] Splat;
    public SoundSO[] Jetpack;
    public SoundSO[] GrenadeBeep;
    public SoundSO[] GrenadeLaunch;
    public SoundSO[] GrenadeExplosion;
    public SoundSO[] PlayerHit;
    public SoundSO[] MegaKill;
}