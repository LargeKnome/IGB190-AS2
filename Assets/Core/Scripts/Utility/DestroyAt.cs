using UnityEngine;

public class DestroyAt : MonoBehaviour
{
    private float time;
    private int iteration = 0;
    
    void Update()
    {
        if (Time.time > time)
        {
            ObjectPooler.DestroyPooled(gameObject);
        }
    }

    public void Run (float time)
    {
        this.time = time;
    }
}
