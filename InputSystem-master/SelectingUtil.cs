using UnityEngine;

namespace RTS.Controlls
{
    public static class SelectingUtil
    {
        static Texture2D texture;
        public static Texture2D RectTexture
        {
            get
            {
                if (texture == null)
                {
                    texture = new Texture2D(1, 1);
                    texture.SetPixel(0, 0, Color.white);
                    texture.Apply();
                }
                
                return texture;
            }
        }

        public static Vector2 WorldToScreen(this Matrix4x4 viewMatrix, Vector2 screenSize, Vector3 point3D)
        {
            Vector2 returnVector = Vector2.zero;
            float w = viewMatrix[3, 0] * point3D.x + viewMatrix[3, 1] * point3D.y + viewMatrix[3, 2] * point3D.z + viewMatrix[3, 3];
            
            if (w >= 0.01f)
            {
                float inverseX = 1f / w;
                returnVector.x =
                    (screenSize.x / 2f) +
                    (0.5f * (
                    (viewMatrix[0, 0] * point3D.x + viewMatrix[0, 1] * point3D.y + viewMatrix[0, 2] * point3D.z + viewMatrix[0, 3])
                    * inverseX)
                    * screenSize.x + 0.5f);
                returnVector.y =
                    (screenSize.x / 2f) -
                    (0.5f * (
                    (viewMatrix[1, 0] * point3D.x + viewMatrix[1, 1] * point3D.y + viewMatrix[1, 2] * point3D.z + viewMatrix[1, 3])
                    * inverseX)
                    * screenSize.y + 0.5f);
            }
            
            return returnVector;
        }
        
        public static void DrawScreenRect(Rect rect, Color color)
        {
            GUI.color = color;
            GUI.DrawTexture(rect, RectTexture);
            GUI.color = Color.white;
        }

        public static void DrawScreenRectBorder(Rect rect, float thickness, Color color)
        {
            // Top
            DrawScreenRect(new Rect(rect.xMin, rect.yMin, rect.width, thickness), color);
            // Left
            DrawScreenRect(new Rect(rect.xMin, rect.yMin, thickness, rect.height), color);
            // Right
            DrawScreenRect(new Rect(rect.xMax - thickness, rect.yMin, thickness, rect.height), color);
            // Bottom
            DrawScreenRect(new Rect(rect.xMin, rect.yMax - thickness, rect.width, thickness), color);
        }
    }
}
