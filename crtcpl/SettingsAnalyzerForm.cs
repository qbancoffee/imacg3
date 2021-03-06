﻿using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Forms;

namespace crtcpl
{
    public partial class SettingsAnalyzerForm : Form
    {
        public SettingsAnalyzerForm()
        {
            InitializeComponent();

#if MONO
            // Mono doesn't support tool windows properly
            this.FormBorderStyle = FormBorderStyle.Sizable;
#endif

            this.listView.Items.Add("CONFIG_OFFSET_CONTRAST");
            this.listView.Items.Add("CONFIG_OFFSET_RED_DRIVE");
            this.listView.Items.Add("CONFIG_OFFSET_GREEN_DRIVE");
            this.listView.Items.Add("CONFIG_OFFSET_BLUE_DRIVE");
            this.listView.Items.Add("CONFIG_OFFSET_RED_CUTOFF");
            this.listView.Items.Add("CONFIG_OFFSET_GREEN_CUTOFF");
            this.listView.Items.Add("CONFIG_OFFSET_BLUE_CUTOFF");
            this.listView.Items.Add("CONFIG_OFFSET_HORIZONTAL_POS");
            this.listView.Items.Add("CONFIG_OFFSET_HEIGHT");
            this.listView.Items.Add("CONFIG_OFFSET_VERTICAL_POS");
            this.listView.Items.Add("CONFIG_OFFSET_S_CORRECTION");
            this.listView.Items.Add("CONFIG_OFFSET_KEYSTONE");
            this.listView.Items.Add("CONFIG_OFFSET_PINCUSHION");
            this.listView.Items.Add("CONFIG_OFFSET_WIDTH");
            this.listView.Items.Add("CONFIG_OFFSET_PINCUSHION_BALANCE");
            this.listView.Items.Add("CONFIG_OFFSET_PARALLELOGRAM");
            this.listView.Items.Add("CONFIG_OFFSET_BRIGHTNESS_DRIVE");
            this.listView.Items.Add("CONFIG_OFFSET_BRIGHTNESS");
            this.listView.Items.Add("CONFIG_OFFSET_ROTATION");
            this.listView.Items.Add("CONFIG_OFFSET_CHECKSUM");

            foreach (ListViewItem item in this.listView.Items)
            {
                item.SubItems.Add("?");
                item.SubItems.Add("?");
            }
        }

        private void refreshButton_Click(object sender, EventArgs e)
        {
            Trace.Assert(UCCom.IsOpen, "UCCom has no connection at the moment!");

            this.refreshButton.Enabled = false;
            this.UseWaitCursor = true;
            this.listView.BeginUpdate();
            Application.DoEvents();

            byte[] sram;

            try
            {
                sram = UCCom.SendCommand(2, 0, 0);
            }
            catch (UCComException ex)
            {
                MessageBox.Show(this, string.Format(CultureInfo.CurrentCulture, StringRes.StringRes.CantUpdatePages,
                    ex.Message), this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);

                for (int i = 0; i < this.listView.Items.Count; i++)
                {
                    this.listView.Items[i].SubItems[1].Text = "?";
                    this.listView.Items[i].SubItems[2].Text = "?";
                }

                goto end;
            }

            for (int i = 0; i < sram.Length; i++)
            {
                this.listView.Items[i].SubItems[1].Text = string.Format(CultureInfo.InvariantCulture, "0x{0:X2}", sram[i]);
                this.listView.Items[i].SubItems[2].Text = string.Format(CultureInfo.InvariantCulture, "{0}", sram[i]);
            }

        end:
            this.refreshButton.Enabled = true;
            this.UseWaitCursor = false;
            this.listView.EndUpdate();
        }
    }
}
