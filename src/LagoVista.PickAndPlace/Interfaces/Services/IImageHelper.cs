﻿using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Models;
using System;
using System.Drawing;

namespace LagoVista.PickAndPlace.Interfaces.Services
{
    public interface IImageHelper<T> where T: class, IDisposable
    {
        void Circle(IMVImage<T> img, double zoomLevel, MVLocatedCircle circle, Size size, int thickness = 1);
        void Circle(IMVImage<T> img, int x, int y, int radius, System.Drawing.Color color, int thickness = 1);
        void Line(IMVImage<T> img, int x1, int y1, int x2, int y2, System.Drawing.Color color, int thickness = 1);
        void DrawRect(IMVImage<T> img, double zoomLevel, MVLocatedRectangle p, System.Drawing.Color color);
        void ShowCalibrationSquare(IMVImage<T> destImage, System.Drawing.Size size);
        void DrawCrossHairs(IMVImage<T> destImage, MachineCamera camera, System.Drawing.Size size);
    }
}
