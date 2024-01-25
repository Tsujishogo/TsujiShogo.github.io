using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// �v���C���[���f���̎�𓮂����N���X
/// </summary>

public class FingerMover : MonoBehaviour
{
    [Header("���ꂼ��̓��͂����InputAction��o�^")]
    [SerializeField] InputActionReference _rightTriggerButtonAction;
    [SerializeField] InputActionReference _leftTriggerButtonAction;
    [SerializeField] InputActionReference _rightGripButtonAction;
    [SerializeField] InputActionReference _leftGripButtonAction;
    [SerializeField] InputActionReference _rightThumbButtonAction;
    [SerializeField] InputActionReference _leftThumbButtonAction;
    [SerializeField] InputAction _rightGripAction;

    [Header("�͂�object(��)")]
    [SerializeField] GameObject _grubObject; // ��ɒǏ]������Ώ�
    [Header("��̈ʒu")]
    [SerializeField] GameObject _handPos;
    [Header("�Ǐ]�Ώ�")]
    [SerializeField] GameObject _grubRoot;
    [Header("Animator��isGrub�̃��C���[�̏���")]
    [SerializeField] int _isGrubLayer = 1;
    [Header("�͂ނ��Ƃ��ł��鋗��")]
    [SerializeField] float _grubableDis = 0.05f;
    [Header("�{�^���������݂̂������l")]
    [SerializeField] float _inputThreshold = 0.9f;
    [Header("�R���g���[���[�U����")]
    [SerializeField] bool _isHapitics = true; // �R���g���[���[��U�������邩
    [Header("�U����0�`1f")]
    [SerializeField] float _hapticsAmplitude = 0.5f;
    [Header("�U������")]
    [SerializeField] float _hapticsDuration = 0.2f;

    [Header("hierarchy��̃R���g���[���[Prefab")]
    [SerializeField] ActionBasedController _leftController; 
    [SerializeField] ActionBasedController _rightController;

    // private�ϐ�
    bool _startFlag = false;
    bool _isAnimRight = true; // ����A�j���[�V���������邩�ǂ���
    bool _isAnimLeft = true;
    bool _isGrubed = false; //�����Ă��邩�ǂ���
    Animator _animator; // �v���C���[���f����animator

    // parameter����ID�ɕϊ�
    readonly int _hashLTrigger = Animator.StringToHash("LeftTrigger");
    readonly int _hashLGrip = Animator.StringToHash("LeftGrip");
    readonly int _hashLThumbs = Animator.StringToHash("LeftThumbs");
    readonly int _hashRTrigger = Animator.StringToHash("RightTrigger");
    readonly int _hashRGrip = Animator.StringToHash("RightGrip");
    readonly int _hashRThumbs = Animator.StringToHash("RightThumbs");


    void Awake()
    {
        _animator = GetComponent<Animator>(); // animator�擾
    }

    void Update()
    {
        // ���ꂼ��̃{�^���̉������݂̒l�𔽉f
        if(_isAnimLeft) // ����
        {
            _animator.SetFloat(_hashLTrigger, _leftTriggerButtonAction.action.ReadValue<float>());
            _animator.SetFloat(_hashLGrip, _leftGripButtonAction.action.ReadValue<float>());
            _animator.SetFloat(_hashLThumbs, _leftThumbButtonAction.action.ReadValue<float>());
        }

        if(_isAnimRight) // �E��
        {
            _animator.SetFloat(_hashRTrigger, _rightTriggerButtonAction.action.ReadValue<float>());
            _animator.SetFloat(_hashRGrip, _rightThumbButtonAction.action.ReadValue<float>());
            _animator.SetFloat(_hashRThumbs, _rightGripButtonAction.action.ReadValue<float>());
        }

        float righttriInt = _rightTriggerButtonAction.action.ReadValue<float>();

        //leftController.SendHapticImpulse(hapticsAmplitude, hapticsDuration);
        if (Input.GetKey(KeyCode.X ) || righttriInt > _inputThreshold) 
        {
            GrubObject();
        }
    }

    // Trigger�{�^���̓��͂��擾
    void OnActivate(InputValue value) 
    {
        GrubObject();
    }

    // �͂�object����ɒǏ]������
    void GrubObject()
    {
        if (_isGrubed || StageManager.instance.IsShowUI == false) return;

        // ��ƑΏ�object�̋������v�Z
        float grubDis = Vector3.Distance(_handPos.transform.position, _grubObject.transform.position); 

        if (grubDis < _grubableDis) // �������������l�ȉ���������͂�
        {
            _animator.SetLayerWeight(_animator.GetLayerIndex("IsGrub"), _isGrubLayer); // ���̃��C���[���A�N�e�B�u�ɂ���Ǝ������
                                                                                       // object����̈ʒu�Ɉړ������Ď�ɒǏ]������
            _grubObject.transform.parent = _grubRoot.transform;
            _grubObject.transform.localPosition = Vector3.zero;
            _grubObject.transform.localRotation = Quaternion.identity;
            _isGrubed = true;

            if (!_startFlag) // ��x��������
            {
                _startFlag = true;
                StageManager.instance.GameStart();
            }   
        }
    }    
}
