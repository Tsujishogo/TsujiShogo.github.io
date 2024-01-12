using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//参考　https://techblog.kayac.com/tracking-calculation-of-homing-laser

public class Homing : MonoBehaviour
{
	[SerializeField] float countPerMeter = 20f;
	[SerializeField] float speed = 30f;
	[SerializeField] float curvatureRadius = 25f;
	[SerializeField] float damping = 0.1f;
	[SerializeField] float impact = 1f; 

	Vector3 velocity;
	public float time;

	float maxCentripetalAccel;
	float propulsion; // 推進力
	GameObject target;

	public void Start()
	{

		// 速さv、半径rで円を描く時、その向心力はv^2/r。これを計算しておく。
		maxCentripetalAccel = speed * speed / curvatureRadius;

		// 終端速度がspeedになるaccelを求める
		// v = a / kだからa=v*k
		propulsion = speed * damping;

		// 目標取得
		target = GameObject.FindWithTag("Enemy");

		// 初速に加えるランダムに散らばる要素
		velocity = new Vector3(Random.Range(-5f, 5f), Random.Range(0, 5f), Random.Range(-5f, 5f));

		propulsion = speed * damping;
	}
	public void Update()
	{
		
		var toTarget = target.transform.position - transform.position;
		var vn = velocity.normalized;
		var dot = Vector3.Dot(toTarget, vn);
		var centripetalAccel = toTarget - (vn * dot);
		var centripetalAccelMagnitude = centripetalAccel.magnitude;
		if (centripetalAccelMagnitude > 1f)
		{
			centripetalAccel /= centripetalAccelMagnitude;
		}
		var force = centripetalAccel * maxCentripetalAccel ;
		force += vn * propulsion;
		force -= velocity * damping;
		velocity += force * Time.deltaTime;
		transform.position += velocity * Time.deltaTime;
		time += Time.deltaTime;
		maxCentripetalAccel += time * 10;
		//Debug.Log(toTarget);
	}
	

	// IApplicableDamageを継承しているobjectに当たったらダメージを与える
	private void OnTriggerEnter(Collider other)
	{
		var hit = other.GetComponent<IApplicableDamage>();
		if (hit != null)
		{
			hit.ReceiveDamage();
			Destroy(gameObject);
			ParticleManager.PlayParticle("TrisSpark", transform.position);
		}
	}
}



