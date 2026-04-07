using UnityEngine;
using System.Collections;

public class ReactiveTarget3 : MonoBehaviour , IReactiveTarget
{
    //private ParticleSystem blood;
    private MeshRenderer flick;
    private bool dead = false;
    [SerializeField] private AudioClip enemyHit;
    [SerializeField] private AudioClip shrinking;
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
            StartCoroutine(Die()); // dying animation
        }
	}

    private void PlayHitSound()
    {
        audioSource.clip = enemyHit;
        audioSource.Play();
    }

    private void PlayShrinkSound()
    {
        audioSource.clip = shrinking;
        audioSource.Play();
    }

    private IEnumerator Die()
    {
        dead = true;        
        int i = 0;
        int j = 0;
        int speed = 10;
        float shrinkSpeed = 1f;
        while (i < 15)
        {            
            this.transform.Rotate(-6, 0, 0);
            this.transform.Translate(0, -0.0333f, 0, Space.World);
            i++;
            yield return new WaitForFixedUpdate();
        }
        PlayShrinkSound();
        while (this.transform.localScale.x > 0.01f)
        {
            this.transform.Rotate(0, 0, j*speed);
            this.transform.localScale -= Vector3.one * Time.deltaTime * shrinkSpeed;
            j++;
            yield return new WaitForFixedUpdate();
        }
        yield return new WaitForSeconds(1.5f);
        Destroy(this.gameObject);
    }
}
