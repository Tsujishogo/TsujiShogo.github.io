using System;
using UnityEngine;
using System.Collections;
using UniRx;
using UniRx.Triggers;
using Random = UnityEngine.Random;

/// <summary>
/// 敵キャラの行動管理クラス
/// </summary>
public class Enemy: MonoBehaviour, IApplicableHit
{
    [Header("攻撃タイプ")]
    [SerializeField] EnemyType _enemyType;
    [Header("敵のHP")]
    [SerializeField] int _enemyHP = 1;
    [Header("敵に弾をはじき返すまでの回数")]
    [SerializeField] int _parryfCount = 5;
    [Header("攻撃間隔")]
    [SerializeField] float _shotInterval = 0.1f;
    [Header("弾の速度")]
    [SerializeField] float _speed = 50f;
    [Header("弾のばらけ具合")]
    [SerializeField] float _aimRadius = 0.7f;
    [Header("弾を弾いたときのランダム角度")]
    [SerializeField] float _parryAngle = 135f;
    [Header("プレイヤーの当たり判定のレイヤーを選択")]
    [SerializeField] LayerMask _mask; // Ray対象のレイヤー
    [Header("弾の発射位置")]
    [SerializeField] Transform _shotPoint;
    [Header("攻撃可否")]
    [SerializeField] bool _isShoot = true;
    [Header("敵が出現してから攻撃が始まるまでの待機時間")]
    [SerializeField] float _waitingShotTime = 2.0f; 

    [Header("EnemyType Burst用")]
    [Header("連続発射される弾の発射間隔")]
    [SerializeField] float _burstSpan = 0.05f;
    [Header("連続発射する弾数")]
    [SerializeField] int _burstNumber = 3;
    [Header("EnemyType MachineGun用")]
    [Header("掃射する角度範囲")]
    [SerializeField] float _shotAngle = 45f;
    [Header("掃射一往復の時間")]
    [SerializeField] float _reversalTime = 5f;
    [Header("掃射の開始方向 1で右から-1で左から")]
    [SerializeField] int _mgReversal = 1;
    [Header("生死状態変数")]
    [SerializeField] bool _isDead = false;          

    // private変数
    Transform _playerPos;　        // プレイヤー位置の保持変数
    float _DeadJudgeTime = 2f;      // 死亡判定の待機時間
    float _currentAngle;           // 発射角保持用変数
    RecordManager _recordManager;
    StageManager _stageManager;
    EnemyBulletPool _pool;

    // disposeによる停止用
    IDisposable _attackStream;

    // 死んだ時に通知する用のSubject
    AsyncSubject<Unit> _onDeadSubject = new AsyncSubject<Unit>();
    public IObservable<Unit> OndeadAsync => _onDeadSubject;

    // 弾のrayhitを通知するSubject
    Subject<Unit> _onRayHitSubject = new Subject<Unit>();
    public IObservable<Unit> OnSlowJudge => _onRayHitSubject;

    enum EnemyType // 攻撃タイプを定義
    {
        OneShot,
        Burst,
        ShotGun,
        MachineGun,
    }

    void Awake()
    {
        // Destroy時に停止させる
        _onRayHitSubject.AddTo(this);
        _onDeadSubject.AddTo(this);
    }

    void Start()
    {
        // instance取得して保持
        _stageManager = StageManager.instance;
        _recordManager = RecordManager.instance;

        _playerPos = _stageManager.GetPlayerTransform(); // プレイヤーのtransformを取得
        if (_shotPoint == null) _shotPoint = transform;

        // プレイヤーの方を向かせる
        Vector3 target = _playerPos.position - transform.position;
        transform.rotation = Quaternion.LookRotation(target, Vector3.up);
        _currentAngle = _shotAngle * _mgReversal; // 発射開始時の角度を計算

        switch (_enemyType)　// 攻撃タイプに合わせて弾の発射処理を実行
        {
            // 出現してからwaitingShotTime/秒待って発射開始
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
                // 攻撃タイプを途中で切り替える場合はattackStreamをdisposeして切り替え先の関数を呼ぶ
        }
    }

