using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Empire_Rewritten.Utils
{
    public class TestFlexAnimationWindow : Window
    {
        private FlexRect root;
        private int step;
        public TestFlexAnimationWindow()
        {
            closeOnClickedOutside = true;
            forcePause = true;
            preventCameraMotion = true;
            onlyOneOfTypeAllowed = true;
            resizeable = true;
            draggable = true;

            //Test all anchor types and MergeChildren (there should be 2 merges)
            root = new FlexRect("root");
            FlexRect center = root.Center(0.4f, 0.4f, "center");
            center.TopLeft(0.25f, 0.25f, "tl");
            center.TopRight(0.25f, 0.25f, "tr");
            center.BottomLeft(0.25f, 0.25f, "bl");
            center.BottomRight(0.25f, 0.25f, "br");

            root.Top(0.2f, "t");
            root.Bottom(0.2f, "b");
            root.Left(0.2f, "l");
            root.Right(0.2f, "r");
            root.Grid(9, 9, 1, 7, "grid");
            root.Grid(9, 9, 7, 1, "grid2");
            root.MergeChildren();
            
            step = 0;
        }

        public override void DoWindowContents(Rect inRect)
        {
            root.RemoveChild("center");
            float size = (float)Math.Sin((float)step / 30f) * 0.1f + 0.3f;
            FlexRect center = root.Center(size, size, "center");
            center.TopLeft(0.25f, 0.25f, "tl");
            center.TopRight(0.25f, 0.25f, "tr");
            center.BottomLeft(0.25f, 0.25f, "bl");
            center.BottomRight(0.25f, 0.25f, "br");

            Dictionary<string, Rect> rects = root.Resolve(inRect);

            Widgets.DrawBox(rects["root"]);
            Widgets.DrawBoxSolid(rects["center"], Color.white);
            Widgets.DrawBoxSolid(rects["t"], Color.green);
            Widgets.DrawBoxSolid(rects["b"], Color.red);
            Widgets.DrawBoxSolid(rects["l"], Color.blue);
            Widgets.DrawBoxSolid(rects["r"], Color.yellow);

            Widgets.DrawBoxSolid(rects["tl"], Color.green);
            Widgets.DrawBoxSolid(rects["tr"], Color.red);
            Widgets.DrawBoxSolid(rects["bl"], Color.blue);
            Widgets.DrawBoxSolid(rects["br"], Color.yellow);

            Widgets.DrawBoxSolidWithOutline(rects["grid"], Color.blue, Color.cyan);
            Widgets.DrawBoxSolidWithOutline(rects["grid2"], Color.blue, Color.cyan);
            step += 1;
        }
    }
}
