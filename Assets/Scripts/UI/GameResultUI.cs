using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum GameResult
{
    Victory,
    Defeat
}

public class GameResultUI : MonoBehaviour
{
    private const string VictoryMessage = "VICTORY";
    private const string DefeatMessage = "GAME OVER";

    public static GameResultUI Instance { get; private set; }

    [SerializeField] private TMP_Text resultText;
    [SerializeField] private Button playAgainButton;

    private bool resultShown;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (playAgainButton != null)
        {
            playAgainButton.onClick.AddListener(RestartGame);
        }

        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (playAgainButton != null)
        {
            playAgainButton.onClick.RemoveListener(RestartGame);
        }

        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void Show(GameResult result)
    {
        if (resultShown)
        {
            return;
        }

        resultShown = true;

        if (resultText != null)
        {
            resultText.text = result == GameResult.Victory
                ? VictoryMessage
                : DefeatMessage;
        }

        GameInput.Instance?.DisableGameplayInput();
        SoundManager.Play(result == GameResult.Victory
            ? SoundId.Victory
            : SoundId.Lose);
        gameObject.SetActive(true);
    }

    public void RestartGame()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(activeScene.buildIndex);
    }
}
