
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MarkController : MonoBehaviour
{
    public SpriteRenderer[] MarkRenderers;
    public Sprite[] MarkSprites;
    public Color InactiveColor;

    private void Awake()
    {
       MarkSprites =  MarkSprites.Reverse().ToArray();
       MarkRenderers = transform.GetComponentsInChildren<SpriteRenderer>();
       MarkRenderers = MarkRenderers.Reverse().ToArray();
        foreach (var renderer in MarkRenderers)
        {
            renderer.sprite = MarkSprites[0];
            renderer.color = InactiveColor;
        }
    }

    [Button]
    public void SetPowerLevel(int powerLevel)
    {
        for (int i = 0; i < MarkRenderers.Length; i++)
        {
            if(i < powerLevel)
            {
                SetMarkActive(i);
            } else
            {
                SetMarkInactive(i);
            }
        }
    }

    private void SetMarkActive(int markIndex)
    {
        var markRenderer = MarkRenderers[markIndex];
        var markSprite = markIndex < 4 ? MarkSprites[1] : markIndex < 9 ? MarkSprites[2] : markIndex < 15 ? MarkSprites[3] : markIndex < 21 ? MarkSprites[4] : MarkSprites[5];
        markRenderer.sprite = markSprite;
        markRenderer.color = Color.white;
    }

    private void SetMarkInactive(int markIndex)
    {
        var markRenderer = MarkRenderers[markIndex];
        markRenderer.sprite = MarkSprites[0];
        markRenderer.color = InactiveColor;
    }
}
