using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public enum State { Idle, Patrol, Chase, Search, Return }
    public State state = State.Patrol;

    [Header("References")]
    public NavMeshAgent agent;
    public Transform eyes; // origin for raycasts
    public Transform[] patrolPoints;
    public LayerMask obstacleMask;
    public string playerTag = "Player";

    [Header("Detection")]
    public float detectionRange = 16f;
    [Range(0, 180)] public float fieldOfView = 100f;
    public float lostSightTime = 3f;
    public float searchDuration = 6f;

    [Header("Movement")]
    public float patrolSpeed = 2f;
    public float chaseSpeed = 3.5f;
    public float stoppingDistance = 1.6f;

    [Header("Audio / VFX")]
    public AudioSource audioSource;
    public AudioClip alertClip;
    public AudioClip footstepClip;
    public Renderer bodyRenderer; // to tweak material on reveal

    // runtime
    Transform player;
    Vector3 lastKnownPlayerPos;
    int patrolIndex = 0;
    float lostSightCounter = 0f;
    float searchTimer = 0f;
    Vector3 patrolOrigin;

    void Start()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        agent.stoppingDistance = stoppingDistance;
        agent.speed = patrolSpeed;

        if (eyes == null) eyes = transform; // fallback
        patrolOrigin = transform.position;

        var playerObj = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObj) player = playerObj.transform;

        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            // fallback single patrol point = origin
            patrolPoints = new Transform[1];
            GameObject go = new GameObject("PatrolPoint0");
            go.transform.position = patrolOrigin;
            patrolPoints[0] = go.transform;
        }

        GoToNextPatrol();
    }

    void Update()
    {
        if (player == null) return;

        switch (state)
        {
            case State.Patrol:
                PatrolUpdate();
                break;
            case State.Chase:
                ChaseUpdate();
                break;
            case State.Search:
                SearchUpdate();
                break;
            case State.Return:
                ReturnUpdate();
                break;
            case State.Idle:
                // small idle behavior
                break;
        }

        // Optional: footstep sound trigger by agent velocity
        if (agent.velocity.magnitude > 0.1f && !audioSource.isPlaying)
        {
            audioSource.clip = footstepClip;
            audioSource.loop = true;
            audioSource.Play();
        }
        else if (agent.velocity.magnitude <= 0.1f && audioSource.clip == footstepClip)
        {
            audioSource.Stop();
            audioSource.loop = false;
        }

        // detection check (can trigger chase)
        if (CanSeePlayer())
        {
            lastKnownPlayerPos = player.position;
            if (state != State.Chase)
            {
                StartChase();
            }
            lostSightCounter = lostSightTime;
        }
    }

    #region State logic
    void PatrolUpdate()
    {
        agent.speed = patrolSpeed;
        if (!agent.pathPending && agent.remainingDistance < 0.7f)
        {
            GoToNextPatrol();
        }
    }

    void ChaseUpdate()
    {
        agent.speed = chaseSpeed;
        agent.SetDestination(player.position);

        float dist = Vector3.Distance(transform.position, player.position);
        if (dist <= stoppingDistance)
        {
            // reached player: apply damage / trigger catch
            OnCatchPlayer();
        }

        if (!CanSeePlayer())
        {
            lostSightCounter -= Time.deltaTime;
            if (lostSightCounter <= 0f)
            {
                // start searching around last known pos
                state = State.Search;
                searchTimer = searchDuration;
                agent.SetDestination(lastKnownPlayerPos);
            }
        }
    }

    void SearchUpdate()
    {
        agent.speed = patrolSpeed;
        if (!agent.pathPending && agent.remainingDistance < 0.7f)
        {
            // wander a bit around last known pos
            Vector3 randomOffset = Random.insideUnitSphere * 3f;
            randomOffset.y = 0;
            Vector3 target = lastKnownPlayerPos + randomOffset;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(target, out hit, 2f, NavMesh.AllAreas))
                agent.SetDestination(hit.position);
        }

        searchTimer -= Time.deltaTime;
        if (searchTimer <= 0f)
        {
            state = State.Return;
            agent.SetDestination(patrolPoints[patrolIndex].position);
        }
    }

    void ReturnUpdate()
    {
        agent.speed = patrolSpeed;
        if (!agent.pathPending && agent.remainingDistance < 0.7f)
        {
            state = State.Patrol;
            GoToNextPatrol();
        }
    }

    void GoToNextPatrol()
    {
        if (patrolPoints.Length == 0) return;
        patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
        agent.SetDestination(patrolPoints[patrolIndex].position);
    }

    void StartChase()
    {
        state = State.Chase;
        agent.SetDestination(player.position);
        PlayAlert();
        // visual cue: make the enemy slightly brighter
        if (bodyRenderer) SetMaterialReveal(1.5f);
    }
    #endregion

    #region Detection
    bool CanSeePlayer()
    {
        if (player == null) return false;

        Vector3 dir = (player.position - eyes.position);
        float distanceToPlayer = dir.magnitude;
        if (distanceToPlayer > detectionRange) return false;

        float angle = Vector3.Angle(eyes.forward, dir.normalized);
        if (angle > fieldOfView * 0.5f) return false;

        // raycast for line of sight
        RaycastHit hit;
        Vector3 origin = eyes.position;
        Vector3 to = player.position + Vector3.up * 0.9f; // aim for center
        if (Physics.Raycast(origin, (to - origin).normalized, out hit, detectionRange, ~0))
        {
            if (hit.transform.CompareTag(playerTag)) return true;
            // hit obstacle before player
            if (((1 << hit.collider.gameObject.layer) & obstacleMask) != 0) return false;
        }

        return false;
    }
    #endregion

    #region Utilities
    void PlayAlert()
    {
        if (audioSource && alertClip) audioSource.PlayOneShot(alertClip);
    }

    void SetMaterialReveal(float emissiveMultiplier)
    {
        if (bodyRenderer == null) return;
        foreach (var mat in bodyRenderer.materials)
        {
            if (mat.HasProperty("_EmissionColor"))
            {
                Color baseColor = mat.GetColor("_EmissionColor");
                mat.SetColor("_EmissionColor", baseColor * emissiveMultiplier);
            }
        }
    }

    void OnCatchPlayer()
    {
        // Example: call player's health script (if present)
        var hp = player.GetComponent<PlayerHealth>();
        if (hp != null) hp.ApplyDamage(1);

        // fallback: push player or play sound
        // After catching, you can choose to stop chase or continue
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Vector3 fwd = (eyes != null) ? eyes.forward : transform.forward;
        Gizmos.color = Color.red;
        Gizmos.DrawRay((eyes != null)? eyes.position : transform.position, Quaternion.Euler(0, fieldOfView/2, 0) * fwd * (detectionRange));
        Gizmos.DrawRay((eyes != null)? eyes.position : transform.position, Quaternion.Euler(0, -fieldOfView/2, 0) * fwd * (detectionRange));
    }
    #endregion
}
