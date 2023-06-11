using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] private int          maxHealth = 100;
    [SerializeField] private HealthBar    healthBar;
    [SerializeField] private GameObject   player;
    [SerializeField] private AudioClip    hurtSound;
    [SerializeField] private AudioClip    deathSound;

    private int              currentHealth;
    private WeaponsClass     currentWeapon;
    private PlayerMovement   playerMovement;
    private PlayerShooting   playerShooting;
    private SpriteRenderer[] spriteRenderers;
    private GameManager      gameManager;

    // Get and set ammo
    public int          Ammo { get; set; }
    // Get player's current health
    public int          CurrentHealth { get => currentHealth; }
    // Get current weapon
    public WeaponsClass CurrentWeapon { get => currentWeapon; }
    // Get sound manager's audio source
    private AudioSource audioSource { get => FindObjectOfType<SoundManager>().AudioSource; }

    // Start is called before the first frame update
    void Start()
    {
        // Health bar setup
        currentHealth = maxHealth;
        healthBar.SetMaxHealth(maxHealth);

        // Weapon setup
        currentWeapon = WeaponsClass.Pistol;
        Ammo = currentWeapon.MaxAmmo;

        // Get all sprite renderers
        spriteRenderers = player.GetComponentsInChildren<SpriteRenderer>();

        // Get game manager
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
    }

    private void Update()
    {
        if (currentHealth <= 0)
        {
            // Get player movement script
            playerMovement = player.GetComponent<PlayerMovement>();
            // Get player shooting script
            playerShooting = player.GetComponent<PlayerShooting>();

            // Change animation to death
            playerMovement.ChangeAnimationState("Player_Death");
            // Turn of player movement
            playerMovement.enabled = false;
            // Turn off player shooting
            playerShooting.enabled = false;
            // Take of rigid body material
            player.GetComponent<Rigidbody2D>().sharedMaterial = null;
            // Turn off player's sprite renderer
            foreach (SpriteRenderer spriteRenderer in spriteRenderers)
            {
                // All sprite renderers except the player's body
                if (spriteRenderer.gameObject.name != "Player")
                {
                    spriteRenderer.enabled = false;
                }
            }

            // Restart level after 2 seconds
            StartCoroutine(RestartLevelAfterDelay(2f));
        }

        // Can't have more ammo than the weapon's max ammo
        if (Ammo > currentWeapon.MaxAmmo)
        {
            Ammo = currentWeapon.MaxAmmo;
        }
    }

    // Take damage
    public void TakeDamage(int damage)
    {
        if (damage < currentHealth)
            audioSource.PlayOneShot(hurtSound, 1f);
        else
            audioSource.PlayOneShot(deathSound, 1f);
            
        currentHealth -= damage;

        healthBar.SetHealth(currentHealth);

        StartCoroutine(Blink());
    }

    // Coroutine to make player blink red for a second
    private IEnumerator Blink()
    {
        // Set all SpriteRenderer components to red
        foreach (SpriteRenderer spriteRenderer in spriteRenderers)
        {
            spriteRenderer.color = Color.red;
        }

        // Wait for a second
        yield return new WaitForSeconds(0.3f);

        // Set all SpriteRenderer components back to their original color
        foreach (SpriteRenderer spriteRenderer in spriteRenderers)
        {
            spriteRenderer.color = Color.white;
        }
    }

    // Change weapon
    public void ChangeWeapon(WeaponsClass newWeapon)
    {
        currentWeapon = newWeapon;
        Ammo = currentWeapon.MaxAmmo;
    }

    // Restart the level after a delay
    private IEnumerator RestartLevelAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        // Restart the level from the last checkpoint
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}