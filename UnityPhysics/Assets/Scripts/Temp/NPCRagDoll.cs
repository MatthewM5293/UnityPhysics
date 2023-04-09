using System.Linq;
using UnityEditor.PackageManager;
using UnityEngine;

public class NPCRagDoll : MonoBehaviour
{
    private enum NPCState
    {
        Walking,
        Ragdoll
    }

    [SerializeField]
    private Transform target;

    private Rigidbody[] ragdollRigidbodies;
    private NPCState currentState = NPCState.Walking;
    private Animator animator;
    private CharacterController characterController;

    void Awake()
    {
        ragdollRigidbodies = GetComponentsInChildren<Rigidbody>();
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();

        DisableRagdoll();
    }

    // Update is called once per frame
    void Update()
    {
        switch (currentState)
        {
            case NPCState.Walking:
                WalkingBehaviour();
                break;
            case NPCState.Ragdoll:
                RagdollBehaviour();
                break;
        }
    }

    public void TriggerRagdoll(Vector3 force, Vector3 hitPoint)
    { 
        EnableRagdoll();

        Rigidbody hitRigidbody = ragdollRigidbodies.OrderBy(rigidbody => Vector3.Distance(rigidbody.position, hitPoint)).First();

        hitRigidbody.AddForceAtPosition(force, hitPoint, ForceMode.Impulse);

        currentState = NPCState.Ragdoll;
    } 

    private void DisableRagdoll()
    {
        foreach (var rigidbody in ragdollRigidbodies)
        {
            rigidbody.isKinematic = true;
        }

        animator.enabled = true;
        characterController.enabled = true;
    }

    private void EnableRagdoll()
    {
        foreach (var rigidbody in ragdollRigidbodies)
        {
            rigidbody.isKinematic = false;
        }

        animator.enabled = false;
        characterController.enabled = false;
    }

    private void WalkingBehaviour()
    {
        Vector3 direction = target.transform.position - transform.position;
        direction.y = 0;
        direction.Normalize();

        Quaternion toRotation = Quaternion.LookRotation(direction, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, 20 * Time.deltaTime);
    }

    private void RagdollBehaviour()
    {

    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            EnableRagdoll();

            currentState = NPCState.Ragdoll;
        }
    }


}