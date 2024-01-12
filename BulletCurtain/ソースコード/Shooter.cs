using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shooter : MonoBehaviour
{
    // �v���C���[�p�e���˃N���X�@���ꂼ��ThirdPersonController�N���X����Ă΂��

    [SerializeField] GameObject homingGameObject;//�eprefab
    [SerializeField] float shotPosY = 2f;


    // �ʏ픭��
    public void homingShoot()  
    {
        var shootPos = this.transform.position;
        Instantiate(homingGameObject, new Vector3(shootPos.x, shootPos.y + shotPosY, shootPos.z), transform.rotation) ;
    }


    public void BurstShoot(int num)//�o�[�X�g�V���b�g�@int num �̐��������˂���
    {
        var shootPos = transform.position;
        for (var i = 0; i < num; i++)
        {
            Instantiate(homingGameObject, new Vector3(shootPos.x, shootPos.y + shotPosY, shootPos.z), transform.rotation);
        }

        Debug.Log("�΁[����");
    }
}
