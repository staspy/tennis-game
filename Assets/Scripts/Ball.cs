using UnityEngine;

public class Ball : MonoBehaviour
{
    [HideInInspector] public bool isInPlay = false;
    
    private Vector3 velocity;
    private GameManager gm;
    private int bounceCount = 0; 
    private bool hasBounced = false; 
    private float lastZ; 

    void Start()
    {
        gm = FindObjectOfType<GameManager>();
    }

    public void HitBall(Vector3 startPos, Vector3 targetPos, float upForce, float hitForce)
    {
        transform.position = startPos;
        lastZ = startPos.z;
        Physics.SyncTransforms(); 

        Vector3 dir = (targetPos - startPos).normalized;
        velocity = (dir * hitForce) + (Vector3.up * upForce);
        
        isInPlay = true;
        bounceCount = 0; 
        hasBounced = false;
    }

    void Update()
    {
        if (!isInPlay) return;

        velocity += Physics.gravity * Time.deltaTime;
        transform.position += velocity * Time.deltaTime;
        transform.Rotate(Vector3.right * velocity.magnitude * Time.deltaTime);

        // Проверка сетки
        if ((Mathf.Sign(lastZ) != Mathf.Sign(transform.position.z)) && transform.position.y < 1.0f)
        {
            velocity.z *= -0.3f; 
            velocity.x *= 0.5f;
            
            if (lastZ > 0) transform.position = new Vector3(transform.position.x, transform.position.y, 0.2f);
            else transform.position = new Vector3(transform.position.x, transform.position.y, -0.2f);

            if (AudioManager.instance != null) AudioManager.instance.PlayGroundBounce(); 
            lastZ = transform.position.z;
            return;
        }
        lastZ = transform.position.z;

        // Мгновенный аут
        if (Mathf.Abs(transform.position.x) > 4.115f || Mathf.Abs(transform.position.z) > 11.885f)
        {
            if (bounceCount == 0)
            {
                isInPlay = false;
                gm.OnBallLanded(transform.position, true);
                return;
            }
        }

        // КАСАНИЕ ЗЕМЛИ (ПОДНЯЛИ ПОРОГ С 0.1 ДО 0.25)
        if (transform.position.y <= 0.25f && !hasBounced)
        {
            hasBounced = true;
            // Принудительно ставим мяч ровно на этот уровень
            transform.position = new Vector3(transform.position.x, 0.25f, transform.position.z);
            
            if (bounceCount == 0)
            {
                bounceCount++;
                velocity.x *= 0.3f; 
                velocity.z *= 0.5f; 
                velocity.y = Mathf.Abs(velocity.y) * 0.6f; 

                if (AudioManager.instance != null) AudioManager.instance.PlayGroundBounce();
                gm.OnBallLanded(transform.position, false);
            }
            else
            {
                isInPlay = false;
                gm.OnBallLanded(transform.position, false);
            }
        }
        else if (transform.position.y > 0.35f) // Сброс флага отскока, если мяч подлетел выше
        {
            hasBounced = false;
        }
    }
}