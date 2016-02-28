using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public class RenderWaterCB : MonoBehaviour
{

    //public GameObject objectToRender;
    //public Material materialToRenderWith;
    private Dictionary<Camera, CommandBuffer> m_Cameras = new Dictionary<Camera, CommandBuffer>();

    public void OnDisable()
    {
        foreach (KeyValuePair<Camera, CommandBuffer> cam in m_Cameras)
        {
            if (cam.Key)
            {
                cam.Key.RemoveCommandBuffer(CameraEvent.BeforeLighting, cam.Value);
            }
        }
    }

    public void OnWillRenderObject()
    {
        bool act = gameObject.activeInHierarchy && enabled;
        if (!act)
        {
            OnDisable();
            return;
        }

        Camera cam = Camera.current;
        if (!cam)
            return;

        CommandBuffer buf = null;
        if (m_Cameras.ContainsKey(cam))
        {
            return;
            //buf = m_Cameras[cam];
            //buf.Clear();
        }

        buf = new CommandBuffer();
        buf.name = "WaterRenderTest";
        m_Cameras[cam] = buf;

        
        int propertyID = Shader.PropertyToID("_SceneCopy");
        buf.GetTemporaryRT(propertyID, -1, -1, 0, FilterMode.Bilinear);
        buf.SetRenderTarget(propertyID);
        buf.Blit(BuiltinRenderTextureType.CurrentActive, propertyID);
        
        /*
        int propertyID = Shader.PropertyToID("_DepthCopy");
        buf.GetTemporaryRT(propertyID, -1, -1, 0, FilterMode.Bilinear);
        buf.SetRenderTarget(propertyID);
        buf.Blit(BuiltinRenderTextureType.Depth, propertyID);
        */
        //buf.DrawMesh(objectToRender.GetComponent<MeshFilter>().sharedMesh, trns, materialToRenderWith); 
        //buf.DrawRenderer(objectToRender.GetComponent<Renderer>(), materialToRenderWith);

        //buf.Blit(BuiltinRenderTextureType.CurrentActive, propertyID);

        cam.AddCommandBuffer(CameraEvent.AfterLighting, buf);
    }
}
