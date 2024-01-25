using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// プレイヤーモデルの手を動かすクラス
/// </summary>

public class FingerMover : MonoBehaviour
{
    [Header("それぞれの入力を取るInputActionを登録")]
    [SerializeField] InputActionReference _rightTriggerButtonAction;
    [SerializeField] InputActionReference _leftTriggerButtonAction;
    [SerializeField] InputActionReference _rightGripButtonAction;
    [SerializeField] InputActionReference _leftGripButtonAction;
    [SerializeField] InputActionReference _rightThumbButtonAction;
    [SerializeField] InputActionReference _leftThumbButtonAction;
    [SerializeField] InputAction _rightGripAction;

    [Header("掴むobject(刀)")]
    [SerializeField] GameObject _grubObject; // 手に追従させる対象
    [Header("手の位置")]
    [SerializeField] GameObject _handPos;
    [Header("追従対象")]
    [SerializeField] GameObject _grubRoot;
    [Header("AnimatorのisGrubのレイヤーの順番")]
    [SerializeField] int _isGrubLayer = 1;
    [Header("掴むことができる距離")]
    [SerializeField] float _grubableDis = 0.05f;
    [Header("ボタン押し込みのしきい値")]
    [SerializeField] float _inputThreshold = 0.9f;
    [Header("コントローラー振動可否")]
    [SerializeField] bool _isHapitics = true; // コントローラーを振動させるか
    [Header("振動幅0～1f")]
    [SerializeField] float _hapticsAmplitude = 0.5f;
    [Header("振動時間")]
    [SerializeField] float _hapticsDuration = 0.2f;

    [Header("hierarchy上のコントローラーPrefab")]
    [SerializeField] ActionBasedController _leftController; 
    [SerializeField] ActionBasedController _rightController;

    // private変数
    bool _startFlag = false;
    bool _isAnimRight = true; // 手をアニメーションさせるかどうか
    bool _isAnimLeft = true;
    bool _isGrubed = false; //握っているかどうか
    Animator _animator; // プレイヤーモデルのanimator

    // parameter名をIDに変換
    readonly int _hashLTrigger = Animator.StringToHash("LeftTrigger");
    readonly int _hashLGrip = Animator.StringToHash("LeftGrip");
    readonly int _hashLThumbs = Animator.StringToHash("LeftThumbs");
    readonly int _hashRTrigger = Animator.StringToHash("RightTrigger");
    readonly int _hashRGrip = Animator.StringToHash("RightGrip");
    readonly int _hashRThumbs = Animator.StringToHash("RightThumbs");


    void Awake()
    {
        _animator = GetComponent<Animator>(); // animator取得
    }

    void Update()
    {
        // それぞれのボタンの押し込みの値を反映
        if(_isAnimLeft) // 左手
        {
            _animator.SetFloat(_hashLTrigger, _leftTriggerButtonAction.action.ReadValue<float>());
            _animator.SetFloat(_hashLGrip, _leftGripButtonAction.action.ReadValue<float>());
            _animator.SetFloat(_hashLThumbs, _leftThumbButtonAction.action.ReadValue<float>());
        }

        if(_isAnimRight) // 右手
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

    // Triggerボタンの入力を取得
    void OnActivate(InputValue value) 
    {
        GrubObject();
    }

    // 掴んだobjectを手に追従させる
    void GrubObject()
    {
        if (_isGrubed || StageManager.instance.IsShowUI == false) return;

        // 手と対象objectの距離を計算
        float grubDis = Vector3.Distance(_handPos.transform.position, _grubObject.transform.position); 

        if (grubDis < _grubableDis) // 距離がしきい値以下だったら掴む
        {
            _animator.SetLayerWeight(_animator.GetLayerIndex("IsGrub"), _isGrubLayer); // このレイヤーをアクティブにすると手を握る
                                                                                       // objectを手の位置に移動させて手に追従させる
            _grubObject.transform.parent = _grubRoot.transform;
            _grubObject.transform.localPosition = Vector3.zero;
            _grubObject.transform.localRotation = Quaternion.identity;
            _isGrubed = true;

            if (!_startFlag) // 一度だけ処理
            {
                _startFlag = true;
                StageManager.instance.GameStart();
            }   
        }
    }    
}
