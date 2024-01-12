using System.Collections;
using System.Collections.Generic;
using System;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.Events;
// 敵の発射する弾クラス

public class EnemyBullet : MonoBehaviour
{
    [SerializeField] float speed = 20f;　//飛ぶ速度
    [SerializeField] new Renderer renderer;
    [SerializeField] new SphereCollider collider;
    [SerializeField] Rigidbody rd;
    [SerializeField] float poolRetrunCount = 5;// 弾をpoolに戻すまでの時間

    BoolReactiveProperty isRidid = new BoolReactiveProperty(true);// 当たり判定スイッチ用bool


    private void Start()
    {
       // 当たり判定のオンオフ
       // isRididがfalse だったら非表示にしてisTriggerをオフに
        isRidid.Where(_ => isRidid.Value == false)
            .Subscribe(_ =>
            {
                renderer.enabled = false;// レンダラ非表示
                collider.isTrigger = false;// コライダー消す
            })
            .AddTo(this);
        isRidid.Where(_ => isRidid.Value == true)
            .Subscribe(_ =>
            {
                renderer.enabled = true;
                collider.isTrigger = true;
            })
            .AddTo(this);

        this.OnTriggerEnterAsObservable().Where(x => x.gameObject.CompareTag("Player") || x.gameObject.CompareTag("Ground"))
                                              .Subscribe(_ =>
                                              {
                                                  // プレイヤーか壁objectに当たったらエフェクトを再生して消す
                                                  ParticleManager.PlayParticle("Hit_02", transform.position);
                                                  isRidid.Value = false;
                                                  
                                              })
                                              .AddTo(this);

        // 前に飛ばす処理
        this.UpdateAsObservable().Subscribe(_ => transform.Translate(Vector3.forward * speed * Time.deltaTime))
                                 .AddTo(this);

    }

   
    public IObservable<Unit> ShotBullet(Vector3 position, Vector3 direction)
    {
        //　positionは発射場所 Directionは発射向き

        //　慣性初期化
        rd.velocity = Vector3.zero;
        rd.angularVelocity = Vector3.zero;
        
        //　発射位置に移動
        transform.position = position;
        
        direction.y++;//　少し上を狙う
        transform.rotation = Quaternion.LookRotation(direction);//　向き変更

        if (isRidid.Value == false)//　poolから呼び出したときに当たり判定がなかったらオンに
        {
            isRidid.Value = true;
        }
        //　5秒たったら当たり判定を消してpoolに戻す
        return Observable.Timer(TimeSpan.FromSeconds(poolRetrunCount))
                                             .ForEachAsync(_ => isRidid.Value = false);

    }
   
}