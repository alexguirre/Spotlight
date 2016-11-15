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

    internal class Spotlight
    {
        public Vehicle Vehicle { get; }

        public SpotlightData Data { get; }

        public Vector3 Offset { get; }

        public Vector3 Position { get; private set; }
        
        public Rotator RelativeRotation { get; set; }

        public Vector3 Direction { get; private set; }

        public bool IsActive { get; set; }

        private List<SpotlightController> controllers = new List<SpotlightController>();

        public Spotlight(Vehicle vehicle)
        {
            Vehicle = vehicle;
            Offset = GetOffsetForModel(vehicle.Model);
            Data = GetSpotlightDataForModel(vehicle.Model);
            SpotlightData d = Data; // currently color isn't saved in the XML file
            d.Color = Color.White;
            Data = d;

            if (vehicle.Model.IsHelicopter)
                RelativeRotation = new Rotator(-50.0f, 0.0f, 0.0f);
        }

        public void Update()
        {
            if (!IsActive)
                return;
            
            Position = Vehicle.GetOffsetPosition(Offset);

            for (int i = 0; i < controllers.Count; i++)
            {
                SpotlightController controller = controllers[i];

                Rotator newRotDelta;
                if (controller.GetUpdatedRotationDelta(out newRotDelta))
                {
                    RelativeRotation += newRotDelta;
                    break;
                }
            }

            Direction = (Vehicle.Rotation + RelativeRotation).ToVector();
            Utility.DrawSpotlight(Position, Direction, Data);
        }


        public void RegisterController<T>() where T : SpotlightController
        {
            if (controllers.Any(c => c.GetType() == typeof(T)))
                throw new InvalidOperationException("The controller of type " + typeof(T).Name + " is already registered on this " + nameof(Spotlight) + " instance.");

            T controller = (T)Activator.CreateInstance(typeof(T), this);
            controllers.Add(controller);
        }

        public void UnregisterController<T>() where T : SpotlightController
        {
            controllers.RemoveAll(c => c.GetType() == typeof(T));
        }


        private Vector3 GetOffsetForModel(Model model)
        {
            InitializationFile iniFile = Plugin.Settings.SpotlightOffsetsIniFile;
            string modelName = model.Name;

            float x = 0.0f, y = 0.0f, z = 0.0f;

            bool success = false;
            if (iniFile.DoesSectionExist(modelName))
            {
                if (iniFile.DoesKeyExist(modelName, "X") &&
                    iniFile.DoesKeyExist(modelName, "Y") &&
                    iniFile.DoesKeyExist(modelName, "Z"))
                {

                    x = iniFile.ReadSingle(modelName, "X", -0.8f);
                    y = iniFile.ReadSingle(modelName, "Y", 1.17f);
                    z = iniFile.ReadSingle(modelName, "Z", 0.52f);

                    Game.LogTrivial($"Spotlight offset position settings found and loaded for vehicle model: {modelName}");
                    success = true;
                }
            }

            if (!success)
            {
                Game.LogTrivial($"<WARNING> Spotlight offset position settings not found for vehicle model: {modelName}");
                Game.LogTrivial("Using default settings");
                x = -0.8f;
                y = 1.17f;
                z = 0.52f;
            }

            return new Vector3(x, y, z);
        }

        private SpotlightData GetSpotlightDataForModel(Model model)
        {
            return model.IsCar ? Plugin.Settings.CarsSpotlightData :
                   model.IsBoat ? Plugin.Settings.BoatsSpotlightData :
                   model.IsHelicopter ? Plugin.Settings.HelicoptersSpotlightData : new SpotlightData();
        }
    }
}
