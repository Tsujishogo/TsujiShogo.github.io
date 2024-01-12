using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class Enemy : MonoBehaviour, IApplicableDamage
{
    [SerializeField] GameObject tama1; // 飛ばす弾
    [SerializeField] bool isAttack = false;// 攻撃オンオフ
    [SerializeField] int enemyHP = 100;　//最大体力
    [SerializeField] Slider slider;
    [SerializeField] AudioClip hit;
    [SerializeField] AudioClip bomb;
    [SerializeField] float moveDis = 10f; // 移動する範囲の半径の長さ
    [SerializeField] float moveLow = 1f;  // 移動する高さの範囲　下限
    [SerializeField] float moveHigh = 7f;　// 移動する高さの範囲　上限
    [SerializeField] float moveTime = 2f; // 移動する処理にかける時間

    // 攻撃タイプを変更する回数、それぞれのHPと攻撃タイプをinspectorから指定
    [System.Serializable] struct ChangeTiming
    {
        public int HP;　// このHP以下になると変更
        public int Type; //変更先攻撃タイプ
    }
    [Header("HPの大きい順に登録")]
    [SerializeField] ChangeTiming[] TypeChangeNo;
           
    
    GameObject player;　// プレイヤー
    int currentHP; //現在のHP
    int currentType = 111;　　// 攻撃タイプ変更用変数
    int currentTypeNo= 0;　//攻撃タイプ変更の条件分岐用変数
    public BulletManager bulletmanager;
    Vector3 startPos;
    AudioSource audioSource;

    void Start()
    {
        player = GameObject.FindWithTag("Player");//プレイヤーobject取得
        startPos = transform.position;// 初期座標を保存しておく
        RandomMove();//ランダム移動
        currentHP = enemyHP;//敵の最大hp登録
        slider.value = 1;//HPスライダー初期化
        audioSource = GetComponent<AudioSource>();//audiosource取得
       
    }

    void Update()
    {
        if (currentHP > 0)
        {
            transform.LookAt(player.transform);// 敵のHPが残っているときはプレイヤーのほうを見る
        }
    }

    //初期位置を中心にmoveDistanseを半径とする円内をランダム移動
    private void RandomMove()
    {
        
        
        Vector3 pos = moveDis * Random.insideUnitCircle; // 円内のランダム座標を取得

        // 円内のランダム座標に初期位置、ランダムな高さを加えて次の移動先座標を計算
        Vector3 nextPos = new Vector3(
            pos.x + startPos.x , Random.Range(moveLow, moveHigh), pos.y + startPos.z); 
        if (currentHP > 0||currentType !=0)
        {
            // 次の位置にmovetime/秒かけて移動、その後ループ
            transform.DOMove(nextPos, moveTime).OnComplete(() => RandomMove());
        }

    }

    // 被ダメージ処理
    public void  ReceiveDamage()
    {
        currentHP--;// HP減らす
        // TypeChangeにinspectorから設定されたHP以下になるとその攻撃タイプに変更、
        // 変更後一つしたの条件に待機
        if (currentTypeNo < TypeChangeNo.Length  && currentType != -1)
        {
            if (currentHP < TypeChangeNo[currentTypeNo].HP)
            {
                currentType = TypeChangeNo[currentTypeNo].Type;
                bulletmanager.changeType(currentType);
                currentTypeNo++;
            }
        }
        
        
        if (currentHP <= 0)// HPなくなったら移動を止める
        {
            transform.DOKill();
            bulletmanager.changeType(-1); //撃破処理
            currentType = -1;
;            audioSource.PlayOneShot(bomb);// 撃破時のSE再生
        }
        slider.value = (float)currentHP / (float)enemyHP;// 現在HPをHPスライダーに反映

        audioSource.PlayOneShot(hit) ;// 攻撃ヒット時のSE再生

    }
}
