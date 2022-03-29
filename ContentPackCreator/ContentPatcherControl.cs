using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ContentPackCreator
{
    public partial class ContentPatcherControl : UserControl
    {
        private static Dictionary<string, ChangeData> changesDict = new Dictionary<string, ChangeData>();

        public static AutoCompleteStringCollection contentFileNames = new AutoCompleteStringCollection();

        public ContentPatcherControl()
        {
            InitializeComponent();
            ScanContentFolder();
        }

        private void ScanContentFolder()
        {
            contentFileNames.Clear();
            string content = Path.Combine(Directory.GetParent(Environment.CurrentDirectory).Parent.FullName, "Content");

            if (!Directory.Exists(content))
                return;
            foreach (string folder in Directory.GetDirectories(content))
            {
                foreach (string file in Directory.GetFiles(folder, "*.*", SearchOption.AllDirectories))
                {
                    string aFile = file.Substring(folder.Length + 1);
                    string aDir = "" + Path.GetDirectoryName(aFile);
                    if(aDir != "")
                        aDir = aDir.Replace("\\", "/") + "/";
                    contentFileNames.Add(Path.GetFileName(folder) + "/" + aDir + Path.GetFileNameWithoutExtension(aFile));
                }
            }

        }


        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string key = (string)((ListBox)sender).SelectedItem;
            if (key is null || !ContentPack.configDataDict.TryGetValue(key, out ConfigData data))
                return;
            configFieldName.Text = key;
            configAllowValues.Text = data.AllowValues;
            configDefault.Text = data.Default;
        }

        private void configAdd_Click(object sender, EventArgs e)
        {
            ConfigData data = new ConfigData()
            {
                AllowValues = configAllowValues.Text,
                Default = configDefault.Text
            };
            ContentPack.configDataDict[configFieldName.Text] = data;
            ReloadConfigList();

        }
        private void configRemove_Click(object sender, EventArgs e)
        {
            ContentPack.configDataDict.Remove(configFieldName.Text);
            ReloadConfigList();
        }
        private void ReloadConfigList()
        {
            configFieldName.Text = "";
            configAllowValues.Text = "";
            configDefault.Text = "";
            configListBox.Items.Clear();
            foreach(string key in ContentPack.configDataDict.Keys)
            {
                configListBox.Items.Add(key);
            }
        }

        private void actionComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            cpActionPanel.Controls.Clear();
            switch (actionComboBox.SelectedItem.ToString())
            {
                case "Load":
                    var c = new ActionLoadControl() { Anchor = AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Top | AnchorStyles.Right };
                    c.Size = cpActionPanel.Size;
                    cpActionPanel.Controls.Add(c);
                    break;
                case "EditData":
                    cpActionPanel.Controls.Add(new ActionEditDataControl() { Anchor = AnchorStyles.Left | AnchorStyles.Right });
                    break;
                case "EditImage":
                    cpActionPanel.Controls.Add(new ActionEditImageControl() { Anchor = AnchorStyles.Left | AnchorStyles.Right });
                    break;
                case "EditMap":
                    cpActionPanel.Controls.Add(new ActionEditMapControl() { Anchor = AnchorStyles.Left | AnchorStyles.Right });
                    break;
                case "Include":
                    cpActionPanel.Controls.Add(new ActionIncludeControl() { Anchor = AnchorStyles.Left | AnchorStyles.Right });
                    break;
            }
        }

        private void changesAddButton_Click(object sender, EventArgs e)
        {
            changesDict.Add($"entry #{changesDict.Count} (Load )", new ChangeData());
        }
    }
}
