using UnityEngine;
using System.Collections;

public class ReactiveTargetX : MonoBehaviour 
{
    [SerializeField] private AudioClip enemyHit;
    private bool dead = false;

    private AudioSource audioSource;
    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

	public void ReactToHit() 
    {
		WanderingAI behavior = GetComponent<WanderingAI>();
		if (behavior != null) 
        {
			behavior.SetAlive(false);
		}

        if (!dead)
        {
            if (audioSource != null) PlayHitSound();

            if (gameObject.name == "Enemy1")
                StartCoroutine(Die3());
            else StartCoroutine(Die3());
        }
	}

    private void PlayHitSound()
    {
        audioSource.clip = enemyHit;
        audioSource.Play();
    }

	private IEnumerator Die()
    {
        //this.transform.Rotate(-75, 0, 0);

        //yield return new WaitForSeconds(1.5f);

        // Modified version
        float angle = 0;

        while (angle > -90.0f)
        {
            this.transform.Rotate(-1, 0, 0);
            angle -= 1.0f;
            yield return null;
        }

        yield return new WaitForSeconds(1.5f);

        Destroy(this.gameObject);
	}

    private IEnumerator Die2() // by Payton Harris
    {
        dead = true;

        int i = 0;
        while (i < 45)
        {
            this.transform.Rotate(-2, 0, 0);
            float angle = 2 * (i + 1);
            float y = (1.0f - Mathf.Cos((float)Mathf.Deg2Rad * angle));
            //this.transform.Translate(0, -y, 0, Space.World);
            this.transform.Translate(0, -0.0111f, 0, Space.World);

            i++;
            yield return new WaitForFixedUpdate();
        }
        //this.transform.Rotate(-90 * Time.deltaTime, 0, 0);
        //      this.transform.Translate(0, -0.5f, 0, Space.World);

        yield return new WaitForSeconds(1.5f);
        Destroy(this.gameObject);
    }

    private IEnumerator Die3()
    {
        //Time.timeScale = 1;
        WanderingAI behavior = GetComponent<WanderingAI>();
        behavior.enabled = false;
        Destroy(behavior);        

        var rb = gameObject.AddComponent<Rigidbody>();

        var player = FindFirstObjectByType<CharacterController>();
        Vector3 dir = (transform.position - player.transform.position).normalized;

        rb.AddForce(dir * 10, ForceMode.Impulse);
        yield return new WaitForSeconds(3f);
        //yield return null;

        //Time.timeScale = 1;
        Destroy(gameObject);        
    }
}
