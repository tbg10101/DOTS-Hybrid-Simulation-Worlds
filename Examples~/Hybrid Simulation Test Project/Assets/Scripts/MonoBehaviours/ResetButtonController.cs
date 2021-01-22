using UnityEngine;
using UnityEngine.SceneManagement;

public class ResetButtonController : MonoBehaviour
{
    public void OnResetClicked() {
        SceneManager.LoadScene(0);
    }
}
