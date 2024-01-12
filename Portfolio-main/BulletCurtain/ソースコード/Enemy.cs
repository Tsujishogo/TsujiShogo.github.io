using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class Enemy : MonoBehaviour, IApplicableDamage
{
    [SerializeField] GameObject tama1; // ��΂��e
    [SerializeField] bool isAttack = false;// �U���I���I�t
    [SerializeField] int enemyHP = 100;�@//�ő�̗�
    [SerializeField] Slider slider;
    [SerializeField] AudioClip hit;
    [SerializeField] AudioClip bomb;
    [SerializeField] float moveDis = 10f; // �ړ�����͈͂̔��a�̒���
    [SerializeField] float moveLow = 1f;  // �ړ����鍂���͈̔́@����
    [SerializeField] float moveHigh = 7f;�@// �ړ����鍂���͈̔́@���
    [SerializeField] float moveTime = 2f; // �ړ����鏈���ɂ����鎞��

    // �U���^�C�v��ύX����񐔁A���ꂼ���HP�ƍU���^�C�v��inspector����w��
    [System.Serializable] struct ChangeTiming
    {
        public int HP;�@// ����HP�ȉ��ɂȂ�ƕύX
        public int Type; //�ύX��U���^�C�v
    }
    [Header("HP�̑傫�����ɓo�^")]
    [SerializeField] ChangeTiming[] TypeChangeNo;
           
    
    GameObject player;�@// �v���C���[
    int currentHP; //���݂�HP
    int currentType = 111;�@�@// �U���^�C�v�ύX�p�ϐ�
    int currentTypeNo= 0;�@//�U���^�C�v�ύX�̏�������p�ϐ�
    public BulletManager bulletmanager;
    Vector3 startPos;
    AudioSource audioSource;

    void Start()
    {
        player = GameObject.FindWithTag("Player");//�v���C���[object�擾
        startPos = transform.position;// �������W��ۑ����Ă���
        RandomMove();//�����_���ړ�
        currentHP = enemyHP;//�G�̍ő�hp�o�^
        slider.value = 1;//HP�X���C�_�[������
        audioSource = GetComponent<AudioSource>();//audiosource�擾
       
    }

    void Update()
    {
        if (currentHP > 0)
        {
            transform.LookAt(player.transform);// �G��HP���c���Ă���Ƃ��̓v���C���[�̂ق�������
        }
    }

    //�����ʒu�𒆐S��moveDistanse�𔼌a�Ƃ���~���������_���ړ�
    private void RandomMove()
    {
        
        
        Vector3 pos = moveDis * Random.insideUnitCircle; // �~���̃����_�����W���擾

        // �~���̃����_�����W�ɏ����ʒu�A�����_���ȍ����������Ď��̈ړ�����W���v�Z
        Vector3 nextPos = new Vector3(
            pos.x + startPos.x , Random.Range(moveLow, moveHigh), pos.y + startPos.z); 
        if (currentHP > 0||currentType !=0)
        {
            // ���̈ʒu��movetime/�b�����Ĉړ��A���̌ニ�[�v
            transform.DOMove(nextPos, moveTime).OnComplete(() => RandomMove());
        }

    }

    // ��_���[�W����
    public void  ReceiveDamage()
    {
        currentHP--;// HP���炷
        // TypeChange��inspector����ݒ肳�ꂽHP�ȉ��ɂȂ�Ƃ��̍U���^�C�v�ɕύX�A
        // �ύX�������̏����ɑҋ@
        if (currentTypeNo < TypeChangeNo.Length  && currentType != -1)
        {
            if (currentHP < TypeChangeNo[currentTypeNo].HP)
            {
                currentType = TypeChangeNo[currentTypeNo].Type;
                bulletmanager.changeType(currentType);
                currentTypeNo++;
            }
        }
        
        
        if (currentHP <= 0)// HP�Ȃ��Ȃ�����ړ����~�߂�
        {
            transform.DOKill();
            bulletmanager.changeType(-1); //���j����
            currentType = -1;
;            audioSource.PlayOneShot(bomb);// ���j����SE�Đ�
        }
        slider.value = (float)currentHP / (float)enemyHP;// ����HP��HP�X���C�_�[�ɔ��f

        audioSource.PlayOneShot(hit) ;// �U���q�b�g����SE�Đ�

    }
}
