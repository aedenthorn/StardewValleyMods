using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ContentPackCreator
{
    public partial class ContentPatcherControl : UserControl
    {
        private AutoCompleteStringCollection actionTypeList = new AutoCompleteStringCollection() 
        {
            "Load",
            "EditData",
            "EditImage",
            "EditMap",
            "Include"
        };

        public ContentPatcherControl()
        {
            InitializeComponent();
            changeAction.AutoCompleteCustomSource = actionTypeList;
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
            switch (actionComboBox.SelectedItem.ToString())
            {
                case "Load":
                    break;
                case "EditData":
                    break;
                case "EditImage":
                    break;
                case "EditMap":
                    break;
                case "Include":
                    break;
            }
        }
    }
}
