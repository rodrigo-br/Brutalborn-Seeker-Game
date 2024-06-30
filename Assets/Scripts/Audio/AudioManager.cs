using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Pool;

public class AudioManager : MonoBehaviour
{
    public class AudioSourceAndObject
    {
        public AudioSource AudioSource;
        public GameObject AudioObject;
    }
    [Range(0f, 2f)]
    [SerializeField] private float _masterVolume = 1f;
    [SerializeField] private SoundsCollectionSO _soundsCollectionSO;
    [SerializeField] private AudioMixerGroup _sfxMixerGroup;
    [SerializeField] private AudioMixerGroup _musicMixerGroup;

    private ObjectPool<AudioSourceAndObject> _audioSourcePool;
    private AudioSource _currentMusic;
    private int ENEMY_HITABLE_COLLIDERS_LAYER;

    #region Unity Methods

    private void Start()
    {
        CreateAudioSourcePool();
        GamePlayMusic();
        ENEMY_HITABLE_COLLIDERS_LAYER = LayerMask.NameToLayer("EnemyHitableColliders");
    }

    private void OnEnable()
    {
        Bullet.OnShoot += Bullet_OnShoot;
        PlayerAnimator.OnJump1 += PlayerAnimator_OnJump1;
        PlayerAnimator.OnJump2 += PlayerAnimator_OnJump2;
        PlayerAnimator.OnDash += PlayerAnimator_OnDash;
        PlayerAnimator.OnJetpack += PlayerAnimator_OnJetpack;
        PlayerAnimator.OnFallingGround += PlayerAnimator_OnFallingGround;
        Health.OnDeath += HandleDeath;
        Grenade.OnGrenadeBeep += Grenade_OnGrenadeBeep;
        Grenade.OnGrenadeLaunch += Grenade_OnGrenadeLaunch;
        Grenade.OnGrenadeExplode += Grenade_OnGrenadeExplosion;
        //Enemy.OnHitPlayer += Enemy_OnHitPlayer;
    }

    private void OnDisable()
    {
        Bullet.OnShoot -= Bullet_OnShoot;
        PlayerAnimator.OnJump1 -= PlayerAnimator_OnJump1;
        PlayerAnimator.OnJump2 -= PlayerAnimator_OnJump2;
        PlayerAnimator.OnDash -= PlayerAnimator_OnDash;
        PlayerAnimator.OnJetpack -= PlayerAnimator_OnJetpack;
        PlayerAnimator.OnFallingGround -= PlayerAnimator_OnFallingGround;
        Health.OnDeath -= HandleDeath;
        Grenade.OnGrenadeBeep -= Grenade_OnGrenadeBeep;
        Grenade.OnGrenadeLaunch -= Grenade_OnGrenadeLaunch;
        Grenade.OnGrenadeExplode -= Grenade_OnGrenadeExplosion;
        //Enemy.OnHitPlayer -= Enemy_OnHitPlayer;
    }

    private void CreateAudioSourcePool()
    {
        _audioSourcePool = new ObjectPool<AudioSourceAndObject>(() =>
        {
            GameObject audioObject = new GameObject("Pooled Audio Source");
            AudioSource audioSource = audioObject.AddComponent<AudioSource>();
            return new AudioSourceAndObject { AudioSource = audioSource, AudioObject = audioObject };
        }, audioSourceAndObject =>
        {
            audioSourceAndObject.AudioObject.SetActive(true);
        }, audioSourceAndObject =>
        {
            audioSourceAndObject.AudioObject.SetActive(false);
        }, audioSourceAndObject =>
        {
            Destroy(audioSourceAndObject.AudioObject);
        }, false, 20);
    }

    private IEnumerator ReleaseAfterDelay(AudioSourceAndObject audioSourceAndObject, float delay)
    {
        yield return new WaitForSeconds(delay);
        _audioSourcePool.Release(audioSourceAndObject);
    }

    #endregion

    #region Sound Methods

    private void SoundToPlay(SoundSO audioSO)
    {
        AudioClip clip = audioSO.Clip;
        float pitch = audioSO.Pitch;
        float volume = audioSO.Volume * _masterVolume;
        bool loop = audioSO.Loop;
        AudioMixerGroup audioMixerGroup;
        pitch = RandomizePitch(audioSO, pitch);
        audioMixerGroup = DetermineAudioMixerGroup(audioSO);

        PlaySound(clip, pitch, volume, loop, audioMixerGroup);
    }

    private AudioMixerGroup DetermineAudioMixerGroup(SoundSO audioSO)
    {
        AudioMixerGroup audioMixerGroup = audioSO.AudioType switch
        {
            SoundSO.AudioTypes.SFX => _sfxMixerGroup,
            SoundSO.AudioTypes.Music => _musicMixerGroup,
            _ => null,
        };
        return audioMixerGroup;
    }

