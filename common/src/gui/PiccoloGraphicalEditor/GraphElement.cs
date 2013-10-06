﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

using UMD.HCIL.Piccolo;
using UMD.HCIL.Piccolo.Util;
using UMD.HCIL.Piccolo.Nodes;
using UMD.HCIL.Piccolo.Event;

using VVVV.Core.Viewer.GraphicalEditor;
using VVVV.Core.View.GraphicalEditor;
using VVVV.Core.View;
using VVVV.Core.Collections;
using VVVV.Core;

namespace VVVV.HDE.GraphicalEditing
{
    public abstract class GraphElement : EditableCollection<IGraphElement>, IGraphElement
    {
    	protected IGraphElementHost FHost;
    	protected bool FIsVisible = true;
    	protected Brush FBrush = Brushes.Black;
    	protected Pen FPen = Pens.Black;
        
        #region functional interfaces
        
        public bool IsSelectable
        {
            get;
            private set;
        }

        public ISelectable Selectable
        {
            get;
            private set;            
        }
        
        public bool IsMovable
        {
            get;
            private set;
        }

        public IMovable Movable
        {
            get;
            private set;
        }

        public bool IsConnectable
        {
            get;
            private set;
        }

        public IConnectable Connectable
        {
            get;
            private set;
        }

        public bool IsHoverable
        {
            get;
            private set;
        }

        public IHoverable Hoverable
        {
            get;
            private set;
        }
        
        public bool IsClickable
        {
        	get;
        	private set;
        }
        
        public IClickable Clickable
        {
        	get;
        	private set;
        }
        
        #endregion functional interfaces

        #region parents
        //graph parent
        protected IGraphElement FParent;
        public IGraphElement Parent
        {
            get { return FParent; }
            set
            {
                if (FParent != value)
                {
                    if (value == null)
                        PiccoloParent = null;

                    FParent = value;

                    if (value != null)
                        PiccoloParent = (Parent as GraphElement).PNode;
                }
            }
        }

        //piccolo parent
        internal PNode FPiccoloParent;
        public PNode PiccoloParent
        {            
            get { return FPiccoloParent; }
            set
            {
                if (FPiccoloParent != value)
                {
                    if (FPiccoloParent != null)
                    {                        
                        FPiccoloParent.RemoveChild(PNode);                      
                    }

                    if (Parent != null)
                    {
                        FPiccoloParent = value;

                        if (FPiccoloParent != null)
                        {
                            FPiccoloParent.AddChild(PNode);
                        }
                    }
                }
            }
        }
        #endregion parents
        
        #region graphical parameters
        public PNode PNode
        {
        	get;
        	private set;
        }

        public PPath PPath
        {
        	get
        	{
        		return PNode as PPath;
        	}
        }
        
        public virtual Brush Brush
        {
            get
            {
                return FBrush;
            }
            set
            {
               FBrush = value;
               SetVisibility();
            }
        }
        
        public virtual Pen Pen
        {
        	get
        	{
        		return FPen;
        	}
        	set
        	{
        		FPen = value;
        		SetVisibility();
        	}
        }
        
        //set the actual visibility
        protected void SetVisibility()
        {
        	if(FIsVisible)
        	{
        		PNode.Brush = FBrush;
        		if(PPath != null) PPath.Pen = FPen;
        		if(PNode is CombinedText) (PNode as CombinedText).Pen = FPen;
        	}
        	else
        	{
        		PNode.Brush = null;
        		if(PPath != null) PPath.Pen = null;
        		if(PNode is CombinedText) (PNode as CombinedText).Pen = null;
        	}
        	
        	//force redraw
        	PNode.Visible = false;
        	PNode.Visible = true;
        }

        public bool Visible
        {
            get
            {
                return FIsVisible;
            }
            set
            {
                FIsVisible = value;
                SetVisibility();
            }
        }
        
        public void BringToFront()
        {
            PNode.MoveToFront();
            foreach (var item in this)
                item.BringToFront();
        }
        #endregion graphical parameters

        /// <summary>
        /// Creates the piccolo node
        /// </summary>
        /// <returns>Piccolo node specific for this graph element</returns>
        protected abstract PNode CreatePNode();

        public GraphElement(IGraphElementHost host)
        {
            PNode = CreatePNode();
            PNode.Tag = this;
            FHost = host;
                     
            PNode.Brush = Brushes.Black;

            IsSelectable = (host is ISelectable);
            IsMovable = (host is IMovable);
            IsConnectable = (host is IConnectable);
            IsHoverable = (host is IHoverable);
            IsClickable = (host is IClickable);

            Selectable = (host as ISelectable);
            Movable = (host as IMovable);
            Connectable = (host as IConnectable);
            Hoverable = (host as IHoverable);
            Clickable = (host as IClickable);

            //enable piccolo input events?
            PNode.Pickable = IsSelectable || IsMovable || IsConnectable || IsHoverable || IsClickable;
            
            if (IsMovable)
            {
            	PNode.TransformChanged += BoundsChanged;
            }         
            
            if (IsHoverable)
            {
                PNode.MouseEnter += new PInputEventHandler(GraphElement_MouseEnter);
                PNode.MouseLeave += new PInputEventHandler(GraphElement_MouseLeave);
                PNode.MouseMove += new PInputEventHandler(GraphElement_MouseMove);
            }
            
            if(IsClickable)
            {
            	PNode.Click += new PInputEventHandler(PNode_Click);
            	PNode.DoubleClick += new PInputEventHandler(PNode_DoubleClick);
            	PNode.MouseDown += new PInputEventHandler(PNode_MouseDown);
            	PNode.MouseUp += new PInputEventHandler(PNode_MouseUp);
            }
       }

