using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AIState {
    Idle = 0,
    RandomMove = 1,
    WaypointMove = 2,
    ChasePlayer = 3
}

public class WanderingAI : MonoBehaviour {
    public float speed = 3.0f;
    public float obstacleRange = 5.0f;
    public AIState currentState = AIState.RandomMove;
    public Transform[] waypoints;
    public float detectionRange = 10f;
    public float viewAngle = 90f;  // 视野角度
    public bool showDebugVisuals = true;  // 是否显示调试视觉
    public Color debugColor = new Color(1, 0, 0, 0.2f);  // 调试视觉的颜色
    
    [SerializeField] GameObject fireballPrefab;
    private GameObject fireball;
    private int currentWaypoint = 0;
    private Transform player;
    private AIState previousState;  // 记录之前的状态
    
    private bool isAlive;
	
    void Start() {
        isAlive = true;
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }
    
    void Update() {
        if (!isAlive) return;

        // 检查是否可以看到玩家并更新状态
        bool canSeePlayer = CheckCanSeePlayer();
        UpdateAIState(canSeePlayer);

        // Handle movement based on current state
        switch (currentState) {
            case AIState.Idle:
                // Do nothing, stay in place
                break;

            case AIState.RandomMove:
                RandomMovement();
                break;

            case AIState.WaypointMove:
                WaypointMovement();
                break;

            case AIState.ChasePlayer:
                ChasePlayerMovement();
                break;
        }

        // Always check for shooting, regardless of movement state
        CheckForShooting();
    }

    private void UpdateAIState(bool canSeePlayer) {
        switch (currentState) {
            case AIState.Idle:
                // 静止状态永远不会追击
                break;

            case AIState.RandomMove:
                // 随机移动状态看到玩家会追击
                if (canSeePlayer) {
                    previousState = AIState.RandomMove;
                    currentState = AIState.ChasePlayer;
                }
                break;

            case AIState.WaypointMove:
                // 路点移动状态看到玩家会追击
                if (canSeePlayer) {
                    previousState = AIState.WaypointMove;
                    currentState = AIState.ChasePlayer;
                }
                break;

            case AIState.ChasePlayer:
                // ChasePlayer是独立状态，不会自动切换回其他状态
                break;
        }
    }

    private bool CheckCanSeePlayer() {
        if (player == null) return false;

        Vector3 directionToPlayer = player.position - transform.position;
        float distanceToPlayer = directionToPlayer.magnitude;

        // 检查是否在检测范围内
        if (distanceToPlayer > detectionRange) return false;

        // 检查是否在视角范围内
        float angle = Vector3.Angle(transform.forward, directionToPlayer);
        if (angle > viewAngle / 2f) return false;

        // 检查是否有障碍物阻挡
        RaycastHit hit;
        if (Physics.Raycast(transform.position, directionToPlayer.normalized, out hit, detectionRange)) {
            // 如果射线击中的第一个物体不是玩家，说明有墙壁阻挡
            if (hit.transform != player) {
                return false;
            }
            return true;
        }

        return false;
    }

    void OnDrawGizmos() {
        if (!showDebugVisuals) return;

        // 绘制视线范围
        Gizmos.color = debugColor;
        
        // 绘制扇形视野
        Vector3 leftRayDirection = Quaternion.Euler(0, -viewAngle / 2f, 0) * transform.forward;
        Vector3 rightRayDirection = Quaternion.Euler(0, viewAngle / 2f, 0) * transform.forward;

        Gizmos.DrawLine(transform.position, transform.position + leftRayDirection * detectionRange);
        Gizmos.DrawLine(transform.position, transform.position + rightRayDirection * detectionRange);

        // 绘制弧线
        int segments = 20;
        Vector3 prevPoint = transform.position + leftRayDirection * detectionRange;
        for (int i = 1; i <= segments; i++) {
            float angle = -viewAngle / 2f + (viewAngle * i / segments);
            Vector3 direction = Quaternion.Euler(0, angle, 0) * transform.forward;
            Vector3 point = transform.position + direction * detectionRange;
            Gizmos.DrawLine(prevPoint, point);
            prevPoint = point;
        }
    }

    private void RandomMovement() {
        transform.Translate(0, 0, speed * Time.deltaTime);
        
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;
        if (Physics.SphereCast(ray, 0.75f, out hit) && hit.distance < obstacleRange) {
            float angle = Random.Range(-110, 110);
            transform.Rotate(0, angle, 0);
        }
    }

    private void WaypointMovement() {
        if (waypoints == null || waypoints.Length == 0) return;

        Vector3 targetPosition = waypoints[currentWaypoint].position;
        Vector3 direction = (targetPosition - transform.position).normalized;
        
        // Rotate towards waypoint
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        
        // Move towards waypoint
        transform.Translate(0, 0, speed * Time.deltaTime);

        // Check if we've reached the waypoint
        if (Vector3.Distance(transform.position, targetPosition) < 0.5f) {
            currentWaypoint = (currentWaypoint + 1) % waypoints.Length;
        }
    }

    private void ChasePlayerMovement() {
        if (player == null) return;

        // 只有在能看到玩家时才追击
        if (CheckCanSeePlayer()) {
            Vector3 direction = (player.position - transform.position).normalized;
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            
            // 持续旋转面向玩家
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
            
            // 移动追击
            transform.Translate(0, 0, speed * Time.deltaTime);
        }
        // 如果看不到玩家，就停在原地
    }

    private void CheckForShooting() {
        // 只有在能看到玩家的情况下才射击
        if (CheckCanSeePlayer()) {
            if (fireball == null) {
                fireball = Instantiate(fireballPrefab) as GameObject;
                fireball.transform.position = transform.TransformPoint(Vector3.forward * 1.5f);
                fireball.transform.rotation = transform.rotation;
            }
        }
    }

	public void SetAlive(bool alive) {
		isAlive = alive;
	}
}
