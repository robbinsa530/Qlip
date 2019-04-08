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
        private static readonly int defaultSaveCount = 40;
        private static readonly bool defaultResetOnPaste = true;
        private static readonly bool defaultResetOnCancel = false;
        private static readonly int defaultPasteTimeout = 2;
        private static readonly bool defaultMovePastedToFront = false;

        public Config config;

        public ConfigHelper()
        {
            LoadJson();
        }

        public void LoadJson()
        {
            if (!File.Exists(".config.json"))
            {
                using (FileStream fs = File.Create(".config.json"))
                {
                    fs.Write(Encoding.ASCII.GetBytes(defaultConfig), 0, defaultConfig.Length);
                }
            }
            try
            {
                using (StreamReader r = new StreamReader(".config.json"))
                {
                    string json = r.ReadToEnd();
                    config = JsonConvert.DeserializeObject<Config>(json);
                }
            } catch (Exception e)
            {
                MessageBox.Show("Could not read config file! If manually editing Qlip config file, please ensure it is in valid JSON format.\n" +
                                 "Falling back to default options.", "Qlip: Config file error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                config = new Config();
                config.save_count = defaultSaveCount;
                config.reset_on_paste = defaultResetOnPaste;
                config.reset_on_cancel = defaultResetOnCancel;
                config.paste_timeout = defaultPasteTimeout;
                config.move_pasted_to_front = defaultMovePastedToFront;
            }
            
        }

        public class Config
        {
            public int save_count;
            public bool reset_on_paste;
            public bool reset_on_cancel;
            public int paste_timeout;
            public bool move_pasted_to_front;
        }
    }
}
