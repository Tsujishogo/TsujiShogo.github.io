using System.Collections;
using System;
using UnityEngine;

/// <summary>
///  �Q�[���X�e�[�W�S�̂��Ǘ�����N���X
/// </summary>
public class StageManager : MonoBehaviour
{
    public static StageManager instance;

    [Header("�Q�[���J�n���Ă������UI���o��܂ł̎���")]
    [SerializeField] float _showUITime = 5f;
    [Header("�N���A�ナ�v���C�Đ����J�n�����܂ł̎���")]
    [SerializeField] float _replayPlayTime = 1f;
    [Header("�v���C���[�̑_����ʒu�iSnipePoint")]
    [SerializeField] Transform _player;
    [Header("UI�̂ݕ\������Camera")]
    [SerializeField] Camera _interactionCamera;
    [Header("�f�X�N�g�b�v�p�T�uCamera")]
    [SerializeField] GameObject _subCamera;
    [Header("���ɗU������K�C�h���C��")] 
    [Header("���U���gUI")]
    [SerializeField] GameObject _resultUI;
    [Header("ManagerClass�Q")]
    [SerializeField] EnemyManager _enemyManager;
    [SerializeField] TimeManager _timeManager;

    public bool IsShowUI { get; private set; }


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
        // UI�\���p�̃J�������A�N�e�B�u��
        DelayAction.Play(this, _showUITime, () => 
        {
            _interactionCamera.enabled = true;
            IsShowUI = true;
        });

        // �T�u�J������\���ۂ𔽉f
        _subCamera.SetActive(SceneChanger.instance.IsSubCamera); 
    }

     public void GameStart()
     {
        // UI�������ēG�̃��X�|�[���J�n
        _interactionCamera.enabled = false;
        _enemyManager.EnemySpawn();
        // ���v���C�̋L�^�J�n
        RecordManager.instance.StartRecording(); 
     }

    public void StageClear()
    {
        //�N���A������
        DelayAction.Play(this, _showUITime, () =>
        {
            Debug.Log("clear");
            ExplanationUI(true); // UI�A�R���g���[���[�\��
            _resultUI.SetActive(true); // ���U���g��ʕ\��
            DelayAction.Play(this, _replayPlayTime, () => RecordManager.instance.StartReplay());// ���v���C�Đ�
        });
    }

    void ExplanationUI(bool value) // UI�\���p�J����ONOFF
    {
        _interactionCamera.enabled = value;
    }

    public Transform GetPlayerTransform() // �v���C���[��tranform��Ԃ�
    {
        return _player;
    }

    public void RayHit() // �X���[���� 
    {
        _timeManager.SlowChecker();
    }
}
