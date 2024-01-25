using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Toolkit;
public class EnemyBulletPool : ObjectPool<Bullet>
{
    Bullet _bulletPrefab;
    Transform _parentTransform;


    // コンストラクタ
    public EnemyBulletPool(Transform parentTransform, Bullet prefab)
    {
        _parentTransform = parentTransform;
        _bulletPrefab = prefab;
    }

    protected override Bullet CreateInstance()
    {
        // インスタンス生成
        Bullet b = GameObject.Instantiate(_bulletPrefab);
        // まとめる
        b.transform.SetParent(_parentTransform);

        return b;
    }

    // 非アクティブのまま渡す
    protected override void OnBeforeRent(Bullet instance)
    {
        instance.gameObject.SetActive(false);
    }

}
