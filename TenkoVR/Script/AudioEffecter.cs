using UnityEngine;
using UnityEngine.Audio;

/// <summary>
///  �X���[���̃I�[�f�B�I�G�t�F�N�g�Ǘ��N���X
/// </summary>
public class AudioEffecter : MonoBehaviour // �}�W�b�N�i���o�[��const��
{
    [Header("AudioMixer��o�^")]
    [SerializeField] AudioMixer _audioMixier;
    [Header("�Đ����x�̉���")]
    [SerializeField] float _slowPitch = 0.5f;
    [Header("Reberb�̋���(0�ɋ߂��قǋ���)")]
    [SerializeField] float _reverbValue = 0f;


    // 1���ʏ�0�ɋ߂��قǒx��
    public void SetSlowEffect(float value) 
    {
        float pitchValue = Mathf.Lerp(_slowPitch, 1f, value); // slowPitch�̒l��1.0�ɕϊ�
       
        // �l�𔽉f
        _audioMixier.SetFloat("PitchShifter", pitchValue);
        _audioMixier.SetFloat("Pitch", pitchValue);
        // �f�V�x���ɕϊ����Ĕ��f
        _audioMixier.SetFloat("ReverbLevel", ConvertVolume2dB(1 - value));
    }

    // 0����1�̒l���f�V�x����-80����0�ɕϊ�
    float ConvertVolume2dB(float value)
    {
        return Mathf.Clamp(20f * Mathf.Log10(Mathf.Clamp01(value)), -80f, _reverbValue);
    }
}
