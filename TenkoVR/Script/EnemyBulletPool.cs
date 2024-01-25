using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Toolkit;
public class EnemyBulletPool : ObjectPool<Bullet>
{
    Bullet _bulletPrefab;
    Transform _parentTransform;


    // �R���X�g���N�^
    public EnemyBulletPool(Transform parentTransform, Bullet prefab)
    {
        _parentTransform = parentTransform;
        _bulletPrefab = prefab;
    }

    protected override Bullet CreateInstance()
    {
        // �C���X�^���X����
        Bullet b = GameObject.Instantiate(_bulletPrefab);
        // �܂Ƃ߂�
        b.transform.SetParent(_parentTransform);

        return b;
    }

    // ��A�N�e�B�u�̂܂ܓn��
    protected override void OnBeforeRent(Bullet instance)
    {
        instance.gameObject.SetActive(false);
    }

}
