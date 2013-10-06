using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Text;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Collections;

using UMD.HCIL.Piccolo;
using UMD.HCIL.Piccolo.Nodes;
using UMD.HCIL.PiccoloX.Events;
using UMD.HCIL.Piccolo.Event;
using UMD.HCIL.Piccolo.Util;

using VVVV.Core.Viewer.GraphicalEditor;
using VVVV.Core.View.GraphicalEditor;
using VVVV.Core;

namespace VVVV.HDE.GraphicalEditing
{
    ///// <summary>
    ///// a UserControl on which graphical editing takes place
    ///// </summary>
    public partial class GraphEditor: UserControl, ICanvas
    {
        //fields
        private DragDropEventHandler FDragDropEventHandler;
        private SelectionEventHandler FSelectionEventHandler;
        private PanEventHandler FMyPanEventHandler;
        private ZoomEventHandler FMyZoomEventHandler;
        private PathEventHandler FPathEventHandler;
        private EventPassThrougHandler FEventPassThrougHandler;
        private List<IGraphElement> FConnectables = new List<IGraphElement>();

        //layers
        public PLayer SolidLayer { get; protected set; }
        public PLayer LinkLayer { get; protected set; }
        
        public GraphEditor()
        {
            //
            // The InitializeComponent() call is required for Windows Forms designer support.
            //
            InitializeComponent();
            
            SolidLayer = FCanvas.Layer;
            LinkLayer = new PLayer();
            FCanvas.Root.AddChild(LinkLayer);
            
            //add linklayer below nodelayer
            FCanvas.Camera.AddLayer(LinkLayer);
            FCanvas.Camera.AddLayer(SolidLayer);
            
            //set default roots
            LinkRoot = CreateDot(null);
            Root = CreateDot(null);
            
            //set render quality
            FCanvas.HighRenderQuality +=
                delegate(Graphics graphics)
            {
                //improves text render quality when antialiased and speeds up drawing
                graphics.CompositingQuality = CompositingQuality.HighSpeed;
            };
            FCanvas.AnimatingRenderQuality = RenderQuality.HighQuality;
            FCanvas.InteractingRenderQuality = RenderQuality.HighQuality;
            FCanvas.DefaultRenderQuality = RenderQuality.HighQuality;
            
            //remove default zoom and pan event handlers
            FCanvas.RemoveInputEventListener(FCanvas.ZoomEventHandler);
            FCanvas.RemoveInputEventListener(FCanvas.PanEventHandler);

            //create custom event handlers
            FDragDropEventHandler = new DragDropEventHandler();
            FMyPanEventHandler = new PanEventHandler();
            FMyZoomEventHandler = new ZoomEventHandler();
            FPathEventHandler = new PathEventHandler(this);
            FEventPassThrougHandler = new EventPassThrougHandler(this);
            
            //add custom event handlers
            FCanvas.AddInputEventListener(FDragDropEventHandler);
            FCanvas.AddInputEventListener(FMyPanEventHandler);
            FCanvas.AddInputEventListener(FMyZoomEventHandler);
            FCanvas.AddInputEventListener(FPathEventHandler);
            FCanvas.AddInputEventListener(FEventPassThrougHandler);
            
            FCanvas.KeyPress += FCanvas_KeyPress;
            FCanvas.KeyDown += FCanvas_KeyDown;
            FCanvas.KeyUp += FCanvas_KeyUp;

            FCanvas.MinimumSize = new Size(10, 10);
        }
        
        public void EndSelectionDrag()
        {
            if (Host != null)
            {
                Host.MoveSelected();
            }
        }
        
        #region canvas input events
        internal void FCanvas_KeyPress(object sender, KeyPressEventArgs e)
        {
            OnKeyPress(e);
        }
        
        internal void FCanvas_KeyDown(object sender, KeyEventArgs e)
        {
            if(Host != null)
            {
                if (e.KeyData == Keys.Back || e.KeyData == Keys.Delete)
                {
                    Host.DeleteSelected();
                }
                else if (e.KeyData == (Keys.Control | Keys.Z))
                {
                    Host.Undo();
                }
                else if (e.KeyData == (Keys.Control | Keys.Shift | Keys.Z))
                {
                    Host.Redo();
                }
            }
            
            OnKeyDown(e);
        }

