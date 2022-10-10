using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace Empire_Rewritten.Utils.FlexRect
{
    public class TestFlexWindow : Window
    {
        FlexRect root;
        public TestFlexWindow()
        {
            closeOnClickedOutside = true;
            forcePause = true;
            preventCameraMotion = true;
            onlyOneOfTypeAllowed = true;
            resizeable = true;
            draggable = true;

            root = new FlexRect("root");
            root.Top(0.5f).Center(0.75f, 0.25f, "center");
            FlexRect br = root.BottomLeft(0.4f, 0.4f, "bottomleft");
            br.Grid(3, 3, 0, 1, "grid");
            br.Grid(3, 3, 1, 0, "grid2");
            root.MergeChildren();//This call does nothing atm

            Dictionary<string, Rect> rects = root.Resolve(new Rect(0, 0, 500, 500));
            foreach (var pair in rects)
            {
                Logger.Log(pair.Key + " || " + pair.Value);
            }
        }

        public override void DoWindowContents(Rect inRect)
        {

            
            Dictionary<string, Rect> rects = root.Resolve(inRect);
            Widgets.DrawBox(rects["root"]);
            Widgets.DrawBoxSolid(rects["center"], Color.red);
            Widgets.DrawBox(rects["bottomleft"]);
            Widgets.DrawBoxSolidWithOutline(rects["grid"], Color.blue, Color.cyan);
            Widgets.DrawBoxSolidWithOutline(rects["grid2"], Color.blue, Color.cyan);
            
        }
    }
}
