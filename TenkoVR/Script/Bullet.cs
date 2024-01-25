using UnityEngine;
using System;
using UniRx;
using UniRx.Triggers;

/// <summary>
/// 弾の挙動処理を行うクラス
/// </summary>

public class Bullet :MonoBehaviour
{
    [Header("弾を弾く対象レイヤー名")]
    [SerializeField] string _parryingTag = "Katana";
    [Header("発射された弾を自動的にオブジェクトプールに戻す時間/秒")]
    [SerializeField] float _returnBulletTime = 2.0f;
    [Header("弾がプレイヤーに近づくとスロー処理が始まる距離/m")]
    [SerializeField] float _rayDis = 3f;
    [Header("スロー判定を行う間隔/フレーム数")]
    [SerializeField] int _rayInterval = 2;

    bool _isHitable;         // 当たり判定をするかどうか
    bool _isRaying;          // rayを飛ばすかどうか
    

    // 弾を弾いたイベント通知するSubject
    Subject<Vector3> _onParrySubject = new Subject<Vector3>();
    public IObservable<Vector3> OnParryAsync => _onParrySubject;

    IDisposable _rayDisposable;
    IDisposable _timerDisposable;


    public Rigidbody Rigidbody { get; private set; } // rigidbody公開用プロパティ

    // Instantiateでの複製なのでAwakeで初期化
    void Awake()    
    {
        Rigidbody = GetComponent<Rigidbody>();
    }

    void Start()
    {
        // 何かに当たった時の処理
        this.OnTriggerEnterAsObservable()
         // parryingTagのタグかIApplicableHitのインターフェースを持っていたら
         .Where(collider => _isHitable && (collider.gameObject.CompareTag(_parryingTag) || collider.gameObject.TryGetComponent<IApplicableHit>(out var _)))
         .Subscribe(collider =>
         {
             // 刀で弾いた時の処理
             if (collider.gameObject.CompareTag(_parryingTag))
             {
                 // パーティクル再生
                 ParticleManager.PlayParticle(ParticleManager.ParticleName.ElectricalSparks, transform.position); 
                 // 弾かれたイベントを通知
                 _onParrySubject.OnNext(transform.position);
                 // Rayを出さないようにする
                 _isRaying = false;
                 return;
             }

             if (_isHitable) // 被弾処理を当たった相手に行わせる
             {
                 IApplicableHit hit = collider.gameObject.GetComponent<IApplicableHit>();
                 hit.ReceiveHit();
                 // poolに戻す
                 EnemyManager.Instance.Return(this); 
             }

             _isHitable = false; // 当たり判定をなくす
         });  
    }

    public void InitializieBullet() // boolの初期化
    {
        _isHitable = true;
        _isRaying = true;
    }

    // 発射処理
    public void BulletShot(Vector3 dir, float speed)
    {
        // 進行方向を向かせて動かす
        transform.LookAt(dir);
        Rigidbody.velocity = dir.normalized * speed;

        // すでにカウント中のタイマーはリセット
        _timerDisposable?.Dispose(); 
        // 指定の秒数がたったらプールに返却してDisposeして処理を止める
        _timerDisposable = Observable.Timer(TimeSpan.FromSeconds(_returnBulletTime))
            .TakeUntilDestroy(this)
            .Subscribe(_ =>
            {
                EnemyManager.Instance.Return(this);
                _rayDisposable.Dispose();
            })
            .AddTo(this);
    }

    // Rayを飛ばして対象レイヤーがあればtrueを返す
    public void RayShot(LayerMask mask, Subject<Unit> subject) 
    {
        // 弾の前方にRayを飛ばしてプレイヤーのコライダーに当たったら通知を行う
        _rayDisposable = Observable.IntervalFrame(_rayInterval) // インターバル毎に
            .Where(_ => _isRaying && Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, _rayDis, mask))
            .Subscribe(_ => subject.OnNext(Unit.Default)) // 通知
            .AddTo(this);
    }
}



