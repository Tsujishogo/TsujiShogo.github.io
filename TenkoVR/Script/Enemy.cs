using System;
using UnityEngine;
using System.Collections;
using UniRx;
using UniRx.Triggers;
using Random = UnityEngine.Random;

/// <summary>
/// �G�L�����̍s���Ǘ��N���X
/// </summary>
public class Enemy: MonoBehaviour, IApplicableHit
{
    [Header("�U���^�C�v")]
    [SerializeField] EnemyType _enemyType;
    [Header("�G��HP")]
    [SerializeField] int _enemyHP = 1;
    [Header("�G�ɒe���͂����Ԃ��܂ł̉�")]
    [SerializeField] int _parryfCount = 5;
    [Header("�U���Ԋu")]
    [SerializeField] float _shotInterval = 0.1f;
    [Header("�e�̑��x")]
    [SerializeField] float _speed = 50f;
    [Header("�e�̂΂炯�")]
    [SerializeField] float _aimRadius = 0.7f;
    [Header("�e��e�����Ƃ��̃����_���p�x")]
    [SerializeField] float _parryAngle = 135f;
    [Header("�v���C���[�̓����蔻��̃��C���[��I��")]
    [SerializeField] LayerMask _mask; // Ray�Ώۂ̃��C���[
    [Header("�e�̔��ˈʒu")]
    [SerializeField] Transform _shotPoint;
    [Header("�U����")]
    [SerializeField] bool _isShoot = true;
    [Header("�G���o�����Ă���U�����n�܂�܂ł̑ҋ@����")]
    [SerializeField] float _waitingShotTime = 2.0f; 

    [Header("EnemyType Burst�p")]
    [Header("�A�����˂����e�̔��ˊԊu")]
    [SerializeField] float _burstSpan = 0.05f;
    [Header("�A�����˂���e��")]
    [SerializeField] int _burstNumber = 3;
    [Header("EnemyType MachineGun�p")]
    [Header("�|�˂���p�x�͈�")]
    [SerializeField] float _shotAngle = 45f;
    [Header("�|�ˈꉝ���̎���")]
    [SerializeField] float _reversalTime = 5f;
    [Header("�|�˂̊J�n���� 1�ŉE����-1�ō�����")]
    [SerializeField] int _mgReversal = 1;
    [Header("������ԕϐ�")]
    [SerializeField] bool _isDead = false;          

    // private�ϐ�
    Transform _playerPos;�@        // �v���C���[�ʒu�̕ێ��ϐ�
    float _DeadJudgeTime = 2f;      // ���S����̑ҋ@����
    float _currentAngle;           // ���ˊp�ێ��p�ϐ�
    RecordManager _recordManager;
    StageManager _stageManager;
    EnemyBulletPool _pool;

    // dispose�ɂ���~�p
    IDisposable _attackStream;

    // ���񂾎��ɒʒm����p��Subject
    AsyncSubject<Unit> _onDeadSubject = new AsyncSubject<Unit>();
    public IObservable<Unit> OndeadAsync => _onDeadSubject;

    // �e��rayhit��ʒm����Subject
    Subject<Unit> _onRayHitSubject = new Subject<Unit>();
    public IObservable<Unit> OnSlowJudge => _onRayHitSubject;

    enum EnemyType // �U���^�C�v���`
    {
        OneShot,
        Burst,
        ShotGun,
        MachineGun,
    }

    void Awake()
    {
        // Destroy���ɒ�~������
        _onRayHitSubject.AddTo(this);
        _onDeadSubject.AddTo(this);
    }

    void Start()
    {
        // instance�擾���ĕێ�
        _stageManager = StageManager.instance;
        _recordManager = RecordManager.instance;

        _playerPos = _stageManager.GetPlayerTransform(); // �v���C���[��transform���擾
        if (_shotPoint == null) _shotPoint = transform;

        // �v���C���[�̕�����������
        Vector3 target = _playerPos.position - transform.position;
        transform.rotation = Quaternion.LookRotation(target, Vector3.up);
        _currentAngle = _shotAngle * _mgReversal; // ���ˊJ�n���̊p�x���v�Z

        switch (_enemyType)�@// �U���^�C�v�ɍ��킹�Ēe�̔��ˏ��������s
        {
            // �o�����Ă���waitingShotTime/�b�҂��Ĕ��ˊJ�n
            case EnemyType.OneShot:
                DelayAction.Play(this, _waitingShotTime, () => OneShot());
                break;
            case EnemyType.Burst:
                DelayAction.Play(this, _waitingShotTime, () => Burst());
                break;
            case EnemyType.ShotGun:
                DelayAction.Play(this, _waitingShotTime, () => ShotGun());
                break;
            case EnemyType.MachineGun:
                DelayAction.Play(this, _waitingShotTime, () => MachineGun());
                break;
                // �U���^�C�v��r���Ő؂�ւ���ꍇ��attackStream��dispose���Đ؂�ւ���̊֐����Ă�
        }
    }

