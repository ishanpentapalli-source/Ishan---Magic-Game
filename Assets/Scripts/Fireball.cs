using UnityEngine;
using UnityEngine.UIElements;

public class Fireball : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 20f;
    public float maxDistance = 15f;

    [Header("Effects")]
    public GameObject chargeVFX;
    public GameObject trailVFX;

    private bool launched = false;
    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;

        if (trailVFX != null) 
        {
            trailVFX.SetActive(false);
        }
    }

    void Update()
    {
        if (!launched)
        {
            return;
        }

        transform.position += transform.forward * speed * Time.deltaTime;

        if (Vector3.Distance(startPos, transform.position) >= maxDistance)
        {
            Destroy(gameObject);
        }
    }

    public void Launch()
    {
        launched = true;

        if (trailVFX != null)
        {
            trailVFX.SetActive(true);
        }
    }

    public void EnbableVFX()
    {
        if (chargeVFX != null)
        {
            chargeVFX.SetActive(true);
        }
    }

    public void DisableVFX()
    {
        if (chargeVFX != null)
        {
            chargeVFX.SetActive(false);
        }
    }
}
