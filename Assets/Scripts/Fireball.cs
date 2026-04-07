using UnityEngine;
using System.Collections;

public class Fireball : MonoBehaviour 
{
	public float speed = 10.0f;
	public int damage = 1;

    void Start()
    {
        StartCoroutine("SelfDestruct");
    }

	void OnTriggerEnter(Collider other) 
    {
		PlayerCharacter player = other.GetComponent<PlayerCharacter>();
		if (player != null) 
        {
			player.Hurt(damage);
		}
		Destroy(this.gameObject);
	}
    private IEnumerator SelfDestruct()
    {
        yield return new WaitForSeconds(5f);
        Destroy(gameObject);
    }
}
