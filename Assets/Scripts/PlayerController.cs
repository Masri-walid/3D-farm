using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public float speed = 5f;
    public float gravity = -9.81f;
    public float mouseSensitivity = 2f;
    public float interactDistance = 3f;

    CharacterController controller;
    float verticalVelocity = 0f;
    Transform cam;
    float pitch = 0f;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        cam = transform.Find("PlayerCamera");
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        Move();
        Look();
        if (Input.GetKeyDown(KeyCode.E)) Interact();
        if (Input.GetKeyDown(KeyCode.I)) Inventory.InventoryUI.Toggle();
        if (Input.GetKeyDown(KeyCode.F5)) SaveSystem.Instance?.Save();
        if (Input.GetKeyDown(KeyCode.F9)) SaveSystem.Instance?.Load();
    }

    void Move()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        Vector3 move = transform.right * x + transform.forward * z;
        move *= speed;

        if (controller.isGrounded && verticalVelocity < 0) verticalVelocity = -1f;
        verticalVelocity += gravity * Time.deltaTime;
        move.y = verticalVelocity;

        controller.Move(move * Time.deltaTime);
    }

    void Look()
    {
        float mx = Input.GetAxis("Mouse X") * mouseSensitivity;
        float my = Input.GetAxis("Mouse Y") * mouseSensitivity;
        transform.Rotate(Vector3.up * mx);
        pitch -= my;
        pitch = Mathf.Clamp(pitch, -85f, 85f);
        if (cam) cam.localEulerAngles = new Vector3(pitch, 0, 0);
    }

    void Interact()
    {
        Ray ray = new Ray(cam.position, cam.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance))
        {
            var spot = hit.collider.GetComponentInParent<PlantingSpot>();
            if (spot != null)
            {
                if (spot.HasCrop())
                {
                    var result = spot.Harvest();
                    if (result != null)
                    {
                        Inventory.Instance.AddItem(result);
                    }
                }
                else
                {
                    // plant if player has seed
                    var seed = Inventory.Instance.GetFirstSeed();
                    if (seed != null)
                    {
                        spot.Plant(seed.itemId);
                        Inventory.Instance.RemoveItem(seed.itemId, 1);
                    }
                    else
                    {
                        Debug.Log("No seeds in inventory to plant.");
                    }
                }
            }
        }
    }
}