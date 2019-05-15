using System;
using System.Text;
using System.IO;
using System.Windows.Forms;

using Newtonsoft.Json;

namespace Qlip
{
    public class ConfigHelper
    {
        private static readonly string defaultConfig =
@"{
    ""save_count"":""40"",
    ""reset_on_paste"":true,
    ""reset_on_cancel"":false,
    ""paste_timeout"":""2"",
    ""move_pasted_to_front"":false
}";
        public static readonly int defaultSaveCount = 40;
        public static readonly bool defaultResetOnPaste = true;
        public static readonly bool defaultResetOnCancel = false;
        public static readonly double defaultPasteTimeout = 2.0;
        public static readonly bool defaultMovePastedToFront = false;

        public Config config;

        public ConfigHelper()
        {
            LoadJson();
        }

        public void LoadJson()
        {
            if (!File.Exists(".qlipconfig.json"))
            {
                using (FileStream fs = File.Create(".qlipconfig.json"))
                {
                    fs.Write(Encoding.ASCII.GetBytes(defaultConfig), 0, defaultConfig.Length);
                }
            }
            try
            {
                using (StreamReader r = new StreamReader(".qlipconfig.json"))
                {
                    string json = r.ReadToEnd();
                    config = JsonConvert.DeserializeObject<Config>(json);
                }
            } catch (Exception e)
            {
                MessageBox.Show("Could not read config file! If manually editing Qlip config file, please ensure it is in valid JSON format.\n" +
                                 "Falling back to default options.", "Qlip: Config file error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                config = new Config();
                SetDefaults();
            }      
        }

        public void SetDefaults()
        {
            config.save_count = defaultSaveCount;
            config.reset_on_paste = defaultResetOnPaste;
            config.reset_on_cancel = defaultResetOnCancel;
            config.paste_timeout = defaultPasteTimeout;
            config.move_pasted_to_front = defaultMovePastedToFront;
        }

        public void Save()
        {
            string jsonStr = JsonConvert.SerializeObject(this.config);
            File.WriteAllText(".qlipconfig.json", jsonStr);
        }

        public class Config
        {
            public int save_count;
            public bool reset_on_paste;
            public bool reset_on_cancel;
            public double paste_timeout;
            public bool move_pasted_to_front;
        }
    }
}
