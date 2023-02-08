using System.Collections;
using UnityEngine;

public class scr_SelfDestruct : MonoBehaviour
{
    public float seconds;
    void Start()
    {
        StartCoroutine(selfDestruct(seconds));
    }

    IEnumerator selfDestruct(float time)
    {
        yield return new WaitForSeconds(time);
        Destroy(gameObject);
    }
}
