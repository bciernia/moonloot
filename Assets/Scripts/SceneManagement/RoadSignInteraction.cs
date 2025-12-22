using UnityEngine;

namespace SceneManagement
{
    public class RoadSignInteraction : MonoBehaviour, IInteractable
    {
        [SerializeField] private AreaExit areaExit;
        [SerializeField] private string _destinationAreaName;
        
        public void Interact()
        {
            FindFirstObjectByType<InteractionManager>().UnregisterInteractable(this);
            areaExit.ExitArea();
        }

        public string GetInteractionText() => $"Go to: {_destinationAreaName}";

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                FindFirstObjectByType<InteractionManager>().RegisterInteractable(this);
            }
        }
    
        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                FindFirstObjectByType<InteractionManager>().UnregisterInteractable(this);
            }
        }
    }
}