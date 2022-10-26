using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using System;
public class MyScreenCapture
{

    // return file name
    static string fileName(int width, int height, bool pngOrJpg)
    {
        if (pngOrJpg)
            return string.Format("screen_{0}x{1}_{2}.png",
                              width, height,
                              System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
        else
            return string.Format("screen_{0}x{1}_{2}.jpg",
                              width, height,
                              System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
    }

    public static IEnumerator TakeScreenShot(Camera cam, string abspath, bool drawBoundingBox, Rect boundingBox, bool pngOrJpg, int width, int height, List<Rect> egoBB = null)
    {
        yield return new WaitForEndOfFrame();


        //  Camera camOV = cam;  
        //  RenderTexture currentRT = RenderTexture.active; 
        //  RenderTexture.active = camOV.targetTexture;

        //  camOV.Render();
        //  Texture2D imageOverview = new Texture2D(camOV.targetTexture.width, camOV.targetTexture.height, TextureFormat.RGB24, false);
        //  imageOverview.ReadPixels(new Rect(0, 0, camOV.targetTexture.width, camOV.targetTexture.height), 0, 0);
        //  imageOverview.Apply();
        //  RenderTexture.active = currentRT;   

        RenderTexture rt = new RenderTexture(width, height, 24);
        cam.targetTexture = rt;

        var currentRT = RenderTexture.active;
        RenderTexture.active = cam.targetTexture;

        // Render the camera's view.
        cam.Render();

        // Make a new texture and read the active Render Texture into it.
        Texture2D image = new Texture2D(cam.targetTexture.width, cam.targetTexture.height);
        image.ReadPixels(new Rect(0, 0, cam.targetTexture.width, cam.targetTexture.height), 0, 0);
        image.Apply();
        if (drawBoundingBox)
        {
            if (egoBB != null)
            {
                foreach (Rect rect in egoBB)
                {
                    DrawBoundingBox(image, rect, Color.red);
                }
            }
            DrawBoundingBox(image, boundingBox, Color.green);
        }

        // Replace the original active Render Texture.
        RenderTexture.active = currentRT;



        // Encode texture into PNG
        byte[] bytes;
        string filename;
        if (pngOrJpg)
        {
            bytes = image.EncodeToPNG();
            filename = "frame.png";

        }
        else
        {
            bytes = image.EncodeToJPG();
            filename = "frame.jpg";

        }

        // save in memory
        //string filename = fileName(Convert.ToInt32(image.width), Convert.ToInt32(image.height), pngOrJpg);
        /*[setting_name:only_object/both_gaze_gesture_........][view_name:ego_view/exo_view/top_view]_[modality:rgb/depth/skeleton].png */
        //string filename = ""
        string path = abspath + filename;

        //Debug.Log (path);
        System.IO.File.WriteAllBytes(path, bytes);

        cam.targetTexture = null;
    }
    static void DrawBoundingBox(Texture2D image, Rect boundingBox, Color color)
    {
        //make sure they are positive

        //do it like this... trace along x axis for all y and both x'es
        //then trace along y axis for all x'es and both y'es
        //Debug.Log("x: " + boundingBox.x + " y: " + boundingBox.y + " width " + boundingBox.width + " height " + boundingBox.height);
        List<Vector2> list = GetRegularizedBB(boundingBox, false);
        boundingBox = new Rect(list[0], list[1] - list[0]);
        for (int y = (int)boundingBox.y; y <= (int)(boundingBox.y + boundingBox.height); y++)
        {
            image.SetPixel((int)boundingBox.x, y, color);
            image.SetPixel((int)(boundingBox.x + boundingBox.width), y, color);
        }
        for (int x = (int)boundingBox.x; x <= (int)(boundingBox.x + boundingBox.width); x++)
        {
            image.SetPixel(x, (int)boundingBox.y, color);
            image.SetPixel(x, (int)(boundingBox.y + boundingBox.height), color);
        }
    }
    static List<Vector2> GetRegularizedBB(Rect rect, bool topLBottomL)
    {
        bool invalidBoundingBox = (rect.xMin == 1 && rect.xMax == 1 || rect.xMin == Screen.width && rect.xMax == Screen.width || rect.yMin == 1 && rect.yMax == 1 || rect.yMin == Screen.height && rect.yMax == Screen.height || rect.xMin == 1 && rect.xMax == Screen.width || rect.xMin == Screen.width && rect.xMax == 1 || rect.yMin == 1 && rect.yMax == Screen.height || rect.yMin == Screen.height && rect.yMax == 1);
        if (invalidBoundingBox)
        {
            return new List<Vector2> { new Vector2(-1, -1), new Vector2(-1, -1) };
        }
        else
        {
            if (topLBottomL)
            {
                return new List<Vector2> { new Vector2(rect.min.x, Screen.height - rect.min.y), new Vector2(rect.max.x, Screen.height - rect.max.y) };
            }
            else
            {
                return new List<Vector2> { rect.min, rect.max };
            }

        }
    }
}