using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using DG.Tweening;

/// <summary>
/// �X���[���o�Ǘ��N���X
/// </summary>
public class TimeManager : MonoBehaviour
{
    [Header("�X���[�t���O���Ȃ��Ȃ��Ă��X���[��ێ����Ă�������")]
    [SerializeField] float _timeKeepCount = 0.1f;
    [Header("�X���[����timescale�̒l")]
    [SerializeField] float _slowSpeed = 0.05f;
    [Header("�X���[�ɂȂ肫��܂ł̎���")]
    [SerializeField] float _slowDuration = 0.5f; 
    [SerializeField] PostProcessVolume _postProcess; // �G�t�F�N�g��������|�X�g�v���Z�X
    [SerializeField] AudioEffecter _audioEffecter;

    // private�ϐ�
    float _toggleTimer; // �X���[���肪�Ȃ��Ȃ��Ă���莞�ԃX���[���ێ����Ă�������
    bool _isSlow = false; // �X���[����
    float _currentSpeed; // ���݂�TimeScale�̒l
    Tweener _tweener; 
    Vignette _vignette;


    void Start()
    {
        // �ϐ��̏������ƃ|�X�g�v���Z�X��vignette���擾
        _currentSpeed = 1f;
        _vignette = _postProcess.profile.GetSetting<Vignette>();
    }

    //update�֐���timescale�̉e�����󂯂�
    void Update()
    {
        // timeKeepCount��0�ɂȂ�܂ŃX���[��ێ����Ă���X���[����
        if (_toggleTimer > 0)
        {
            _toggleTimer -= Time.deltaTime;
        }
        else if(_toggleTimer <= 0 && _isSlow)
        {
            SlowToggle(false);
        }
    }

    public void SlowChecker()// �X���[�������΂̌��J�֐�
    {
        SlowToggle(true);
        _toggleTimer = _timeKeepCount;

    }
    void SlowToggle(bool slow)
    {
        
        if(slow && !_isSlow) // �x������
        {
            _tweener.Kill(); // ���x��߂������𒆒f
            _isSlow = true;
            //������葬�x�ύX
            _tweener = DOVirtual.Float(_currentSpeed, _slowSpeed, _slowDuration, value => 
               {
                   Time.timeScale = value;
                   _currentSpeed = value;
                   // vignette�̒l��ύX
                   _vignette.intensity.Override(1.0f - value);
                   AudioSlower(value);
               });
        }
        else if(!slow && _isSlow) // ���x��߂�
        {
            _tweener.Kill();�@// ���x�������������𒆒f
            _isSlow = false;
            //������葬�x�ύX
            _tweener = DOVirtual.Float(_currentSpeed, 1f, _slowDuration, value =>
            {
                Time.timeScale = value;
                _currentSpeed = value;
                // vignette�̒l��ύX
                _vignette.intensity.Override(1.0f - value);
                AudioSlower(value);
            });
        }

        void AudioSlower(float slowValue)// AudioManager�Ƀ^�C���X�P�[���̒l�𑗂���ʉ��ɂ��������s��
        {
            _audioEffecter.SetSlowEffect(slowValue);
        }
    }
}