    // ���˂��ꂽ�e�̏���
    void Shot(Vector3 dir, float angle = default) 
    {
        // object�v�[������e���؂��
        Bullet bullet = EnemyManager.Instance.Rent();

        Rigidbody rb = bullet.Rigidbody; //�@�e��RigidBody���L���b�V��
        Transform bulletTrans = bullet.transform; // �e��transform���L���b�V��
        bulletTrans.position = _shotPoint.position; //�e�𔭎ˈʒu�Ɉړ�

        // �e��������A����
        bullet.InitializieBullet();  
        bullet.gameObject.SetActive(true);
        bullet.BulletShot(dir, _speed);

        // Ray���΂��ăX���[�����̔�����s��
        if (!_isDead)
        {
            bullet.RayShot(_mask, _onRayHitSubject);
        }

        // �e�̒e���ꏈ�����w�ǂ��Ēʒm�������珈�����s��
        bullet.OnParryAsync.Subscribe(bulletPos =>
        {
            // enemy��parrycount���c���Ă��Ȃ�������G�ɂ͂����Ԃ�
            if (_parryfCount <= 0 && !_isDead)
            {
                Vector3 enemyDir = _shotPoint.position - bulletPos;
                bullet.BulletShot(enemyDir, _speed);
            }
            else // �c���Ă���ꍇ�̓J�E���g�����炵�ă����_���ȕ����ɒe��
            {
                _parryfCount--;
                Vector3 ranDir = Quaternion.Euler(Random.Range(0f, _parryAngle), Random.Range(-_parryAngle, _parryAngle), 0).eulerAngles;
                bullet.BulletShot(ranDir, _speed);
            }
            // �G�t�F�N�g�̍Đ������v���C�ɋL�^
            if (!_isDead)
            {
                _recordManager.PoolObjectRecorder(bulletPos, Vector3.zero, RecordManager.PoolObjectType.ElectricalSparks);
            }
        })
            .AddTo(this);
       
        if (!_isDead) // RecordManager�֔��ˏ����L�^������
        {
            _recordManager.PoolObjectRecorder(_shotPoint.position, dir, RecordManager.PoolObjectType.Bullet);
        }
    }

    // ���v���C�Đ����p�֐�
    public void RecordedShot(Vector3 pos, Vector3 dir) 
    {
        // �L�^���ꂽ�ʒu�������̌����Ɍ����Ĕ���
        _shotPoint.position = pos;
        Shot(dir);
        _isDead = true;
    }

    // �P������
    void OneShot() 
    {
        // shotInterval���ɂP�������˂���
        _attackStream = this.UpdateAsObservable()
          .Where(_ => _isShoot)
          .ThrottleFirst(TimeSpan.FromSeconds(_shotInterval))
          .Subscribe(_ =>
          {
              // �΂炯������͈͓��̒l���Ƃ��Ĕ��ˈʒu����̌������v�Z���Ĕ���
              Vector3 circlePos = _aimRadius * Random.insideUnitCircle;
              Shot(circlePos + _playerPos.position - _shotPoint.position);
              transform.LookAt(_playerPos);
          })
          .AddTo(this);
    }

    // �o�[�X�g����
    void Burst() 
    {
        // shotInterval���ɓ��������Ɏw��e�������˂���
        _attackStream = Observable.Interval(TimeSpan.FromSeconds(_shotInterval))
            .Where(_ => _isShoot)
            .Subscribe(_ =>
            {
                // �����_���Ȉʒu���擾
                Vector3 circlePos = _aimRadius * Random.insideUnitCircle;

                // ���������Ƀo�[�X�g����
                Observable.Interval(TimeSpan.FromSeconds(_burstSpan))
                    .Take(_burstNumber)
                    .Subscribe(_ => Shot(circlePos + _playerPos.position - _shotPoint.position));

                transform.LookAt(_playerPos);
            })
            .AddTo(this);
    }

    // �V���b�g�K���ˌ�
    void ShotGun() 
    {
        // shotInterval���Ɏw��e���΂�܂�
        _attackStream = this.UpdateAsObservable()
               .ThrottleFirst(TimeSpan.FromSeconds(_shotInterval))
               .Where(_ => _isShoot)
               .Subscribe(_ =>
               {
                   transform.LookAt(_playerPos);
                   // burstNumber �̒l�����˂̌v�Z���J��Ԃ�
                   for (int i = 0; i < _burstNumber; i++)
                   {
                       Vector3 circlePos = _aimRadius * Random.insideUnitCircle;
                       Shot(circlePos + _playerPos.position - _shotPoint.position);
                   }
               })
               .AddTo(this);
    }

    // �}�V���K�����ˁi���E�Ɋp�x��ς��Ȃ���|�ˁj
    void MachineGun() 
    {
        _attackStream = this.UpdateAsObservable()
               .ThrottleFirst(TimeSpan.FromSeconds(_shotInterval))
               .Where(_ => _isShoot)
               .Subscribe(_ =>
               {
                   Vector3 circlePos = _aimRadius * Random.insideUnitCircle; // �����_���ʒu���擾
                   Vector3 dir = circlePos + _playerPos.position - _shotPoint.position; // ���ˌ������v�Z
                   dir = Quaternion.Euler(0, Sin(_reversalTime) * _shotAngle, 0) * dir; // ���ݎ��Ԃɉ��������ˊp�x���v�Z
                   Shot(dir);
                   transform.LookAt(_playerPos);
               })
               .AddTo(this);
    }

    public void ReceiveHit() // �@��e����
    {
        _enemyHP--; // HP�����炷 

        if (_enemyHP == 0) // HP��0�ɂȂ����Ƃ��̏���
        {
            // ���S�G�t�F�N�g���o���čs�����~
            ParticleManager.PlayParticle(ParticleManager.ParticleName.SpawnSmoke, transform.position);
            DelayAction.Play(this, _DeadJudgeTime, () => _isDead = true);
            _isShoot = false;

            // ���S���ʂ�ʒm
            _onDeadSubject.OnNext(Unit.Default);
            _onDeadSubject.OnCompleted();

            gameObject.SetActive(false); //��active��
        }
    }

    // -1����1�̊Ԃ̐������[�v���ĕԂ��֐�
    // speed�͂P�b������̃��[�v��
    float Sin(float speed)
    {
        return Mathf.Sin(2 * Mathf.PI * 1 / speed * Time.time);
    }
}
