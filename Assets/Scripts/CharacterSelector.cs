using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelector : MonoBehaviour
{
    [Header("References")]
    public SpriteRenderer playerSpriteRenderer; // Reference to the player's SpriteRenderer
    public Image characterUIImage;              // Reference to the UI Image (e.g., HUD portrait)

    // This function sets the player's sprite
    public void SetCharacter(Sprite newCharacterSprite)
    {
        if (playerSpriteRenderer != null)
        {
            playerSpriteRenderer.sprite = newCharacterSprite;
        }

        if (characterUIImage != null)
        {
            characterUIImage.sprite = newCharacterSprite;
        }
    }
}

