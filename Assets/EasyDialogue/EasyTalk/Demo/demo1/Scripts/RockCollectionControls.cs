using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace EasyTalk.Demo
{
    /// <summary>
    /// Handles controls for collecting rocks in the EasyTalk Demo.
    /// </summary>
    public class RockCollectionControls : MonoBehaviour
    {
        /// <summary>
        /// The camera used by the player character.
        /// </summary>
        [SerializeField]
        private Camera playerCamera;

#if ENABLE_INPUT_SYSTEM
        /// <summary>
        /// The input action asset to use for player controls.
        /// </summary>
        [SerializeField]
        private InputActionAsset inputActions;
#endif

        /// <summary>
        /// The number of rocks collected.
        /// </summary>
        private int rocksCollected = 0;

        private void Update()
        {
            //Do a raycast to see if the player is looking at a rock.
            Ray ray = playerCamera.ScreenPointToRay(new Vector3(playerCamera.pixelWidth / 2.0f, playerCamera.pixelHeight / 2.0f));
            RaycastHit rayHit;

            if (Physics.Raycast(ray, out rayHit, 4.0f))
            {
                if (rayHit.collider.gameObject.tag.Equals("rock"))
                {
#if ENABLE_INPUT_SYSTEM

                InputAction mineAction = inputActions.FindAction("Pickup");
                if (mineAction != null && mineAction.triggered)
                {
                    float value = mineAction.ReadValue<float>();
                    if (value > 0)
                    {
                        Pickup(rayHit.transform.gameObject);
                    }
                }
#else
                    if (Input.GetButtonDown("Submit"))
                    {
                        Pickup(rayHit.transform.gameObject);
                    }
#endif
                }
            }
        }

        /// <summary>
        /// Incrememnts the counter which keeps tracks of the number of rocks the player has picked up, deletes the picked up rock, and plays an audio clip.
        /// </summary>
        /// <param name="rock">The  rock object to pick up.</param>
        private void Pickup(GameObject rock)
        {
            GameObject.Destroy(rock);
            rocksCollected++;

            GetComponent<AudioSource>().Play();
        }

        /// <summary>
        /// Returns the number of rocks the player has picked up.
        /// </summary>
        /// <returns>The number of rocks picked up.</returns>
        public int GetNumRocksCollected()
        {
            return rocksCollected;
        }
    }
}