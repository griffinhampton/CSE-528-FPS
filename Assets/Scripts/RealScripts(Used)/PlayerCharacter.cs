using UnityEngine;
using System.Collections;

public class PlayerCharacter : MonoBehaviour 
{
	private int _health;
    CharacterController cc;

    void Start() 
    {
		_health = 5;
        cc = (CharacterController)GetComponent<CharacterController>();
        //if (cc.isTrigger)
        //    Debug.Log("Palyer's Character controller is a trigger");
        //else
         //   Debug.Log("Palyer's Character controller is not a trigger");

    }

	public void Hurt(int damage) 
    {
		_health -= damage;
		Debug.Log("Health: " + _health);
        //if (cc.isTrigger)
        //    Debug.Log("Palyer's Character controller is a trigger");
        //else
        //    Debug.Log("Palyer's Character controller is not a trigger");

    }
}
