using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using System.Linq;
using System;
using DG.Tweening;
using UniRx;
using UniRx.Triggers;
using Random = UnityEngine.Random;

/// <summary>
/// �^�C�g����ʗpNPC����N���X
/// </summary>

public class NPCMover : MonoBehaviour
{
    [Header("�܂΂����̍ŒZ�Ԋu")]
    [SerializeField] float _blinkMinTime;
    [Header("�܂΂����̍Œ��Ԋu")]
    [SerializeField] float _blinkMaxTime;
    [Header("�܂΂����̑��x")]
    [SerializeField] float _blinkSpeed = 0.1f; 
    [Header("�\��ύX���ɂ��܂΂����������邩�ǂ���")]
    [SerializeField] bool _isForceBlink = false; 
    [Header("Animator��blinkLayer�̈ʒu")]
    [SerializeField] int _blinkLayer = 3; 
    [Header("�ړ���̈ʒu���X�g")]
    [SerializeField] List<Transform> _wayPoint; 
    [Header("�������x")]
    [SerializeField] float _walkSpeed; 
    [Header("���鑬�x")]
    [SerializeField] float _runSpeed; 
    [Header("�ړ��̉����x")]
    [SerializeField] float _acceleration = 1f; 
    [Header("��]�Ɋ|���鎞��")]
    [SerializeField] float _rotateDuration = 1f; 
    [Header("wayPoint���ł̏����ʒu")]
    [SerializeField] int _startPos;
    [Header("�ړ���̑ҋ@����")]
    [SerializeField] float _actionWaitTime = 2f; 
    [Header("�ړ��㓞�B����̂������l")]
    [SerializeField] float _arrivalThreshold = 0.1f;
    [System.Serializable]
    struct ActionList
    {
        [Header("�s�����X�g�i�����_���őI���j")]
        List<Transform> _movePosition;�@�@// �ړ���
        float _moveSpeed;�@// �ړ��ɂ����鎞��
        int _actionPattern; //�s���̂̎��
    }
    [Header("�s����o�^")]
    [SerializeField] ActionList[] _actionList;

    // private�ϐ�
    float _blinkTimer; // �܂΂����J�E���g�p�ϐ�
    float _currentSpeed = 0;
    int _currentPos = 0; // movePosition�̌��݈ʒu
    CancellationTokenSource _cancellationTokenSource;
    Animator _animator;


    float MoveSpeed //animator�̈ړ����x�𐧌䂷��v���p�e�B
    {
        set => _animator.SetFloat("MoveSpeed", value); 
    }

    float BlinkWeight // animator�̂܂΂����𐧌䂷��v���p�e�B
    {
        set =>_animator.SetFloat("BlinkWeight", value); 
    }

    int Facial // animator�̕\��̐���p�v���p�e�B
    {
        get => _animator.GetInteger("Facial");
        set =>_animator.SetInteger("Facial", value); 
    }


    void Awake()
    {
        // component�擾
        _animator = GetComponent<Animator>();
    }

    void Start()
    {
        _cancellationTokenSource = new CancellationTokenSource(); // �L�����Z���g�[�N������
        CancellationToken token = this.GetCancellationTokenOnDestroy();
        _blinkTimer = Random.Range(_blinkMinTime, _blinkMaxTime); //����̂܂΂�������܂ł̎��Ԃ��v�Z
        transform.position = _wayPoint[_startPos].position; // �����ʒu�ֈړ����Č��݈ʒu��������

        Blink(token).Forget(); // �܂΂�������
        Move(token).Forget(); // �ړ�����
    }

    void OnDestroy()
    {
        // Destroy���ɃL�����Z������
        _cancellationTokenSource?.Cancel();
    }

    // �܂΂�������
    async UniTaskVoid Blink(CancellationToken token)
    {
        while(!token.IsCancellationRequested) // �L�����Z������ĂȂ���ΌJ��Ԃ�
        {
            // blinkMin,Max�̕ϐ��̎��Ԃ̊ԂŌJ��Ԃ�
            await UniTask.Delay(TimeSpan.FromSeconds(Random.Range(_blinkMinTime, _blinkMaxTime)),cancellationToken : token);
            // �f�t�H���g�\��̂Ƃ������܂΂���������B��������isForceBlink��true�̏ꍇ�͋����܂΂���
            if (Facial == 0 || !_isForceBlink)
            {
                _animator.SetLayerWeight(_blinkLayer, 1); // �܂΂��������̃��C���[��L���ɂ���
                // blinkSpeed�̑��x�Ŗڂ���Ėڂ��J����
                await DOVirtual.Float(0, 1, _blinkSpeed, value =>
                {
                    BlinkWeight = value;
                }).WithCancellation(token);
                
                await DOVirtual.Float(1, 0, _blinkSpeed, value =>
                {
                    BlinkWeight = value;
                }).WithCancellation(token); ;
                _animator.SetLayerWeight(_blinkLayer, 0);
            }
        } 
    }

    // �ړ�����
    async UniTask Move(CancellationToken token)
    {
        // �����_���Ɏ��̍s���������ꏊ�ȊO�Ō��߂�
        int ran;
        do { 
            ran = Random.Range(0, _wayPoint.Count); 
        } while (_currentPos == ran);
        _currentPos = ran;

        // �ړ����x�������_���Ō���
        float maxSpeed = Random.value < 0.5f ? _walkSpeed : _runSpeed;
        Vector3 dir = (_wayPoint[ran].position - transform.position).normalized; 

        Debug.Log("����" + _currentPos + "��" + maxSpeed + "�̑��x�ōs����");
        // �s����̕�������
        transform.DOLookAt(_wayPoint[ran].position, _rotateDuration).SetEase(Ease.OutCubic);

        IDisposable disposable = this.UpdateAsObservable() // �ړ�����
            .Subscribe(_ =>
            {
                // �ő呬�x�܂ŉ������Ȃ���ړ�
                _currentSpeed = Mathf.Clamp(_currentSpeed + _acceleration * Time.deltaTime, 0f, maxSpeed);
                transform.position +=  _currentSpeed * Time.deltaTime * dir;
                // animator�ɑ��x�𔽉f
                MoveSpeed = _currentSpeed;
            });
        //token�ŃL�����Z�����ꂽ��subscribe���ꏏ�Ɏ~�߂�
        token.Register(() => disposable.Dispose());

        // ��r�p�T�C�Y
        float sqrthreshold = _arrivalThreshold * _arrivalThreshold;

        //�ړI�n�ɂ��܂őҋ@
        await UniTask.WaitUntil(() =>
        {
            return (_wayPoint[ran].position - transform.position).sqrMagnitude <= sqrthreshold;
            //return Vector3.Distance(transform.position, _wayPoint[ran].position) <= _arrivalThreshold;
        }, cancellationToken: token);

        // ��~
        _currentSpeed = 0;
        MoveSpeed = _currentSpeed;
        disposable.Dispose(); 

        // �ړ����������玟�̍s��
        await UniTask.Delay(TimeSpan.FromSeconds(_actionWaitTime), cancellationToken: token);
        Move(token).Forget();
    }
}
