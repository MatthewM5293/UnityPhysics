using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shoot : MonoBehaviour
{
    [SerializeField]
    private float maximumForce;

    [SerializeField]
    AudioSource source;
    [SerializeField]
    private float maximumForceTime;

    private float timeMouseButtonDown;

    private Camera camera;

    void Awake()
    {
        camera = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            timeMouseButtonDown = Time.time;

            source.Play();
        }

        if (Input.GetMouseButtonUp(0))
        {
            Ray ray = camera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hitInfo))
            {
                NPCRagDoll npc = hitInfo.collider.GetComponentInParent<NPCRagDoll>();

                if (npc != null)
                {
                    float mouseButtonDownDuration = Time.time - timeMouseButtonDown;
                    float forcePercentage = mouseButtonDownDuration / maximumForceTime;
                    float forceMagnitude = Mathf.Lerp(1, maximumForce, forcePercentage);

                    Vector3 forceDirection = npc.transform.position - camera.transform.position;
                    forceDirection.y = 1;
                    forceDirection.Normalize();

                    Vector3 force = forceMagnitude * forceDirection;

                    npc.TriggerRagdoll(force, hitInfo.point);
                }
            }
        }
    }
}