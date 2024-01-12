using System.Collections;
using System;
using UnityEngine;
using UniRx.Triggers;
using UniRx;
using UnityEngine.Events;


public class BulletManager : MonoBehaviour
{
    [SerializeField] EnemyBullet  _bulettPrefab; // 飛ばす弾のprefab
    [SerializeField] Transform _hierarchyTransform;  // プールする弾の格納object
    [SerializeField] bool isAttack = true; // 攻撃オンオフ用bool
    [SerializeField] GameObject tama1; // 敵の発射する弾prefab
    [SerializeField] int poolSize;　// 最初に弾をプールしておく数
    [SerializeField] TimeCounter timeCounter;  //タイマーobject
    [SerializeField] ResultManager resultManager; //リザルト管理object
    [SerializeField] int attackType = 0; //攻撃タイプ管理変数
    [SerializeField] float changeDelay = 1f;//攻撃タイプ変更時の待機時間
    [SerializeField] int shotgunBullets = 10;　// ショットガン攻撃の１発ごとのばらまく数
    [SerializeField] float shotgunRadius = 1f; //ショットガンのばらまく半径
    GameObject player; //プレイヤー
    BulletPool _bulletPool;
    float sin;

    


    public  void changeType(int typeNo)
    {
        // 少し攻撃を止めてから攻撃タイプを変える処理
        isAttack = false;//攻撃をいったん止める

        // 攻撃タイプ変更時のパーティクル再生
        StartCoroutine(DelayCoroutine(0.5f, () => ParticleManager.PlayParticle("Hit", transform.position)));
        

        StartCoroutine(DelayCoroutine(changeDelay, () => isAttack = true));// 攻撃再開までの待機
        attackType = typeNo;　// 引数の攻撃タイプにかえる
        timeCounter.StartCount();// ゲーム開始時のみ処理される　タイマースタート
    }

    // 遅延処理
    IEnumerator DelayCoroutine(float seconds, UnityAction callback)
    {
        yield return new WaitForSeconds(seconds);
        callback?.Invoke();
    }


    // -1から1の間の数をループして返す関数
    //speedは１秒あたりのループ数
    float Sin(float speed)
    {
        float sin = Mathf.Sin(2 * Mathf.PI * 1/speed * Time.time);
        return (sin);
    }

