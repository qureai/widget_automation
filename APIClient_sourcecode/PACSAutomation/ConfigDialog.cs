using System;
using System.Drawing;
using System.Windows.Forms;

namespace PACSAutomation
{
    /// <summary>
    /// Modal dialog for editing ScenarioConfig at runtime.
    /// </summary>
    public class ConfigDialog : Form
    {
        public ScenarioConfig Config { get; private set; }

        private readonly TextBox[] _textBoxes;
        private readonly string[]  _labels = {
            "Username",
            "Password",
            "Site Name (blank = all sites)",
            "Accession — New (S01, S04, S05, S06)",
            "Accession — Existing Draft (S02, S07, S08, S10)",
            "Accession — PendingSignature (S03)",
            "Accession — Discard (S05 alt)",
            "Accession — Extra / Secondary (S06, S10)",
            "Accession — Multi (comma-sep, S09)",
            "Patient MRN (optional, S07)"
        };

        public ConfigDialog(ScenarioConfig existing)
        {
            Config = existing;
            Text            = "Edit Scenario Configuration";
            Size            = new Size(480, 420);
            MinimumSize     = new Size(400, 380);
            StartPosition   = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox     = false;
            Font            = new Font("Segoe UI", 9f);
            BackColor       = Color.FromArgb(245, 248, 255);

            string[] values =
            {
                existing.Username,
                existing.Password,
                existing.SiteName,
                existing.AccessionNew,
                existing.AccessionExisting,
                existing.AccessionPendingSignature,
                existing.AccessionDiscard,
                existing.AccessionExtra,
                existing.AccessionMulti,
                existing.PatientMRN
            };

            var pnl = new Panel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(12) };
            _textBoxes = new TextBox[_labels.Length];

            int y = 10;
            for (int i = 0; i < _labels.Length; i++)
            {
                pnl.Controls.Add(new Label
                {
                    Text     = _labels[i],
                    Location = new Point(10, y),
                    AutoSize = true,
                    ForeColor = Color.DimGray
                });
                y += 18;
                var tb = new TextBox
                {
                    Location  = new Point(10, y),
                    Width     = 420,
                    Text      = values[i],
                    Font      = new Font("Consolas", 9f),
                    BorderStyle = BorderStyle.FixedSingle
                };
                if (_labels[i].ToLower().Contains("password"))
                    tb.PasswordChar = '●';
                _textBoxes[i] = tb;
                pnl.Controls.Add(tb);
                y += 30;
            }

            var btnOk = new Button
            {
                Text      = "Save",
                DialogResult = DialogResult.OK,
                Location  = new Point(250, 10),
                Width     = 90,
                Height    = 30,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(30, 110, 60),
                ForeColor = Color.White
            };
            var btnCancel = new Button
            {
                Text      = "Cancel",
                DialogResult = DialogResult.Cancel,
                Location  = new Point(350, 10),
                Width     = 90,
                Height    = 30,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(140, 40, 40),
                ForeColor = Color.White
            };

            var pnlButtons = new Panel
            {
                Dock   = DockStyle.Bottom,
                Height = 50,
                BackColor = Color.FromArgb(230, 235, 245)
            };
            pnlButtons.Controls.AddRange(new Control[] { btnOk, btnCancel });

            Controls.Add(pnl);
            Controls.Add(pnlButtons);
            AcceptButton = btnOk;
            CancelButton = btnCancel;

            btnOk.Click += (s, e) => SaveConfig();
        }

        private void SaveConfig()
        {
            Config = new ScenarioConfig
            {
                Username                 = _textBoxes[0].Text.Trim(),
                Password                 = _textBoxes[1].Text,
                SiteName                 = _textBoxes[2].Text.Trim(),
                AccessionNew             = _textBoxes[3].Text.Trim(),
                AccessionExisting        = _textBoxes[4].Text.Trim(),
                AccessionPendingSignature= _textBoxes[5].Text.Trim(),
                AccessionDiscard         = _textBoxes[6].Text.Trim(),
                AccessionExtra           = _textBoxes[7].Text.Trim(),
                AccessionMulti           = _textBoxes[8].Text.Trim(),
                PatientMRN               = _textBoxes[9].Text.Trim()
            };
        }
    }
}
