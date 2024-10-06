using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class FlashMeshRenderer
{
    private MeshRenderer meshRenderer;
    private SkinnedMeshRenderer skinnedMeshRenderer;

    private List<Color> defaultColors = new List<Color>();

    public FlashMeshRenderer(MeshRenderer meshRenderer, SkinnedMeshRenderer skinnedMeshRenderer)
    {
        this.meshRenderer = meshRenderer;
        this.skinnedMeshRenderer = skinnedMeshRenderer;

        if (meshRenderer != null)
        {
            foreach (Material material in meshRenderer.materials)
                defaultColors.Add(material.color);
        }

        if (skinnedMeshRenderer != null)
        {
            foreach (Material material in skinnedMeshRenderer.materials)
                defaultColors.Add(material.color);
        }

    }

    public void ChangeColor(Color flashColor)
    {
        if (meshRenderer != null)
        {
            for (int i = 0; i < meshRenderer.materials.Length; i++)
                meshRenderer.materials[i].color = flashColor;
        }

        if (skinnedMeshRenderer != null)
        {
            for (int i = 0; i < skinnedMeshRenderer.materials.Length; i++)
                skinnedMeshRenderer.materials[i].color = flashColor;
        }
    }

    public void RestoreColor()
    {
        if (meshRenderer != null)
        {
            for (int i = 0; i < meshRenderer.materials.Length; i++)
                meshRenderer.materials[i].color = defaultColors[i];
        }

        if (skinnedMeshRenderer != null)
        {
            for (int i = 0; i < skinnedMeshRenderer.materials.Length; i++)
                skinnedMeshRenderer.materials[i].color = defaultColors[i];
        }
    }
}