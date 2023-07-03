﻿using System;
using UnityEditor;
using UnityEngine;

namespace WooAsset
{
    partial class AssetsWindow
    {
        [Serializable]
        class SplitView
        {
            public bool vertical = true;
            public float split = 200;
            public float minSize = 100;
            public event Action<Rect> fistPan, secondPan;
            public event Action onBeginResize;
            public event Action onEndResize;
            public bool dragging
            {
                get { return _resizing; }
                private set
                {
                    if (_resizing != value)
                    {
                        _resizing = value;
                        if (value)
                        {
                            if (onBeginResize != null)
                            {
                                onBeginResize();
                            }
                        }
                        else
                        {
                            if (onEndResize != null)
                            {
                                onEndResize();
                            }
                        }
                    }
                }
            }
            private bool _resizing;
            public void OnGUI(Rect position)
            {
                var rs = RectEx.Split(position, vertical, split, 4);
                var mid = RectEx.SplitRect(position, vertical, split, 4);
                if (fistPan != null)
                {
                    fistPan(rs[0]);
                }
                if (secondPan != null)
                {
                    secondPan(rs[1]);
                }
                EditorGUI.DrawRect(RectEx.Zoom(mid, TextAnchor.MiddleCenter, -2), Color.gray);
                Event e = Event.current;
                if (mid.Contains(e.mousePosition))
                {
                    if (vertical)
                        EditorGUIUtility.AddCursorRect(mid, MouseCursor.ResizeHorizontal);
                    else
                        EditorGUIUtility.AddCursorRect(mid, MouseCursor.ResizeVertical);
                }
                switch (Event.current.type)
                {
                    case EventType.MouseDown:
                        if (mid.Contains(Event.current.mousePosition))
                        {
                            dragging = true;
                        }
                        break;
                    case EventType.MouseDrag:
                        if (dragging)
                        {
                            if (vertical)
                                split += Event.current.delta.x;
                            else
                                split += Event.current.delta.y;

                            split = Mathf.Clamp(split, minSize, vertical ? position.width - minSize : position.height - minSize);

                            e.Use();
                   
                        }
                        break;
                    case EventType.MouseUp:
                        if (dragging)
                        {
                            dragging = false;
                        }
                        break;
                }
            }

        }
    }


}
