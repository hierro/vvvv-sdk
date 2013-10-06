﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

using UMD.HCIL.Piccolo;
using UMD.HCIL.Piccolo.Nodes;
using UMD.HCIL.Piccolo.Util;
using VVVV.Core;
using VVVV.Core.Collections;
using VVVV.Core.View.GraphicalEditor;
using VVVV.Core.Viewer.GraphicalEditor;

namespace VVVV.HDE.GraphicalEditing
{
	public class Poly : Solid, IPolygon
	{
		protected EditableList<PointF> FPoints;
		protected PPath FPolygon;
		
		public Poly(IGraphElementHost host)
			: base(host)
		{
			Position = new PointF(0, 0);
			FPoints = new EditableList<PointF>();
			FPoints.Added += new CollectionDelegate<PointF>(PointsChanged);
			FPoints.Removed += new CollectionDelegate<PointF>(PointsChanged);
			IsClosed = false;
		}

		protected void PointsChanged(IViewableCollection<PointF> collection, PointF item)
		{
			Rebuild();
		}
		
		public EditableList<PointF> Points 
		{
			get
			{
				return FPoints;
			}
		}
		
		protected bool FIsClosed;
		public bool IsClosed
		{
			get
			{
				return FIsClosed;
			}
			set
			{
				if(FIsClosed != value)
				{
					FIsClosed = value;
					Rebuild();
				}
			}
		}
		
		protected void Rebuild()
		{
			if(IsClosed) //solid
			{
				if (FPoints.Count > 2)
				{
					PPath.Reset();
					PPath.AddPolygon(FPoints.ToArray());
				}
			}
			else //line strip
			{
				if (FPoints.Count > 1)
				{
					PPath.Reset();
					
					for(int i=0; i<FPoints.Count-1; i++)
					{
						PPath.AddLine(FPoints[i].X, FPoints[i].Y, FPoints[i+1].X, FPoints[i+1].Y);
					}
				}	
			}
		}
	}
}
