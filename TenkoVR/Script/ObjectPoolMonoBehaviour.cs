using System.Collections.Generic;
using UnityEngine;
using UniRx;


public abstract class ObjectPoolMonoBehaviour<T1, T2> : MonoBehaviour
    where T1 : Component // �I�u�W�F�N�g�v�[���̌^
    where T2 : Component // �I�u�W�F�N�g�v�[���ŊǗ�����I�u�W�F�N�g�̌^
{
    // �I�u�W�F�N�g�v�[���̃C���X�^���X
    static T1 instance;
    public static T1 Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindAnyObjectByType<T1>();
                if (instance == null)
                {
                    Debug.LogError(typeof(T1) + " ���A�^�b�`���Ă���GameObject�͂���܂���");
                }
            }
            return instance;
        }
    }
    [Header("�I�u�W�F�N�g�v�[���ŊǗ�����Prefab")]
    [SerializeField] T2 _prefab;

    [Header("�v�[�����������ɐ�������鐔")]
    [SerializeField] int _initiarizeSize = 10;
    [Header("���������ɐ�������Ԋu/�t���[����")]
    [SerializeField] int _initiarizeSpan = 1;

    // �Ǘ��p���X�g
    List<T2> _poolList = new();
    public List<T2> PoolList => _poolList;


    protected virtual void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this as T1;
    }

    // �v�[�������� 
    public virtual void InitiarizePool()
    {
        // intiarizeSpan�̃t���[���o�ߖ���initiarizeSize���̕���������
        Observable.TimerFrame(_initiarizeSpan, _initiarizeSpan)
            .Take(_initiarizeSize)
            .Subscribe(_ => CreatePoolObject());
    }

    // �V������������
    protected virtual T2 CreatePoolObject()
    {
        T2 t = Instantiate(_prefab, transform).GetComponent<T2>();
        _poolList.Add(t);
        t.gameObject.SetActive(false);
        return t;
    }

    // �v�[������؂��
    public virtual T2 Rent()
    {
        T2 t = null;

        // ���X�g�����A�N�e�B�u��object���Ăяo��
        foreach (var listT in _poolList)
        {
            if (!listT.gameObject.activeInHierarchy)
            {
                t = listT;
                t.gameObject.SetActive(true);
                break;
            }
        }
        if (t == null) // �Ȃ��ꍇ�͐V��������
        {
            t = CreatePoolObject();
            t.gameObject.SetActive(true);
        }

        return t;
    }

    // �v�[���֕ԋp���ꂽ�Ƃ��̋���
    public virtual void Return(T2 t)
    {
        t.gameObject.SetActive(false);
    }

    public virtual void ResetPool() //�v�[������
    {
        _poolList.Clear();
    }
}
