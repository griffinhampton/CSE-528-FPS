using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{

    //bool for keeping track of gun's ability to fire
    private bool readyToFire;
    [SerializeField] private AudioClip gunShot;
    private AudioSource audioSource;

    void Start ()
    {
        readyToFire = true;
        audioSource = GetComponent<AudioSource>();
    }

    public void Bang()
    {
        StartCoroutine(ShootAnim());
    }

    //accessor for checking gun's state
    public bool ReadyToFire()
    {
        return readyToFire;
    }

    //animation for gun recoil; set readyToFire to false while animation plays
    private IEnumerator ShootAnim()
    {
        if (audioSource != null)
        {
            audioSource.clip = gunShot;
            audioSource.Play();
        }
            readyToFire = false;

        //rotate in small increments every 1/30th of a second
        for(int x = 0; x < 4; x++)
        {
            transform.Rotate(-12f, 0, 0);
            yield return new WaitForSeconds(0.033f);
        }
        for (int i = 0; i < 4; i++)
        {
            transform.Rotate(12f, 0, 0);
            yield return new WaitForSeconds(0.033f);
        }
        readyToFire = true;
    }
}
