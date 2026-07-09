using UnityEngine;

public class Fireball : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 20f;
    public float maxDistance = 15f;

    [Header("Effects")]
    public GameObject chargeVFX;
    public GameObject trailVFX;

    [Header("Debug")]
    public bool debugLogs = true;
    public bool debugDrawMovement = true;

    private bool launched = false;
    private Vector3 startPos;
    private Vector3 moveDirection = Vector3.forward;
    private bool loggedFirstMove = false;

    void Start()
    {
        startPos = transform.position;
        Log(
            $"Start. active={gameObject.activeInHierarchy}, " +
            $"pos={transform.position}, forward={transform.forward}, " +
            $"speed={speed}, chargeVFX={chargeVFX?.name ?? "None"}, trailVFX={trailVFX?.name ?? "None"}."
        );

        if (trailVFX != null && trailVFX != chargeVFX)
        {
            SetVFXActive(trailVFX, false);
        }
    }

    void Update()
    {
        if (!launched)
        {
            return;
        }

        transform.position += moveDirection * speed * Time.deltaTime;

        if (!loggedFirstMove)
        {
            loggedFirstMove = true;
            Log($"First movement tick. pos={transform.position}, moveDirection={moveDirection}, delta={moveDirection * speed * Time.deltaTime}.");

            if (debugDrawMovement)
            {
                Debug.DrawRay(transform.position, moveDirection * 3f, Color.yellow, 3f);
            }
        }

        if (Vector3.Distance(startPos, transform.position) >= maxDistance)
        {
            Log($"Max distance reached. distance={Vector3.Distance(startPos, transform.position):0.00}. Destroying.");
            Destroy(gameObject);
        }
    }

    public void Launch()
    {
        Launch(transform.forward);
    }

    public void Launch(Vector3 direction)
    {
        launched = true;
        startPos = transform.position;
        moveDirection = direction.sqrMagnitude > 0.0001f ? direction.normalized : transform.forward;
        loggedFirstMove = false;

        transform.rotation = Quaternion.LookRotation(moveDirection, Vector3.up);

        Log($"Launch called. pos={transform.position}, rot={transform.rotation.eulerAngles}, moveDirection={moveDirection}, speed={speed}, parent={transform.parent?.name ?? "None"}.");

        if (chargeVFX != null && chargeVFX != trailVFX)
        {
            SetVFXActive(chargeVFX, false);
        }

        if (trailVFX != null)
        {
            SetVFXActive(trailVFX, true);
        }
    }

    public void EnableVFX()
    {
        Log("EnableVFX called.");

        if (chargeVFX != null)
        {
            SetVFXActive(chargeVFX, true);
        }
    }

    public void DisableVFX()
    {
        Log("DisableVFX called.");

        if (chargeVFX != null && chargeVFX != trailVFX)
        {
            SetVFXActive(chargeVFX, false);
        }
    }

    private void SetVFXActive(GameObject vfxRoot, bool active)
    {
        if (vfxRoot == null)
        {
            return;
        }

        if (vfxRoot != gameObject)
        {
            vfxRoot.SetActive(active);
            Log($"SetVFXActive {vfxRoot.name} active={active}.");
            return;
        }

        ParticleSystem[] particles = vfxRoot.GetComponentsInChildren<ParticleSystem>(true);
        Log($"SetVFXActive on projectile root active={active}; controlling {particles.Length} particle systems without disabling root.");

        foreach (ParticleSystem particle in particles)
        {
            if (active)
            {
                particle.gameObject.SetActive(true);
                particle.Play(true);
            }
            else
            {
                particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }
    }

    private void Log(string message)
    {
        if (!debugLogs)
        {
            return;
        }

        Debug.Log($"[Fireball] t={Time.time:0.00} frame={Time.frameCount} {message}", this);
    }
}
