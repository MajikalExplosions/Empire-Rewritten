using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Empire_Rewritten.Utils
{
    public class FlexRect
    {
        public AnchorType AnchorType { get; private set; }
        public string Name { get; set; }

        private Vector2 MinimumSize;
        private List<FlexRect> Children;
        private float VerticalRatio, HorizontalRatio;

        /// <summary>
        /// Stores the ratio that this rect's min-min corner should be offset from 0, 0 of the parent rect.
        /// E.g. 0.1 = margin of 10% of the total width/height of the parent.
        /// </summary>
        private float GridOffsetX, GridOffsetY;

        public FlexRect(string name = "")
        {
            AnchorType = AnchorType.Root;
            Name = name;
            Children = new List<FlexRect>();
            MinimumSize = Vector2.zero;
        }

        public FlexRect GetChild(string name)
        {
            foreach (FlexRect child in Children)
            {
                if (child.Name == name) return child;
                FlexRect r = child.GetChild(name);
                if (r != null) return r;
            }
            return null;
        }

        public bool RemoveChild(string name)
        {
            foreach (FlexRect child in Children)
            {
                if (child.Name == name)
                {
                    Children.Remove(child);
                    return true;
                }
                if (child.RemoveChild(name)) return true;
            }
            return false;
        }

        public FlexRect Top(float ratio, string name = "")
        {
            FlexRect fr = new FlexRect(name)
            {
                AnchorType = AnchorType.Top,
                VerticalRatio = ratio
            };
            Children.Add(fr);
            return fr;
        }

        public FlexRect Bottom(float ratio, string name = "")
        {
            FlexRect fr = new FlexRect(name)
            {
                AnchorType = AnchorType.Bottom,
                VerticalRatio = ratio
            };
            Children.Add(fr);
            return fr;
        }

        public FlexRect Left(float ratio, string name = "")
        {
            FlexRect fr = new FlexRect(name)
            {
                AnchorType = AnchorType.Left,
                HorizontalRatio = ratio
            };
            Children.Add(fr);
            return fr;
        }

        public FlexRect Right(float ratio, string name = "")
        {
            FlexRect fr = new FlexRect(name)
            {
                AnchorType = AnchorType.Right,
                HorizontalRatio = ratio
            };
            Children.Add(fr);
            return fr;
        }

        public FlexRect TopLeft(float ratioX, float ratioY, string name = "")
        {
            FlexRect fr = new FlexRect(name)
            {
                AnchorType = AnchorType.TopLeft,
                HorizontalRatio = ratioX,
                VerticalRatio = ratioY
            };
            Children.Add(fr);
            return fr;
        }

        public FlexRect TopRight(float ratioX, float ratioY, string name = "")
        {
            FlexRect fr = new FlexRect(name)
            {
                AnchorType = AnchorType.TopRight,
                HorizontalRatio = ratioX,
                VerticalRatio = ratioY
            };
            Children.Add(fr);
            return fr;
        }

        public FlexRect BottomLeft(float ratioX, float ratioY, string name = "")
        {
            FlexRect fr = new FlexRect(name)
            {
                AnchorType = AnchorType.BottomLeft,
                HorizontalRatio = ratioX,
                VerticalRatio = ratioY
            };
            Children.Add(fr);
            return fr;
        }

        public FlexRect BottomRight(float ratioX, float ratioY, string name = "")
        {
            FlexRect fr = new FlexRect(name)
            {
                AnchorType = AnchorType.BottomRight,
                HorizontalRatio = ratioX,
                VerticalRatio = ratioY
            };
            Children.Add(fr);
            return fr;
        }

        public FlexRect Center(float ratioX, float ratioY, string name = "")
        {
            FlexRect fr = new FlexRect(name)
            {
                AnchorType = AnchorType.Center,
                HorizontalRatio = ratioX,
                VerticalRatio = ratioY
            };
            Children.Add(fr);
            return fr;
        }

        public FlexRect Grid(int gridSizeX, int gridSizeY, int posX, int posY, string name = "")
        {
            FlexRect fr = new FlexRect(name)
            {
                AnchorType = AnchorType.Grid,
                HorizontalRatio = 1f / gridSizeX,
                VerticalRatio = 1f / gridSizeY,
                GridOffsetX = (1f / gridSizeX) * posX,
                GridOffsetY = (1f / gridSizeY) * posY
            };
            Children.Add(fr);
            return fr;
        }

        public void SetMinimumSize(Vector2 size)
        {
            MinimumSize = size;
        }

        public Vector2 GetMinimumSize()
        {
            foreach (FlexRect child in Children)
            {
                Vector2 minChildSize = child.MinimumSize;
                //Rescale minimum size of child to get the minimum size of this node
                switch(child.AnchorType)
                {
                    case AnchorType.Root:
                        Logger.Warn("Root node found when solving FlexRect minimum size. Ignoring node.");
                        break;

                    case AnchorType.Top:
                    case AnchorType.Bottom:
                        minChildSize = new Vector2(child.MinimumSize.x, child.MinimumSize.y / child.VerticalRatio);
                        break;
                    case AnchorType.Left:
                    case AnchorType.Right:
                        minChildSize = new Vector2(child.MinimumSize.x / child.HorizontalRatio, child.MinimumSize.y);
                        break;
                    case AnchorType.TopLeft:
                    case AnchorType.TopRight:
                    case AnchorType.BottomLeft:
                    case AnchorType.BottomRight:
                    case AnchorType.Center:
                    case AnchorType.Grid:
                        minChildSize = new Vector2(child.MinimumSize.x / child.HorizontalRatio, child.MinimumSize.y / child.VerticalRatio);
                        break;
                }

                //Minimum size is max of the minimum heights of all children and the max of the min widths of all children
                MinimumSize = new Vector2(Mathf.Max(MinimumSize.x, minChildSize.x), Mathf.Max(MinimumSize.y, minChildSize.y));
            }
            return MinimumSize;
        }

        public Dictionary<string, Rect> Resolve(Rect rect) {
            Vector2 minSize = GetMinimumSize();
            if (rect.width < minSize.x || rect.height < minSize.y)
            {
                Logger.Error("Provided rect is smaller than the computed minimum size for a FlexRect.");
                return null;
            }
            //Resolve by going through children and figuring out where they belong.
            //  Each child contains information on how it should be placed relative to the parent.

            Dictionary<string, Rect> result = new Dictionary<string, Rect>();
            if (Name.Length != 0)
            {
                result.Add(Name, rect);
            }

            foreach (FlexRect child in Children)
            {
                Dictionary<string, Rect> childDict = null;
                switch (child.AnchorType)
                {
                    case AnchorType.Root:
                        Logger.Error("Root node found when resolving FlexRect.");
                        return null;

                    case AnchorType.Top:
                        childDict = child.Resolve(rect.TopPart(child.VerticalRatio));
                        break;
                    case AnchorType.Bottom:
                        childDict = child.Resolve(rect.BottomPart(child.VerticalRatio));
                        break;
                    case AnchorType.Left:
                        childDict = child.Resolve(rect.LeftPart(child.HorizontalRatio));
                        break;
                    case AnchorType.Right:
                        childDict = child.Resolve(rect.RightPart(child.HorizontalRatio));
                        break;

                    case AnchorType.TopLeft:
                        childDict = child.Resolve(rect.TopPart(child.VerticalRatio).LeftPart(child.HorizontalRatio));
                        break;
                    case AnchorType.TopRight:
                        childDict = child.Resolve(rect.TopPart(child.VerticalRatio).RightPart(child.HorizontalRatio));
                        break;
                    case AnchorType.BottomLeft:
                        childDict = child.Resolve(rect.BottomPart(child.VerticalRatio).LeftPart(child.HorizontalRatio));
                        break;
                    case AnchorType.BottomRight:
                        childDict = child.Resolve(rect.BottomPart(child.VerticalRatio).RightPart(child.HorizontalRatio));
                        break;

                    case AnchorType.Center:
                        float newHeight = rect.height * child.VerticalRatio;
                        float newWidth = rect.width * child.HorizontalRatio;
                        childDict = child.Resolve(new Rect(rect.center.x - newWidth / 2, rect.center.y - newHeight / 2, newWidth, newHeight));
                        break;

                    case AnchorType.Grid:
                        childDict = child.Resolve(new Rect(
                            rect.xMin + child.GridOffsetX * rect.width,
                            rect.yMin + child.GridOffsetY * rect.height,
                            child.HorizontalRatio * rect.width,
                            child.VerticalRatio * rect.height
                            ));
                        break;

                }

                if (childDict != null)
                {
                    foreach (KeyValuePair<string, Rect> pair in childDict)
                    {
                        if (result.ContainsKey(pair.Key)) Logger.Error("FlexRect with given name already exists; ignoring duplicate.");
                        else result.Add(pair.Key, pair.Value);
                    }
                }
            }

            return result;
        }
        public void MergeChildren()
        {
            Children.Sort((x, y) => x.AnchorType != y.AnchorType ? x.AnchorType.CompareTo(y.AnchorType) :
                x.HorizontalRatio != y.HorizontalRatio ? x.HorizontalRatio.CompareTo(y.HorizontalRatio) :
                x.VerticalRatio != y.VerticalRatio ? x.VerticalRatio.CompareTo(y.VerticalRatio) :
                x.GridOffsetX != y.GridOffsetX ? x.GridOffsetX.CompareTo(y.GridOffsetY) :
                x.GridOffsetY.CompareTo(y.GridOffsetY));

            List<FlexRect> oldChildren = Children;
            Children = new List<FlexRect>();
            foreach (FlexRect child in oldChildren)
            {
                bool merged = false;
                if (Children.Count != 0)
                {
                    //Attempt to merge this child with the last child.
                    FlexRect last = Children[Children.Count - 1];

                    //If both are named, don't merge.
                    if (last.Name.Length != 0 && child.Name.Length != 0)
                    {
                        Children.Add(child);
                        continue;
                    }

                    if (last.AnchorType == child.AnchorType)
                    {
                        //If both the anchor type and all the type's corresponding configs are the same, we can merge the two child lists
                        //  into a single node.
                        switch (last.AnchorType)
                        {
                            case AnchorType.Root:
                                Logger.Warn("Root node found when merging children.");
                                break;

                            case AnchorType.Top:
                            case AnchorType.Bottom:
                                if (last.VerticalRatio == child.VerticalRatio) merged = true;
                                break;
                            case AnchorType.Left:
                            case AnchorType.Right:
                                if (last.HorizontalRatio == child.HorizontalRatio) merged = true;
                                break;
                            case AnchorType.TopLeft:
                            case AnchorType.TopRight:
                            case AnchorType.BottomLeft:
                            case AnchorType.BottomRight:
                            case AnchorType.Center:
                                if (last.VerticalRatio == child.VerticalRatio && last.HorizontalRatio == child.HorizontalRatio) merged = true;
                                break;
                            case AnchorType.Grid:
                                if (last.VerticalRatio == child.VerticalRatio && last.HorizontalRatio == child.HorizontalRatio
                                    && last.GridOffsetX == child.GridOffsetX && last.GridOffsetY == child.GridOffsetY) merged = true;
                                break;
                        }

                        if (merged)
                        {
                            //Take the non-empty name (if any)
                            if (last.Name.Length == 0) last.Name = child.Name;

                            //Add its children to the already existent node.
                            foreach (FlexRect c in child.Children) last.Children.Add(c);

                        }
                    }
                }

                //If the new node wasn't merged with the last node in the list, it means it's not a duplicate.
                if (!merged) Children.Add(child);
            }
        }

        public override string ToString()
        {
            return AnchorType.ToString() + " | " + VerticalRatio + " " + HorizontalRatio + " " + GridOffsetX + " " + GridOffsetY;
        }

        /*
        private Rect RoundRect(Rect r)
        {
            bool hX = (r.x + r.width) % 1 >= 0.5, hY = (r.y + r.height) % 1 >= 0.5;
            return new Rect(Mathf.Round(r.x), Mathf.Round(r.y), hX ? (int)r.width : (int)r.width + 1, hY ? (int)r.height : (int)r.height + 1);
        }
        */
    }


    public enum AnchorType
    {
        Root,

        Top,
        Bottom,
        Left,
        Right,

        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight,

        Center,

        Grid
    }
}
