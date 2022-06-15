using UnityEngine;

namespace Cards
{
    public class TestDraw : MonoBehaviour
    {
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawCube(transform.position, new Vector3(0.7f, 0.01f, 1f));
        }

    } 
}
