using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Highlightable : MonoBehaviour
{
    // Drag the Material Asset you want to target here
    [SerializeField] private Material targetMaterial;
    [SerializeField] private MeshRenderer meshRenderer;

    [SerializeField] private bool playerObjbool;

    private MaterialPropertyBlock _propBlock;

    private void OnValidate()
    {
        if (meshRenderer == null)
            meshRenderer = GetComponent<MeshRenderer>();
    }

    private void Awake()
    {
        _propBlock = new MaterialPropertyBlock();
    }

    public void Highlight(bool doHighlight)
    {
        if (meshRenderer == null || targetMaterial == null) return;

        Material[] currentMaterials = meshRenderer.sharedMaterials;

        for (int i = 0; i < currentMaterials.Length; i++)
        {
            if (currentMaterials[i] == targetMaterial)
            {
                ApplyHighlight(i, doHighlight);
            }
        }
    }

    private void ApplyHighlight(int index, bool active)
    {
        meshRenderer.GetPropertyBlock(_propBlock, index);

        if (active)
        {
            if(playerObjbool)
                _propBlock.SetFloat("_outline_Scale", 1.05f);
            else
                _propBlock.SetFloat("_outline_Scale", 1.03f);
        }
        else
        {
            if (playerObjbool)
                _propBlock.SetFloat("_outline_Scale", 0f);
            else
                _propBlock.SetFloat("_outline_Scale", 1f);
        }

        meshRenderer.SetPropertyBlock(_propBlock, index);
    }
}