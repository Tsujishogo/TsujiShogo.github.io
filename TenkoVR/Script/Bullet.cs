using UnityEngine;
using System;
using UniRx;
using UniRx.Triggers;

/// <summary>
/// �e�̋����������s���N���X
/// </summary>

public class Bullet :MonoBehaviour
{
    [Header("�e��e���Ώۃ��C���[��")]
    [SerializeField] string _parryingTag = "Katana";
    [Header("���˂��ꂽ�e�������I�ɃI�u�W�F�N�g�v�[���ɖ߂�����/�b")]
    [SerializeField] float _returnBulletTime = 2.0f;
    [Header("�e���v���C���[�ɋ߂Â��ƃX���[�������n�܂鋗��/m")]
    [SerializeField] float _rayDis = 3f;
    [Header("�X���[������s���Ԋu/�t���[����")]
    [SerializeField] int _rayInterval = 2;

    bool _isHitable;         // �����蔻������邩�ǂ���
    bool _isRaying;          // ray���΂����ǂ���
    

    // �e��e�����C�x���g�ʒm����Subject
    Subject<Vector3> _onParrySubject = new Subject<Vector3>();
    public IObservable<Vector3> OnParryAsync => _onParrySubject;

    IDisposable _rayDisposable;
    IDisposable _timerDisposable;


    public Rigidbody Rigidbody { get; private set; } // rigidbody���J�p�v���p�e�B

    // Instantiate�ł̕����Ȃ̂�Awake�ŏ�����
    void Awake()    
    {
        Rigidbody = GetComponent<Rigidbody>();
    }

    void Start()
    {
        // �����ɓ����������̏���
        this.OnTriggerEnterAsObservable()
         // parryingTag�̃^�O��IApplicableHit�̃C���^�[�t�F�[�X�������Ă�����
         .Where(collider => _isHitable && (collider.gameObject.CompareTag(_parryingTag) || collider.gameObject.TryGetComponent<IApplicableHit>(out var _)))
         .Subscribe(collider =>
         {
             // ���Œe�������̏���
             if (collider.gameObject.CompareTag(_parryingTag))
             {
                 // �p�[�e�B�N���Đ�
                 ParticleManager.PlayParticle(ParticleManager.ParticleName.ElectricalSparks, transform.position); 
                 // �e���ꂽ�C�x���g��ʒm
                 _onParrySubject.OnNext(transform.position);
                 // Ray���o���Ȃ��悤�ɂ���
                 _isRaying = false;
                 return;
             }

             if (_isHitable) // ��e�����𓖂���������ɍs�킹��
             {
                 IApplicableHit hit = collider.gameObject.GetComponent<IApplicableHit>();
                 hit.ReceiveHit();
                 // pool�ɖ߂�
                 EnemyManager.Instance.Return(this); 
             }

             _isHitable = false; // �����蔻����Ȃ���
         });  
    }

    public void InitializieBullet() // bool�̏�����
    {
        _isHitable = true;
        _isRaying = true;
    }

    // ���ˏ���
    public void BulletShot(Vector3 dir, float speed)
    {
        // �i�s�������������ē�����
        transform.LookAt(dir);
        Rigidbody.velocity = dir.normalized * speed;

        // ���łɃJ�E���g���̃^�C�}�[�̓��Z�b�g
        _timerDisposable?.Dispose(); 
        // �w��̕b������������v�[���ɕԋp����Dispose���ď������~�߂�
        _timerDisposable = Observable.Timer(TimeSpan.FromSeconds(_returnBulletTime))
            .TakeUntilDestroy(this)
            .Subscribe(_ =>
            {
                EnemyManager.Instance.Return(this);
                _rayDisposable.Dispose();
            })
            .AddTo(this);
    }

    // Ray���΂��đΏۃ��C���[�������true��Ԃ�
    public void RayShot(LayerMask mask, Subject<Unit> subject) 
    {
        // �e�̑O����Ray���΂��ăv���C���[�̃R���C�_�[�ɓ���������ʒm���s��
        _rayDisposable = Observable.IntervalFrame(_rayInterval) // �C���^�[�o������
            .Where(_ => _isRaying && Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, _rayDis, mask))
            .Subscribe(_ => subject.OnNext(Unit.Default)) // �ʒm
            .AddTo(this);
    }
}



