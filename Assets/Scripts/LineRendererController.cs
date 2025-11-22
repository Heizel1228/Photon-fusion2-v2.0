using UnityEngine;
using Fusion;
using System.Collections.Generic;
using System.Collections;

public class LineRendererController : NetworkBehaviour
{
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private Transform playerHand;
    private bool isLineVisible = false;

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void TriggerLineRendererRPC(Vector3 playerHandPosition, Vector3 shotPointPosition)
    {
        //Debug.Log($"RPC Triggered: {playerHandPosition}, {shotPointPosition}");

        // Enable the LineRenderer
        isLineVisible = true;

        // Set the positions
        lineRenderer.enabled = true;
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, playerHandPosition);
        lineRenderer.SetPosition(1, shotPointPosition);

        // Start the coroutine to hide the line
        StartCoroutine(HideLineAfterDelay());
    }

    private void Update()
    {
        // Dynamically update the LineRenderer's first position if it's visible
        if (isLineVisible && playerHand != null)
        {
            lineRenderer.SetPosition(0, playerHand.position);
        }
    }

    private IEnumerator HideLineAfterDelay()
    {
        yield return new WaitForSeconds(1f); // Wait for 1 second

        // Disable the LineRenderer
        lineRenderer.enabled = false;
        isLineVisible = false;

        // Clear the positions of the LineRenderer (optional)
        lineRenderer.positionCount = 0;
    }
}
