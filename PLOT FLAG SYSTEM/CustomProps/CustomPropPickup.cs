using UnityEngine;

namespace CustomProps
{
    /// <summary>
    /// Component attached to spawned custom props to handle pickup behavior
    /// </summary>
    public class CustomPropPickup : MonoBehaviour
    {
        public PropData PropData { get; private set; }

        private bool _isCollected = false;
        private float _bobSpeed = 2f;
        private float _bobHeight = 0.1f;
        private float _rotateSpeed = 45f;
        private Vector3 _startPosition;

        public void Initialize(PropData data)
        {
            PropData = data;
            _startPosition = transform.position;
        }

        private void Update()
        {
            if (_isCollected) return;

            // Bob up and down
            float newY = _startPosition.y + Mathf.Sin(Time.time * _bobSpeed) * _bobHeight;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);

            // Rotate slowly
            transform.Rotate(Vector3.up, _rotateSpeed * Time.deltaTime);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_isCollected) return;

            // Check if this is the player
            if (!IsPlayer(other)) return;

            // Collect the prop
            Collect(other.gameObject);
        }

        private bool IsPlayer(Collider other)
        {
            // Try multiple ways to identify player
            if (other.CompareTag("Player")) return true;
            if (other.gameObject.layer == LayerMask.NameToLayer("Player")) return true;
            if (other.name.ToLower().Contains("player")) return true;

            // Check for common player components
            var rb = other.attachedRigidbody;
            if (rb != null && rb.name.ToLower().Contains("player")) return true;

            // Check parent hierarchy
            var parent = other.transform.parent;
            while (parent != null)
            {
                if (parent.name.ToLower().Contains("player")) return true;
                parent = parent.parent;
            }

            return false;
        }

        private void Collect(GameObject player)
        {
            _isCollected = true;

            CustomPropsPlugin.Log?.LogInfo($"Player collected: {PropData.PropId} ({PropData.DisplayName})");

            // Handle based on behavior type
            switch (PropData.Behavior)
            {
                case PropBehaviorType.Treasure:
                    HandleTreasure(player);
                    break;
                case PropBehaviorType.Key:
                    HandleKey(player);
                    break;
                case PropBehaviorType.Resource:
                    HandleResource(player);
                    break;
                case PropBehaviorType.Consumable:
                    HandleConsumable(player);
                    break;
                case PropBehaviorType.Quest:
                    HandleQuest(player);
                    break;
            }

            // Play pickup effects
            PlayPickupEffects();

            // Destroy the prop
            Destroy(gameObject);
        }

        private void HandleTreasure(GameObject player)
        {
            // TODO: Hook into Endstar's score system
            CustomPropsPlugin.Log?.LogInfo($"  +{PropData.Value} treasure points");

            // Try to find and call Endstar's score system
            // This will need to be implemented based on game analysis
        }

        private void HandleKey(GameObject player)
        {
            CustomPropsPlugin.Log?.LogInfo($"  Collected key: {PropData.DisplayName}");
            // TODO: Add to player's key inventory
        }

        private void HandleResource(GameObject player)
        {
            CustomPropsPlugin.Log?.LogInfo($"  +{PropData.Value} {PropData.DisplayName}");
            // TODO: Add to player's resource inventory
        }

        private void HandleConsumable(GameObject player)
        {
            CustomPropsPlugin.Log?.LogInfo($"  Used consumable: {PropData.DisplayName}");
            // TODO: Apply immediate effect (health, etc.)
        }

        private void HandleQuest(GameObject player)
        {
            CustomPropsPlugin.Log?.LogInfo($"  Quest item collected: {PropData.DisplayName}");
            // TODO: Update quest state
        }

        private void PlayPickupEffects()
        {
            // Play pickup sound
            if (PropData.PickupSound != null)
            {
                AudioSource.PlayClipAtPoint(PropData.PickupSound, transform.position);
            }

            // Spawn pickup VFX
            if (PropData.PickupVFX != null)
            {
                var vfx = Instantiate(PropData.PickupVFX, transform.position, Quaternion.identity);
                Destroy(vfx, 3f); // Auto-destroy after 3 seconds
            }

            // Simple scale-down animation before destroy
            StartCoroutine(PickupAnimation());
        }

        private System.Collections.IEnumerator PickupAnimation()
        {
            float duration = 0.2f;
            float elapsed = 0f;
            Vector3 startScale = transform.localScale;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
                transform.position += Vector3.up * Time.deltaTime * 2f;
                yield return null;
            }
        }
    }
}
