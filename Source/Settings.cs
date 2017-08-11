namespace Spotlight
{
    using System.IO;
    using System.Xml;
    using System.Drawing;
    using System.Xml.Schema;
    using System.Globalization;
    using System.Windows.Forms;
    using System.Xml.Serialization;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    
    using Rage;

    internal sealed class Settings
    {
        public string GeneralSettingsIniFileName { get; }
        public string SpotlightOffsetsIniFileName { get; }
        public string VisualSettingsFileName { get; }

        public InitializationFile GeneralSettingsIniFile { get; }

        public ReadOnlyDictionary<string, Vector3> SpotlightOffsets { get; private set; }

        public VisualSettings Visual { get; }

        public Keys EditorKey { get; }

        internal Settings(string generalSettingsIniFileName, string spotlightOffsetsIniFileName, string visualSettingsFileName, bool generateDefaultsIfFileNotFound)
        {
            GeneralSettingsIniFileName = generalSettingsIniFileName;
            SpotlightOffsetsIniFileName = spotlightOffsetsIniFileName;
            VisualSettingsFileName = visualSettingsFileName;

            if (generateDefaultsIfFileNotFound)
            {
                if (!File.Exists(generalSettingsIniFileName))
                {
                    Game.LogTrivial("General settings file doesn't exists, creating default...");
                    CreateDefaultGeneralSettingsIniFile(generalSettingsIniFileName);
                }

                if (!File.Exists(spotlightOffsetsIniFileName))
                {
                    Game.LogTrivial("Spotlight offsets file doesn't exists, creating default...");
                    CreateDefaultSpotlightOffsetsIniFile(spotlightOffsetsIniFileName);
                }

                if (!File.Exists(visualSettingsFileName))
                {
                    Game.LogTrivial("Visual settings file doesn't exists, creating default...");
                    CreateDefaultVisualSettingsXMLFile(visualSettingsFileName);
                }
            }

            Game.LogTrivial("Reading settings...");
            GeneralSettingsIniFile = new InitializationFile(generalSettingsIniFileName);
            SpotlightOffsets = new ReadOnlyDictionary<string, Vector3>(ReadSpotlightOffsets(new InitializationFile(spotlightOffsetsIniFileName)));
            Visual = ReadVisualSettingsFromXMLFile(visualSettingsFileName);

            EditorKey = GeneralSettingsIniFile.ReadEnum<Keys>("Misc", "EditorKey", Keys.F11);
        }

        internal void UpdateOffsets(IDictionary<string, Vector3> offsets, bool saveToFile)
        {
            SpotlightOffsets = new ReadOnlyDictionary<string, Vector3>(offsets);

            if (saveToFile)
            {
                using (StreamWriter writer = new StreamWriter(SpotlightOffsetsIniFileName, false))
                {
                    writer.WriteLine(PluginTextTitle);
                    foreach (KeyValuePair<string, Vector3> item in SpotlightOffsets)
                    {
                        writer.WriteLine($"[{item.Key}]");
                        writer.WriteLine($"X = {item.Value.X.ToString(CultureInfo.InvariantCulture)}");
                        writer.WriteLine($"Y = {item.Value.Y.ToString(CultureInfo.InvariantCulture)}");
                        writer.WriteLine($"Z = {item.Value.Z.ToString(CultureInfo.InvariantCulture)}");
                    }
                }
            }
        }

        private Dictionary<string, Vector3> ReadSpotlightOffsets(InitializationFile iniFile)
        {
            Game.LogTrivial("Loading spotlight offsets...");

            Dictionary<string, Vector3> dict = new Dictionary<string, Vector3>();

            foreach (string modelName in iniFile.GetSectionNames())
            {
                float x = 0.0f, y = 0.0f, z = 0.0f;

                bool success = false;
                System.Exception exc = null;
                try
                {
                    if (iniFile.DoesSectionExist(modelName))
                    {
                        if (iniFile.DoesKeyExist(modelName, "X") &&
                            iniFile.DoesKeyExist(modelName, "Y") &&
                            iniFile.DoesKeyExist(modelName, "Z"))
                        {

                            x = iniFile.ReadSingle(modelName, "X", -0.8f);
                            y = iniFile.ReadSingle(modelName, "Y", 1.17f);
                            z = iniFile.ReadSingle(modelName, "Z", 0.52f);

                            Game.LogTrivial($"  Spotlight offset position settings found and loaded for vehicle model: {modelName}");
                            success = true;
                        }
                    }

                }
                catch (System.Exception ex)
                {
                    exc = ex;
                }

                if (!success)
                {
                    Game.LogTrivial($"  <WARNING> Failed to load spotlight offset position settings for vehicle model: {modelName}");
                    if (exc != null)
                    {
                        Game.LogTrivial($"  <WARNING> {exc}");
                    }
                    Game.LogTrivial("       Using default settings");
                    x = -0.8f;
                    y = 1.17f;
                    z = 0.52f;
                }

                dict.Add(modelName, new Vector3(x, y, z));
            }

            Game.LogTrivial("Finished loading spotlight offsets...");
            return dict;
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
                writer.Write(PluginTextTitle + DefaultGeneralSettingsText);
            }
        }

        private void CreateDefaultSpotlightOffsetsIniFile(string fileName)
        {
            using (StreamWriter writer = new StreamWriter(fileName, false))
            {
                writer.Write(PluginTextTitle + DefaultSpotlightOffsetsText);
            }
        }

        private void CreateDefaultVisualSettingsXMLFile(string fileName)
        {
            VisualSettings v = new VisualSettings()
            {
                Default = DefaultDefaultSpotlightData,
                Helicopter = DefaultHelicopterSpotlightData,
                Boat = DefaultBoatSpotlightData,
            };

            XmlSerializer ser = new XmlSerializer(typeof(VisualSettings));
            using (StreamWriter writer = new StreamWriter(fileName, false))
            {
                ser.Serialize(writer, v);
            }
        }
        #endregion

        #region Default Values
        static readonly string PluginTextTitle = $"Spotlight v{System.Reflection.Assembly.GetExecutingAssembly().GetName().Version} by alexguirre{System.Environment.NewLine}";

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

TrackPedKey = NumPad1
TrackVehicleKey = NumPad3



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


        const string DefaultSpotlightOffsetsText = @"[POLICE]
X = -0.8
Y = 1.17
Z = 0.45
[POLICE2]
X = -0.84
Y = 0.85
Z = 0.43
[POLICE3]
X = -0.84
Y = 0.85
Z = 0.59
[POLICE4]
X = -0.8
Y = 1.17
Z = 0.52
[POLICET]
X = -1.05
Y = 1.42
Z = 0.95
[RIOT]
X = -1.18
Y = 1.75
Z = 1.55
[FBI]
X = -0.84
Y = 0.85
Z = 0.43
[FBI2]
X = -0.9
Y = 1.2
Z = 1.0
[POLICEOLD1]
X = -0.85
Y = 0.89
Z = 0.78
[POLICEOLD2]
X = -0.88
Y = 0.8
Z = 0.6
[SHERIFF]
X = -0.8
Y = 1.17
Z = 0.52
[SHERIFF2]
X = -0.9
Y = 1.2 
Z = 1.0
[PRANGER]
X = -0.9
Y = 1.2 
Z = 1.0
[LGUARD]
X = -0.9
Y = 1.2 
Z = 1.0
[POLMAV]
X = 0.0
Y = 2.95
Z = -1.0
[CHINO]
X= -0.8
Y= 1.17
Z= 0.52
[BUZZARD]
X= 0.0
Y= 2.369999
Z= -0.4799996
[BUZZARD2]
X= 0.0
Y= 2.369999
Z= -0.4799996
[PREDATOR]
X= 0.0
Y= -0.42
Z= 1.8
";

        static readonly SpotlightData DefaultDefaultSpotlightData = new SpotlightData(
                                                                                   color: new Core.RGB(80, 80, 80),
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
                                                                                   movementSpeed: 1
                                                                                   );

        static readonly SpotlightData DefaultHelicopterSpotlightData = new SpotlightData(
                                                                                   color: new Core.RGB(80, 80, 80),
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
                                                                                   movementSpeed: 1
                                                                                   );

        static readonly SpotlightData DefaultBoatSpotlightData = new SpotlightData(
                                                                                   color: new Core.RGB(80, 80, 80),
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
                                                                                   movementSpeed: 1
                                                                                   );
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
}