    private float RandomizePitch(SoundSO audioSO, float pitch)
    {
        if (audioSO.RandomizePitch)
        {
            float randomPitchModifier = Random.Range(-audioSO.RandomPitchRangeModifier, audioSO.RandomPitchRangeModifier);
            pitch = audioSO.Pitch + randomPitchModifier;
        }

        return pitch;
    }

    private void PlaySound(AudioClip clip, float pitch, float volume, bool loop, AudioMixerGroup audioMixerGroup)
    {
        AudioSourceAndObject audioSourceAndObject = _audioSourcePool.Get();
        AudioSource audioSource = audioSourceAndObject.AudioSource;
        audioSource.playOnAwake = false;
        audioSource.clip = clip;
        audioSource.pitch = pitch;
        audioSource.volume = volume;
        audioSource.loop = loop;
        audioSource.outputAudioMixerGroup = audioMixerGroup;
        audioSource.Play();

        if (!loop)
        {
            StartCoroutine(ReleaseAfterDelay(audioSourceAndObject, audioSource.clip.length));
        }

        DetermineMusic(audioMixerGroup, audioSource);
    }

    private void DetermineMusic(AudioMixerGroup audioMixerGroup, AudioSource audioSource)
    {
        if (audioMixerGroup != _musicMixerGroup) { return; }

        if (_currentMusic != null)
        {
            _currentMusic.Stop();
        }

        _currentMusic = audioSource;
    }

    private void PlayRandomSound(SoundSO[] audios)
    {
        if (audios == null || audios.Length == 0) { return; }
        SoundSO audioSO = audios[Random.Range(0, audios.Length)];
        SoundToPlay(audioSO);
    }

    #endregion

    #region SFX

    private void Bullet_OnShoot(SoundSO soundSO)
    {
        SoundToPlay(soundSO);
    }

    private void PlayerAnimator_OnJump1()
    {
        PlayRandomSound(_soundsCollectionSO.Jump1);
    }

    private void PlayerAnimator_OnJump2()
    {
        PlayRandomSound(_soundsCollectionSO.Jump2);
    }

    private void PlayerAnimator_OnDash()
    {
        PlayRandomSound(_soundsCollectionSO.Dash);
    }

    private void PlayerAnimator_OnJetpack(bool jetpack)
    {
        if (jetpack)
        {
            PlayRandomSound(_soundsCollectionSO.Jetpack);
        }
    }

    private void PlayerAnimator_OnFallingGround()
    {
        PlayRandomSound(_soundsCollectionSO.FallingGround);
    }

    private void Health_OnDeath(Health health)
    {
        PlayRandomSound(_soundsCollectionSO.BodyPop);
    }

    private void Health_OnDeath()
    {
        PlayRandomSound(_soundsCollectionSO.BodyPop);
    }

    private void PlayerController_OnJetpack()
    {
        PlayRandomSound(_soundsCollectionSO.Jetpack);
    }

    private void Grenade_OnGrenadeBeep()
    {
        PlayRandomSound(_soundsCollectionSO.GrenadeBeep);
    }

    private void Grenade_OnGrenadeLaunch()
    {
        PlayRandomSound(_soundsCollectionSO.GrenadeLaunch);
    }

    private void Grenade_OnGrenadeExplosion()
    {
        PlayRandomSound(_soundsCollectionSO.GrenadeExplosion);
    }

    private void Enemy_OnHitPlayer()
    {
        PlayRandomSound(_soundsCollectionSO.PlayerHit);
    }

    private void AudioManager_MegaKill()
    {
        PlayRandomSound(_soundsCollectionSO.MegaKill);
    }

    #endregion

    #region Music

    private void GamePlayMusic()
    {
        PlayRandomSound(_soundsCollectionSO.GamePlayMusic);
    }

    #endregion

    #region Custom SFX Logic

    private List<Health> _deathList = new List<Health>();
    private Coroutine _deathCoroutine;

    private void HandleDeath(Health health)
    {
        bool isEnemy = health.gameObject.layer == ENEMY_HITABLE_COLLIDERS_LAYER;

        if (isEnemy)
        {
            _deathList.Add(health);
        }

        if (_deathCoroutine == null)
        {
            _deathCoroutine = StartCoroutine(DeathWindowRoutine());
        }
    }

    private IEnumerator DeathWindowRoutine()
    {
        yield return null;

        int megaKillAmount = 3;
        if (_deathList.Count >= megaKillAmount)
        {
            AudioManager_MegaKill();
        }

        Health_OnDeath();
        _deathList.Clear();
        _deathCoroutine = null;
    }

    #endregion
}