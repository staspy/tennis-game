using UnityEngine;

public class Bot : MonoBehaviour
{
    private Ball ball; 
    public Transform[] targets; 
    public GameManager gm; 
    
    private float speed;
    private float hitAccuracy; 
    private float hitRadius; 
    private float botPowerMultiplier; 
    private float hitCooldown;

    Vector3 targetPosition; 
    ShotManager shotManager; 
    float lastHitTime = 0f; 
    
    // Запоминаем, какая сложность сейчас применена
    private int currentAppliedDifficulty = -1; 

    void Start()
    {
        targetPosition = transform.position; 
        shotManager = GetComponent<ShotManager>(); 
        
        if (ball == null) ball = FindObjectOfType<Ball>();
        if (gm == null) gm = FindObjectOfType<GameManager>();

        ApplyDifficultySettings(); // Применяем при старте
    }

    void Update()
    {
        if (ball == null || gm == null) return;

        // ПРОВЕРКА: Если сложность изменилась (например, в меню паузы), обновляем настройки!
        if (currentAppliedDifficulty != GameSettings.botDifficulty)
        {
            ApplyDifficultySettings();
        }
        
        Move(); 
        
        if (Time.time - lastHitTime > hitCooldown) 
        {
            TryHitBall(); 
        }
    }

    // Метод, который применяет настройки в зависимости от сложности
    void ApplyDifficultySettings()
    {
        currentAppliedDifficulty = GameSettings.botDifficulty;
        Debug.Log($"Бот применил сложность: {currentAppliedDifficulty}");

        switch (GameSettings.botDifficulty)
        {
            case 0: // 🟢 ЛЕГКИЙ
                speed = 5f; 
                hitAccuracy = 0.2f; 
                hitRadius = 1.1f; 
                botPowerMultiplier = 0.7f; 
                hitCooldown = 0.8f;
                break;
                
            case 1: // 🟡 СРЕДНИЙ
                speed = 7.5f; 
                hitAccuracy = 0.6f; 
                hitRadius = 1.4f; 
                botPowerMultiplier = 0.85f; 
                hitCooldown = 0.6f;
                break;
                
            case 2: // 🔴 СЛОЖНЫЙ
                speed = 10f; 
                hitAccuracy = 1.0f; 
                hitRadius = 1.7f; 
                botPowerMultiplier = 0.8f; 
                hitCooldown = 0.4f;
                break;
        }
    }

    void Move()
    {
        if (ball.transform.position.z > 0) 
        {
            targetPosition.x = ball.transform.position.x; 
            targetPosition.z = ball.transform.position.z; 
        }
        else 
        {
            targetPosition.x = 0;
            targetPosition.z = 7f; 
        }
        
        transform.position = Vector3.MoveTowards(transform.position, new Vector3(
            Mathf.Clamp(targetPosition.x, -5.4f, 5.4f), 1f, Mathf.Clamp(targetPosition.z, 1f, 11.8f)
        ), speed * Time.deltaTime); 
    }

    void TryHitBall()
    {
        if (!ball.isInPlay) return;

        if (ball.transform.position.z > 0.5f && ball.transform.position.y > 0.5f && ball.transform.position.y < 3.5f)
        {
            if (gm.lastHitter == "Bot") return; 
            if (ball.transform.position.z < transform.position.z - 0.5f) return; 

            Vector2 botPos = new Vector2(transform.position.x, transform.position.z);
            Vector2 ballPos = new Vector2(ball.transform.position.x, ball.transform.position.z);
            float distXZ = Vector2.Distance(botPos, ballPos);

            if (distXZ < hitRadius) 
            {
                Transform bestTarget = targets[0];
                GameObject playerObj = GameObject.FindWithTag("Player"); 

                if (playerObj != null && targets.Length > 0) 
                {
                    float maxDist = 0;
                    foreach (Transform t in targets)
                    {
                        float d = Vector3.Distance(t.position, playerObj.transform.position);
                        if (d > maxDist) { maxDist = d; bestTarget = t; }
                    }
                }

                if (Random.value > hitAccuracy) 
                {
                    bestTarget = targets[Random.Range(0, targets.Length)];
                }

                Shot currentShot = shotManager.topSpin; 
                
                float finalHitForce = currentShot.hitForce * botPowerMultiplier;
                float finalUpForce = currentShot.upForce; 
                
                finalHitForce = Mathf.Clamp(finalHitForce, 5f, 22f);

                ball.HitBall(ball.transform.position, bestTarget.position, finalUpForce, finalHitForce);
                gm.RegisterHit("Bot");
                
                lastHitTime = Time.time; 

                if (AudioManager.instance != null) AudioManager.instance.PlayRacketHit();
            }
        }
    }
}