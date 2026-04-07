using UnityEngine;
using System.Collections;

public class ReactiveTarget2 : MonoBehaviour , IReactiveTarget
{
    [SerializeField] private GameObject shatterPrefab;
    [SerializeField] private AudioClip death;
    private GameObject _shatter;
	public void ReactToHit() 
    {
		WanderingAI behavior = GetComponent<WanderingAI>();
		if (behavior != null) {
			behavior.SetAlive(false);
		}
        GetComponent<AudioSource>().PlayOneShot(death);
		StartCoroutine(Die());
	}

	private IEnumerator Die() 
    {
        this.GetComponentInChildren<MeshRenderer>().enabled = false;
        this.GetComponent<MeshRenderer>().enabled = false;
        this.GetComponent<BoxCollider>().enabled = false;
        GameObject.Destroy(transform.GetChild(0).gameObject);
        Vector3 pos = new Vector3(this.transform.position.x, 0f, this.transform.position.z);

        _shatter = Instantiate(shatterPrefab) as GameObject;

        _shatter.transform.position = pos;

        Time.timeScale = 1;
        yield return new WaitForSeconds(3f);

        Destroy(this.gameObject);
        Destroy(_shatter.gameObject);

        //yield return new WaitForSeconds(4);
    }
}
