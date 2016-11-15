namespace Spotlight
{
    // System
    using System.IO;
    using System.Drawing;
    using System.Windows.Forms;
    using System.Xml.Serialization;

    // RPH
    using Rage;

    internal class Settings
    {
        public readonly InitializationFile GeneralSettingsIniFile;

        public readonly Keys ToggleSpotlightKey;

        public readonly InitializationFile SpotlightOffsetsIniFile;

        public readonly SpotlightData CarsSpotlightData;
        public readonly SpotlightData HelicoptersSpotlightData;
        public readonly SpotlightData BoatsSpotlightData;

        public Settings(string generalSettingsIniFileName, string spotlightOffsetsIniFileName, string carsSpotlightDataFileName, string helicoptersSpotlightDataFileName, string boatsSpotlightDataFileName, bool generateDefaultsIfFileNotFound)
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
            SpotlightOffsetsIniFile = new InitializationFile(spotlightOffsetsIniFileName);
            CarsSpotlightData = ReadSpotlightDataFromXMLFile(carsSpotlightDataFileName);
            HelicoptersSpotlightData = ReadSpotlightDataFromXMLFile(helicoptersSpotlightDataFileName);
            BoatsSpotlightData = ReadSpotlightDataFromXMLFile(boatsSpotlightDataFileName);

            ToggleSpotlightKey = GeneralSettingsIniFile.ReadEnum<Keys>("Keyboard", "Toggle", Keys.I);
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
                writer.Write(DefaultGeneralSettingsText);
            }
        }

        private void CreateDefaultSpotlightOffsetsIniFile(string fileName)
        {
            using (StreamWriter writer = new StreamWriter(fileName, false))
            {
                writer.Write(DefaultSpotlightOffsetsText);
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
        const string DefaultGeneralSettingsText = @"
[Keyboard]
//** VALID KEYS: https://msdn.microsoft.com/en-us/library/system.windows.forms.keys(v=vs.110).aspx **\\
Toggle = I

Move Left = NumPad4
Move Right = NumPad6

Move Up = NumPad8
Move Down = NumPad2


[ControllerButtons]
//** VALID BUTTONS: http://docs.ragepluginhook.net/html/558BC34.htm **\\



[MouseControls]
";


        const string DefaultSpotlightOffsetsText = @"[POLICE]
X = -0.8
Y = 1.17
Z = 0.52
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
X = -0.78
Y = 2.4
Z = -1.0
[CHINO]
X= -0.8
Y= 1.17
Z= 0.52
[BUZZARD]
X= -0.1000004
Y= 2.369999
Z= -0.4799996
[BUZZARD2]
X= -0.1000004
Y= 2.369999
Z= -0.4799996
[PREDATOR]
X= -0.03000049
Y= -0.4299991
Z= 1.679999
";

        static readonly SpotlightData DefaultCarsSpotlightData = new SpotlightData(
                                                                                   color: Color.White, 
                                                                                   shadow: true, 
                                                                                   radius: 10,
                                                                                   brightness: 20,
                                                                                   distance: 60,
                                                                                   falloff: 40,
                                                                                   roundness: 100,
                                                                                   movementSpeed: 1
                                                                                   );

        static readonly SpotlightData DefaultHelicoptersSpotlightData = new SpotlightData(
                                                                                   color: Color.White,
                                                                                   shadow: true,
                                                                                   radius: 12,
                                                                                   brightness: 20,
                                                                                   distance: 230,
                                                                                   falloff: 50,
                                                                                   roundness: 100,
                                                                                   movementSpeed: 1
                                                                                   );

        static readonly SpotlightData DefaultBoatsSpotlightData = new SpotlightData(
                                                                                   color: Color.White,
                                                                                   shadow: true,
                                                                                   radius: 10,
                                                                                   brightness: 20,
                                                                                   distance: 80,
                                                                                   falloff: 45,
                                                                                   roundness: 100,
                                                                                   movementSpeed: 1
                                                                                   );
        #endregion
    }
}