        internal void FCanvas_KeyUp(object sender, KeyEventArgs e)
        {
            OnKeyUp(e);
        }
        
        internal void FCanvas_MouseClick(object sender, MouseEventArgs e)
        {
            FCanvas.Select();
            OnMouseClick(e);
        }
        
        internal void FCanvas_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            OnMouseDoubleClick(e);
        }
        
        internal void FCanvas_MouseUp(object sender, MouseEventArgs e)
        {
            OnMouseUp(e);
        }

        internal void FCanvas_MouseDown(object sender, MouseEventArgs e)
        {
            OnMouseDown(e);
        }
        
        #endregion canvas input events

        #region link drawing
        internal Solid GetConnectionCandidate(PointF position, IConnectable startingConnectable)
        {
            GraphElement target = null;
            IConnectable targetConnectable = null;

            foreach (GraphElement e in FConnectables)
                if (e is Solid &&
                    ((startingConnectable==null) || e.Connectable.CanConnectTo(startingConnectable)) &&
                    e.Connectable.CommitAsConnectionCandidate(position, targetConnectable))
            {
                target = e;
                targetConnectable = target.Connectable;
            }

            return target as Solid;
        }
        
        internal void ShowAwaitingConnections(IConnectable aConnectable)
        {
            foreach (GraphElement e in FConnectables)
            {
                IConnectable c = e.Connectable;
                if ((aConnectable.ConnectableType != c.ConnectableType) && (c.CanConnectTo(aConnectable)))
                    c.AwaitingConnection = true;
            }
        }

        internal void NoAwaitingConnections()
        {
            foreach (GraphElement e in FConnectables)
            {
                IConnectable c = e.Connectable;
                c.AwaitingConnection = false;
            }
        }

        void OnCreateGraphElement(GraphElement addedNode)
        {
            if (addedNode.IsConnectable)
                FConnectables.Add(addedNode);
            //FSelectionEventHandler.VerifySelectableElements(addedNode);
        }
        #endregion link drawing

        #region ICanvas Members

        //dot
        public IDot CreateDot(IGraphElementHost host)
        {
            var e = new Dot(host);
            OnCreateGraphElement(e);
            return e;
        }

        //rectangle
        public IRectangle CreateRectangle(IGraphElementHost host)
        {
            var e = new Rectangle(host);
            OnCreateGraphElement(e);
            return e;
        }

        //polygon
        public IPolygon CreatePoly(IGraphElementHost host)
        {
            var e = new Poly(host);
            OnCreateGraphElement(e);
            return e;
        }

        //circle
        public ICircle CreateCircle(IGraphElementHost host)
        {
            var e = new Circle(host);
            OnCreateGraphElement(e);
            return e;
        }
        
        //text
        public IText CreateText(IGraphElementHost host, string caption)
        {
            var e = new Text(host, caption);
            OnCreateGraphElement(e);
            return e;
        }
        
        public void RemoveGraphElement(IGraphElement element)
        {
            if (element.IsConnectable)
            {
                FConnectables.Remove(element);
            }
        }

        //create a path with an already started temp path
        public IPath CreatePath(IPathHost host, ITempPath temppath, ISolid end)
        {
            var p = new LinkPath(host, temppath, end);
            if (p.Start.IsConnectable)
            {
                p.Start.Connectable.ConnectTo(p.End.Connectable, host);
            }
            if (p.End.IsConnectable)
            {
                p.End.Connectable.ConnectTo(p.Start.Connectable, host);
            }
            LinkRoot.Add(p);
            return p;
        }

        //create a path between two solids
        public IPath CreatePath(IPathHost host, ISolid start, ISolid end)
        {
            var p = new LinkPath(host, start, end);
            if (p.Start.IsConnectable)
            {
                p.Start.Connectable.ConnectTo(p.End.Connectable, host);
            }
            if (p.End.IsConnectable)
            {
                p.End.Connectable.ConnectTo(p.Start.Connectable, host);
            }
            LinkRoot.Add(p);
            return p;
        }
        
        //remove a path
        public void RemovePath(IPath path)
        {
            LinkRoot.Remove(path);

            if (path.Start.IsConnectable)
            {
                path.Start.Connectable.DisconnectFrom(path.End.Connectable, path.Host);
            }
            if (path.End.IsConnectable)
            {
                path.End.Connectable.DisconnectFrom(path.Start.Connectable, path.Host);
            }
            
            var disposablePath = path as IDisposable;
            if (disposablePath != null)
            {
                disposablePath.Dispose();
            }
        }

