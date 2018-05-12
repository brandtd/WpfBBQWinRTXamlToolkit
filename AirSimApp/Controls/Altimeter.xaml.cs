﻿#region MIT License (c) 2018 Dan Brandt

// Copyright 2018 Dan Brandt
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and
// associated documentation files (the "Software"), to deal in the Software without restriction,
// including without limitation the rights to use, copy, modify, merge, publish, distribute,
// sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT
// NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT
// OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

#endregion MIT License (c) 2018 Dan Brandt

using DotSpatial.Positioning;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;

namespace AirSimApp.Controls
{
    /// <summary>Interaction logic for Altimeter.xaml</summary>
    public partial class Altimeter : UserControl
    {
        /// <summary>Vehicle's actual altitude.</summary>
        public static readonly DependencyProperty ActualAltitudeProperty =
            DependencyProperty.Register(
                nameof(ActualAltitude),
                typeof(Distance),
                typeof(Altimeter),
                new PropertyMetadata(Distance.Invalid, onActualAltitudeChanged));

        /// <summary>Vehicle's commanded altitude.</summary>
        public static readonly DependencyProperty CommandedAltitudeProperty =
            DependencyProperty.Register(
                nameof(CommandedAltitude),
                typeof(Distance),
                typeof(Altimeter),
                new PropertyMetadata(Distance.Invalid));

        public Altimeter()
        {
            InitializeComponent();
        }

        /// <inheritdoc cref="ActualAltitudeProperty" />
        public Distance ActualAltitude
        {
            get => (Distance)GetValue(ActualAltitudeProperty);
            set => SetValue(ActualAltitudeProperty, value);
        }

        /// <inheritdoc cref="CommandedAltitudeProperty" />
        public Distance CommandedAltitude
        {
            get => (Distance)GetValue(CommandedAltitudeProperty);
            set => SetValue(CommandedAltitudeProperty, value);
        }

        private static void onActualAltitudeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            setActualAltitude((Altimeter)d);
        }

        private static void setActualAltitude(Altimeter control)
        {
            if (control.ActualAltitude.IsInvalid)
            {
                control.Digit0.Digit = double.NaN;
                control.Digit1.Digit = double.NaN;
                control.Digit2.Digit = double.NaN;
                control.Digit3.Digit = double.NaN;
                control.Digit4.Digit = double.NaN;
            }
            else
            {
                double altitude = control.ActualAltitude.Value;

                double digit0 = Mod.CanonicalModulo(altitude, 10);
                double digit1 = Mod.CanonicalModulo(altitude, 100) / 10;
                double digit2 = Mod.CanonicalModulo(altitude, 1000) / 100;
                double digit3 = Mod.CanonicalModulo(altitude, 10000) / 1000;
                double digit4 = Mod.CanonicalModulo(altitude, 100000) / 10000;

                control.Digit0.Digit = digit0;
                control.Digit1.Digit = digit1;
                control.Digit2.Digit = altitude > 100 ? digit2 : double.NaN;
                control.Digit3.Digit = altitude > 1000 ? digit3 : double.NaN;
                control.Digit4.Digit = altitude > 10000 ? digit4 : double.NaN;
            }
        }
    }
}