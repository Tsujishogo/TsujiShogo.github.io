using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// �I�u�W�F�N�g�v�[���N���X
/// </summary>
public class BulletPool
{
	public BulletPool(GameObject bullet)
    {
		this.bulletPrefab = bullet;  
	}

	// �v�[�������e�̃��X�g��������
	[SerializeField]�@List<GameObject> bulletList = new List<GameObject>();

	// �v�[������object
	GameObject bulletPrefab = null;


	// �e��ێ����Ă������X�g
	public List<GameObject> BulletList
	{
		get { return bulletList; }
	}

	public int ActiveBulletCount //�v�[�����̃A�N�e�B�u��object��Ԃ�
	{
		get { return bulletList.Count(b => b.activeInHierarchy); }
	}

	public GameObject Call()
    {
		GameObject obj = null;

		// ���X�g�����A�N�e�B�u��object���Ăяo��
		foreach (var listObj in bulletList)
		{
			if (!listObj.activeInHierarchy)
			{
				obj = listObj;
				break;
			}
		}
		if (obj == null) //�@�Ȃ��ꍇ�͐V��������
        {
			obj = InstantiateBullet();
        }
		return obj;
	}

	public GameObject InstantiateBullet() // �V�����������ăv�[�����X�g�ɒǉ�
    {
		GameObject obj = GameObject.Instantiate(bulletPrefab) as GameObject;
		obj.SetActive(false);
		if (!bulletList.Contains(obj))
		{
			bulletList.Add(obj);
		}
		return obj;
    }
	public void Reset() //�v�[������
    {
		bulletList.Clear();
		bulletList = null;
		bulletPrefab = null;
	}
}
