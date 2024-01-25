using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Linq;
using System;
using UniRx;
using UnityEngine.UI;

/// <summary>
/// ���v���C�Ǘ��N���X
/// </summary>
public class RecordManager : MonoBehaviour
{
    [Header ("�������L�^������object��o�^")]
    [SerializeField] List<GameObject> _recordList; // �������L�^���Ă������X�g
    
    [SerializeField,Header("�L�^����Ԋu")] float _frameInterval = 0.0166666666666f; 
    [SerializeField, Header("���v���C�̃��[�v�Ԋu")] float _replayInterval = 0.5f;

    // private�ϐ��Q
    Dictionary<int, GameObject> _target = new Dictionary<int, GameObject>(); //������
    List<int> _objectIDs;
    List<GameObject> _recordingObjects;
    List<FrameData> _frames;
    RecordedObjectsData _recordData;
    RecordedPoolObjectsData _recordPoolData;
    float _time = 0; // ���݂̃t���[������ێ�����ϐ�
    Enemy _enemy;�@// �L�������e�𔭎˂�����enemy
    int _replayIndex = 0; // ���v���C�̍Đ��ʒu
    bool _isRecording = false; // �L�^�����ǂ���
    bool _isLoop = true; // ���v���C�����[�v���邩�ǂ���
    bool _isReplay = false; // ���v���C�Đ������ǂ���

    public static RecordManager instance; // �C���X�^���X
    public enum PoolObjectType // �L�^���Ă���object�̎��
    {
        Bullet,
        ElectricalSparks,
        SpawnSmoke,
    }


    // �L�^�f�[�^�i�[�N���X�Q
    public class RecordedObjectsData
    {
        public List<FrameData> frames;
    }

    [Serializable]
    public class FrameData
    {
        public float time; // �t���[���ʒu
        public List<ObjectPoseData> objectPoses;
    }

    [Serializable]
    public class ObjectPoseData
    {
        public Vector3 position; // position
        public Quaternion rotation; // rotation
        public int id; // dictionary�pkey
    }

    // pool�I�u�W�F�N�g�p�i�[class
    public class RecordedPoolObjectsData
    {
        public List<PoolFrameData> poolFrames;
    }

    public class PoolFrameData
    {
        public float playtime; // �t���[���ʒu
        public List<PoolObjectPoseData> poolObjectPoses;
    }

    public class PoolObjectPoseData
    {
        // ���
        public Vector3 position;
        public Vector3 direction;
        public PoolObjectType type;
    }


    void Awake()
    {
        if (instance == null)// �V���O���g��
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        _enemy = GetComponent<Enemy>();

        // ���X�g����gameobject��dictionary�ɒǉ�
        for (int i = 0; i < _recordList.Count; i++)
        {
            _target.Add(i, _recordList[i]);
        }

        // ���v���C���L������class��������
        _recordData = new RecordedObjectsData();
        _recordData.frames = new List<FrameData>();
        _recordPoolData = new RecordedPoolObjectsData();
        _recordPoolData.poolFrames = new List<PoolFrameData>();

        // dictionary�̃f�[�^�����X�g�Ɋi�[
        _objectIDs = _target.Keys.ToList();
        _recordingObjects = _target.Values.ToList();
    }

    void Update()
    {
#if UNITY_EDITOR
        // �f�o�b�O�p
        if (Input.GetKey(KeyCode.R))
        {
            StartRecording();
        }
        if (Input.GetKey(KeyCode.P))
        {
            StartReplay();
        }
#endif
    }

    public async void StartRecording() //�^��J�n����
    {
        _isRecording = true;
        Debug.Log("�L�^�J�n");
        RecordTask().Forget();�@//�L�^�p�R���[�`���N��
    }

    public void StopRecording() // �^���~�p����
    {
        _isRecording = false;
    }

    public async void StartReplay() // ���v���C�Đ��p�֐�
    {
        _isRecording = false; // �^����~
        //StopCoroutine(recordCoroutine); // �^��p�R���[�`�����~
        foreach(GameObject record in _recordList) // �L�^�Ώ�object�����g�̎q�ɂ��ă��v���C�ɓ����������Ȃ��悤�ɂ���
        {
            record.transform.parent = transform;
        }
        _isReplay = true;
        Debug.Log("���v���C�J�n");
        ReplayTask().Forget(); // �Đ��J�n
    }

