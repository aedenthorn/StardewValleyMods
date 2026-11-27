using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
    public partial class ContentPack : Form
    {
        public static Dictionary<string, ConfigData> configDataDict = new Dictionary<string,ConfigData>();
        public static Dictionary<string, ChangeData> changesDict = new Dictionary<string, ChangeData>();
        public static string currentContentPackLayout = "";
        public static AutoCompleteStringCollection uniqueIDs = new AutoCompleteStringCollection()
        {
            "Pathoschild.ContentPatcher",
            "spacechase0.JsonAssets",
            "Digus.ProducerFrameworkMod",
            "Platonymous.TMXLoader",
            "PeacefulEnd.AlternativeTextures",
            "Cherry.ShopTileFramework",
            "DIGUS.MailFrameworkMod",
            "Esca.FarmTypeManager",
            "Paritee.BetterFarmAnimalVariety",
            "Platonymous.CustomFurniture",
            "Platonymous.CustomMusic"
        };

        public ContentPack()
        {
            InitializeComponent();
            forID.AutoCompleteCustomSource = uniqueIDs;
        }


        private void buildButton_Click(object sender, EventArgs e)
        {
            ManifestData mData = new ManifestData();

            mData.Name = nameText.Text.Trim();
            mData.Author = authorText.Text.Trim();
            mData.Version = versionText.Text.Trim();
            mData.Description = descText.Text.Trim();
            mData.UniqueID = idText.Text.Trim();
            mData.MinimumApiVersion = minText.Text.Trim();
            mData.ContentPackFor = new ContentPackForData()
            {
                UniqueID = forID.Text,
                MinimumVersion = forMin.Text
            };

            mData.Dependencies = null;
            foreach(Control c in depTable.Controls)
            {
                if(c is DepControl)
                {
                    if (mData.Dependencies == null)
                        mData.Dependencies = new List<DependencyData>();
                    DependencyData dd = new DependencyData();
                    dd.UniqueID = ((TextBox)c.Controls["depID"]).Text.Trim();
                    if (!((CheckBox)c.Controls["depReq"]).Checked)
                        dd.IsRequired = false;

                    if (((TextBox)c.Controls["depMin"]).Text.Trim().Length > 0)
                        dd.MinimumVersion = ((TextBox)c.Controls["depMin"]).Text.Trim();
                    mData.Dependencies.Add(dd);
                }
            }

            ContentPatcherData cData = new ContentPatcherData();
            cData.ConfigSchema = configDataDict;
            cData.Changes.AddRange(changesDict.Values);

            string folder = folderText.Text;
            Directory.CreateDirectory(folder);
            string manifestJson = JsonConvert.SerializeObject(mData, Formatting.Indented, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });
            File.WriteAllText(Path.Combine(folder, "manifest.json"), manifestJson);

            string contentJson = JsonConvert.SerializeObject(cData, Formatting.Indented, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });
            File.WriteAllText(Path.Combine(folder, "content.json"), contentJson);


            string message = $"Mod written to {Path.Combine("Mods", folder)}.";
            MessageBox.Show(message);
        }

        private void addDepButton_Click(object sender, EventArgs e)
        {
            depTable.RowCount = depTable.RowCount + 1;
            depTable.RowStyles.RemoveAt(depTable.RowStyles.Count - 1);
            depTable.Controls.RemoveByKey("addDepButton");
            depTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 120F));
            var dc = new DepControl() { Anchor = AnchorStyles.Left | AnchorStyles.Right };
            ((TextBox)dc.Controls["depID"]).AutoCompleteCustomSource = uniqueIDs;
            depTable.Controls.Add(dc, 0, depTable.RowCount - 2);
            depTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            depTable.Controls.Add(addDepButton, 0, depTable.RowCount - 1);
        }
        private void tabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(((TabControl)sender).SelectedIndex == 1 && forID.Text != currentContentPackLayout)
            {
                currentContentPackLayout = forID.Text;
                contentTab.Controls.Clear();
                if(currentContentPackLayout.ToLower() == uniqueIDs[0].ToLower())
                {
                    contentTab.Controls.Add(new ContentPatcherControl() { Anchor = AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Top | AnchorStyles.Right, Dock = DockStyle.Fill });
                }
            }
        }

    }
}
