using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    private GameObject pauseCanvas;
    private bool isPaused = false;
    private Text difficultyText; 
    private Text musicText; 

    void Start()
    {
        CreatePauseUI();
        pauseCanvas.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) ResumeGame();
            else PauseGame();
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        pauseCanvas.SetActive(true);
        Time.timeScale = 0f;
        UpdateUI();
    }

    public void ResumeGame()
    {
        isPaused = false;
        pauseCanvas.SetActive(false);
        Time.timeScale = 1f;
    }

    void CreatePauseUI()
    {
        pauseCanvas = new GameObject("PauseCanvas");
        Canvas canvas = pauseCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        pauseCanvas.AddComponent<CanvasScaler>();
        pauseCanvas.AddComponent<GraphicRaycaster>();

        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(pauseCanvas.transform, false);
        Image bgImage = bg.AddComponent<Image>();
        bgImage.color = new Color(0, 0, 0, 0.85f);
        RectTransform bgRect = bg.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;

        CreateText("ПАУЗА", new Vector2(0, 200), 70, Color.white);
        CreateText("СЛОЖНОСТЬ БОТА:", new Vector2(0, 90), 25, Color.yellow);
        
        GameObject diffTxtGO = new GameObject("DifficultyText");
        diffTxtGO.transform.SetParent(pauseCanvas.transform, false);
        difficultyText = diffTxtGO.AddComponent<Text>();
        difficultyText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        difficultyText.fontSize = 30;
        difficultyText.color = Color.white;
        difficultyText.alignment = TextAnchor.MiddleCenter;
        RectTransform diffRect = diffTxtGO.GetComponent<RectTransform>();
        diffRect.anchoredPosition = new Vector2(0, 50);
        diffRect.sizeDelta = new Vector2(600, 50);

        CreateButton("СМЕНИТЬ СЛОЖНОСТЬ", new Vector2(0, 0), () => {
            GameSettings.botDifficulty++;
            if (GameSettings.botDifficulty > 2) GameSettings.botDifficulty = 0;
            UpdateUI();
        });

        // НОВАЯ КНОПКА МУЗЫКИ
        GameObject musicBtn = CreateButton("МУЗЫКА: ВКЛ", new Vector2(0, -90), () => {
            if (MusicManager.instance != null)
            {
                MusicManager.instance.ToggleMusic();
                musicText.text = MusicManager.instance.IsMusicMuted() ? "МУЗЫКА: ВЫКЛ" : "МУЗЫКА: ВКЛ";
            }
        });
        musicText = musicBtn.GetComponentInChildren<Text>();

        CreateButton("ПРОДОЛЖИТЬ", new Vector2(0, -180), ResumeGame);
        
        CreateButton("В ГЛАВНОЕ МЕНЮ", new Vector2(0, -270), () => {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        });
    }

    void UpdateUI()
    {
        if (GameSettings.botDifficulty == 0) difficultyText.text = "ЛЕГКАЯ";
        else if (GameSettings.botDifficulty == 1) difficultyText.text = "СРЕДНЯЯ";
        else difficultyText.text = "СЛОЖНАЯ";

        if (MusicManager.instance != null)
            musicText.text = MusicManager.instance.IsMusicMuted() ? "МУЗЫКА: ВЫКЛ" : "МУЗЫКА: ВКЛ";
    }

    void CreateText(string textContent, Vector2 pos, int fontSize, Color color)
    {
        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(pauseCanvas.transform, false);
        Text text = textGO.AddComponent<Text>();
        text.text = textContent;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.color = color;
        text.alignment = TextAnchor.MiddleCenter;
        RectTransform rect = textGO.GetComponent<RectTransform>();
        rect.anchoredPosition = pos;
        rect.sizeDelta = new Vector2(600, 100);
    }

    GameObject CreateButton(string btnText, Vector2 pos, UnityEngine.Events.UnityAction onClickAction)
    {
        GameObject btnGO = new GameObject("Button");
        btnGO.transform.SetParent(pauseCanvas.transform, false);
        
        Image img = btnGO.AddComponent<Image>();
        img.color = new Color(0.1f, 0.4f, 0.6f); 

        Button btn = btnGO.AddComponent<Button>();
        btn.onClick.AddListener(onClickAction);

        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(btnGO.transform, false);
        Text text = textGO.AddComponent<Text>();
        text.text = btnText;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 30;
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleCenter;
        
        RectTransform textRect = textGO.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;

        RectTransform rect = btnGO.GetComponent<RectTransform>();
        rect.anchoredPosition = pos;
        rect.sizeDelta = new Vector2(400, 60);
        return btnGO;
    }
}