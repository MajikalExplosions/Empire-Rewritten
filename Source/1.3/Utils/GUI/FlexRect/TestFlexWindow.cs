using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace Empire_Rewritten.Utils
{
    public class TestFlexWindow : Window
    {
        private FlexRect root;
        public TestFlexWindow()
        {
            closeOnClickedOutside = true;
            forcePause = true;
            preventCameraMotion = true;
            onlyOneOfTypeAllowed = true;
            resizeable = true;
            draggable = true;

        }

        public override void DoWindowContents(Rect inRect)
        {
            bool action = WindowHelper.ConfirmDialog(inRect, "Test question?", "Cancel", "Confirm",
                () => Logger.Log("Canceled!"), () => Logger.Log("Confirmed!"));
            if (action) this.Close();
        }
    }
}
