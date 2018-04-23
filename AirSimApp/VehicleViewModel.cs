﻿#region MIT License

// Copyright 2018 Dan Brandt
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal in
// the Software without restriction, including without limitation the rights to
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
// of the Software, and to permit persons to whom the Software is furnished to do
// so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

#endregion

using AirSimRpc;
using DotSpatial.Positioning;
using MsgPackRpc;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace AirSimApp
{
    /// <summary>
    /// View model for a vehicle.
    /// </summary>
    public class VehicleViewModel : PropertyChangedBase, IDisposable
    {
        private readonly ProxyController _controller;

        private bool _disposed = false;
        private CancellationTokenSource _cancellationTokenSource;

        private bool _isApiControlEnabled;
        private Distance _homeAltitude;
        private Latitude _homeLatitude;
        private Longitude _homeLongitude;
        private Distance _vehicleAltitude;
        private Latitude _vehicleLatitude;
        private Longitude _vehicleLongitude;
        private Angle _vehicleRoll;
        private Angle _vehiclePitch;
        private Angle _vehicleYaw;
        private Distance _gpsAltitude;
        private Latitude _gpsLatitude;
        private Longitude _gpsLongitude;

        /// <summary>
        /// Are we able to control the vehicle via the API?
        /// </summary>
        public bool IsApiControlEnabled
        {
            get => _isApiControlEnabled;
            set
            {
                if (_isApiControlEnabled != value)
                {
                    _isApiControlEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Vehicle's GPS latitude.
        /// </summary>
        public Latitude GpsLatitude
        {
            get => _gpsLatitude;
            set
            {
                if (_gpsLatitude != value)
                {
                    _gpsLatitude = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Vehicle's GPS longitude.
        /// </summary>
        public Longitude GpsLongitude
        {
            get => _gpsLongitude;
            set
            {
                if (_gpsLongitude != value)
                {
                    _gpsLongitude = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Vehicle's GPS altitude.
        /// </summary>
        public Distance GpsAltitude
        {
            get => _gpsAltitude;
            set
            {
                if (_gpsAltitude != value)
                {
                    _gpsAltitude = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Vehicle latitude.
        /// </summary>
        public Latitude VehicleLatitude
        {
            get => _vehicleLatitude;
            set
            {
                if (_vehicleLatitude != value)
                {
                    _vehicleLatitude = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Vehicle longitude.
        /// </summary>
        public Longitude VehicleLongitude
        {
            get => _vehicleLongitude;
            set
            {
                if (_vehicleLongitude != value)
                {
                    _vehicleLongitude = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Vehicle altitude.
        /// </summary>
        public Distance VehicleAltitude
        {
            get => _vehicleAltitude;
            set
            {
                if (_vehicleAltitude != value)
                {
                    _vehicleAltitude = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Vehicle roll.
        /// </summary>
        public Angle VehicleRoll
        {
            get => _vehicleRoll;
            set
            {
                if (_vehicleRoll != value)
                {
                    _vehicleRoll = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Vehicle pitch.
        /// </summary>
        public Angle VehiclePitch
        {
            get => _vehiclePitch;
            set
            {
                if (_vehiclePitch != value)
                {
                    _vehiclePitch = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Vehicle yaw.
        /// </summary>
        public Angle VehicleYaw
        {
            get => _vehicleYaw;
            set
            {
                if (_vehicleYaw != value)
                {
                    _vehicleYaw = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Home point latitude.
        /// </summary>
        public Latitude HomeLatitude
        {
            get => _homeLatitude;
            set
            {
                if (_homeLatitude != value)
                {
                    _homeLatitude = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Home point longitude.
        /// </summary>
        public Longitude HomeLongitude
        {
            get => _homeLongitude;
            set
            {
                if (_homeLongitude != value)
                {
                    _homeLongitude = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Home point altitude.
        /// </summary>
        public Distance HomeAltitude
        {
            get => _homeAltitude;
            set
            {
                if (_homeAltitude != value)
                {
                    _homeAltitude = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Wire up view model.
        /// </summary>
        public VehicleViewModel(ProxyController controller)
        {
            _controller = controller;
            _controller.PropertyChanged += onControllerPropertyChanged;

            startOrStopStateLoop();
        }

        /// <inheritdoc cref="IDisposable.Dispose" />
        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                _controller.PropertyChanged -= onControllerPropertyChanged;

                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;
            }
        }

        private void onControllerPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_controller.Connected))
            {
                startOrStopStateLoop();
            }
        }

        private void startOrStopStateLoop()
        {
            if (_controller.Connected)
            {
                _cancellationTokenSource = new CancellationTokenSource();
                updateStateLoop(_cancellationTokenSource.Token);
            }
            else
            {
                _cancellationTokenSource?.Cancel();
                _cancellationTokenSource = null;
            }
        }

        private async void updateStateLoop(CancellationToken token)
        {
            while (true)
            {
                try
                {
                    token.ThrowIfCancellationRequested();
                    RpcResult<GeoPoint> homeLocation = await _controller.GetHomeGeoPointAsync();
                    if (homeLocation.Successful)
                    {
                        HomeLatitude = new Latitude(homeLocation.Value.Latitude);
                        HomeLongitude = new Longitude(homeLocation.Value.Longitude);
                        HomeAltitude = Distance.FromMeters(homeLocation.Value.Altitude);
                    }
                    else
                    {
                        if (!_controller.Connected)
                        {
                            return;
                        }
                    }

                    token.ThrowIfCancellationRequested();
                    RpcResult<Vector3R> vehicleLocation = await _controller.GetPositionAsync();
                    if (vehicleLocation.Successful)
                    {
                        Position3D homePosition = new Position3D(HomeLatitude, HomeLongitude, HomeAltitude);
                        CartesianPoint homeEcef = homePosition.ToCartesianPoint();
                        Distance vehicleX = homeEcef.X + Distance.FromMeters(vehicleLocation.Value.X);
                        Distance vehicleY = homeEcef.Y + Distance.FromMeters(vehicleLocation.Value.Y);
                        Distance vehicleZ = homeEcef.Z - Distance.FromMeters(vehicleLocation.Value.Z);
                        CartesianPoint vehicleEcef = new CartesianPoint(vehicleX, vehicleY, vehicleZ);
                        Position3D vehiclePosition = vehicleEcef.ToPosition3D();

                        VehicleLatitude = vehiclePosition.Latitude;
                        VehicleLongitude = vehiclePosition.Longitude;
                        VehicleAltitude = vehiclePosition.Altitude;
                    }
                    else
                    {
                        if (!_controller.Connected)
                        {
                            return;
                        }
                    }

                    token.ThrowIfCancellationRequested();
                    RpcResult<QuaternionR> vehicleOrientation = await _controller.GetOrientationAsync();
                    if (vehicleOrientation.Successful)
                    {
                        toEulerAngles(vehicleOrientation.Value, out double roll, out double pitch, out double yaw);
                        VehicleRoll = Angle.FromRadians(roll);
                        VehiclePitch = Angle.FromRadians(pitch);
                        VehicleYaw = Angle.FromRadians(yaw);
                    }
                    else
                    {
                        if (!_controller.Connected)
                        {
                            return;
                        }
                    }

                    token.ThrowIfCancellationRequested();
                    RpcResult<MultirotorState> state = await _controller.GetMultirotorStateAsync();
                    if (state.Successful)
                    {
                        GpsLatitude = new Latitude(state.Value.GpsLocation.Latitude);
                        GpsLongitude = new Longitude(state.Value.GpsLocation.Longitude);
                        GpsAltitude = Distance.FromMeters(state.Value.GpsLocation.Altitude);
                    }
                    else
                    {
                        if (!_controller.Connected)
                        {
                            return;
                        }
                    }

                    token.ThrowIfCancellationRequested();
                    RpcResult<bool> isApiControlEnabled = await _controller.GetIsApiControlEnabledAsync();
                    if (state.Successful)
                    {
                        GpsLatitude = new Latitude(state.Value.GpsLocation.Latitude);
                        GpsLongitude = new Longitude(state.Value.GpsLocation.Longitude);
                        GpsAltitude = Distance.FromMeters(state.Value.GpsLocation.Altitude);
                    }
                    else
                    {
                        if (!_controller.Connected)
                        {
                            return;
                        }
                    }

                    await Task.Delay(100, token);
                }
                catch (OperationCanceledException e)
                {
                    if (e.CancellationToken == token)
                    {
                        throw;
                    }
                }
            }
        }

        private void toEulerAngles(QuaternionR q, out double roll, out double pitch, out double yaw)
        {
            double ysqr = q.Y * q.Y;

            double t0 = 2.0 * (q.W + q.Y * q.Z);
            double t1 = 1.0 - 2.0 * (q.X * q.X + ysqr);
            roll = Math.Atan2(t0, t1);

            double t2 = 2.0 * (q.W * q.Y - q.Z * q.X);
            if (t2 > 1.0) { t2 = 1.0; }
            if (t2 < -1.0) { t2 = -1.0; }

            pitch = Math.Asin(t2);

            double t3 = 2.0 * (q.W * q.Z + q.X * q.Y);
            double t4 = 1.0 - 2.0 * (ysqr + q.Z * q.Z);
            yaw = Math.Atan2(t3, t4);
        }
    }
}