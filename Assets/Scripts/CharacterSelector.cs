using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelector : MonoBehaviour
{
    public SpriteRenderer playerSpriteRenderer; // Reference to the player's SpriteRenderer

    // This function sets the player's sprite
    public void SetCharacter(Sprite newCharacterSprite)
    {
        if (playerSpriteRenderer != null)
        {
            playerSpriteRenderer.sprite = newCharacterSprite;
        }
    }
}
