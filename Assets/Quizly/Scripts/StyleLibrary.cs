using UnityEngine;

namespace Quizly
{
    public static class StyleLibrary
    {
        public static GUIStyle HeaderStyle = new GUIStyle()
        {
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold, fontSize = 18,
            padding = new RectOffset(10, 10, 25, 15),
            normal = new GUIStyleState()
            {
                textColor = Color.white,
            }
        };
        
        public static GUIStyle Header2Style = new GUIStyle()
        {
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold, fontSize = 16,
            padding = new RectOffset(5, 5, 15, 10),
            normal = new GUIStyleState()
            {
                textColor = Color.white,
            }
        };

        public static GUIStyle Header2LeftStyle = new GUIStyle()
        {
            fontStyle = Header2Style.fontStyle, fontSize = Header2Style.fontSize,
            padding = Header2Style.padding,
            normal = Header2Style.normal
        };

        public static GUIStyle Header3Style = new GUIStyle()
        {
            fontStyle = FontStyle.Bold, fontSize = 14,
            padding = new RectOffset(5, 5, 5, 5),
            normal = new GUIStyleState()
            {
                textColor = Color.white,
            }
        };
        
        public static GUIStyle Header4Style = new GUIStyle()
        {
            fontStyle = FontStyle.Bold, fontSize = 12,
            padding = new RectOffset(5, 5, 5, 5),
            normal = new GUIStyleState()
            {
                textColor = Color.white,
            }
        };
        
        public static GUIStyle WarningStyle = new GUIStyle()
        {
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold, fontSize = 12,
            padding = new RectOffset(0, 0, 5, 5),
            normal = new GUIStyleState()
            {
                textColor = Color.red,
            }
        };

        public static GUIStyle Bold = new GUIStyle()
        {
            fontStyle = FontStyle.Bold,
            normal = new GUIStyleState()
            {
                textColor = Color.white,
            },
            padding = new RectOffset(5, 5, 5, 5),
        };

        public static GUIStyle BoldItalic = new GUIStyle()
        {
            fontStyle = FontStyle.BoldAndItalic,
            normal = new GUIStyleState()
            {
                textColor = Color.white,
            },
        };
        
        public static GUIStyle BottomMessage = new GUIStyle()
        {
            fontStyle = FontStyle.Italic,
            normal = new GUIStyleState()
            {
                textColor = Color.white,
            },
            margin = new RectOffset(5, 5, 5, 5),
        };
    }
}
