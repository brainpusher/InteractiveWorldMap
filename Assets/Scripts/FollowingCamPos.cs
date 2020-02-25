using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowingCamPos : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.LookAt(new Vector3(Camera.main.transform.position.x, transform.position.y,Camera.main.transform.position.z));
    }
}
