using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // Required for UI components

public class SceneChangeButton : MonoBehaviour
{
    public string ScenetoLoad;
    
    void Start()
    {
        // Get the Button component and add a listener
        GetComponent<Button>().onClick.AddListener(ChangeScene);
    }

    void ChangeScene()
    {
        if (!string.IsNullOrEmpty(ScenetoLoad))
        {
            SceneManager.LoadScene(ScenetoLoad);
        }
    }
}
