using UnityEngine;

public class Teleporter : MonoBehaviour
{
    public Transform teleportTarget;
    public Transform cameraTarget;
    [SerializeField] BossEnemy roomBoss;
    private void OnTriggerEnter2D(Collider2D collider)
    {
        if(collider.TryGetComponent(out Player player))
        {
            player.TeleportPlayer(teleportTarget.position, cameraTarget.position);

            if (roomBoss != null) roomBoss.Awaken();
        }
    }
}
