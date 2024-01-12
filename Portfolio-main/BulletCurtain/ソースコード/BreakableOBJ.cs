using UnityEngine;


public class BreakableOBJ : MonoBehaviour
{
    // �󂹂�object��HP
    [SerializeField] int hp = 100;   
     

    void OnTriggerEnter(Collider other)
    {
        // �e������������HP���炷
        hp--;
        // HP���Ȃ��Ȃ����甚������object����
        if (hp <= 0)
        {
            ParticleManager.PlayParticle("Explosion", transform.position);
            Destroy(gameObject);
        }
    }
}
