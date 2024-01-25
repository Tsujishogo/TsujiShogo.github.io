using UnityEngine;
using UnityEngine.Audio;

/// <summary>
///  スロー時のオーディオエフェクト管理クラス
/// </summary>
public class AudioEffecter : MonoBehaviour // マジックナンバーをconstに
{
    [Header("AudioMixerを登録")]
    [SerializeField] AudioMixer _audioMixier;
    [Header("再生速度の下限")]
    [SerializeField] float _slowPitch = 0.5f;
    [Header("Reberbの強さ(0に近いほど強い)")]
    [SerializeField] float _reverbValue = 0f;


    // 1が通常0に近いほど遅い
    public void SetSlowEffect(float value) 
    {
        float pitchValue = Mathf.Lerp(_slowPitch, 1f, value); // slowPitchの値を1.0に変換
       
        // 値を反映
        _audioMixier.SetFloat("PitchShifter", pitchValue);
        _audioMixier.SetFloat("Pitch", pitchValue);
        // デシベルに変換して反映
        _audioMixier.SetFloat("ReverbLevel", ConvertVolume2dB(1 - value));
    }

    // 0から1の値をデシベルの-80から0に変換
    float ConvertVolume2dB(float value)
    {
        return Mathf.Clamp(20f * Mathf.Log10(Mathf.Clamp01(value)), -80f, _reverbValue);
    }
}
