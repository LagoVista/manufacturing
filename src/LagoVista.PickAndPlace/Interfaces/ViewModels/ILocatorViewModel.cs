﻿using Emgu.CV.Structure;
using LagoVista.Core.Models.Drawing;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.PickAndPlace.Interfaces.ViewModels
{
    public enum MVLocatorState
    {
        Idle,
        MachineFidicual,
        BoardFidicual1,
        BoardFidicual2,
        Default,
        NozzleCalibration,
        WorkHome,
        PartInTape,
    }

    public enum LocatedByCamera
    {
        Position,
        PartInspection
    }

    public interface ILocatorViewModel
    {
        MVLocatorState LocatorState { get; }
        string Status { get; }
        void SetLocatorState(MVLocatorState state);

        void RectLocated(RotatedRect rect, LocatedByCamera camera, Point2D<double> stdDeviation) { }
        void RectCentered(RotatedRect rect, LocatedByCamera camera, Point2D<double> stdDeviation) { }


        void CornerLocated(Point2D<double> point, LocatedByCamera camera, Point2D<double> stdDeviation) { }
        void CornerCentered(Point2D<double> point, LocatedByCamera camera, Point2D<double> stdDeviation) { }


        void CircleLocated(Point2D<double> point, LocatedByCamera camera, double diameter, Point2D<double> stdDeviation) { }
        void CircleCentered(Point2D<double> point, LocatedByCamera camera, double diameter) { }

    }
}
