using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private Ball ball; 
    public Transform player;
    public Transform bot;

    public string currentServer = "Player"; 
    public string lastHitter = ""; 
    public bool isServeReady = false;
    
    private int playerPoints = 0;
    private int botPoints = 0;
    private int playerGames = 0;
    private int botGames = 0;
    private string[] scoreNames = { "0", "15", "30", "40" };
    
    private Text scoreTextUI;
    private bool isServe = true;
    private bool secondServe = false;
    private bool gameStarted = false; 
    private bool firstBounceHappened = false; 
    private bool matchFinished = false; 

    private bool serveFromRight = true; 

    private GameObject endGameCanvas;

    void Start()
    {
        ball = FindObjectOfType<Ball>(); 
        CreateUI(); 
        currentServer = (Random.Range(0, 2) == 0) ? "Player" : "Bot";
    }

    public void StartGame()
    {
        if (gameStarted) return; 
        gameStarted = true;
        StartNewPoint();
    }

    public void RegisterHit(string hitter)
    {
        if (matchFinished) return;
        lastHitter = hitter;
        isServeReady = false; 
        firstBounceHappened = false; 
        
        // ВАЖНО: Мы больше не ставим isServe = false здесь! 
        // Подача завершится только когда мяч упадет в правильный квадрат.
    }

    public void OnBallLanded(Vector3 pos, bool isOut)
    {
        if (!gameStarted || matchFinished) return;

        float x = pos.x;
        float z = pos.z;

        // 1. СНАЧАЛА ПРОВЕРЯЕМ ПОДАЧУ
        if (isServe)
        {
            bool validServe = false;

            // Если мяч вышел за пределы корта, это точно не засчитанная подача
            if (isOut) validServe = false;
            else
            {
                if (currentServer == "Player")
                {
                    if (serveFromRight) // Подача справа -> левый квадрат бота
                    {
                        if (z > 0 && z < 6.4f && x < 0 && Mathf.Abs(x) < 4.115f) validServe = true;
                    }
                    else // Подача слева -> правый квадрат бота
                    {
                        if (z > 0 && z < 6.4f && x > 0 && Mathf.Abs(x) < 4.115f) validServe = true;
                    }
                }
                else
                {
                    if (serveFromRight) // Бот подает справа -> правый квадрат игрока
                    {
                        if (z < 0 && z > -6.4f && x > 0 && Mathf.Abs(x) < 4.115f) validServe = true;
                    }
                    else // Бот подает слева -> левый квадрат игрока
                    {
                        if (z < 0 && z > -6.4f && x < 0 && Mathf.Abs(x) < 4.115f) validServe = true;
                    }
                }
            }

            if (validServe)
            {
                isServe = false; // Подача прошла! Теперь это обычный розыгрыш.
                firstBounceHappened = true; 
                return;
            }
            else
            {
                HandleFault(currentServer); // Ошибка подачи (не туда или в сетку)
                return;
            }
        }

        // 2. ЕСЛИ ЭТО УЖЕ ОБЫЧНЫЙ РОЗЫГРЫШ (не подача)
        if (isOut)
        {
            if (!firstBounceHappened)
            {
                if (lastHitter == "Player") AwardPoint("Bot", "Аут!");
                else AwardPoint("Player", "Аут!");
            }
            return;
        }

        if (!firstBounceHappened)
        {
            firstBounceHappened = true;
            return;
        }
        else
        {
            if (z < 0) AwardPoint("Bot", "Ты не отбил!");
            else if (z > 0) AwardPoint("Player", "Бот не отбил!");
        }
    }

    void HandleFault(string faultingPlayer)
    {
        // ОСТАНАВЛИВАЕМ МЯЧ, чтобы он не отскочил еще раз и не вызвал двойную ошибку
        ball.isInPlay = false;
        ball.transform.position = new Vector3(0, 10, 0); // Убираем мяч с экрана

        if (!secondServe)
        {
            secondServe = true;
            Invoke("ResetForSecondServe", 1.5f); 
        }
        else
        {
            AwardPoint(faultingPlayer == "Player" ? "Bot" : "Player", "Двойная ошибка!");
        }
    }

    void ResetForSecondServe()
    {
        isServe = true;
        isServeReady = true;
        firstBounceHappened = false;
        
        if (currentServer == "Player") 
        {
            float xPos = serveFromRight ? 2f : -2f;
            ball.transform.position = new Vector3(xPos, 3, -11);
            ball.isInPlay = false;
            player.position = new Vector3(xPos, 1f, -11);
        }
        else 
        { 
            float xPos = serveFromRight ? -2f : 2f;
            ball.transform.position = new Vector3(xPos, 3, 11); 
            ball.isInPlay = false;
            bot.position = new Vector3(xPos, 1f, 11);
            Invoke("BotAutoServe", 1f); 
        }
        UpdateScoreUI();
    }

    void AwardPoint(string winner, string reason)
    {
        if (matchFinished) return;

        Debug.Log($"ОЧКО: {winner} ({reason})");
        if (winner == "Player") playerPoints++;
        else botPoints++;

        if (playerPoints >= 4 && playerPoints - botPoints >= 2) 
        { 
            playerGames++; 
            currentServer = (currentServer == "Player") ? "Bot" : "Player"; 
            playerPoints = 0; botPoints = 0;
        }
        else if (botPoints >= 4 && botPoints - playerPoints >= 2) 
        { 
            botGames++; 
            currentServer = (currentServer == "Player") ? "Bot" : "Player"; 
            playerPoints = 0; botPoints = 0;
        }
        else if (playerPoints >= 3 && botPoints >= 3)
        {
            if (playerPoints == botPoints) { playerPoints = 3; botPoints = 3; } 
            else if (playerPoints > botPoints) { playerPoints = 4; } 
            else { botPoints = 4; } 
        }

        if (playerGames >= 3 && playerGames - botGames >= 2) 
        {
            matchFinished = true;
            ShowEndGameModal("ВЫ ПОБЕДИЛИ!", playerGames, botGames);
        }
        else if (botGames >= 3 && botGames - playerGames >= 2) 
        {
            matchFinished = true;
            ShowEndGameModal("ВЫ ПРОИГРАЛИ", botGames, playerGames);
        }
        else 
        {
            UpdateScoreUI();
            StartNewPoint();
        }
    }

    public void StartNewPoint()
    {
        if (matchFinished) return;

        CancelInvoke("BotAutoServe");
        CancelInvoke("ResetForSecondServe");

        lastHitter = ""; 
        isServe = true;
        isServeReady = true;
        secondServe = false;
        firstBounceHappened = false;
        
        serveFromRight = !serveFromRight;

        if (currentServer == "Player") 
        {
            float xPos = serveFromRight ? 2f : -2f;
            ball.transform.position = new Vector3(xPos, 3, -11);
            ball.isInPlay = false;
            player.position = new Vector3(xPos, 1f, -11); 
        }
        else 
        {
            float xPos = serveFromRight ? -2f : 2f;
            ball.transform.position = new Vector3(xPos, 3, 11); 
            ball.isInPlay = false;
            bot.position = new Vector3(xPos, 1f, 11); 
            Invoke("BotAutoServe", 1.0f); 
        }
        UpdateScoreUI();
    }

    void BotAutoServe()
    {
        if (!gameStarted || matchFinished) return;
        isServeReady = false;
        
        float targetX;
        if (serveFromRight) targetX = Random.Range(1f, 3f); 
        else targetX = Random.Range(-3f, -1f); 

        Vector3 targetPos = new Vector3(targetX, 0, -5f); 
        
        ball.HitBall(ball.transform.position, targetPos, 5f, 15f); 
        RegisterHit("Bot");
        
        if (AudioManager.instance != null) AudioManager.instance.PlayRacketHit();
    }

    void UpdateScoreUI()
    {
        if (scoreTextUI == null) return;

        string p = playerPoints > 3 ? "Ad" : scoreNames[playerPoints];
        string b = botPoints > 3 ? "Ad" : scoreNames[botPoints];
        if (playerPoints == 3 && botPoints >= 3) p = "40";
        if (botPoints == 3 && playerPoints >= 3) b = "40";

        string serveHint = "";
        if (isServe && !matchFinished)
        {
            if (currentServer == "Player")
                serveHint = serveFromRight ? "\nПОДАЧА: В ЛЕВЫЙ КВАДРАТ" : "\nПОДАЧА: В ПРАВЫЙ КВАДРАТ";
            else
                serveHint = "\nБОТ ГОТОВИТСЯ К ПОДАЧЕ";
        }

        scoreTextUI.text = $"Игрок: {p} | Бот: {b}\nГеймы: {playerGames} - {botGames}\nПодает: {currentServer}" + (secondServe ? "\nВТОРАЯ ПОДАЧА" : "") + serveHint;
    }

    void ShowEndGameModal(string title, int winnerGames, int loserGames)
    {
        Time.timeScale = 0f;

        endGameCanvas = new GameObject("EndGameCanvas");
        Canvas canvas = endGameCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10; 
        endGameCanvas.AddComponent<CanvasScaler>();
        endGameCanvas.AddComponent<GraphicRaycaster>();

        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(endGameCanvas.transform, false);
        Image bgImage = bg.AddComponent<Image>();
        bgImage.color = new Color(0, 0, 0, 0.85f);
        RectTransform bgRect = bg.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;

        GameObject titleGO = new GameObject("Title");
        titleGO.transform.SetParent(endGameCanvas.transform, false);
        Text titleText = titleGO.AddComponent<Text>();
        titleText.text = title;
        titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        titleText.fontSize = 80;
        titleText.color = Color.yellow;
        titleText.alignment = TextAnchor.MiddleCenter;
        RectTransform titleRect = titleGO.GetComponent<RectTransform>();
        titleRect.anchoredPosition = new Vector2(0, 150);
        titleRect.sizeDelta = new Vector2(800, 150);

        GameObject scoreGO = new GameObject("FinalScore");
        scoreGO.transform.SetParent(endGameCanvas.transform, false);
        Text scoreText = scoreGO.AddComponent<Text>();
        scoreText.text = $"Счет: {winnerGames} - {loserGames}";
        scoreText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        scoreText.fontSize = 50;
        scoreText.color = Color.white;
        scoreText.alignment = TextAnchor.MiddleCenter;
        RectTransform scoreRect = scoreGO.GetComponent<RectTransform>();
        scoreRect.anchoredPosition = new Vector2(0, 40);
        scoreRect.sizeDelta = new Vector2(600, 100);

        CreateButton("ИГРАТЬ ЕЩЕ", new Vector2(0, -80), () => {
            Time.timeScale = 1f; 
            Destroy(endGameCanvas); 
            ResetMatch(); 
        });

        CreateButton("В ГЛАВНОЕ МЕНЮ", new Vector2(0, -180), () => {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        });

        if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }
    }

    void CreateButton(string btnText, Vector2 pos, UnityEngine.Events.UnityAction onClickAction)
    {
        GameObject btnGO = new GameObject("Button_" + btnText);
        btnGO.transform.SetParent(endGameCanvas.transform, false);
        
        Image img = btnGO.AddComponent<Image>();
        img.color = new Color(0.1f, 0.5f, 0.1f); 

        Button btn = btnGO.AddComponent<Button>();
        btn.onClick.AddListener(onClickAction);

        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(btnGO.transform, false);
        Text text = textGO.AddComponent<Text>();
        text.text = btnText;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 36;
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleCenter;
        
        RectTransform textRect = textGO.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;

        RectTransform rect = btnGO.GetComponent<RectTransform>();
        rect.anchoredPosition = pos;
        rect.sizeDelta = new Vector2(400, 70);
    }

    void ResetMatch()
    {
        playerPoints = 0; botPoints = 0;
        playerGames = 0; botGames = 0;
        matchFinished = false;
        gameStarted = true;
        
        currentServer = (Random.Range(0, 2) == 0) ? "Player" : "Bot";
        StartNewPoint();
    }

    void CreateUI() 
    { 
        GameObject canvasGO = new GameObject("MainCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();
        
        GameObject textGO = new GameObject("ScoreText");
        textGO.transform.SetParent(canvasGO.transform, false);
        scoreTextUI = textGO.AddComponent<Text>();
        scoreTextUI.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        scoreTextUI.fontSize = 36;
        scoreTextUI.color = Color.white;
        scoreTextUI.alignment = TextAnchor.UpperLeft;
        
        RectTransform rect = textGO.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 1); rect.anchorMax = new Vector2(0, 1);
        rect.pivot = new Vector2(0, 1);
        rect.anchoredPosition = new Vector2(20, -20); 
        rect.sizeDelta = new Vector2(600, 200);
    }
}