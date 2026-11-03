using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    private GameObject menuCanvas;
    private Text difficultyText; 
    private Text musicText; 

    void Start()
    {
        CreateMenuUI();
        Time.timeScale = 0f; 
    }

    void CreateMenuUI()
    {
        menuCanvas = new GameObject("MenuCanvas");
        Canvas canvas = menuCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        menuCanvas.AddComponent<CanvasScaler>();
        menuCanvas.AddComponent<GraphicRaycaster>();

        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(menuCanvas.transform, false);
        Image bgImage = bg.AddComponent<Image>();
        bgImage.color = new Color(0, 0, 0, 0.85f);
        RectTransform bgRect = bg.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;

        CreateText("TENNIS GAME", new Vector2(0, 250), 70, Color.white);
        CreateText("СЛОЖНОСТЬ БОТА:", new Vector2(0, 120), 30, Color.yellow);
        
        GameObject diffTxtGO = new GameObject("DifficultyText");
        diffTxtGO.transform.SetParent(menuCanvas.transform, false);
        difficultyText = diffTxtGO.AddComponent<Text>();
        difficultyText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        difficultyText.fontSize = 35;
        difficultyText.color = Color.white;
        difficultyText.alignment = TextAnchor.MiddleCenter;
        RectTransform diffRect = diffTxtGO.GetComponent<RectTransform>();
        diffRect.anchoredPosition = new Vector2(0, 70);
        diffRect.sizeDelta = new Vector2(600, 50);
        
        UpdateDifficultyUI(); 

        CreateButton("СМЕНИТЬ СЛОЖНОСТЬ", new Vector2(0, 10), () => {
            GameSettings.botDifficulty++;
            if (GameSettings.botDifficulty > 2) GameSettings.botDifficulty = 0; 
            UpdateDifficultyUI();
        });

        CreateButton("ИГРАТЬ", new Vector2(0, -90), () => {
            menuCanvas.SetActive(false); 
            Time.timeScale = 1f; 
            
            GameManager gm = FindObjectOfType<GameManager>();
            if (gm != null) gm.StartGame();
        });

        // НОВАЯ КНОПКА МУЗЫКИ
        GameObject musicBtn = CreateButton("МУЗЫКА: ВКЛ", new Vector2(0, -180), () => {
            if (MusicManager.instance != null)
            {
                MusicManager.instance.ToggleMusic();
                musicText.text = MusicManager.instance.IsMusicMuted() ? "МУЗЫКА: ВЫКЛ" : "МУЗЫКА: ВКЛ";
            }
        });
        musicText = musicBtn.GetComponentInChildren<Text>();

        // Обновляем текст при старте
        if (MusicManager.instance != null)
            musicText.text = MusicManager.instance.IsMusicMuted() ? "МУЗЫКА: ВЫКЛ" : "МУЗЫКА: ВКЛ";

        CreateButton("ВЫХОД", new Vector2(0, -270), () => {
            Application.Quit(); 
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false; 
            #endif
        });
        
        if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }
    }

    void UpdateDifficultyUI()
    {
        if (GameSettings.botDifficulty == 0) difficultyText.text = "ЛЕГКАЯ";
        else if (GameSettings.botDifficulty == 1) difficultyText.text = "СРЕДНЯЯ";
        else difficultyText.text = "СЛОЖНАЯ";
    }

    void CreateText(string textContent, Vector2 pos, int fontSize, Color color)
    {
        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(menuCanvas.transform, false);
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
        btnGO.transform.SetParent(menuCanvas.transform, false);
        
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
        rect.sizeDelta = new Vector2(350, 70);

        return btnGO; 
    }
}