        #region clickable
        
        public PInputEventArgs LastEventArgs;
        
        //check if child nodes have processed this event already
        protected bool PreventEvent(PInputEventArgs e)
        {
        	//set this event to parent
        	var p = Parent as GraphElement;
        	if(p != null) p.LastEventArgs = e;
        	
        	if(LastEventArgs == e) return true;
        	else return false;
        }
        
        protected Point FMouseDownPos;
        void PNode_Click(object sender, PInputEventArgs e)
        {
        	if(PreventEvent(e)) return;
        	
        	//prevent click when mouse was dragged
        	if(Cursor.Position.GetDistanceTo(FMouseDownPos) < 3)
        		Clickable.Click(e.Position, GetButton(e));
        }
        
        void PNode_DoubleClick(object sender, PInputEventArgs e)
        {
        	if(PreventEvent(e)) return;
        	Clickable.DoubleClick(e.Position, GetButton(e));
        }
        
        void PNode_MouseDown(object sender, PInputEventArgs e)
        {
        	FMouseDownPos = Cursor.Position;
        	if(PreventEvent(e)) return;
        	Clickable.MouseDown(e.Position, GetButton(e));
        }
        
        void PNode_MouseUp(object sender, PInputEventArgs e)
        {
        	if(PreventEvent(e)) return;
        	Clickable.MouseUp(e.Position, GetButton(e));
        }
        
        protected Mouse_Buttons GetButton(PInputEventArgs e)
        {
        	Mouse_Buttons mb;
        	
        	switch(e.Button)
        	{
        		case MouseButtons.Right: mb = Mouse_Buttons.Right; break;
        		case MouseButtons.Middle: mb = Mouse_Buttons.Middle; break;
        		default: mb = Mouse_Buttons.Left; break;
        	}	
        	
        	return mb;
        }

        #endregion clickable
        
        #region hoverable
        DateTime FMouseEnterTime;
        void GraphElement_MouseEnter(object sender, PInputEventArgs e)
        {
        	if(PreventEvent(e)) return;
            FMouseEnterTime = DateTime.Now;    
            Hoverable.MouseEnter(e.Position);
        }

        void GraphElement_MouseMove(object sender, PInputEventArgs e)
        {
        	if(PreventEvent(e)) return;
        	Hoverable.MouseHover(e.Position);
        }

        void GraphElement_MouseLeave(object sender, PInputEventArgs e)
        {
        	if(PreventEvent(e)) return;
            Hoverable.MouseLeave(e.Position, DateTime.Now - FMouseEnterTime);
        }
        #endregion hoverable

        #region moveable
        protected bool FMoving;

        protected virtual void BoundsChanged(object sender, PPropertyEventArgs e)
        {
        	if (!FMoving)
                Movable.UpdateBounds(((PNode)sender).Bounds);
        }
        #endregion moveable
        
        #region child bounds
        public RectangleF ContentBounds
        {
        	get
        	{
        		return GetFullBounds();
        	}
        }
        
        public PointF ContentCenter
        {
        	get
        	{
        		return GetFullBounds().GetCenter();
        	}
        }
        
        public SizeF ContentSize
        {
        	get
        	{
        		return GetFullBounds().Size;
        	}
        }
        
        protected RectangleF GetFullBounds()
        {
        	var b = new RectangleF();
        		
        	if(PNode != null) b = PNode.GlobalBounds;
        	
        	var top = b.Top;
        	var bot = b.Bottom;
        	var left = b.Left;
        	var right = b.Right;
        	
        	foreach(var g in this)
        	{
        		var gb = g.ContentBounds;

        		top = Math.Min(top, gb.Top);
        		bot = Math.Max(bot, gb.Bottom);
        		left = Math.Min(left, gb.Left);
        		right = Math.Max(right, gb.Right);
        	}
        	
        	return new RectangleF(new PointF(left, top), new SizeF(right-left, bot-top));
        }
        #endregion child bounds
        
        #region collection method overrides
        protected override void AddToInternalCollection(IGraphElement item)
        {
            base.AddToInternalCollection(item);
            ((GraphElement)item).Parent = this;
        }

        protected override bool RemoveFromInternalCollection(IGraphElement item)
        {
            ((GraphElement)item).Parent = null;
            return base.RemoveFromInternalCollection(item);
        }
        
		protected override void ClearInternalCollection()
		{
			foreach(var e in this)
				((GraphElement)e).Parent = null;
			
			base.ClearInternalCollection();
		}
		#endregion collection method overrides
    }
}
