using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraUpdate : MonoBehaviour
{
    public GameObject m_WorldAnchor;

    private Vector3 m_OriginAnchorPosition;
    private Vector3 m_OriginCameraPosition;

    private Vector3 m_OriginAnchorRotation;
    private Vector3 m_OriginCameraRotation;
    private void Awake()
    {
        m_OriginAnchorPosition = m_WorldAnchor.transform.position;
        m_OriginCameraPosition = Camera.main.transform.position; 
        m_OriginAnchorRotation = m_WorldAnchor.transform.eulerAngles;
        m_OriginCameraRotation = Camera.main.transform.eulerAngles;
    }

    void Start()
    {
        
    }

    public void OnBeginDrag()
    {
        
    }
    public void OnEndDrag()
    {

    }
    
    private void Update()
    {
        var offset1 = m_WorldAnchor.transform.position - m_OriginAnchorPosition;
        Camera.main.transform.position = m_OriginCameraPosition - offset1;
            
        var offset2 = m_WorldAnchor.transform.eulerAngles - m_OriginAnchorRotation;
        Camera.main.transform.eulerAngles = m_OriginCameraRotation - offset2;
    }

}