        //root element, parent of all solids
        protected IGraphElement FRoot;
        public IGraphElement Root
        {
            get { return FRoot; }
            set
            {
                FRoot = value;
                
                //set the layer as parent of the root
                ((GraphElement)FRoot).FPiccoloParent = SolidLayer;
                SolidLayer.AddChild(((GraphElement)FRoot).PNode);
                
                if (FSelectionEventHandler != null)
                    FCanvas.RemoveInputEventListener(FSelectionEventHandler);
                
                FSelectionEventHandler = new SelectionEventHandler(this, ((GraphElement)FRoot).PNode, ((GraphElement)FRoot).PNode);
                FCanvas.AddInputEventListener(FSelectionEventHandler);
            }
        }
        
        //link root element, parent of all links
        protected IGraphElement FLinkRoot;
        public IGraphElement LinkRoot
        {
            get { return FLinkRoot; }
            set
            {
                FLinkRoot = value;
                
                //set the layer as parent of the root
                ((GraphElement)FLinkRoot).FPiccoloParent = LinkLayer;
                LinkLayer.AddChild(((GraphElement)FLinkRoot).PNode);
            }
        }

        private ICanvasHost FHost;
        public ICanvasHost Host
        {
            get
            {
                return FHost;
            }
            set
            {
                if (value != Host)
                {
                    FHost = value;
                    FDragDropEventHandler.Host = value;
                }
            }
        }
        
        public Color Color
        {
            get
            {
                return FCanvas.BackColor;
            }
            set
            {
                FCanvas.BackColor = value;
            }
        }
        
        public PointF ViewCenter
        {
            get
            {
                return FCanvas.Camera.ViewBounds.GetCenter();
            }
            set
            {
                var vs = FCanvas.Camera.ViewBounds.Size;
                var vb = new RectangleF();
                vb.X = value.X - 0.5f * vs.Width;
                vb.Y = value.Y - 0.5f * vs.Height;
                vb.Size = vs;
                FCanvas.Camera.ViewBounds = vb;
                FCanvas.Invalidate();
            }
        }
        
        public SizeF ViewSize
        {
            get
            {
                return FCanvas.Camera.ViewBounds.Size;
            }
            set
            {
                var vs = FCanvas.Camera.ViewBounds.Size;
                var vp = FCanvas.Camera.ViewBounds.Location;
                var offX = (value.Width - vs.Width) * 0.5f;
                var offY = (value.Height - vs.Height) * 0.5f;
                var vb = new RectangleF();
                vb.X = vp.X - offX;
                vb.Y = vp.Y - offY;
                vb.Size = value;
                FCanvas.Camera.ViewBounds = vb;
                FCanvas.Invalidate();
            }
        }
        
        public RectangleF ContentBounds
        {
            get
            {
                return SolidLayer.FullBounds;
            }
        }
        
        public PointF ContentCenter
        {
            get
            {
                return SolidLayer.FullBounds.GetCenter();
            }
        }
        
        public SizeF ContentSize
        {
            get
            {
                return SolidLayer.FullBounds.Size;
            }
        }
        
        public void Clear()
        {
            Root.Clear();
            LinkRoot.Clear();
        }
        
        void ICanvas.Invalidate()
        {
            FCanvas.Invalidate();
        }
        
        void CanvasToControl(ref Point p)
        {
            p.X = (int) ((p.X - FCanvas.Camera.ViewBounds.X) * FCanvas.Camera.ViewScale);
            p.Y = (int) ((p.Y - FCanvas.Camera.ViewBounds.Y) * FCanvas.Camera.ViewScale);
        }
        
        public void ShowToolTip(Point tipPoint, string tip, bool center)
        {
            CanvasToControl(ref tipPoint);
            if (center)
            {
                using (var g = CreateGraphics())
                {
                    var width = (int) g.MeasureString(tip, FCanvas.Font).Width;
                    tipPoint.X -= width/2;
                }
            }
            
            tipPoint.Y += 20;
            FToolTip.Show(tip, this, tipPoint);
        }
        
        public void HideToolTip()
        {
            FToolTip.Hide(this);
        }

        #endregion ICanvas Members
    }


}