    // 発射された弾の処理
    void Shot(Vector3 dir, float angle = default) 
    {
        // objectプールから弾を借りる
        Bullet bullet = EnemyManager.Instance.Rent();

        Rigidbody rb = bullet.Rigidbody; //　弾のRigidBodyをキャッシュ
        Transform bulletTrans = bullet.transform; // 弾のtransformをキャッシュ
        bulletTrans.position = _shotPoint.position; //弾を発射位置に移動

        // 弾初期化後、発射
        bullet.InitializieBullet();  
        bullet.gameObject.SetActive(true);
        bullet.BulletShot(dir, _speed);

        // Rayを飛ばしてスロー処理の判定を行う
        if (!_isDead)
        {
            bullet.RayShot(_mask, _onRayHitSubject);
        }

        // 弾の弾かれ処理を購読して通知が来たら処理を行う
        bullet.OnParryAsync.Subscribe(bulletPos =>
        {
            // enemyのparrycountが残っていなかったら敵にはじき返す
            if (_parryfCount <= 0 && !_isDead)
            {
                Vector3 enemyDir = _shotPoint.position - bulletPos;
                bullet.BulletShot(enemyDir, _speed);
            }
            else // 残っている場合はカウントを減らしてランダムな方向に弾く
            {
                _parryfCount--;
                Vector3 ranDir = Quaternion.Euler(Random.Range(0f, _parryAngle), Random.Range(-_parryAngle, _parryAngle), 0).eulerAngles;
                bullet.BulletShot(ranDir, _speed);
            }
            // エフェクトの再生をリプレイに記録
            if (!_isDead)
            {
                _recordManager.PoolObjectRecorder(bulletPos, Vector3.zero, RecordManager.PoolObjectType.ElectricalSparks);
            }
        })
            .AddTo(this);
       
        if (!_isDead) // RecordManagerへ発射情報を記録させる
        {
            _recordManager.PoolObjectRecorder(_shotPoint.position, dir, RecordManager.PoolObjectType.Bullet);
        }
    }

    // リプレイ再生時用関数
    public void RecordedShot(Vector3 pos, Vector3 dir) 
    {
        // 記録された位置から特定の向きに向けて発射
        _shotPoint.position = pos;
        Shot(dir);
        _isDead = true;
    }

    // 単発発射
    void OneShot() 
    {
        // shotInterval毎に１発ずつ発射する
        _attackStream = this.UpdateAsObservable()
          .Where(_ => _isShoot)
          .ThrottleFirst(TimeSpan.FromSeconds(_shotInterval))
          .Subscribe(_ =>
          {
              // ばらけさせる範囲内の値をとって発射位置からの向きを計算して発射
              Vector3 circlePos = _aimRadius * Random.insideUnitCircle;
              Shot(circlePos + _playerPos.position - _shotPoint.position);
              transform.LookAt(_playerPos);
          })
          .AddTo(this);
    }

    // バースト発射
    void Burst() 
    {
        // shotInterval毎に同じ方向に指定弾数分発射する
        _attackStream = Observable.Interval(TimeSpan.FromSeconds(_shotInterval))
            .Where(_ => _isShoot)
            .Subscribe(_ =>
            {
                // ランダムな位置を取得
                Vector3 circlePos = _aimRadius * Random.insideUnitCircle;

                // 同じ方向にバースト発射
                Observable.Interval(TimeSpan.FromSeconds(_burstSpan))
                    .Take(_burstNumber)
                    .Subscribe(_ => Shot(circlePos + _playerPos.position - _shotPoint.position));

                transform.LookAt(_playerPos);
            })
            .AddTo(this);
    }

    // ショットガン射撃
    void ShotGun() 
    {
        // shotInterval毎に指定弾数ばらまく
        _attackStream = this.UpdateAsObservable()
               .ThrottleFirst(TimeSpan.FromSeconds(_shotInterval))
               .Where(_ => _isShoot)
               .Subscribe(_ =>
               {
                   transform.LookAt(_playerPos);
                   // burstNumber の値分発射の計算を繰り返す
                   for (int i = 0; i < _burstNumber; i++)
                   {
                       Vector3 circlePos = _aimRadius * Random.insideUnitCircle;
                       Shot(circlePos + _playerPos.position - _shotPoint.position);
                   }
               })
               .AddTo(this);
    }

    // マシンガン発射（左右に角度を変えながら掃射）
    void MachineGun() 
    {
        _attackStream = this.UpdateAsObservable()
               .ThrottleFirst(TimeSpan.FromSeconds(_shotInterval))
               .Where(_ => _isShoot)
               .Subscribe(_ =>
               {
                   Vector3 circlePos = _aimRadius * Random.insideUnitCircle; // ランダム位置を取得
                   Vector3 dir = circlePos + _playerPos.position - _shotPoint.position; // 発射向きを計算
                   dir = Quaternion.Euler(0, Sin(_reversalTime) * _shotAngle, 0) * dir; // 現在時間に応じた発射角度を計算
                   Shot(dir);
                   transform.LookAt(_playerPos);
               })
               .AddTo(this);
    }

    public void ReceiveHit() // 　被弾処理
    {
        _enemyHP--; // HPを減らす 

        if (_enemyHP == 0) // HPが0になったときの処理
        {
            // 死亡エフェクトを出して行動を停止
            ParticleManager.PlayParticle(ParticleManager.ParticleName.SpawnSmoke, transform.position);
            DelayAction.Play(this, _DeadJudgeTime, () => _isDead = true);
            _isShoot = false;

            // 死亡結果を通知
            _onDeadSubject.OnNext(Unit.Default);
            _onDeadSubject.OnCompleted();

            gameObject.SetActive(false); //非active化
        }
    }

    // -1から1の間の数をループして返す関数
    // speedは１秒あたりのループ数
    float Sin(float speed)
    {
        return Mathf.Sin(2 * Mathf.PI * 1 / speed * Time.time);
    }
}
