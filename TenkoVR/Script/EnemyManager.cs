using UnityEngine;
using System.Linq;
using System;
using UniRx;
using Cysharp.Threading.Tasks;

/// <summary>
/// �G�L�����������Ǘ�����N���X
/// </summary>
public class EnemyManager : ObjectPoolMonoBehaviour<EnemyManager, Bullet>
{
    [Header("�Gprefab�͏�����ԂŔ�A�N�e�B�u�ɂ��Ă�������")]
    [Header("�Gprefab")]
    [SerializeField] GameObject[] _enemyList; 
    [Header("TimeManager")]
    [SerializeField] TimeManager _timeManager;

    int _enemyCount; // �G�̎c���J�E���g�p�ϐ�

    // �G�̏o���^�C�~���O�A�ʒu�A��ނ�inspector����w��
    [System.Serializable]
    struct SpawnObject
    {
        public float _timing;        //�o���^�C�~���O�i�b�j
        public Vector3 _spawnPoint;�@// �o���ʒu
        public int _enemyType;       //�G�̎��
    }
    [Header("�G�̏o������(�b)�A�ʒu�A��ނ�o�^")]
    [Header("Timing�̓Q�[���J�n����o�ߎ���")]
    [Header("EnemyType��enemyList�ɓo�^���Ă���Prefab(�擪���Ăяo���ꍇ��0)")]
    [SerializeField] SpawnObject[] _spawnerList;


    void Start()
    {
        InitiarizePool(); // �v�[��������

        _timeManager = GetComponent<TimeManager>();
        // �G�̎c����������
        _enemyCount = _spawnerList.Length;
        // �o���^�C�~���O���Ƀ\�[�g���Ă���
        _spawnerList = _spawnerList.OrderBy(enemy => enemy._timing).ToArray();
    }

    // false��ԂŌĂяo���悤��override
    public override Bullet Rent()
    {
        Bullet t = base.Rent();
        t.gameObject.SetActive(false);
        return t;
    }

    // �Gprefab�𐶐��A�o���^�C�~���O��active��
    public void EnemySpawn() 
    {
        foreach (var spawn in _spawnerList)
        {
            // SpawnerList�ɓo�^���ꂽ�G�𐶐�
            GameObject enemyObj = Instantiate(_enemyList[spawn._enemyType], spawn._spawnPoint, _enemyList[spawn._enemyType].transform.rotation);
            enemyObj.SetActive(false);

            Observable.Timer(TimeSpan.FromSeconds(spawn._timing))// Timing�i�b�j��ɓG��active��
                .Subscribe(_ =>
                {
                    enemyObj.SetActive(true); // �G��active��
                    ParticleManager.PlayParticle(ParticleManager.ParticleName.SpawnSmoke, spawn._spawnPoint); // �X�|�[���p�[�e�B�N���Đ�

                    // enemy�̊e�C�x���g���w�ǂ��Ď擾�����珈���𑖂点��
                    if (enemyObj.TryGetComponent<Enemy>(out Enemy enemy))
                    {
                        enemy.OndeadAsync.Subscribe(_ => OnEnemyDead());
                        enemy.OnSlowJudge.Subscribe(_ => _timeManager.SlowChecker());
                    }
                })
                .AddTo(this);
        }
    }

    // �G�̎c���Ǘ��֐�
    void OnEnemyDead() 
    {
        _enemyCount--;
        if (_enemyCount <= 0) // �G�̎c����0�ɂȂ�����X�e�[�W�N���A����
        {
            StageManager.instance.StageClear();
        }
    }

}
