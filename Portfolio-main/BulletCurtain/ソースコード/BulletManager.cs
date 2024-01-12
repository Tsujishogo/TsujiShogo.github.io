using System.Collections;
using System;
using UnityEngine;
using UniRx.Triggers;
using UniRx;
using UnityEngine.Events;


public class BulletManager : MonoBehaviour
{
    [SerializeField] EnemyBullet  _bulettPrefab; // ��΂��e��prefab
    [SerializeField] Transform _hierarchyTransform;  // �v�[������e�̊i�[object
    [SerializeField] bool isAttack = true; // �U���I���I�t�pbool
    [SerializeField] GameObject tama1; // �G�̔��˂���eprefab
    [SerializeField] int poolSize;�@// �ŏ��ɒe���v�[�����Ă�����
    [SerializeField] TimeCounter timeCounter;  //�^�C�}�[object
    [SerializeField] ResultManager resultManager; //���U���g�Ǘ�object
    [SerializeField] int attackType = 0; //�U���^�C�v�Ǘ��ϐ�
    [SerializeField] float changeDelay = 1f;//�U���^�C�v�ύX���̑ҋ@����
    [SerializeField] int shotgunBullets = 10;�@// �V���b�g�K���U���̂P�����Ƃ̂΂�܂���
    [SerializeField] float shotgunRadius = 1f; //�V���b�g�K���̂΂�܂����a
    GameObject player; //�v���C���[
    BulletPool _bulletPool;
    float sin;

    


    public  void changeType(int typeNo)
    {
        // �����U�����~�߂Ă���U���^�C�v��ς��鏈��
        isAttack = false;//�U������������~�߂�

        // �U���^�C�v�ύX���̃p�[�e�B�N���Đ�
        StartCoroutine(DelayCoroutine(0.5f, () => ParticleManager.PlayParticle("Hit", transform.position)));
        

        StartCoroutine(DelayCoroutine(changeDelay, () => isAttack = true));// �U���ĊJ�܂ł̑ҋ@
        attackType = typeNo;�@// �����̍U���^�C�v�ɂ�����
        timeCounter.StartCount();// �Q�[���J�n���̂ݏ��������@�^�C�}�[�X�^�[�g
    }

    // �x������
    IEnumerator DelayCoroutine(float seconds, UnityAction callback)
    {
        yield return new WaitForSeconds(seconds);
        callback?.Invoke();
    }


    // -1����1�̊Ԃ̐������[�v���ĕԂ��֐�
    //speed�͂P�b������̃��[�v��
    float Sin(float speed)
    {
        float sin = Mathf.Sin(2 * Mathf.PI * 1/speed * Time.time);
        return (sin);
    }

    void Start()
    {
        //object�v�[������
        _bulletPool = new BulletPool(_hierarchyTransform, _bulettPrefab);

        player = GameObject.FindWithTag("Player");// �v���C���[�擾

        this.OnDestroyAsObservable().Subscribe(_ => _bulletPool.Dispose());// destory���̃v�[�����������o�^

        // poolSize�̕������ŏ��ɒe�����A���̂��Ƃ́u1�v��1�t���[�����ɍ��Ӗ�
        _bulletPool.PreloadAsync(poolSize, 1).Subscribe(_ =>
        {
            Debug.Log("�g���I��");
            SceneManagera.UnEnableFade();// �����I�������t�F�[�h����
        }, Debug.LogError);




        // �����_���΂�T��
        this.UpdateAsObservable()
                .Where(_ => isAttack)
                .Where(_ => attackType == 3)// �U���^�C�v3
                .ThrottleFirst(TimeSpan.FromSeconds(0.05f))// 0.05�t���[����

                .Subscribe(_ =>
                {
                    // �����_���ȏꏊ
                    var rndpos = new Vector3(UnityEngine.Random.Range(-5f, 5f), 0, UnityEngine.Random.Range(-3f, 3f));

                    Vector3 target = player.transform.position - transform.position + rndpos;
                    // Debug.Log(target);

                    // pool����1�擾
                    var bullet = _bulletPool.Rent();

                    // �����ɓ��������������pool�ɕԋp����
                    bullet.ShotBullet(this.transform.position, target)
                        .Subscribe(__ =>
                        {
                            _bulletPool.Return(bullet);

                        }
                        )
                        .AddTo(this);
                }
                ).AddTo(this);

        // 11way���@�_���e
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

                        //pool����1�擾
                        var bullet = _bulletPool.Rent();
                        // �����ɓ��������������pool�ɕԋp����

                        bullet.ShotBullet(this.transform.position, newpos)
                            .Subscribe(__ =>
                            {
                                _bulletPool.Return(bullet);

                            })
                            .AddTo(this);
                    }

                }).AddTo(this);

        // ���Ԋu��^�e��
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
                                      // pool����1�擾
                                      var bullet = _bulletPool.Rent();
                                      // �����ɓ��������������pool�ɕԋp����
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


        this.UpdateAsObservable()// �������@�͂���10way
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
                                         // pool����1�擾
                                         var bullet = _bulletPool.Rent();
                                         // �����ɓ��������������pool�ɕԋp����
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

        // �G���j�t���O���ɕς���p�U���^�C�v
        this.UpdateAsObservable()
               .Where(_ => attackType == -1)
               .Take(1)// ��x����
               .Subscribe(_ =>
               {
                   ParticleManager.PlayParticle("Nuke", transform.position);// �G���j���̔����Đ�
                   timeCounter.StopCount();// �^�C�}�[�X�g�b�v
                   Observable.Timer(TimeSpan.FromSeconds(2f))// 2�b��
                             .Subscribe(_ =>
                             {
                                 resultManager.GameClear();// ���U���g��ʏo��
                             }).AddTo(this);

               }).AddTo(this);
        // 2Way���˂��˒e
        this.UpdateAsObservable()
            .Where(_ => attackType == 5)
            .Where(_ => isAttack)
            .ThrottleFirst(TimeSpan.FromSeconds(0.03f))// 0.05�t���[����

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
        //�V���b�g�K����
        this.UpdateAsObservable()
                .Where(_ => isAttack)
                .Where(_ => attackType == 6)// �U���^�C�v
                .ThrottleFirst(TimeSpan.FromSeconds(1f))//�b��

                .Subscribe(_ =>
                {

                    for (int i = 0; i < shotgunBullets; i++)
                    {
                        var circlePos = shotgunRadius * UnityEngine.Random.insideUnitSphere;
                        Vector3 targetDir = player.transform.position - transform.position + circlePos;

                        // pool����1�擾
                        var bullet = _bulletPool.Rent();

                        // �����ɓ��������������pool�ɕԋp����
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

        this.UpdateAsObservable()// �����e��
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
                                         // pool����1�擾
                                         var bullet = _bulletPool.Rent();
                                         // �����ɓ��������������pool�ɕԋp����
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
            .ThrottleFirst(TimeSpan.FromSeconds(0.03f))// 0.05�t���[����

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
