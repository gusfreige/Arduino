using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace BoardsEditor
{
    public partial class FormMain : Form
    {
        private BoardsContainer _boards;
        public FormMain()
        {
            InitializeComponent();

            backgroundWorkerLoad.RunWorkerAsync(Environment.GetCommandLineArgs()[1]);
        }

        private void backgroundWorkerLoad_DoWork(object sender, DoWorkEventArgs e)
        {
            _boards = new BoardsContainer((string)e.Argument);
        }

        private void backgroundWorkerLoad_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            RebuildGui();
        }

        private void RebuildGui()
        {
            comboBoxBoards.Items.Clear();

            foreach (var b in _boards.Boards)
            {
                comboBoxBoards.Items.Add(b.Name);
            }
        }

        private void comboBoxBoards_SelectedIndexChanged(object sender, EventArgs e)
        {
            foreach(var p in _boards.Boards[comboBoxBoards.SelectedIndex].Properties)
            {
                flowLayoutPanelProperties.Controls.Add(new Label() {Text = p.Name, Width = 200});

                var l = new TextBox() {Text = p.Value, Width = 200};
                flowLayoutPanelProperties.Controls.Add(l);
            }
        }
    }
}
