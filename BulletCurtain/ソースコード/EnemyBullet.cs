using System.Collections;
using System.Collections.Generic;
using System;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.Events;
// �G�̔��˂���e�N���X

public class EnemyBullet : MonoBehaviour
{
    [SerializeField] float speed = 20f;�@//��ԑ��x
    [SerializeField] new Renderer renderer;
    [SerializeField] new SphereCollider collider;
    [SerializeField] Rigidbody rd;
    [SerializeField] float poolRetrunCount = 5;// �e��pool�ɖ߂��܂ł̎���

    BoolReactiveProperty isRidid = new BoolReactiveProperty(true);// �����蔻��X�C�b�`�pbool


    private void Start()
    {
       // �����蔻��̃I���I�t
       // isRidid��false ���������\���ɂ���isTrigger���I�t��
        isRidid.Where(_ => isRidid.Value == false)
            .Subscribe(_ =>
            {
                renderer.enabled = false;// �����_����\��
                collider.isTrigger = false;// �R���C�_�[����
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
                                                  // �v���C���[����object�ɓ���������G�t�F�N�g���Đ����ď���
                                                  ParticleManager.PlayParticle("Hit_02", transform.position);
                                                  isRidid.Value = false;
                                                  
                                              })
                                              .AddTo(this);

        // �O�ɔ�΂�����
        this.UpdateAsObservable().Subscribe(_ => transform.Translate(Vector3.forward * speed * Time.deltaTime))
                                 .AddTo(this);

    }

   
    public IObservable<Unit> ShotBullet(Vector3 position, Vector3 direction)
    {
        //�@position�͔��ˏꏊ Direction�͔��ˌ���

        //�@����������
        rd.velocity = Vector3.zero;
        rd.angularVelocity = Vector3.zero;
        
        //�@���ˈʒu�Ɉړ�
        transform.position = position;
        
        direction.y++;//�@�������_��
        transform.rotation = Quaternion.LookRotation(direction);//�@�����ύX

        if (isRidid.Value == false)//�@pool����Ăяo�����Ƃ��ɓ����蔻�肪�Ȃ�������I����
        {
            isRidid.Value = true;
        }
        //�@5�b�������瓖���蔻���������pool�ɖ߂�
        return Observable.Timer(TimeSpan.FromSeconds(poolRetrunCount))
                                             .ForEachAsync(_ => isRidid.Value = false);

    }
   
}