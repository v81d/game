using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerDeath : MonoBehaviour
{
    [Tooltip("Objects on this layer will kill the player on contact.")]
    [SerializeField] private LayerMask deathLayer;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (IsInDeathLayer(other.gameObject))
        {
            Die();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (IsInDeathLayer(collision.gameObject))
        {
            Die();
        }
    }

    private bool IsInDeathLayer(GameObject obj)
    {
        // Check if the object's layer is included in the deathLayer mask
        return (deathLayer.value & (1 << obj.layer)) != 0;
    }

    private void Die()
    {
        // Fade to black, then reload the current scene so the player respawns
        string currentSceneName = SceneManager.GetActiveScene().name;

        if (ScreenFader.Instance != null)
        {
            ScreenFader.Instance.FadeToScene(currentSceneName);
        }
        else
        {
            // Fallback if no ScreenFader exists in the scene
            SceneManager.LoadScene(currentSceneName);
        }
    }
}
