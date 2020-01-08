namespace Spotlight
{
    using System;
    using System.IO;
    using System.Xml;
    using System.Xml.Schema;
    using System.Globalization;
    using System.Windows.Forms;
    using System.Xml.Serialization;
    using System.Collections.Generic;

    using Rage;

    using Spotlight.Core;

    internal sealed class Settings
    {
        public string GeneralSettingsFileName { get; }
        public string VehiclesSettingsFileName { get; }
        public string VisualSettingsFileName { get; }

        public InitializationFile GeneralSettingsIniFile { get; }

        public VehiclesSettings Vehicles { get; }
        public VisualSettings Visual { get; }

        public Keys EditorKey { get; }

        public bool EnableLightEmissives { get; } = true;

        internal Settings(string generalSettingsFileName, string vehiclesSettingsFileName, string visualSettingsFileName, bool generateDefaultsIfFileNotFound)
        {
            GeneralSettingsFileName = generalSettingsFileName;
            VehiclesSettingsFileName = vehiclesSettingsFileName;
            VisualSettingsFileName = visualSettingsFileName;

            if (generateDefaultsIfFileNotFound)
            {
                if (!File.Exists(generalSettingsFileName))
                {
                    Game.LogTrivial($"'{Path.GetFileName(generalSettingsFileName)}' file doesn't exists, creating default...");
                    CreateDefaultGeneralSettingsIniFile(generalSettingsFileName);
                }

                if (!File.Exists(vehiclesSettingsFileName))
                {
                    Game.LogTrivial($"'{Path.GetFileName(vehiclesSettingsFileName)}' file doesn't exists, creating default...");
                    CreateDefaultVehiclesSettingsFile(vehiclesSettingsFileName, true);
                }

                if (!File.Exists(visualSettingsFileName))
                {
                    Game.LogTrivial($"'{Path.GetFileName(visualSettingsFileName)}' file doesn't exists, creating default...");
                    CreateDefaultVisualSettingsXMLFile(visualSettingsFileName);
                }
            }

            Game.LogTrivial("Reading settings...");
            GeneralSettingsIniFile = new InitializationFile(generalSettingsFileName);
            Vehicles = ReadVehiclesSettings(vehiclesSettingsFileName);
            Visual = ReadVisualSettingsFromXMLFile(visualSettingsFileName);

            EditorKey = GeneralSettingsIniFile.ReadEnum<Keys>("Misc", "EditorKey", Keys.F11);
        }

        internal void UpdateVehicleSettings(IDictionary<string, Tuple<Vector3, bool>> settingsByModelName, bool saveToFile)
        {
            foreach (KeyValuePair<string, Tuple<Vector3, bool>> item in settingsByModelName)
            {
                if (Vehicles.Data.ContainsKey(item.Key))
                {
                    Vehicles.Data[item.Key].Offset = item.Value.Item1;
                    Vehicles.Data[item.Key].DisableTurret = item.Value.Item2;
                }
                else
                {
                    Vehicles.Data.Add(item.Key, new VehicleData(item.Value.Item1, item.Value.Item2));
                }
            }

            if (saveToFile)
            {
                if (Vehicles.IsLegacy)
                {
                    Vehicles.WriteLegacy(VehiclesSettingsFileName);
                }
                else
                {
                    XmlSerializer ser = new XmlSerializer(typeof(VehiclesSettings));
                    using (StreamWriter writer = new StreamWriter(VehiclesSettingsFileName, false))
                    {
                        ser.Serialize(writer, Vehicles);
                    }
                }
            }
        }

        private VehiclesSettings ReadVehiclesSettings(string fileName)
        {
            if (!File.Exists(fileName))
                throw new FileNotFoundException("", fileName);

            return ReadVehiclesSettingsFromIniFile(fileName);

            //if (fileName.EndsWith(".xml"))
            //{
            //    return ReadVehiclesSettingsFromXMLFile(fileName);
            //}
            //else if (fileName.EndsWith(".ini"))
            //{
            //    return ReadVehiclesSettingsFromIniFile(fileName);
            //}
            //return null;
        }

        private VehiclesSettings ReadVehiclesSettingsFromXMLFile(string fileName)
        {
            VehiclesSettings v;
            XmlSerializer ser = new XmlSerializer(typeof(VehiclesSettings));
            using (StreamReader reader = new StreamReader(fileName))
            {
                v = (VehiclesSettings)ser.Deserialize(reader);
            }

            return v;
        }

        private VehiclesSettings ReadVehiclesSettingsFromIniFile(string fileName)
        {
            VehiclesSettings v = new VehiclesSettings();
            v.ReadLegacy(fileName);
            return v;
        }

        private VisualSettings ReadVisualSettingsFromXMLFile(string fileName)
        {
            if (!File.Exists(fileName))
                throw new FileNotFoundException("", fileName);

            VisualSettings v;
            XmlSerializer ser = new XmlSerializer(typeof(VisualSettings));
            using (StreamReader reader = new StreamReader(fileName))
            {
                v = (VisualSettings)ser.Deserialize(reader);
            }

            return v;
        }


        #region Create Default Methods
        private void CreateDefaultGeneralSettingsIniFile(string fileName)
        {
            using (StreamWriter writer = new StreamWriter(fileName, false))
            {
                writer.Write(DefaultGeneralSettingsText);
            }
        }

        private void CreateDefaultVehiclesSettingsFile(string fileName, bool legacy)
        {
            VehiclesSettings v = new VehiclesSettings
            {
                Data = new Dictionary<string, VehicleData>
                {
                    { "POLICE",     new VehicleData(new XYZ(-0.8f, 1.17f, 0.45f)) },
                    { "POLICE2",    new VehicleData(new XYZ(-0.84f, 0.85f, 0.43f)) },
                    { "POLICE3",    new VehicleData(new XYZ(-0.84f, 0.78f, 0.5f), VehicleData.DefaultDisableTurret, 1) },
                    { "POLICE4",    new VehicleData(new XYZ(-0.8f, 1.17f, 0.45f)) },
                    { "POLICET",    new VehicleData(new XYZ(-1.1f, 1.37f, 0.94f)) },
                    { "RIOT",       new VehicleData(new XYZ(-1.18f, 1.65f, 1.55f)) },
                    { "FBI",        new VehicleData(new XYZ(-0.84f, 0.71f, 0.44f)) },
                    { "FBI2",       new VehicleData(new XYZ(-1.01f, 1.04f, 0.81f)) },
                    { "POLICEOLD1", new VehicleData(new XYZ(-0.95f, 0.71f, 0.75f)) },
                    { "POLICEOLD2", new VehicleData(new XYZ(-0.88f, 0.805f, 0.49f)) },
                    { "SHERIFF",    new VehicleData(new XYZ(-0.8f, 1.17f, 0.45f)) },
                    { "SHERIFF2",   new VehicleData(new XYZ(-0.92f, 1.16f, 0.925f)) },
                    { "PRANGER",    new VehicleData(new XYZ(-0.92f, 1.16f, 0.925f)) },
                    { "LGUARD",     new VehicleData(new XYZ(-1.01f, 1.04f, 0.81f)) },
                    { "POLMAV",     new VehicleData(new XYZ(0.0f, 0.0f, 0.0f)) },
                    { "BUZZARD",    new VehicleData(new XYZ(0.0f, 0.0f, 0.0f)) },
                    { "BUZZARD2",   new VehicleData(new XYZ(0.0f, 2.34f, -0.36f)) },
                    { "PREDATOR",   new VehicleData(new XYZ(0.0f, -0.43f, 1.77f)) },
                }
            };

            if (legacy)
            {
                File.WriteAllLines(fileName, new[]
                {
                    // TODO: explain requirements for turret movement and DisableTurret option
                    "",
                });
                v.WriteLegacy(fileName);
            }
            else
            {
                XmlSerializer ser = new XmlSerializer(typeof(VehiclesSettings));
                using (StreamWriter writer = new StreamWriter(fileName, false))
                {
                    ser.Serialize(writer, v);
                }
            }
        }

        private void CreateDefaultVisualSettingsXMLFile(string fileName)
        {
            VisualSettings v = new VisualSettings
            {
                Default = new SpotlightData(
                    color: new RGB(80, 80, 80),
                    castShadows: true,
                    outerAngle: 8.25f,
                    innerAngle: 5f,
                    intensity: 30f,
                    range: 45f,
                    falloff: 45f,
                    volumeIntensity: 0.06f,
                    volumeSize: 0.175f,
                    coronaIntensity: 20f,
                    coronaSize: 1.5f,
                    volume: true,
                    corona: true,
                    specular: true,
                    movementSpeed: 1),
                Helicopter = new SpotlightData(
                    color: new RGB(80, 80, 80),
                    castShadows: true,
                    outerAngle: 9f,
                    innerAngle: 6f,
                    intensity: 35f,
                    range: 230f,
                    falloff: 50f,
                    volumeIntensity: 0.05f,
                    volumeSize: 0.125f,
                    coronaIntensity: 20f,
                    coronaSize: 1.5f,
                    volume: true,
                    corona: true,
                    specular: true,
                    movementSpeed: 1),
                Boat = new SpotlightData(
                    color: new RGB(80, 80, 80),
                    castShadows: true,
                    outerAngle: 8.5f,
                    innerAngle: 5.5f,
                    intensity: 30f,
                    range: 80f,
                    falloff: 45f,
                    volumeIntensity: 0.05f,
                    volumeSize: 0.125f,
                    coronaIntensity: 20f,
                    coronaSize: 1.5f,
                    volume: true,
                    corona: true,
                    specular: true,
                    movementSpeed: 1),
            };

            XmlSerializer ser = new XmlSerializer(typeof(VisualSettings));
            using (StreamWriter writer = new StreamWriter(fileName, false))
            {
                ser.Serialize(writer, v);
            }
        }
        #endregion

        #region Default Values
        const string DefaultGeneralSettingsText = @"
[Misc]
; The key to open the in-game editor
EditorKey = F11 

[Controls] ; Settings to specify which controls are enabled
KeyboardControlsEnabled = true
ControllerControlsEnabled = true
MouseControlsEnabled = true


[Keyboard] ; Settings for the keyboard controls
; VALID KEYS: https://msdn.microsoft.com/en-us/library/system.windows.forms.keys(v=vs.110).aspx
Modifier = None
Toggle = I

Move Left = NumPad4
Move Right = NumPad6

Move Up = NumPad8
Move Down = NumPad2

ToggleTrackingKey = NumPad3
SearchModeKey = Decimal


[Controller] ; Settings for the controller controls
; VALID BUTTONS: http://docs.ragepluginhook.net/html/558BC34.htm
Modifier = LeftShoulder
Toggle = X

; Set it to LeftStick, RightStick or DPad, to select what to use
Method = LeftStick



[Mouse] ; Settings for the mouse controls
; VALID KEYS: https://msdn.microsoft.com/en-us/library/system.windows.forms.keys(v=vs.110).aspx
Modifier = LControlKey
Toggle = I
";
        #endregion
    }

    public sealed class VisualSettings : IXmlSerializable
    {
        public SpotlightData Default { get; set; }
        public SpotlightData Helicopter { get; set; }
        public SpotlightData Boat { get; set; }

        XmlSchema IXmlSerializable.GetSchema() => null;

        void IXmlSerializable.ReadXml(XmlReader reader)
        {
            reader.ReadStartElement();

            XmlAttributeOverrides attr = new XmlAttributeOverrides();

            attr.Add(typeof(SpotlightData), new XmlAttributes() { XmlType = new XmlTypeAttribute("Default") });
            Default = (SpotlightData)new XmlSerializer(typeof(SpotlightData), attr).Deserialize(reader);

            attr[typeof(SpotlightData)].XmlType = new XmlTypeAttribute("Helicopter");
            Helicopter = (SpotlightData)new XmlSerializer(typeof(SpotlightData), attr).Deserialize(reader);

            attr[typeof(SpotlightData)].XmlType = new XmlTypeAttribute("Boat");
            Boat = (SpotlightData)new XmlSerializer(typeof(SpotlightData), attr).Deserialize(reader);

            reader.ReadEndElement();
        }

        void IXmlSerializable.WriteXml(XmlWriter writer)
        {
            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("", "");

            XmlAttributeOverrides attr = new XmlAttributeOverrides();
            attr.Add(typeof(SpotlightData), new XmlAttributes());

            attr[typeof(SpotlightData)].XmlType = new XmlTypeAttribute("Default");
            new XmlSerializer(typeof(SpotlightData), attr).Serialize(writer, Default, ns);

            attr[typeof(SpotlightData)].XmlType = new XmlTypeAttribute("Helicopter");
            new XmlSerializer(typeof(SpotlightData), attr).Serialize(writer, Helicopter, ns);

            attr[typeof(SpotlightData)].XmlType = new XmlTypeAttribute("Boat");
            new XmlSerializer(typeof(SpotlightData), attr).Serialize(writer, Boat, ns);
        }
    }

    public sealed class VehiclesSettings : IXmlSerializable
    {
        public Dictionary<string, VehicleData> Data { get; set; }
        public bool IsLegacy { get; private set; }

        public void ReadLegacy(string iniFile)
        {
            IsLegacy = true;

            InitializationFile ini = new InitializationFile(iniFile);
            Data = new Dictionary<string, VehicleData>();

            string[] sections = ini.GetSectionNames();
            if (sections != null)
            {
                foreach (string modelName in sections)
                {
                    float x = VehicleData.DefaultOffsetX, y = VehicleData.DefaultOffsetY, z = VehicleData.DefaultOffsetZ;
                    bool disableTurret = VehicleData.DefaultDisableTurret;
                    int spotlightExtraLight = VehicleData.DefaultSpotlightExtraLight;

                    bool success = false;
                    System.Exception exc = null;
                    try
                    {
                        if (ini.DoesKeyExist(modelName, VehicleData.IniKeyX) &&
                            ini.DoesKeyExist(modelName, VehicleData.IniKeyY) &&
                            ini.DoesKeyExist(modelName, VehicleData.IniKeyZ))
                        {

                            x = ini.ReadSingle(modelName, VehicleData.IniKeyX, VehicleData.DefaultOffsetX);
                            y = ini.ReadSingle(modelName, VehicleData.IniKeyY, VehicleData.DefaultOffsetY);
                            z = ini.ReadSingle(modelName, VehicleData.IniKeyZ, VehicleData.DefaultOffsetZ);
                            if (ini.DoesKeyExist(modelName, VehicleData.IniKeyDisableTurret))
                            {
                                disableTurret = ini.ReadBoolean(modelName, VehicleData.IniKeyDisableTurret, VehicleData.DefaultDisableTurret);
                            }
                            if (ini.DoesKeyExist(modelName, VehicleData.IniKeySpotlightExtraLight))
                            {
                                spotlightExtraLight = ini.ReadInt32(modelName, VehicleData.IniKeySpotlightExtraLight, VehicleData.DefaultSpotlightExtraLight);
                                if (spotlightExtraLight <= VehicleData.DefaultSpotlightExtraLight || spotlightExtraLight > 4) // there's only four possible extralight_* bones
                                {
                                    spotlightExtraLight = VehicleData.DefaultSpotlightExtraLight;
                                }
                            }

                            success = true;
                        }
                    }
                    catch (System.Exception ex)
                    {
                        exc = ex;
                    }

                    if (!success)
                    {
                        if (exc != null)
                        {
                            Game.LogTrivial($"  <WARNING> Failed to load spotlight offset position settings for vehicle model: {modelName}");
                            Game.LogTrivial($"  <WARNING> {exc}");
                        }

                        x = VehicleData.DefaultOffsetX;
                        y = VehicleData.DefaultOffsetY;
                        z = VehicleData.DefaultOffsetZ;
                        disableTurret = VehicleData.DefaultDisableTurret;
                        spotlightExtraLight = VehicleData.DefaultSpotlightExtraLight;
                    }

                    Data.Add(modelName, new VehicleData(new XYZ(x, y, z), disableTurret, spotlightExtraLight));
                }
            }
        }

        public void WriteLegacy(string iniFile)
        {
            InitializationFile ini = new InitializationFile(iniFile, CultureInfo.InvariantCulture, false);
            foreach (KeyValuePair<string, VehicleData> item in Data)
            {
                // Write overloads don't use the format provider passed in the constructor, so use ToString instead
                ini.Write(item.Key, VehicleData.IniKeyX, item.Value.Offset.X.ToString(CultureInfo.InvariantCulture));
                ini.Write(item.Key, VehicleData.IniKeyY, item.Value.Offset.Y.ToString(CultureInfo.InvariantCulture));
                ini.Write(item.Key, VehicleData.IniKeyZ, item.Value.Offset.Z.ToString(CultureInfo.InvariantCulture));

                if (item.Value.DisableTurretSpecified)
                {
                    ini.Write(item.Key, VehicleData.IniKeyDisableTurret, item.Value.DisableTurret.ToString(CultureInfo.InvariantCulture));
                }
                else if (ini.DoesKeyExist(item.Key, VehicleData.IniKeyDisableTurret))
                {
                    ini.DeleteKey(item.Key, VehicleData.IniKeyDisableTurret);
                }
            }
            ini.Layout();

            //using (StreamWriter writer = new StreamWriter(iniFile, false))
            //{
            //    foreach (KeyValuePair<string, VehicleData> item in Data)
            //    {
            //        writer.WriteLine($"[{item.Key}]");
            //        writer.WriteLine($"{VehicleData.IniKeyX} = {item.Value.Offset.X.ToString(CultureInfo.InvariantCulture)}");
            //        writer.WriteLine($"{VehicleData.IniKeyY} = {item.Value.Offset.Y.ToString(CultureInfo.InvariantCulture)}");
            //        writer.WriteLine($"{VehicleData.IniKeyZ} = {item.Value.Offset.Z.ToString(CultureInfo.InvariantCulture)}");
            //        if (item.Value.DisableTurretSpecified)
            //        {
            //            writer.WriteLine($"{VehicleData.IniKeyDisableTurret} = {item.Value.DisableTurret.ToString(CultureInfo.InvariantCulture)}");
            //        }
            //        writer.WriteLine();
            //    }
            //}
        }

        XmlSchema IXmlSerializable.GetSchema() => null;

        void IXmlSerializable.ReadXml(XmlReader reader)
        {
            IsLegacy = false;

            XmlAttributeOverrides attr = new XmlAttributeOverrides();
            attr.Add(typeof(VehicleData), new XmlAttributes());

            Data = new Dictionary<string, VehicleData>();
            reader.ReadStartElement();
            while (reader.Depth > 0)
            {
                if (reader.Depth == 1)
                {
                    string name = reader.Name;
                    attr[typeof(VehicleData)].XmlType = new XmlTypeAttribute(name);
                    VehicleData data = (VehicleData)new XmlSerializer(typeof(VehicleData), attr).Deserialize(reader);
                    Data[name] = data;
                }
            }
            reader.ReadEndElement();
        }

        void IXmlSerializable.WriteXml(XmlWriter writer)
        {
            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("", "");

            XmlAttributeOverrides attr = new XmlAttributeOverrides();
            attr.Add(typeof(VehicleData), new XmlAttributes());

            foreach (KeyValuePair<string, VehicleData> p in Data)
            {
                attr[typeof(VehicleData)].XmlType = new XmlTypeAttribute(p.Key);
                new XmlSerializer(typeof(VehicleData), attr).Serialize(writer, p.Value, ns);
            }
        }
    }
}
