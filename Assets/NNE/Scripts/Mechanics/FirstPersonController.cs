using UnityEngine;

public class FirstPersonController : MonoBehaviour
{
    [SerializeField] private float movementSpeed = 5.0f;
    [SerializeField] private float mouseSensitivity = 2.0f;
    [SerializeField] private Camera playerCamera;

    private float verticalRotation = 0;
    private float horizontalRotation = 0;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        // playerCamera = GetComponentInChildren<Camera>();
    }

    public void ActivateCamera(bool arg)
    {
        playerCamera.gameObject.SetActive(arg);
    }

    #region Player Movement and Aiming
    public void FreeMovement(ref Vector3 moveTowards, ref Vector3 lookAt)
    {
        moveTowards = GetRequestedPosition();
        transform.Translate(moveTowards);

        lookAt = GetRequestedRotation();
        transform.Rotate(0, lookAt.y, 0);
        playerCamera.transform.localRotation = Quaternion.Euler(lookAt.x, 0, 0);
    }

    public void FetchMovement(ref Vector3 localPos, ref Vector3 localRot)
    {
        localPos = GetRequestedPosition();
        localRot = GetRequestedRotation();
    }

    public void SimulateMovement(Vector3 localPos, Vector3 localRot)
    {
        transform.Translate(localPos);

        transform.Rotate(0, localRot.y, 0);
        playerCamera.transform.localRotation = Quaternion.Euler(localRot.x, 0, 0);
    }
    #endregion

    #region Position and Rotation Input Handling
    private Vector3 GetRequestedPosition()
    {
        // Movement
        float forwardSpeed = Input.GetAxis("Vertical") * movementSpeed * Time.deltaTime;
        float strafeSpeed = Input.GetAxis("Horizontal") * movementSpeed * Time.deltaTime;

        return new Vector3(strafeSpeed, 0, forwardSpeed);
    }

    private Vector3 GetRequestedRotation()
    {
        // Mouse Look
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = -Input.GetAxis("Mouse Y") * mouseSensitivity;

        horizontalRotation = mouseX;
        verticalRotation += mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -90, 90);

        return new Vector3(verticalRotation, horizontalRotation, 0f);
    }
    #endregion

    #region Player Interaction
    public bool InteractionHandler(ref Vector3 pos, ref Vector3 rot)
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, Mathf.Infinity))
            {
                return HandleRaycastHit(hit, ref pos, ref rot);
            }
        }
        return false;
    }
    private bool HandleRaycastHit(RaycastHit hit, ref Vector3 resPos, ref Vector3 resRot)
    {
        if (hit.collider.gameObject == null)
            return false;

        GameObject hitObject = hit.collider.gameObject;
        Debug.Log("Hit object: " + hitObject.name);

        if (hitObject.GetComponent<QRCodeHandler>() != null)
        {
            QRCodeHandler codeHandler = hitObject.GetComponent<QRCodeHandler>();

            DataCubeHandler.Instance.UpdateOrientation(codeHandler.Color, hitObject.transform, ref resPos, ref resRot);
            return true;
        }

        if (hitObject.GetComponent<DataPointHandler>() != null)
        {
            DataPointHandler dataPointHandler = hitObject.GetComponent<DataPointHandler>();

            DataCubeHandler.Instance.ReflectMetaDataAsync(dataPointHandler.Color);
        }
        return false;
    }

    public void SimulateDataCubeOrientation(Vector3 pos, Vector3 rot)
    {
        DataCubeHandler.Instance.SimulateOrientation(pos, rot);
    }
    #endregion
}
