using UnityEngine;
using System.Collections;

public class PlayerCharacter : MonoBehaviour 
{
    [Header("Health")]
    [Tooltip("Damage the player takes when touching an alive pig.")]
    [Min(0)]
    [SerializeField] private int pigTouchDamage = 1;

    [Tooltip("If true, touching an alive pig will kill it (ragdoll) and apply damage to the player.")]
    [SerializeField] private bool killPigAndDamageOnTouch = true;

    private CharacterController cc;

    void Start() 
    {
		cc = (CharacterController)GetComponent<CharacterController>();
        //if (cc.isTrigger)
        //    Debug.Log("Palyer's Character controller is a trigger");
        //else
         //   Debug.Log("Palyer's Character controller is not a trigger");

    }

	public void Hurt(int damage) 
    {
        PlayerHealth.DamagePlayer(damage, source: null);
        Debug.Log("Health: " + PlayerHealth.GetHealth());
        //if (cc.isTrigger)
        //    Debug.Log("Palyer's Character controller is a trigger");
        //else
        //    Debug.Log("Palyer's Character controller is not a trigger");

    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (!killPigAndDamageOnTouch) return;
        if (hit == null) return;
        HandlePigTouch(hit.collider);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!killPigAndDamageOnTouch) return;
        HandlePigTouch(other);
    }

    private void HandlePigTouch(Collider other)
    {
        if (other == null) return;

        EnemyAI pigAI = other.GetComponentInParent<EnemyAI>();
        if (pigAI == null) return;

        LiveAndLetDie liveAndLetDie = pigAI.GetComponent<LiveAndLetDie>();
        if (liveAndLetDie == null) liveAndLetDie = pigAI.GetComponentInChildren<LiveAndLetDie>(true);
        if (liveAndLetDie == null) liveAndLetDie = pigAI.GetComponentInParent<LiveAndLetDie>();

        if (liveAndLetDie != null && !liveAndLetDie.IsDead)
        {
            liveAndLetDie.Death();
            PlayerHealth.DamagePlayer(pigTouchDamage, source: pigAI.gameObject);
        }
    }
}
