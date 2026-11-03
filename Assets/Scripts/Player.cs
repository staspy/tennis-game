using UnityEngine;

public class Player : MonoBehaviour
{
    public Transform aimTarget; 
    public Transform racketModel; 
    public Ball ball; 
    public GameManager gm; 
    
    float moveSpeed = 6f; 
    ShotManager shotManager; 

    void Start()
    {
        shotManager = GetComponent<ShotManager>(); 
        if (gm == null) gm = FindObjectOfType<GameManager>();
        if (ball == null) ball = FindObjectOfType<Ball>();
    }

    void Update()
    {
        if (Time.timeScale == 0f) return;

        float h = Input.GetAxisRaw("Horizontal"); 
        float v = Input.GetAxisRaw("Vertical"); 
        transform.Translate(new Vector3(h, 0, v).normalized * moveSpeed * Time.deltaTime, Space.World); 

        transform.position = new Vector3(
            Mathf.Clamp(transform.position.x, -5.4f, 5.4f), 
            1f, 
            Mathf.Clamp(transform.position.z, -11.8f, -0.5f)
        );

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, new Vector3(0, 0.1f, 0));
        if (groundPlane.Raycast(ray, out float distance))
        {
            Vector3 hitPoint = ray.GetPoint(distance);
            aimTarget.position = new Vector3(hitPoint.x, 0.1f, hitPoint.z);
        }

        if (racketModel != null)
        {
            Vector3 lookPos = aimTarget.position - racketModel.position;
            lookPos.y = 0;
            if (lookPos != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(lookPos);
                racketModel.rotation = Quaternion.Slerp(racketModel.rotation, targetRotation, Time.deltaTime * 15f);
            }
        }

        // ПОДАЧА ИГРОКА
        if (gm != null && gm.currentServer == "Player" && gm.isServeReady && Input.GetKeyDown(KeyCode.Space))
        {
            float finalUpForce = 5f;   // Навесная
            float finalHitForce = 15f;

            if (Input.GetKey(KeyCode.E)) 
            {
                finalUpForce = 2.5f;   // Плоская
                finalHitForce = 20f; 
            }

            ball.HitBall(ball.transform.position, aimTarget.position, finalUpForce, finalHitForce);
            gm.RegisterHit("Player");
            
            if (AudioManager.instance != null) AudioManager.instance.PlayRacketHit();
        }

        // УДАР В ИГРЕ
        if (Input.GetKeyDown(KeyCode.Space) && ball.transform.position.z < 0.5f && !gm.isServeReady)
        {
            Vector2 myPos = new Vector2(transform.position.x, transform.position.z);
            Vector2 ballPos = new Vector2(ball.transform.position.x, ball.transform.position.z);
            float dist = Vector2.Distance(myPos, ballPos);
            
            if (dist < 1.8f) 
            {
                Shot currentShot = shotManager.topSpin; 
                if (Input.GetKey(KeyCode.E)) currentShot = shotManager.flat; 

                ball.HitBall(ball.transform.position, aimTarget.position, currentShot.upForce, currentShot.hitForce);
                gm.RegisterHit("Player");
                
                if (AudioManager.instance != null) AudioManager.instance.PlayRacketHit();
            }
        }
    }
}