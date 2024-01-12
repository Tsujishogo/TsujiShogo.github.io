using UnityEngine;


public class BreakableOBJ : MonoBehaviour
{
    // ‰ó‚¹‚éobject‚ÌHP
    [SerializeField] int hp = 100;   
     

    void OnTriggerEnter(Collider other)
    {
        // ’e‚ª“–‚½‚Á‚½‚çHPŒ¸‚ç‚·
        hp--;
        // HP‚ª‚È‚­‚È‚Á‚½‚ç”š”­‚µ‚ÄobjectÁ‚·
        if (hp <= 0)
        {
            ParticleManager.PlayParticle("Explosion", transform.position);
            Destroy(gameObject);
        }
    }
}
