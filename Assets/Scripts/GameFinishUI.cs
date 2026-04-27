using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameFinishUI : MonoBehaviour
{
    [SerializeField] private Button nextSceneButton;
    [SerializeField] private string nextScene;

    private void Awake()
    {
        gameObject.SetActive(false);
        nextSceneButton.onClick.AddListener(OnNextSceneButtonClicked);
    }

    private void OnNextSceneButtonClicked()
    {
        nextSceneButton.interactable = false; 
        StartCoroutine(LoadNextScene());
    }

    private IEnumerator LoadNextScene()
    {
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene(nextScene);
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }
}