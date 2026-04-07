using System.Collections;
using UnityEngine;

public class Target : MonoBehaviour, IReactiveTarget
{
    bool canhit = true;
    public void ReactToHit()
    {
        if (!canhit) return;
        StartCoroutine(ReactToHitCR());
    }


    IEnumerator ReactToHitCR()
    {
        canhit = false;

        Quaternion startRot = transform.rotation;
        Quaternion hitRot = startRot * Quaternion.Euler(-90f, 0f, 0f);

        float t = 0f;
        float falltime = 0.5f;
        
        while (t/falltime < 1f)
        {
            t += Time.deltaTime * 3f;
            transform.rotation = Quaternion.Slerp(startRot, hitRot, t/falltime);
            yield return null;
        }

        t = 0f;

        yield return new WaitForSeconds(1.5f);

        while (t < 1f)
        {
            t += Time.deltaTime * 3f;
            transform.rotation = Quaternion.Slerp(hitRot, startRot, t);
            yield return null;
        }

        transform.rotation = startRot;
        canhit = true;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
