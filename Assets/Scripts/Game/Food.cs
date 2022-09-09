using UnityEngine;

public class Food : MonoBehaviour
{
    public BoxCollider2D gridArea;

    private void Start()
    {
        RandomizePosition();
    }

    private void RandomizePosition()
    {
        var bounds = gridArea.bounds;

        var x = Random.Range(bounds.min.x, bounds.max.x);
        var y = Random.Range(bounds.min.y, bounds.max.y);

        transform.position = new Vector3(
            Mathf.Round(x),
            Mathf.Round(y),
            0.0f
        );
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            RandomizePosition();
        }
    }
}
