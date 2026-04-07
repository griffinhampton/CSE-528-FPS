using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class MoveByNavMeshAgent : MonoBehaviour
{
    public Camera camera;
    public Transform transform;
    public bool isFollow = false;

    NavMeshAgent agent;

    // Use this for initialization
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;

            //if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition),
            //    out hit, 100))
            if (Physics.Raycast(camera.ScreenPointToRay(Input.mousePosition),
               out hit, 100))
            {
                agent.destination = hit.point;
            }
        }
        else
            if (isFollow && transform!=null) agent.destination = transform.position;
    }
}