    async UniTask RecordTask() // �L�^����
    {
        _time = 0; // ���݃t���[����������
        while (true)
        {
            if (_isRecording)
            {
                // ���݃t���[���̏����L�����郊�X�g��������
                FrameData frame = new FrameData();
                frame.time = _time;
                frame.objectPoses = new List<ObjectPoseData>();

                // ���������邽�߂ɓs�x�L�����X�g��������
                PoolFrameData poolFrame = new PoolFrameData();
                poolFrame.playtime = _time;
                poolFrame.poolObjectPoses = new List<PoolObjectPoseData>();
                _recordPoolData.poolFrames.Add(poolFrame);

                for (int i = 0; i < _recordingObjects.Count; i++)
                {
                    // �L�^�Ώ�object���ɏ�Ԃ��L�^
                    ObjectPoseData pose = new ObjectPoseData();
                    pose.position = _recordingObjects[i].transform.position;
                    pose.rotation = _recordingObjects[i].transform.rotation;
                    pose.id = _objectIDs[i];
                    frame.objectPoses.Add(pose);
                }
                _recordData.frames.Add(frame);
            }
            // frameIntervarl���ɌJ��Ԃ�
            await UniTask.Delay(TimeSpan.FromSeconds(_frameInterval));
            _time += _frameInterval;
        }
    }
    async UniTask ReplayTask()
    {
        while (_isLoop)
        {
            while (_replayIndex < _recordData.frames.Count) // �i�[����Ă���t���[�����̕���������
            {
                // ���݃t���[���̃f�[�^���Ƃ肾��
                FrameData frame = _recordData.frames[_replayIndex];
                for (int i = 0; i < frame.objectPoses.Count; i++) // �^��Ώۂ�object�����ꂼ�ꏈ��
                {
                    ObjectPoseData pose = frame.objectPoses[i];
                    GameObject obj = _target[pose.id];

                    // �t���[������擾���� pose �Ɋ�Â��ăI�u�W�F�N�g�̈ʒu�Ɖ�]�𔽉f
                    obj.transform.position = pose.position;
                    obj.transform.rotation = pose.rotation;
                }

                // �v�[��object�p�̍Đ������l��
                PoolFrameData poolFrame = _recordPoolData.poolFrames[_replayIndex];
                for (int i = 0; i < poolFrame.poolObjectPoses.Count; i++)
                {
                    PoolObjectPoseData poolPose = poolFrame.poolObjectPoses[i];
                    // �e�̏ꍇ�ƃp�[�e�B�N���ł��ꂼ�ꏈ���𕪂���
                    switch(poolPose.type)
                    {
                        case PoolObjectType.Bullet: // �e
                            _enemy.RecordedShot(poolPose.position, poolPose.direction);
                            break;
                        case PoolObjectType.ElectricalSparks: // �p�[�e�B�N��
                            ParticleManager.PlayParticle(ParticleManager.ParticleName.ElectricalSparks, poolPose.position);
                            break;
                        case PoolObjectType.SpawnSmoke: //�p�[�e�B�N��
                            ParticleManager.PlayParticle(ParticleManager.ParticleName.SpawnSmoke, poolPose.position);
                            break;
                    }
                }
                //�P�t���[���҂��Ď��̃t���[����
                _replayIndex++;
                await UniTask.Delay(TimeSpan.FromSeconds(_frameInterval));
            }
            if (!_isLoop)
            {
                break;
            }

            _replayIndex = 0; // �Đ��ʒu������
            await UniTask.Delay(TimeSpan.FromSeconds(_replayInterval),true);  // ���v���C���[�v
        }
    }
 
    // �e�A�p�[�e�B�N���L���֐�
    public void PoolObjectRecorder(Vector3 pos, Vector3 dir, PoolObjectType type)
    {
        if (_isRecording)
        {
            // object�̏�Ԃ��L�^
            PoolObjectPoseData poolPose = new PoolObjectPoseData();
            poolPose.position = pos;
            poolPose.direction = dir;
            poolPose.type = type;

            // ���݂̃t���[�����̈ʒu�ɋL�^�����f�[�^���i�[
            _recordPoolData.poolFrames[_recordPoolData.poolFrames.Count - 1].poolObjectPoses.Add(poolPose);
        }
    }
}
