using Grasshopper.GUI.Canvas;
using Grasshopper.GUI;
using Grasshopper.Kernel.Attributes;
using Grasshopper.Kernel;
using Grasshopper;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



using System.Drawing;



namespace Headless.Utilities
{
    public class NoOutputComponent<T> : GH_ComponentAttributes where T : GH_Component
    {
        public NoOutputComponent(T owner) : base(owner) { }

        protected override void Render(GH_Canvas canvas, Graphics graphics, GH_CanvasChannel channel)
        {
            switch (channel)
            {
                case GH_CanvasChannel.Wires:
                    foreach (IGH_Param item in base.Owner.Params.Input)
                    {
                        item.Attributes.RenderToCanvas(canvas, GH_CanvasChannel.Wires);
                    }

                    break;
                case GH_CanvasChannel.Objects:

                    GH_Palette gH_Palette = GH_CapsuleRenderEngine.GetImpliedPalette(base.Owner);
                    if (gH_Palette == GH_Palette.Normal && !base.Owner.IsPreviewCapable)
                    {
                        gH_Palette = GH_Palette.Hidden;
                    }
                    bool left = base.Owner.Params.Input.Count == 0;
                    bool right = true;

                    GH_Capsule gH_Capsule = GH_Capsule.CreateCapsule(Bounds, gH_Palette);
                    gH_Capsule.SetJaggedEdges(left, right);
                    GH_PaletteStyle impliedStyle = GH_CapsuleRenderEngine.GetImpliedStyle(gH_Palette, Selected, base.Owner.Locked, base.Owner.Hidden);


                    bool drawComponentBaseBox = true;
                    bool drawParameterGrips = true;
                    RectangleF MessageRectangle = RectangleF.Empty;
                    bool drawComponentNameBox = true;
                    bool drawParameterNames = true;
                    bool drawZuiElements = true;




                    if (drawParameterGrips)
                    {
                        foreach (IGH_Param item in base.Owner.Params.Input)
                        {
                            gH_Capsule.AddInputGrip(item.Attributes.InputGrip.Y);
                        }
                    }

                    graphics.SmoothingMode = SmoothingMode.HighQuality;
                    if (GH_Attributes<IGH_Component>.IsIconMode(base.Owner.IconDisplayMode))
                    {
                        if (drawComponentBaseBox)
                        {
                            if (!string.IsNullOrWhiteSpace(base.Owner.Message))
                            {
                                MessageRectangle = gH_Capsule.RenderEngine.RenderMessage(graphics, base.Owner.Message, impliedStyle);
                            }

                            gH_Capsule.Render(graphics, impliedStyle);
                        }

                        if (drawComponentNameBox)
                        {
                            if (base.Owner.Icon_24x24 == null)
                            {
                                //gH_Capsule.RenderEngine.RenderIcon(graphics, Res_ObjectIcons.Icon_White_24x24, m_innerBounds);
                            }
                            else if (base.Owner.Locked)
                            {
                                gH_Capsule.RenderEngine.RenderIcon(graphics, base.Owner.Icon_24x24_Locked, m_innerBounds);
                            }
                            else
                            {
                                gH_Capsule.RenderEngine.RenderIcon(graphics, base.Owner.Icon_24x24, m_innerBounds);
                            }
                        }
                    }
                    else
                    {
                        if (drawComponentBaseBox)
                        {
                            if (base.Owner.Message != null)
                            {
                                gH_Capsule.RenderEngine.RenderMessage(graphics, base.Owner.Message, impliedStyle);
                            }

                            gH_Capsule.Render(graphics, impliedStyle);
                        }

                        if (drawComponentNameBox)
                        {
                            GH_Capsule gH_Capsule2 = GH_Capsule.CreateTextCapsule(m_innerBounds, m_innerBounds, GH_Palette.Black, base.Owner.NickName, GH_FontServer.LargeAdjusted, GH_Orientation.vertical_center, 3, 6);
                            gH_Capsule2.Render(graphics, Selected, base.Owner.Locked, hidden: false);
                            gH_Capsule2.Dispose();
                        }
                    }

                    if (drawComponentBaseBox)
                    {
                        IGH_TaskCapableComponent iGH_TaskCapableComponent = base.Owner as IGH_TaskCapableComponent;
                        if (iGH_TaskCapableComponent != null)
                        {
                            if (iGH_TaskCapableComponent.UseTasks)
                            {
                                gH_Capsule.RenderEngine.RenderBoundaryDots(graphics, 2, impliedStyle);
                            }
                            else
                            {
                                gH_Capsule.RenderEngine.RenderBoundaryDots(graphics, 1, impliedStyle);
                            }
                        }
                    }

                    if (drawComponentNameBox && base.Owner.Obsolete && CentralSettings.CanvasObsoleteTags && canvas.DrawingMode == GH_CanvasMode.Control)
                    {
                        GH_GraphicsUtil.RenderObjectOverlay(graphics, base.Owner, m_innerBounds);
                    }

                    if (drawParameterNames)
                    {
                        RenderComponentParameters(canvas, graphics, base.Owner, impliedStyle);
                    }

                    if (drawZuiElements)
                    {
                        RenderVariableParameterUI(canvas, graphics);
                    }

                    gH_Capsule.Dispose();
                    break;
            }
        }
    }
}
