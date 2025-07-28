using UnityEngine;

public class InCameraDetect : MonoBehaviour
{
    private Renderer renderer;

    private void Awake()
    {
        renderer = GetComponent<Renderer>();    
    }
    
    private void Update()
    {
        var frustum = GeometryUtility.CalculateFrustumPlanes(Camera.main);
        var bound = GetComponent<Collider>().bounds;

        if (GeometryUtility.TestPlanesAABB(frustum, bound))
        {
            renderer.material.color = Color.green;
        }
        else
        {
            renderer.material.color = Color.red;
        }
    }
}
