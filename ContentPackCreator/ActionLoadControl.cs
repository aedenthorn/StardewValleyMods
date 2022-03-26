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
    public partial class ActionLoadControl : UserControl
    {
        public ActionLoadControl()
        {
            InitializeComponent();
        }

        private void addWhenButton_Click(object sender, EventArgs e)
        {
            whenTable.RowCount = whenTable.RowCount + 1;
            whenTable.RowStyles.RemoveAt(whenTable.RowStyles.Count - 1);
            whenTable.Controls.RemoveByKey("addWhenButton");
            whenTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 120F));
            var wc = new WhenControl() { Anchor = AnchorStyles.Left | AnchorStyles.Right };
            whenTable.Controls.Add(wc, 0, whenTable.RowCount - 2);
            whenTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            whenTable.Controls.Add(addWhenButton, 0, whenTable.RowCount - 1);
        }
    }
}
