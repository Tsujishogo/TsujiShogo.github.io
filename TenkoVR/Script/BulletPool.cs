using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// オブジェクトプールクラス
/// </summary>
public class BulletPool
{
	public BulletPool(GameObject bullet)
    {
		this.bulletPrefab = bullet;  
	}

	// プールされる弾のリストを初期化
	[SerializeField]　List<GameObject> bulletList = new List<GameObject>();

	// プールするobject
	GameObject bulletPrefab = null;


	// 弾を保持しておくリスト
	public List<GameObject> BulletList
	{
		get { return bulletList; }
	}

	public int ActiveBulletCount //プール内のアクティブなobjectを返す
	{
		get { return bulletList.Count(b => b.activeInHierarchy); }
	}

	public GameObject Call()
    {
		GameObject obj = null;

		// リストから非アクティブなobjectを呼び出す
		foreach (var listObj in bulletList)
		{
			if (!listObj.activeInHierarchy)
			{
				obj = listObj;
				break;
			}
		}
		if (obj == null) //　ない場合は新しく生成
        {
			obj = InstantiateBullet();
        }
		return obj;
	}

	public GameObject InstantiateBullet() // 新しく生成してプールリストに追加
    {
		GameObject obj = GameObject.Instantiate(bulletPrefab) as GameObject;
		obj.SetActive(false);
		if (!bulletList.Contains(obj))
		{
			bulletList.Add(obj);
		}
		return obj;
    }
	public void Reset() //プール消去
    {
		bulletList.Clear();
		bulletList = null;
		bulletPrefab = null;
	}
}
