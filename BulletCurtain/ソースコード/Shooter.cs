using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shooter : MonoBehaviour
{
    // プレイヤー用弾発射クラス　それぞれThirdPersonControllerクラスから呼ばれる

    [SerializeField] GameObject homingGameObject;//弾prefab
    [SerializeField] float shotPosY = 2f;


    // 通常発射
    public void homingShoot()  
    {
        var shootPos = this.transform.position;
        Instantiate(homingGameObject, new Vector3(shootPos.x, shootPos.y + shotPosY, shootPos.z), transform.rotation) ;
    }


    public void BurstShoot(int num)//バーストショット　int num の数だけ発射する
    {
        var shootPos = transform.position;
        for (var i = 0; i < num; i++)
        {
            Instantiate(homingGameObject, new Vector3(shootPos.x, shootPos.y + shotPosY, shootPos.z), transform.rotation);
        }

        Debug.Log("ばーすと");
    }
}
