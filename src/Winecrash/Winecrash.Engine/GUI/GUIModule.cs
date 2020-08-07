﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winecrash.Engine.GUI
{
    public abstract class GUIModule : Module
    {
        public Vector2F MinAnchor { get; set; } = Vector2F.Zero;
        public Vector2F MaxAnchor { get; set; } = Vector2F.One;

        public GUIModule ParentGUI { get; set; } = null;


        internal virtual Vector3F GlobalPosition
        {
            get
            {
                /*if(this.ParentGUI == null)
                {
                    return this.WObject.Position;
                }*/

                float[] ganchors = GlobalScreenAnchors;

                Vector2F gMinAnchors = new Vector2F(ganchors[0], ganchors[1]);
                Vector2F gMaxAnchors = new Vector2F(ganchors[2], ganchors[3]);

                Vector2F half = (gMaxAnchors - gMinAnchors) / 2.0F;

                Vector2F screenSpacePosition = gMinAnchors + half;

                float horizontalShift = (GlobalRight / 4.0F) - (GlobalLeft / 4.0F);
                float verticalShift = (GlobalBottom / 4.0F) - (GlobalTop / 4.0F);

                Vector2F shift = new Vector2F(horizontalShift, verticalShift);

                //Vector3F sca = new Vector3F(Canvas.ScreenToUISpace(screenSpacePosition) + shift;

                //sca.X = WMath.Clamp(sca.X, Image.MinScale.X, Image.MaxScale.X);
                //sca.Y = WMath.Clamp(sca.Y, Image.MinScale.Y, Image.MaxScale.Y);
                //sca.Z = WMath.Clamp(sca.Z, Image.MinScale.Z, Image.MaxScale.Z);

                return new Vector3F(Canvas.ScreenToUISpace(screenSpacePosition) + shift, Depth);
            }
        }

        internal float GlobalRight
        {
            get
            {
                return this.ParentGUI == null ? this.Right : this.ParentGUI.GlobalRight + this.Right;
            }
        }

        internal float GlobalLeft
        {
            get
            {
                return this.ParentGUI == null ? this.Left : this.ParentGUI.GlobalLeft + this.Left;
            }
        }

        internal float GlobalTop
        {
            get
            {
                return this.ParentGUI == null ? this.Top : this.ParentGUI.Top + this.Top;
            }
        }

        internal float GlobalBottom
        {
            get
            {
                return this.ParentGUI == null ? this.Bottom : this.ParentGUI.GlobalBottom + this.Bottom;
            }
        }

        internal virtual Vector3F GlobalScale
        {
            get//GlobalScreenAnchors
            {
                Vector3F totalExtentsScaled = new Vector3F(((Vector2F)Canvas.Main.Extents) * 2.0F, 1.0F) * this.WObject.Scale;


                float[] anchors = this.GlobalScreenAnchors;


                Vector2F minanchors = new Vector2F(anchors[0], anchors[1]);
                Vector2F maxanchors = new Vector2F(anchors[2], anchors[3]);

                Vector2F deltas = maxanchors - minanchors;

                totalExtentsScaled.XY *= deltas;

                float horizontalScale = -(GlobalRight / 2.0F) - (GlobalLeft / 2.0F);
                float verticalScale = -(GlobalBottom / 2.0F) - (GlobalTop / 2.0F);

                Vector3F sca = totalExtentsScaled * this.WObject.Scale + new Vector3F(horizontalScale, verticalScale, 1.0F);

                return sca;
            }
        }

        /// <summary>
        /// Get the global anchor position of the object, depending of the parents. Indices of the arary => xmin[0], ymin[1], xmax[2], ymax[3]
        /// </summary>
        internal virtual float[] GlobalScreenAnchors
        {
            get
            {
                float[] anchors = new float[4];

                if(this.ParentGUI == null)
                {
                    anchors[0] = this.MinAnchor.X;
                    anchors[1] = this.MinAnchor.Y;
                    anchors[2] = this.MaxAnchor.X;
                    anchors[3] = this.MaxAnchor.Y;
                }

                else
                {
                    //TODO le pb est ici
                    float[] panchors = this.ParentGUI.GlobalScreenAnchors;

                    float xMin = WMath.Remap(this.MinAnchor.X, 0, 1, panchors[0], panchors[2]);      
                    float yMin = WMath.Remap(this.MinAnchor.Y, 0, 1, panchors[1], panchors[3]);                  
                    float xMax = WMath.Remap(this.MaxAnchor.X, 0, 1, panchors[0], panchors[2]);               
                    float yMax = WMath.Remap(this.MaxAnchor.Y, 0, 1, panchors[1], panchors[3]);
                    

                    if (this is IRatioKeeper keepr && keepr.KeepRatio)
                    {
                        float screenRatio = (float)Canvas.Main.Size.X / (float)Canvas.Main.Size.Y;
                        float invScreenRatio = 1F / screenRatio;

                        // X bigger than Y
                        if (keepr.Ratio > 1.0F)
                        {
                            float yRatio = 1F / keepr.Ratio;

                            float yCentre = yMin + (yMax - yMin);
                            yCentre /= 2.0F;

                            

                            /*yMin = (yCentre - (yRatio / 2.0F)) * invScreenRatio;
                            yMax = (yCentre + (yRatio / 2.0F)) * invScreenRatio;*/

                            if (this.ParentGUI?.WObject.Name == "Singleplayer Button")
                            {
                                Debug.Log($"min:{yMin} / max:{yMax}");
                            }
                        }
                        
                    }


                    anchors[0] = xMin;
                    anchors[1] = yMin;
                    anchors[2] = xMax;
                    anchors[3] = yMax;
                }

                return anchors;
            }
        }

        /// <summary>
        /// Distance from the top of the anchor
        /// </summary>
        public float Top { get; set; } = 0.0F;
        /// <summary>
        /// Distance from the bottom of the anchor
        /// </summary>
        public float Bottom { get; set; } = 0.0F;
        /// <summary>
        /// Distance from the right of the anchor
        /// </summary>
        public float Right { get; set; } = 0.0F;
        /// <summary>
        /// Distance from the Left of the anchor
        /// </summary>
        public float Left { get; set; } = 0.0F;
        /// <summary>
        /// Depth
        /// </summary>
        public float Depth { get; set; } = 0.0F;
    }
}