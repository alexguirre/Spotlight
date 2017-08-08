namespace Spotlight
{
    // System
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Drawing;

    // RPH
    using Rage;
    using Rage.Native;

    using SpotlightControllers;

    using Engine.Memory;

    using static Intrin;

    internal class VehicleSpotlight : ISpotlight
    {
        public Vehicle Vehicle { get; }

        public SpotlightData Data { get; }

        public Vector3 Offset { get; }
        public Vector3 Position { get; private set; }
        public Rotator RelativeRotation { get; set; }
        public Vector3 Direction { get; private set; }

        public bool IsActive { get; set; }

        public bool IsTrackingPed { get { return TrackedPed.Exists(); } }
        public Ped TrackedPed { get; set; }

        public bool IsTrackingVehicle { get { return TrackedVehicle.Exists(); } }
        public Vehicle TrackedVehicle { get; set; }
        
        public bool IsCurrentPlayerVehicleSpotlight { get { return Vehicle == Game.LocalPlayer.Character.CurrentVehicle; } }

        public VehicleSpotlight(Vehicle vehicle)
        {
            Vehicle = vehicle;
            Offset = GetOffsetForModel(vehicle.Model);
            Data = GetSpotlightDataForModel(vehicle.Model);

            if (vehicle.Model.IsHelicopter)
                RelativeRotation = new Rotator(-50.0f, 0.0f, 0.0f);

            Game.FrameRender += OnFrameRender;
        }

        ~VehicleSpotlight()
        {
            Game.FrameRender -= OnFrameRender;
        }

        public void Update(IList<SpotlightController> controllers)
        {
            if (!IsActive)
                return;
            
            Position = Vehicle.GetOffsetPosition(Offset);

            bool isCurrentPlayerVehicleSpotlight = IsCurrentPlayerVehicleSpotlight;

            if (isCurrentPlayerVehicleSpotlight)
            {
                for (int i = 0; i < controllers.Count; i++)
                {
                    controllers[i].UpdateControls(this);
                }
            }

            if (IsTrackingVehicle)
            {
                Direction = (TrackedVehicle.Position - Position).ToNormalized();
            }
            else if (IsTrackingPed)
            {
                Direction = (TrackedPed.Position - Position).ToNormalized();
            }
            else
            {
                if (isCurrentPlayerVehicleSpotlight)
                {
                    for (int i = 0; i < controllers.Count; i++)
                    {
                        if (controllers[i].GetUpdatedRotationDelta(this, out Rotator newRotDelta))
                        {
                            RelativeRotation += newRotDelta;
                            break;
                        }
                    }
                }
                Direction = (Vehicle.Rotation + RelativeRotation).ToVector();
            }

            Draw();
        }


        private Vector3 GetOffsetForModel(Model model)
        {
            if (Plugin.Settings.SpotlightOffsets.TryGetValue(model, out Vector3 o))
                return o;
            Game.LogTrivial("No spotlight offset position loaded for model: " + model.Name);
            Game.LogTrivial("Using default values");
            return new Vector3(-0.8f, 1.17f, 0.52f);
        }

        private SpotlightData GetSpotlightDataForModel(Model model)
        {
            return model.IsCar ? Plugin.Settings.CarsSpotlightData :
                   model.IsBoat ? Plugin.Settings.BoatsSpotlightData :
                   model.IsHelicopter ? Plugin.Settings.HelicoptersSpotlightData : new SpotlightData();
        }


        private unsafe void Draw()
        {
            if (!IsActive)
                return;

            // TODO: add shadows
            CLightDrawData* drawData = CLightDrawData.New(eLightType.SPOT_LIGHT, eLightFlags.VolumeConeVisible, Position, Data.Color, Data.Brightness);
            NativeVector3 dir = Direction;
            drawData->Range = Data.Distance;
            drawData->VolumeIntensity = 0.3f;
            drawData->VolumeExponent = 70.0f;
            drawData->VolumeSize = 0.1f;
            drawData->FalloffExponent = Data.Falloff;

            // no idea how this works, copied from a game function
            // not event sure if these functions are the exact equivalents
            // but at least it works :P
            NativeVector3 v16 = _mm_andnot_ps(new Vector3(-0.0f, -0.0f, -0.0f), dir);
            NativeVector3 v17 = _mm_and_ps(_mm_cmple_ps(v16, _mm_shuffle_epi32(v16, -46)), _mm_cmplt_ps(v16, _mm_shuffle_epi32(v16, -55)));
            NativeVector3 v18 = _mm_and_ps(
                                    _mm_or_ps(
                                      _mm_andnot_ps(
                                        _mm_or_ps(
                                          _mm_or_ps(_mm_shuffle_epi32(v17, 85), _mm_shuffle_epi32(v17, 0)),
                                          _mm_shuffle_epi32(v17, -86)),
                                          new Vector3(Single.NaN, Single.NaN, Single.NaN)),
                                      v17),
                                    new Vector3(1.0f, 1.0f, 1.0f));
            v17 = _mm_shuffle_ps(v18, v18, 85);
            NativeVector3 v19 = _mm_shuffle_ps(v18, v18, -86);

            Vector3 v = new Vector3();
            v.X = (v19.X * dir.Y) - (v17.X * dir.Z);
            v.Y = (v18.X * dir.Z) - (v19.X * dir.X);
            v.Z = (v17.X * dir.X) - (v18.X * dir.Y);
            NativeVector3 u = v.ToNormalized();

            GameFunctions.SetLightDrawDataDirection(drawData, &dir, &u);
            GameFunctions.SetLightDrawDataRoundnessAndRadius(drawData, Data.Roundness, Data.Radius);

            // wtf? why calling the wrapper method Utility.DrawCorona crashes, but calling it directly it doesn't?
            // and apparently, now I can call it from a normal gamefiber too, no need for the FrameRender
            //
            // and it stopped working again... :‑|
            //
            // well, calling it from the FrameRender now...
            //NativeVector3 p = Position;
            //NativeVector3 d = Direction;
            //GameFunctions.DrawCorona(CCoronaDrawQueue.GetInstance(), &p, 2.25f, 0xFFFFFFFF, 80.0f, 100.0f, &d, 1.0f, 0.0f, Data.Radius, 3);
        }


        private unsafe void OnFrameRender(object sender, GraphicsEventArgs e)
        {
            if (!IsActive)
                return;
            NativeVector3 p = Position;
            NativeVector3 d = Direction;
            GameFunctions.DrawCorona(CCoronaDrawQueue.GetInstance(), &p, 1.5f, unchecked((uint)Data.Color.ToArgb()), Data.Brightness * 0.625f, 100f, &d, 1.0f, 0.0f, Data.Radius, 3);
        }
    }
}
