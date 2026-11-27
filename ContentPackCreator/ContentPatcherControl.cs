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
            ChangeActionComboBox();
        }

        private void ChangeActionComboBox()
        {
            cpActionPanel.Controls.Clear();
            switch (actionComboBox.SelectedItem.ToString())
            {
                case "Load":
                    cpActionPanel.Controls.Add(new ActionLoadControl() { Anchor = AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Top | AnchorStyles.Right, Dock = DockStyle.Fill });
                    break;
                case "EditData":
                    cpActionPanel.Controls.Add(new ActionEditDataControl() { Anchor = AnchorStyles.Left | AnchorStyles.Right, Dock = DockStyle.Fill });
                    break;
                case "EditImage":
                    cpActionPanel.Controls.Add(new ActionEditImageControl() { Anchor = AnchorStyles.Left | AnchorStyles.Right, Dock = DockStyle.Fill });
                    break;
                case "EditMap":
                    cpActionPanel.Controls.Add(new ActionEditMapControl() { Anchor = AnchorStyles.Left | AnchorStyles.Right, Dock = DockStyle.Fill });
                    break;
                case "Include":
                    cpActionPanel.Controls.Add(new ActionIncludeControl() { Anchor = AnchorStyles.Left | AnchorStyles.Right, Dock = DockStyle.Fill });
                    break;
            }
        }

        private void changesAddButton_Click(object sender, EventArgs e)
        {
            var data = new ChangeData()
            {
                Action = actionComboBox.Text,
                LogName = logName.Text.Length > 0 ? logName.Text : $"entry #{ContentPack.changesDict.Count} ({actionComboBox.SelectedItem.ToString()})",
            };
            List<string> update = new List<string>();
            if (onDayStartCheck.Checked)
                update.Add("OnDayStart");
            if (onLocationChangeCheck.Checked)
                update.Add("OnLocationChange");
            if (onTimeChangeCheck.Checked)
                update.Add("OnTimeChange");
            data.Update = string.Join(",", update);

            if (cpActionPanel.Controls.Find("targetText", true).Length > 0)
                data.Target = (cpActionPanel.Controls.Find("targetText", true)[0] as TextBox).Text;
            if (cpActionPanel.Controls.Find("fromFileText", true).Length > 0)
                data.FromFile = (cpActionPanel.Controls.Find("fromFileText", true)[0] as TextBox).Text;

            var whenTable = (cpActionPanel.Controls.Find("whenTable", true)[0] as TableLayoutPanel);
            for (int j = 0; j <= whenTable.RowCount; j++)
            {
                Control c = whenTable.GetControlFromPosition(0, j);
                if(c is WhenControl)
                {
                    if (data.When is null)
                        data.When = new Dictionary<string, string>();
                    data.When.Add((c.Controls.Find("keyText", true)[0] as TextBox).Text, (c.Controls.Find("valueText", true)[0] as TextBox).Text);
                }
            }

            ContentPack.changesDict.Add(data.LogName, data);
            ReloadChangesList();
        }
        private void ReloadChangesList()
        {
            logName.Text = "";
            changesListBox.Items.Clear();
            foreach (string key in ContentPack.changesDict.Keys)
            {
                changesListBox.Items.Add(key);
            }
            ChangeActionComboBox();
        }
    }
}
