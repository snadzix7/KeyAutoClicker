using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace KeyAutoClicker
{
    public class MainForm : Form
    {
        private readonly System.Windows.Forms.Timer pressTimer = new System.Windows.Forms.Timer();

        private TextBox keyBox;
        private NumericUpDown minutesBox;
        private NumericUpDown secondsBox;
        private NumericUpDown millisecondsBox;
        private Button startButton;
        private Label statusLabel;

        private bool running = false;

        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

        private const uint KEYEVENTF_KEYUP = 0x0002;

        public MainForm()
        {
            BuildInterface();
            pressTimer.Tick += PressTimer_Tick;
        }

        private void BuildInterface()
        {
            Text = "Key AutoClicker";
            Size = new Size(430, 390);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            BackColor = Color.FromArgb(24, 24, 28);
            ForeColor = Color.White;
            Font = new Font("Segoe UI", 10);

            Label title = new Label
            {
                Text = "Key AutoClicker",
                Font = new Font("Segoe UI", 22, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(30, 25)
            };

            Label subtitle = new Label
            {
                Text = "Automatycznie naciska wybrany klawisz.",
                ForeColor = Color.FromArgb(170, 170, 180),
                AutoSize = true,
                Location = new Point(33, 70)
            };

            Label keyLabel = CreateLabel("Klawisz:", 35, 115);
            keyBox = CreateTextBox(160, 110);
            keyBox.Text = "Space";

            Label minutesLabel = CreateLabel("Minuty:", 35, 155);
            minutesBox = CreateNumberBox(160, 150, 0, 999);

            Label secondsLabel = CreateLabel("Sekundy:", 35, 195);
            secondsBox = CreateNumberBox(160, 190, 1, 59);

            Label millisecondsLabel = CreateLabel("Milisekundy:", 35, 235);
            millisecondsBox = CreateNumberBox(160, 230, 0, 999);

            startButton = new Button
            {
                Text = "START",
                Location = new Point(35, 285),
                Size = new Size(340, 44),
                BackColor = Color.FromArgb(70, 130, 255),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Cursor = Cursors.Hand
            };

            startButton.FlatAppearance.BorderSize = 0;
            startButton.Click += StartButton_Click;

            statusLabel = new Label
            {
                Text = "Status: zatrzymany",
                ForeColor = Color.FromArgb(170, 170, 180),
                AutoSize = true,
                Location = new Point(35, 340)
            };

            Controls.Add(title);
            Controls.Add(subtitle);
            Controls.Add(keyLabel);
            Controls.Add(keyBox);
            Controls.Add(minutesLabel);
            Controls.Add(minutesBox);
            Controls.Add(secondsLabel);
            Controls.Add(secondsBox);
            Controls.Add(millisecondsLabel);
            Controls.Add(millisecondsBox);
            Controls.Add(startButton);
            Controls.Add(statusLabel);
        }

        private Label CreateLabel(string text, int x, int y)
        {
            return new Label
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(120, 25),
                ForeColor = Color.FromArgb(220, 220, 230)
            };
        }

        private TextBox CreateTextBox(int x, int y)
        {
            return new TextBox
            {
                Location = new Point(x, y),
                Size = new Size(215, 28),
                BackColor = Color.FromArgb(38, 38, 44),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
        }

        private NumericUpDown CreateNumberBox(int x, int y, int value, int max)
        {
            return new NumericUpDown
            {
                Location = new Point(x, y),
                Size = new Size(215, 28),
                Minimum = 0,
                Maximum = max,
                Value = value,
                BackColor = Color.FromArgb(38, 38, 44),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
        }

        private void StartButton_Click(object sender, EventArgs e)
        {
            if (!running)
            {
                int interval =
                    ((int)minutesBox.Value * 60 * 1000) +
                    ((int)secondsBox.Value * 1000) +
                    (int)millisecondsBox.Value;

                if (interval <= 0)
                {
                    MessageBox.Show("Odstęp musi być większy niż 0.", "Błąd");
                    return;
                }

                pressTimer.Interval = interval;
                pressTimer.Start();

                running = true;
                startButton.Text = "STOP";
                startButton.BackColor = Color.FromArgb(220, 70, 70);
                statusLabel.Text = "Status: działa";
            }
            else
            {
                StopClicker();
            }
        }

        private void StopClicker()
        {
            pressTimer.Stop();

            running = false;
            startButton.Text = "START";
            startButton.BackColor = Color.FromArgb(70, 130, 255);
            statusLabel.Text = "Status: zatrzymany";
        }

        private void PressTimer_Tick(object sender, EventArgs e)
        {
            string keyText = keyBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(keyText))
            {
                statusLabel.Text = "Status: wpisz klawisz";
                return;
            }

            try
            {
                Keys key = (Keys)Enum.Parse(typeof(Keys), keyText, true);
                PressKey((byte)key);
                statusLabel.Text = "Status: działa";
            }
            catch
            {
                statusLabel.Text = "Nieznany klawisz: " + keyText;
            }
        }

        private void PressKey(byte keyCode)
        {
            keybd_event(keyCode, 0, 0, UIntPtr.Zero);
            keybd_event(keyCode, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
        }
    }
}
