using System.Collections.Generic;
using UnityEngine;
using UniRx;


public abstract class ObjectPoolMonoBehaviour<T1, T2> : MonoBehaviour
    where T1 : Component // オブジェクトプールの型
    where T2 : Component // オブジェクトプールで管理するオブジェクトの型
{
    // オブジェクトプールのインスタンス
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
                    Debug.LogError(typeof(T1) + " をアタッチしているGameObjectはありません");
                }
            }
            return instance;
        }
    }
    [Header("オブジェクトプールで管理するPrefab")]
    [SerializeField] T2 _prefab;

    [Header("プール初期化時に生成される数")]
    [SerializeField] int _initiarizeSize = 10;
    [Header("初期化時に生成する間隔/フレーム数")]
    [SerializeField] int _initiarizeSpan = 1;

    // 管理用リスト
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

    // プール初期化 
    public virtual void InitiarizePool()
    {
        // intiarizeSpanのフレーム経過毎にinitiarizeSize数の分生成する
        Observable.TimerFrame(_initiarizeSpan, _initiarizeSpan)
            .Take(_initiarizeSize)
            .Subscribe(_ => CreatePoolObject());
    }

    // 新しく生成する
    protected virtual T2 CreatePoolObject()
    {
        T2 t = Instantiate(_prefab, transform).GetComponent<T2>();
        _poolList.Add(t);
        t.gameObject.SetActive(false);
        return t;
    }

    // プールから借りる
    public virtual T2 Rent()
    {
        T2 t = null;

        // リストから非アクティブなobjectを呼び出す
        foreach (var listT in _poolList)
        {
            if (!listT.gameObject.activeInHierarchy)
            {
                t = listT;
                t.gameObject.SetActive(true);
                break;
            }
        }
        if (t == null) // ない場合は新しく生成
        {
            t = CreatePoolObject();
            t.gameObject.SetActive(true);
        }

        return t;
    }

    // プールへ返却されたときの挙動
    public virtual void Return(T2 t)
    {
        t.gameObject.SetActive(false);
    }

    public virtual void ResetPool() //プール消去
    {
        _poolList.Clear();
    }
}
