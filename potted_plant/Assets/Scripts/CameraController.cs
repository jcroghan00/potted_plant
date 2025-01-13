using UnityEngine;

public class CameraController : MonoBehaviour
{
    public GameObject player;
    public GameObject cameraPoint1;
    public GameObject cameraPoint2;
    public float cameraDepth = -10.0f;

    private Transform _cameraTransform;
    private Transform _playerTransform;
    private Transform _cameraTransform1;
    private Transform _cameraTransform2;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _cameraTransform = gameObject.GetComponent<Transform>();
        _playerTransform = player.GetComponent<Transform>();
        _cameraTransform1 = cameraPoint1.GetComponent<Transform>();
        _cameraTransform2 = cameraPoint2.GetComponent<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        // camera x
        if (_cameraTransform1.position.x > _playerTransform.position.x && _playerTransform.position.x > _cameraTransform2.position.x) {
            _cameraTransform.position = new Vector3(_playerTransform.position.x, _cameraTransform.position.y, cameraDepth);
        }
        else if (_cameraTransform1.position.x <= _playerTransform.position.x) {
            _cameraTransform.position = new Vector3(_cameraTransform1.position.x, _cameraTransform.position.y, cameraDepth);
        }
        else if (_cameraTransform2.position.x >= _playerTransform.position.x) {
            _cameraTransform.position = new Vector3(_cameraTransform2.position.x, _cameraTransform.position.y, cameraDepth);
        }

        // camera y
        if (_cameraTransform2.position.y > _playerTransform.position.y && _playerTransform.position.y > _cameraTransform1.position.y) {
            _cameraTransform.position = new Vector3(_cameraTransform.position.x, _playerTransform.position.y, cameraDepth);
        }
        else if (_cameraTransform1.position.y >= _playerTransform.position.y) {
            _cameraTransform.position = new Vector3(_cameraTransform.position.x, _cameraTransform1.position.y, cameraDepth);
        }
        else if (_cameraTransform2.position.y <= _playerTransform.position.y) {
            _cameraTransform.position = new Vector3(_cameraTransform.position.x, _cameraTransform2.position.y, cameraDepth);
        }
    }
}