    void Start()
    {
        //objectプール生成
        _bulletPool = new BulletPool(_hierarchyTransform, _bulettPrefab);

        player = GameObject.FindWithTag("Player");// プレイヤー取得

        this.OnDestroyAsObservable().Subscribe(_ => _bulletPool.Dispose());// destory時のプール消去処理登録

        // poolSizeの分だけ最初に弾生成、そのあとの「1」は1フレーム毎に作る意味
        _bulletPool.PreloadAsync(poolSize, 1).Subscribe(_ =>
        {
            Debug.Log("拡張終了");
            SceneManagera.UnEnableFade();// 生成終わったらフェード解除
        }, Debug.LogError);




        // ランダムばら撒き
        this.UpdateAsObservable()
                .Where(_ => isAttack)
                .Where(_ => attackType == 3)// 攻撃タイプ3
                .ThrottleFirst(TimeSpan.FromSeconds(0.05f))// 0.05フレーム毎

                .Subscribe(_ =>
                {
                    // ランダムな場所
                    var rndpos = new Vector3(UnityEngine.Random.Range(-5f, 5f), 0, UnityEngine.Random.Range(-3f, 3f));

                    Vector3 target = player.transform.position - transform.position + rndpos;
                    // Debug.Log(target);

                    // poolから1つ取得
                    var bullet = _bulletPool.Rent();

                    // 何かに当たったら消してpoolに返却する
                    bullet.ShotBullet(this.transform.position, target)
                        .Subscribe(__ =>
                        {
                            _bulletPool.Return(bullet);

                        }
                        )
                        .AddTo(this);
                }
                ).AddTo(this);

        // 11way自機狙い弾
        this.UpdateAsObservable()
                .Where(_ => isAttack)
                .Where(_ => attackType == 2)
                .ThrottleFirst(TimeSpan.FromSeconds(0.2f))
                .Subscribe(_ =>
                {
                    Vector3 target = player.transform.position;
                    var dir = target - transform.position;
                    for (int i = -5; i <= 5; i++)
                    {
                        var newpos = Quaternion.Euler(0, 5 * i, 0) * dir;

                        //poolから1つ取得
                        var bullet = _bulletPool.Rent();
                        // 何かに当たったら消してpoolに返却する

                        bullet.ShotBullet(this.transform.position, newpos)
                            .Subscribe(__ =>
                            {
                                _bulletPool.Return(bullet);

                            })
                            .AddTo(this);
                    }

                }).AddTo(this);

        // 一定間隔扇型弾幕
        this.UpdateAsObservable()
                .Where(_ => isAttack)
                .Where(_ => attackType == 1)
                .ThrottleFirst(TimeSpan.FromSeconds(1.5f))
                .Subscribe(_ =>
                {
                    Observable.Timer(TimeSpan.FromSeconds(0f), TimeSpan.FromSeconds(0.1f))
                              .Take(5)
                              .Subscribe(_ =>
                              {
                                  Vector3 target = player.transform.position;
                                  var dir = target - transform.position;

                                  for (int i = -3; i <= 3; i++)
                                  {
                                      var newpos = Quaternion.Euler(0, 2 * i, 0) * dir;
                                      // poolから1つ取得
                                      var bullet = _bulletPool.Rent();
                                      // 何かに当たったら消してpoolに返却する
                                      Debug.Log(newpos);

                                      bullet.ShotBullet(this.transform.position, newpos)
                                          .Subscribe(__ =>
                                          {
                                              _bulletPool.Return(bullet);

                                          })
                                          .AddTo(this);
                                  }
                              });
                }).AddTo(this);


        this.UpdateAsObservable()// 幅狭自機はずし10way
               .Where(_ => isAttack)
               .Where(_ => attackType == 4)
               .ThrottleFirst(TimeSpan.FromSeconds(1.5f))
               .Subscribe(_ =>
               {
                   Observable.Timer(TimeSpan.FromSeconds(0f), TimeSpan.FromSeconds(0.3f))
                             .Take(5)
                             .Subscribe(_ =>
                             {
                                 Vector3 target = player.transform.position;
                                 var dir = target - transform.position;
                                 var rnd = UnityEngine.Random.Range(-10, 10);
                                 for (int i = 0; i <= 5; i++)
                                 {
                                     for (int j = -1; j <= 1; j++)
                                     {
                                         var newpos = Quaternion.Euler(0, 2 * j, 0) * dir;
                                         // poolから1つ取得
                                         var bullet = _bulletPool.Rent();
                                         // 何かに当たったら消してpoolに返却する
                                         newpos.x += i * j * 5;

                                         Observable.Timer(TimeSpan.FromSeconds(i*0.1f))
                                         .Subscribe(_ =>
                                         {
                                             bullet.ShotBullet(this.transform.position, newpos)
                                          .Subscribe(__ =>
                                          {
                                              _bulletPool.Return(bullet);

                                          })
                                          .AddTo(this);
                                         });
                                         j++;
                                     }
                                     
                                 }


                             });
               }).AddTo(this);

        // 敵撃破フラグ時に変える用攻撃タイプ
        this.UpdateAsObservable()
               .Where(_ => attackType == -1)
               .Take(1)// 一度だけ
               .Subscribe(_ =>
               {
                   ParticleManager.PlayParticle("Nuke", transform.position);// 敵撃破時の爆発再生
                   timeCounter.StopCount();// タイマーストップ
                   Observable.Timer(TimeSpan.FromSeconds(2f))// 2秒後
                             .Subscribe(_ =>
                             {
                                 resultManager.GameClear();// リザルト画面出す
                             }).AddTo(this);

               }).AddTo(this);
        // 2Wayうねうね弾
        this.UpdateAsObservable()
            .Where(_ => attackType == 5)
            .Where(_ => isAttack)
            .ThrottleFirst(TimeSpan.FromSeconds(0.03f))// 0.05フレーム毎

                .Subscribe(_ =>
                {
                    Vector3 target = player.transform.position;
                    var dir = target - transform.position;

                for (int i = -1; i <= 1; i++)
                    {
                        var newpos = Quaternion.Euler(0, i* 15 * Sin(2.5f), 0) * dir;
                        var bullet = _bulletPool.Rent();

                        bullet.ShotBullet(this.transform.position, newpos)
                            .Subscribe(__ =>
                            {
                                _bulletPool.Return(bullet);

                            })
                            .AddTo(this);
                        i++;
                    }

                    return;
                }).AddTo(this);
        //ショットガン風
        this.UpdateAsObservable()
                .Where(_ => isAttack)
                .Where(_ => attackType == 6)// 攻撃タイプ
                .ThrottleFirst(TimeSpan.FromSeconds(1f))//秒毎

                .Subscribe(_ =>
                {

                    for (int i = 0; i < shotgunBullets; i++)
                    {
                        var circlePos = shotgunRadius * UnityEngine.Random.insideUnitSphere;
                        Vector3 targetDir = player.transform.position - transform.position + circlePos;

                        // poolから1つ取得
                        var bullet = _bulletPool.Rent();

                        // 何かに当たったら消してpoolに返却する
                        bullet.ShotBullet(this.transform.position, targetDir)
                            .Subscribe(__ =>
                            {
                                _bulletPool.Return(bullet);

                            }
                            )
                            .AddTo(this);
                    }
                    
                }
                ).AddTo(this);

        this.UpdateAsObservable()// 発狂弾幕
              .Where(_ => isAttack)
              .Where(_ => attackType == 7)
              .ThrottleFirst(TimeSpan.FromSeconds(1.5f))
              .Subscribe(_ =>
              {
                  Observable.Timer(TimeSpan.FromSeconds(0f), TimeSpan.FromSeconds(0.3f))
                            .Take(5)
                            .Subscribe(_ =>
                            {
                                Vector3 target = player.transform.position;
                                var dir = target - transform.position;
                                var rnd = UnityEngine.Random.Range(-10, 10);
                                for (int i = 0; i <= 5; i++)
                                {
                                    for (int j = -1; j <= 1; j++)
                                    {
                                        var newpos = Quaternion.Euler(0, 2 * j, 0) * dir;
                                         // poolから1つ取得
                                         var bullet = _bulletPool.Rent();
                                         // 何かに当たったら消してpoolに返却する
                                         newpos.x += i * j * 5;

                                        Observable.Timer(TimeSpan.FromSeconds(i * 0.1f))
                                        .Subscribe(_ =>
                                        {
                                            bullet.ShotBullet(this.transform.position, newpos)
                                         .Subscribe(__ =>
                                         {
                                             _bulletPool.Return(bullet);

                                         })
                                         .AddTo(this);
                                        });
                                        j++;
                                    }

                                }


                            });
              }).AddTo(this);
        this.UpdateAsObservable()
            .Where(_ => attackType == 7)
            .Where(_ => isAttack)
            .ThrottleFirst(TimeSpan.FromSeconds(0.03f))// 0.05フレーム毎

                .Subscribe(_ =>
                {
                    Vector3 target = player.transform.position;
                    var dir = target - transform.position;

                    for (int i = -1; i <= 1; i++)
                    {
                        var newpos = Quaternion.Euler(0, i * 15 * Sin(2.5f), 0) * dir;
                        var bullet = _bulletPool.Rent();

                        bullet.ShotBullet(this.transform.position, newpos)
                            .Subscribe(__ =>
                            {
                                _bulletPool.Return(bullet);

                            })
                            .AddTo(this);
                        i++;
                    }

                    return;
                }).AddTo(this);
    }
}
