using System.Collections;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField]
    float speed = 20f;
    [SerializeField]
    float lifetime = 2f;

    GameObject GM;  // GameManager
    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GM = GameObject.Find("GameManager");
        StartCoroutine(SelfDestruct());

        var rb = GetComponent<Rigidbody>();
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.useGravity = false;
        rb.linearVelocity = transform.forward * speed;
    }


    private void OnCollisionEnter(Collision collisionInfo)
    {
        if (collisionInfo.gameObject.tag == "enemy") //&& 
                                                     //collisionInfo.impactForceSum.magnitude > targetThresh)
        {
            GameObject enemy = collisionInfo.gameObject;
            WanderingAI ai = enemy.GetComponent<WanderingAI>();
            if (ai != null && ai.IsAlive())
            {
                ai.SetAlive(false);
                if (GM != null) GM.SendMessage("EnemyHit");
                enemy.SendMessage("ReactToHit");
            }

            Destroy(gameObject);
        }

        if (collisionInfo.gameObject.GetComponent<IReactiveTarget>() != null)
        {
            IReactiveTarget target = collisionInfo.gameObject.GetComponent<IReactiveTarget>();
            target.ReactToHit();
            Destroy(gameObject);
        }
        Destroy(gameObject);
    }

    private IEnumerator SelfDestruct()
    {
        yield return new WaitForSeconds(1f);

        Destroy(gameObject,lifetime);
    }
}
