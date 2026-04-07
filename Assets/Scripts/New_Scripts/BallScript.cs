using UnityEngine;
using System.Collections;

//[RequireComponent(typeof(AudioSource))]
public class BallScript : MonoBehaviour 
{
    public float targetThresh = 45.0f;
    public float canThresh = 5.0f;

    GameObject GM;  // GameManager

    void Start()
    {
        GM = GameObject.Find("GameManager");
        StartCoroutine(SelfDestruct());
    }

    // Collision class includes info for physics interaction
    // OnTriggerEnter (Collider collider)                                              
    void OnCollisionEnter(Collision collisionInfo)
    {                                                
		//Debug.Log (Collisioninfo.impactForceSum.magnitude.ToString());

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
    }
    
    // Detect if it hit the enemy fire ball which has "is Trigger" on
    void OnTriggerEnter(Collider co)
    {
        if (co.gameObject.tag == "Fire")
        {
            Destroy(co.gameObject); 
            print("destroy fire ball");
            Destroy(gameObject);
        }
    }

    private IEnumerator SelfDestruct()
    {
        yield return new WaitForSeconds(1f);
        
        Destroy(gameObject);
    }

}
