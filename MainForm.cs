using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KeyAutoClicker
{
    public class MainForm : Form
    {
        private TextBox sequenceBox;
        private NumericUpDown repeatMinutesBox;
        private NumericUpDown repeatSecondsBox;
        private NumericUpDown repeatMillisecondsBox;
        private NumericUpDown keyHoldBox;
        private Button startButton;
        private Button exampleButton;
        private Label statusLabel;

        private bool running = false;
        private CancellationTokenSource cancellationSource;

        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

        private const uint KEYEVENTF_KEYUP = 0x0002;

        public MainForm()
        {
            BuildInterface();
        }

        private void BuildInterface()
        {
            Text = "Advanced Key AutoClicker";
            Size = new Size(760, 760);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            BackColor = Color.FromArgb(24, 24, 28);
            ForeColor = Color.White;
            Font = new Font("Segoe UI", 10);

            Label title = new Label
            {
                Text = "Advanced Key AutoClicker",
                Font = new Font("Segoe UI", 22, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(30, 22)
            };

            Label subtitle = new Label
            {
                Text = "Twórz sekwencje klawiszy, opóźnienia i kombinacje kilku klawiszy naraz.",
                ForeColor = Color.FromArgb(170, 170, 180),
                AutoSize = true,
                Location = new Point(33, 68)
            };

            Label sequenceLabel = CreateLabel("Sekwencja:", 35, 110, 200);

            sequenceBox = new TextBox
            {
                Location = new Point(35, 140),
                Size = new Size(675, 250),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                AcceptsReturn = true,
                AcceptsTab = true,
                BackColor = Color.FromArgb(38, 38, 44),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Consolas", 10),
                Text =
@"# Przykład:
# E, po sekundzie Enter

press E
wait 1000
press Enter"
            };

            Label helpLabel = new Label
            {
                Text =
@"LEGENDA SEKWENCJI:

press KLAWISZ        - naciska jeden klawisz
  press E
  press Enter
  press Space

wait CZAS_MS         - czeka podaną liczbę milisekund
  wait 1000          = 1 sekunda
  wait 500           = pół sekundy

combo KLAWISZ+KLAWISZ - naciska kilka klawiszy naraz
  combo ControlKey+A
  combo ShiftKey+E
  combo ControlKey+ShiftKey+S
  Możesz też pisać: combo ctrl+a, combo shift+e, combo alt+tab

hold KLAWISZ CZAS_MS - przytrzymuje klawisz
  hold Space 500

text TEKST           - wpisuje tekst
  text hello

# komentarz          - linie zaczynające się od # są ignorowane

POPULARNE KLAWISZE:
Space, Enter, Tab, Escape, A, B, C, E, W, S, D
F1, F2, F3, Left, Right, Up, Down
ControlKey / ctrl, ShiftKey / shift, Menu / alt",
                ForeColor = Color.FromArgb(180, 180, 190),
                Location = new Point(35, 400),
                Size = new Size(675, 145),
                Font = new Font("Consolas", 8),
                AutoSize = false
            };

            Label repeatLabel = CreateLabel("Przerwa po całej sekwencji:", 35, 565, 220);

            Label minLabel = CreateLabel("Min:", 35, 600, 40);
            repeatMinutesBox = CreateNumberBox(75, 595, 0, 999);

            Label secLabel = CreateLabel("Sek:", 190, 600, 40);
            repeatSecondsBox = CreateNumberBox(230, 595, 1, 59);

            Label msLabel = CreateLabel("Ms:", 345, 600, 35);
            repeatMillisecondsBox = CreateNumberBox(380, 595, 0, 999);

            Label holdLabel = CreateLabel("Domyślny czas trzymania klawisza, ms:", 35, 640, 270);
            keyHoldBox = CreateNumberBox(310, 635, 50, 1000);

            exampleButton = new Button
            {
                Text = "WSTAW PRZYKŁADY",
                Location = new Point(35, 680),
                Size = new Size(180, 42),
                BackColor = Color.FromArgb(65, 65, 75),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            exampleButton.FlatAppearance.BorderSize = 0;
            exampleButton.Click += ExampleButton_Click;

            startButton = new Button
            {
                Text = "START",
                Location = new Point(230, 680),
                Size = new Size(250, 42),
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
                Location = new Point(500, 690)
            };

            Controls.Add(title);
            Controls.Add(subtitle);
            Controls.Add(sequenceLabel);
            Controls.Add(sequenceBox);
            Controls.Add(helpLabel);
            Controls.Add(repeatLabel);
            Controls.Add(minLabel);
            Controls.Add(repeatMinutesBox);
            Controls.Add(secLabel);
            Controls.Add(repeatSecondsBox);
            Controls.Add(msLabel);
            Controls.Add(repeatMillisecondsBox);
            Controls.Add(holdLabel);
            Controls.Add(keyHoldBox);
            Controls.Add(exampleButton);
            Controls.Add(startButton);
            Controls.Add(statusLabel);
        }

        private Label CreateLabel(string text, int x, int y, int width)
        {
            return new Label
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(width, 25),
                ForeColor = Color.FromArgb(220, 220, 230)
            };
        }

        private NumericUpDown CreateNumberBox(int x, int y, int value, int max)
        {
            return new NumericUpDown
            {
                Location = new Point(x, y),
                Size = new Size(100, 28),
                Minimum = 0,
                Maximum = max,
                Value = value,
                BackColor = Color.FromArgb(38, 38, 44),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
        }

        private void ExampleButton_Click(object sender, EventArgs e)
        {
            sequenceBox.Text =
@"# Przykład 1: E, po sekundzie Enter
press E
wait 1000
press Enter

# Przykład 2: Ctrl + A
wait 1000
combo ControlKey+A

# Przykład 3: Shift + E
wait 1000
combo ShiftKey+E

# Przykład 4: przytrzymaj Space przez 500 ms
wait 1000
hold Space 500

# Przykład 5: wpisz tekst
wait 1000
text hello

# Przykład 6: Ctrl + Shift + S
wait 1000
combo ControlKey+ShiftKey+S";
        }

        private async void StartButton_Click(object sender, EventArgs e)
        {
            if (!running)
            {
                List<SequenceAction> actions;

                try
                {
                    actions = ParseSequence(sequenceBox.Text);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Błąd sekwencji");
                    return;
                }

                if (actions.Count == 0)
                {
                    MessageBox.Show("Sekwencja jest pusta.", "Błąd");
                    return;
                }

                int repeatDelay =
                    ((int)repeatMinutesBox.Value * 60 * 1000) +
                    ((int)repeatSecondsBox.Value * 1000) +
                    (int)repeatMillisecondsBox.Value;

                cancellationSource = new CancellationTokenSource();

                running = true;
                startButton.Text = "STOP";
                startButton.BackColor = Color.FromArgb(220, 70, 70);
                statusLabel.Text = "Status: działa";

                try
                {
                    await RunLoop(actions, repeatDelay, cancellationSource.Token);
                }
                catch (TaskCanceledException)
                {
                }
                finally
                {
                    StopProgram();
                }
            }
            else
            {
                cancellationSource?.Cancel();
                StopProgram();
            }
        }

        private async Task RunLoop(List<SequenceAction> actions, int repeatDelay, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                foreach (SequenceAction action in actions)
                {
                    token.ThrowIfCancellationRequested();

                    if (action.Type == ActionType.Press)
                    {
                        statusLabel.Text = "Naciskam: " + action.Value;
                        PressKey(action.Value, (int)keyHoldBox.Value);
                    }
                    else if (action.Type == ActionType.Wait)
                    {
                        statusLabel.Text = "Czekam: " + action.Delay + " ms";
                        await Task.Delay(action.Delay, token);
                    }
                    else if (action.Type == ActionType.Combo)
                    {
                        statusLabel.Text = "Kombinacja: " + action.Value;
                        PressCombo(action.Value, (int)keyHoldBox.Value);
                    }
                    else if (action.Type == ActionType.Hold)
                    {
                        statusLabel.Text = "Trzymam: " + action.Value + " przez " + action.Delay + " ms";
                        HoldKey(action.Value, action.Delay);
                    }
                    else if (action.Type == ActionType.Text)
                    {
                        statusLabel.Text = "Wpisuję tekst";
                        SendKeys.SendWait(action.Value);
                    }

                    await Task.Delay(20, token);
                }

                if (repeatDelay > 0)
                {
                    statusLabel.Text = "Przerwa po sekwencji: " + repeatDelay + " ms";
                    await Task.Delay(repeatDelay, token);
                }
            }
        }

        private void StopProgram()
        {
            running = false;
            startButton.Text = "START";
            startButton.BackColor = Color.FromArgb(70, 130, 255);
            statusLabel.Text = "Status: zatrzymany";
        }

        private List<SequenceAction> ParseSequence(string text)
        {
            List<SequenceAction> actions = new List<SequenceAction>();
            string[] lines = text.Replace("\r", "").Split('\n');

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();

                if (string.IsNullOrWhiteSpace(line))
                    continue;

                if (line.StartsWith("#"))
                    continue;

                string[] parts = line.Split(new char[] { ' ' }, 3, StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length == 0)
                    continue;

                string command = parts[0].ToLower();

                if (command == "press")
                {
                    if (parts.Length < 2)
                        throw new Exception("Linia " + (i + 1) + ": użyj np. press E");

                    ValidateKey(parts[1]);
                    actions.Add(new SequenceAction(ActionType.Press, parts[1], 0));
                }
                else if (command == "wait" || command == "delay")
                {
                    if (parts.Length < 2 || !int.TryParse(parts[1], out int delay) || delay < 0)
                        throw new Exception("Linia " + (i + 1) + ": użyj np. wait 1000");

                    actions.Add(new SequenceAction(ActionType.Wait, "", delay));
                }
                else if (command == "combo")
                {
                    if (parts.Length < 2)
                        throw new Exception("Linia " + (i + 1) + ": użyj np. combo ControlKey+A");

                    ValidateCombo(parts[1]);
                    actions.Add(new SequenceAction(ActionType.Combo, parts[1], 0));
                }
                else if (command == "hold")
                {
                    if (parts.Length < 3)
                        throw new Exception("Linia " + (i + 1) + ": użyj np. hold Space 500");

                    ValidateKey(parts[1]);

                    if (!int.TryParse(parts[2], out int holdTime) || holdTime < 0)
                        throw new Exception("Linia " + (i + 1) + ": czas trzymania musi być liczbą, np. hold Space 500");

                    actions.Add(new SequenceAction(ActionType.Hold, parts[1], holdTime));
                }
                else if (command == "text")
                {
                    if (parts.Length < 2)
                        throw new Exception("Linia " + (i + 1) + ": użyj np. text hello");

                    string value = line.Substring(5);
                    actions.Add(new SequenceAction(ActionType.Text, value, 0));
                }
                else
                {
                    throw new Exception("Linia " + (i + 1) + ": nieznana komenda: " + command);
                }
            }

            return actions;
        }

        private void ValidateCombo(string combo)
        {
            string[] keys = combo.Split('+');

            if (keys.Length < 2)
                throw new Exception("Kombinacja musi mieć minimum 2 klawisze, np. combo ControlKey+A");

            foreach (string key in keys)
            {
                ValidateKey(key.Trim());
            }
        }

        private void ValidateKey(string keyText)
        {
            ParseKey(keyText);
        }

        private Keys ParseKey(string keyText)
        {
            string normalized = keyText.Trim();

            if (normalized.Equals("ctrl", StringComparison.OrdinalIgnoreCase))
                normalized = "ControlKey";

            if (normalized.Equals("control", StringComparison.OrdinalIgnoreCase))
                normalized = "ControlKey";

            if (normalized.Equals("shift", StringComparison.OrdinalIgnoreCase))
                normalized = "ShiftKey";

            if (normalized.Equals("alt", StringComparison.OrdinalIgnoreCase))
                normalized = "Menu";

            if (normalized.Equals("esc", StringComparison.OrdinalIgnoreCase))
                normalized = "Escape";

            if (normalized.Equals("spacja", StringComparison.OrdinalIgnoreCase))
                normalized = "Space";

            try
            {
                return (Keys)Enum.Parse(typeof(Keys), normalized, true);
            }
            catch
            {
                throw new Exception("Nieznany klawisz: " + keyText);
            }
        }

        private void PressKey(string keyText, int holdTime)
        {
            Keys key = ParseKey(keyText);
            KeyDown(key);
            Thread.Sleep(holdTime);
            KeyUp(key);
        }

        private void HoldKey(string keyText, int holdTime)
        {
            Keys key = ParseKey(keyText);
            KeyDown(key);
            Thread.Sleep(holdTime);
            KeyUp(key);
        }

        private void PressCombo(string comboText, int holdTime)
        {
            string[] keyTexts = comboText.Split('+');
            List<Keys> keys = new List<Keys>();

            foreach (string keyText in keyTexts)
            {
                keys.Add(ParseKey(keyText));
            }

            foreach (Keys key in keys)
            {
                KeyDown(key);
                Thread.Sleep(10);
            }

            Thread.Sleep(holdTime);

            for (int i = keys.Count - 1; i >= 0; i--)
            {
                KeyUp(keys[i]);
                Thread.Sleep(10);
            }
        }

        private void KeyDown(Keys key)
        {
            keybd_event((byte)key, 0, 0, UIntPtr.Zero);
        }

        private void KeyUp(Keys key)
        {
            keybd_event((byte)key, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
        }

        private enum ActionType
        {
            Press,
            Wait,
            Combo,
            Hold,
            Text
        }

        private class SequenceAction
        {
            public ActionType Type { get; }
            public string Value { get; }
            public int Delay { get; }

            public SequenceAction(ActionType type, string value, int delay)
            {
                Type = type;
                Value = value;
                Delay = delay;
            }
        }
    }
}
