namespace Spotlight
{
    // System
    using System.IO;
    using System.Drawing;
    using System.Windows.Forms;
    using System.Xml.Serialization;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;


    // RPH
    using Rage;

    public sealed class Settings
    {
        public readonly InitializationFile GeneralSettingsIniFile;

        public readonly ReadOnlyDictionary<Model, Vector3> SpotlightOffsets;

        public readonly SpotlightData CarsSpotlightData;
        public readonly SpotlightData HelicoptersSpotlightData;
        public readonly SpotlightData BoatsSpotlightData;

        internal Settings(string generalSettingsIniFileName, string spotlightOffsetsIniFileName, string carsSpotlightDataFileName, string helicoptersSpotlightDataFileName, string boatsSpotlightDataFileName, bool generateDefaultsIfFileNotFound)
        {
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

                if (!File.Exists(carsSpotlightDataFileName))
                {
                    Game.LogTrivial("Cars spotlight data file doesn't exists, creating default...");
                    CreateDefaultCarsSpotlightDataXMLFile(carsSpotlightDataFileName);
                }

                if (!File.Exists(helicoptersSpotlightDataFileName))
                {
                    Game.LogTrivial("Helicopters spotlight data file doesn't exists, creating default...");
                    CreateDefaultHelicoptersSpotlightDataXMLFile(helicoptersSpotlightDataFileName);
                }

                if (!File.Exists(boatsSpotlightDataFileName))
                {
                    Game.LogTrivial("Boats spotlight data file doesn't exists, creating default...");
                    CreateDefaultBoatsSpotlightDataXMLFile(boatsSpotlightDataFileName);
                }
            }

            Game.LogTrivial("Reading settings...");
            GeneralSettingsIniFile = new InitializationFile(generalSettingsIniFileName);
            SpotlightOffsets = new ReadOnlyDictionary<Model, Vector3>(ReadSpotlightOffsets(new InitializationFile(spotlightOffsetsIniFileName)));
            CarsSpotlightData = ReadSpotlightDataFromXMLFile(carsSpotlightDataFileName);
            HelicoptersSpotlightData = ReadSpotlightDataFromXMLFile(helicoptersSpotlightDataFileName);
            BoatsSpotlightData = ReadSpotlightDataFromXMLFile(boatsSpotlightDataFileName);
        }


        private Dictionary<Model, Vector3> ReadSpotlightOffsets(InitializationFile iniFile)
        {
            Game.LogTrivial("Loading spotlight offsets...");

            Dictionary<Model, Vector3> dict = new Dictionary<Model, Vector3>();

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

        private SpotlightData ReadSpotlightDataFromXMLFile(string fileName)
        {
            if (!File.Exists(fileName))
                throw new FileNotFoundException("", fileName);

            SpotlightData d;
            XmlSerializer ser = new XmlSerializer(typeof(SpotlightData));
            using (StreamReader reader = new StreamReader(fileName))
            {
                d = (SpotlightData)ser.Deserialize(reader);
            }

            return d;
        }

        private void WriteSpotlightDataToXMLFile(string fileName, SpotlightData data)
        {
            XmlSerializer ser = new XmlSerializer(typeof(SpotlightData));
            using (StreamWriter writer = new StreamWriter(fileName, false))
            {
                ser.Serialize(writer, data);
            }
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

        private void CreateDefaultCarsSpotlightDataXMLFile(string fileName)
        {
            WriteSpotlightDataToXMLFile(fileName, DefaultCarsSpotlightData);
        }

        private void CreateDefaultHelicoptersSpotlightDataXMLFile(string fileName)
        {
            WriteSpotlightDataToXMLFile(fileName, DefaultHelicoptersSpotlightData);
        }

        private void CreateDefaultBoatsSpotlightDataXMLFile(string fileName)
        {
            WriteSpotlightDataToXMLFile(fileName, DefaultBoatsSpotlightData);
        }
        #endregion

        #region Default Values
        static readonly string PluginTextTitle = $"Spotlight v{System.Reflection.Assembly.GetExecutingAssembly().GetName().Version} by alexguirre{System.Environment.NewLine}";

        const string DefaultGeneralSettingsText = @"
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

        static readonly SpotlightData DefaultCarsSpotlightData = new SpotlightData(
                                                                                   color: Color.FromArgb(255, 7, 7, 7),
                                                                                   castShadows: true,
                                                                                   radius: 10,
                                                                                   intensity: 20,
                                                                                   range: 60,
                                                                                   falloff: 40,
                                                                                   roundness: 70,
                                                                                   volumeIntensity: 0.35f,
                                                                                   volumeSize: 0.125f,
                                                                                   coronaIntensity: 10.0f,
                                                                                   coronaSize: 1.5f,
                                                                                   movementSpeed: 1
                                                                                   );

        static readonly SpotlightData DefaultHelicoptersSpotlightData = new SpotlightData(
                                                                                   color: Color.FromArgb(255, 7, 7, 7),
                                                                                   castShadows: true,
                                                                                   radius: 12,
                                                                                   intensity: 20,
                                                                                   range: 230,
                                                                                   falloff: 50,
                                                                                   roundness: 70,
                                                                                   volumeIntensity: 0.35f,
                                                                                   volumeSize: 0.125f,
                                                                                   coronaIntensity: 10.0f,
                                                                                   coronaSize: 1.5f,
                                                                                   movementSpeed: 1
                                                                                   );

        static readonly SpotlightData DefaultBoatsSpotlightData = new SpotlightData(
                                                                                   color: Color.FromArgb(255, 7, 7, 7),
                                                                                   castShadows: true,
                                                                                   radius: 10,
                                                                                   intensity: 20,
                                                                                   range: 80,
                                                                                   falloff: 45,
                                                                                   roundness: 70,
                                                                                   volumeIntensity: 0.35f,
                                                                                   volumeSize: 0.125f,
                                                                                   coronaIntensity: 10.0f,
                                                                                   coronaSize: 1.5f,
                                                                                   movementSpeed: 1
                                                                                   );
        #endregion
    }
}
