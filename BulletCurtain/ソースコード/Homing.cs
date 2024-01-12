using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//�Q�l�@https://techblog.kayac.com/tracking-calculation-of-homing-laser

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
	float propulsion; // ���i��
	GameObject target;

	public void Start()
	{

		// ����v�A���ar�ŉ~��`�����A���̌��S�͂�v^2/r�B������v�Z���Ă����B
		maxCentripetalAccel = speed * speed / curvatureRadius;

		// �I�[���x��speed�ɂȂ�accel�����߂�
		// v = a / k������a=v*k
		propulsion = speed * damping;

		// �ڕW�擾
		target = GameObject.FindWithTag("Enemy");

		// �����ɉ����郉���_���ɎU��΂�v�f
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
	

	// IApplicableDamage���p�����Ă���object�ɓ���������_���[�W��^����
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



