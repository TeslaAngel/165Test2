using UnityEngine;

public class TeleportBallProjectile : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform playerRoot;

    [Header("Ground")]
    [SerializeField] private LayerMask groundLayers;

    [Header("Teleport")]
    [SerializeField] private float playerYOffset = 0f;
    [SerializeField] private bool preservePlayerY = true;

    private bool hasTeleported;

    public void SetPlayerRoot(Transform root)
    {
        playerRoot = root;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (hasTeleported)
            return;

        bool hitGround =
            ((1 << collision.gameObject.layer) & groundLayers) != 0;

        if (!hitGround)
            return;

        hasTeleported = true;

        Vector3 contactPoint = collision.GetContact(0).point;

        float targetY = preservePlayerY
            ? playerRoot.position.y
            : contactPoint.y + playerYOffset;

        playerRoot.position = new Vector3(
            contactPoint.x,
            targetY,
            contactPoint.z
        );

        Destroy(gameObject);
    }
}