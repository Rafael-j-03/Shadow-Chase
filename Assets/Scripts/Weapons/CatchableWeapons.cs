using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatchableWeapons : MonoBehaviour
{
    [SerializeField] private AudioClip pickUpSound;
    private PlayerManager playerManager;

    private AudioSource audioSource { get => FindObjectOfType<SoundManager>().AudioSource; }

    // Check the weapon by the tag of the weapon
    private WeaponsClass CatchableWeapon
    {
        get
        {
            switch (gameObject.tag)
            {
                case "Pistol":
                    return WeaponsClass.Pistol;
                case "AK":
                    return WeaponsClass.AK;
                default:
                    return WeaponsClass.Pistol;
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        playerManager = FindObjectOfType<PlayerManager>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && playerManager.Ammo < playerManager.CurrentWeapon.MaxAmmo)
        {
            // Check if players current weapon tag is the same as the catchable weapon tag
            if (playerManager.CurrentWeapon.Tag == CatchableWeapon.Tag)
            {
                // Add ammo to the player
                playerManager.Ammo += (int)(CatchableWeapon.MaxAmmo/Random.Range(3, 4));
            }
            else
            {
                // Change the player's current weapon
                playerManager.ChangeWeapon(CatchableWeapon);
                // Set the player's ammo to the weapon's max ammo
                playerManager.Ammo = (int)(CatchableWeapon.MaxAmmo/2);
            }

            // Play pick up sound
            audioSource.PlayOneShot(pickUpSound, 1f);

            // Destroy the weapon
            Destroy(gameObject);
        }
    }
}