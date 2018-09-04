using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

namespace SubDelete
{
    public partial class GUIForm : Form
    {
        private Form prompt;
        private TextBox textBox;
        private Button add;
        private int deleted, totalToBeDeleted;
        public string lastIndex = "";

        public GUIForm()
        {
            InitializeComponent();
            checkAndLoadBinaries();
            if (comboBox1.Items.Count > 0)
            {
                comboBox1.SelectedIndex = 0;
                richTextBox1.Text = DeSerializeObject<Item>(comboBox1.SelectedItem.ToString() + ".bin").Value;
            }
            try { textBox1.Text = DeSerializeObject<string>("path.bin");
            } catch (Exception e) { }
        }

        private void checkAndLoadBinaries()
        {
            // Create Or Find Binary Directory To Make Working Directory
            if (!Directory.Exists(Path.GetDirectoryName(Application.ExecutablePath) + @"\bin"))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(Application.ExecutablePath) + @"\bin");
            }
            Directory.SetCurrentDirectory(Path.GetDirectoryName(Application.ExecutablePath) + @"\bin");
            lastIndex = comboBox1.Text;
            comboBox1.Items.Clear();
            // Find all Binaries And Load Them Into Combobox
            String[] allfiles = Directory.GetFiles(Path.GetDirectoryName(Application.ExecutablePath) + @"\bin", "*bin", SearchOption.AllDirectories);
            if (allfiles.Length > 0)
            {
                button4.Enabled = true;
                foreach (string path in allfiles)
                { 
                    string pathHelper = path.Substring(path.LastIndexOf(@"\") + 1);
                    string fileName = pathHelper.Substring(0, pathHelper.LastIndexOf("."));
                    if(!fileName.Equals("path"))
                    comboBox1.Items.Add( fileName );
                }
                comboBox1.SelectedIndex = comboBox1.Items.IndexOf(lastIndex);
                richTextBox1.Enabled = true;
            } else
            {
                button4.Enabled = false;
                richTextBox1.Enabled = false;
            }
        }

        public void recursoDeleteOoo(string Path, string[] formats)
        {
            String[] allfiles;
            if(checkBox1.CheckState == CheckState.Checked) allfiles = Directory.GetFiles(Path, "*.*", SearchOption.AllDirectories);
            else allfiles = Directory.GetFiles(Path, "*.*", SearchOption.TopDirectoryOnly);
            int True = 0;
            totalToBeDeleted = 1;
            deleted = 0;
            foreach (string filePath in allfiles)
            {
                True = 0;
                foreach (string format in formats)
                {
                    if (filePath.ToLower().EndsWith(format))
                    {
                        True = 1;
                        break;
                    }
                }
                if (File.Exists(filePath) && True == 0)
                {
                    totalToBeDeleted++;
                }
            }

            foreach (string filePath in allfiles)
            {
                True = 0;
                foreach (string format in formats)
                {
                    if (filePath.ToLower().EndsWith(format))
                    {
                        True = 1;
                        break;
                    }
                }
                if (File.Exists(filePath) && True == 0)
                {
                    File.Delete(filePath);
                    deleted++;
                    label1.Text = "Files Deleted: " + deleted;
                    progressBar1.Value = (deleted * 100) / totalToBeDeleted;
                }
            }
            progressBar1.Visible = false;
        }

        private void deleteEmptyDirectory(string startLocation)
        {
            foreach (var directory in Directory.GetDirectories(startLocation))
            {
                deleteEmptyDirectory(directory);
                if (Directory.GetFiles(directory).Length == 0 &&
                    Directory.GetDirectories(directory).Length == 0)
                {
                    Directory.Delete(directory, false);
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            saveBinary();
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        public static void SerializeObject<T>(T serializableObject, string fileName)
        {
            if (serializableObject == null) { return; }
            XmlDocument xmlDocument = new XmlDocument();
            XmlSerializer serializer = new XmlSerializer(serializableObject.GetType());
            using (MemoryStream stream = new MemoryStream())
            {
                serializer.Serialize(stream, serializableObject);
                stream.Position = 0;
                xmlDocument.Load(stream);
                xmlDocument.Save(fileName);
                stream.Close();
            }
        }

        public static T DeSerializeObject<T>(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) { return default(T); }
            T objectOut = default(T);
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(fileName);
            string xmlString = xmlDocument.OuterXml;
            using (StringReader read = new StringReader(xmlString))
            {
                Type outType = typeof(T);
                XmlSerializer serializer = new XmlSerializer(outType);
                using (XmlReader reader = new XmlTextReader(read))
                {
                    objectOut = (T)serializer.Deserialize(reader);
                    reader.Close();
                }

                read.Close();
            }
            return objectOut;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (File.Exists(Directory.GetCurrentDirectory() + @"\" + comboBox1.Text + ".bin"))
            {
                File.Delete(Directory.GetCurrentDirectory() + @"\" + comboBox1.Text + ".bin");
            }
            // Save Item
            SerializeObject(new Item(comboBox1.Text, richTextBox1.Text), comboBox1.Text + ".bin");
            string dir = textBox1.Text;
            progressBar1.Visible = true;
            this.Enabled = false;
            recursoDeleteOoo(dir, richTextBox1.Text.Split(new[] { "\n" }, StringSplitOptions.None));
            deleteEmptyDirectory(dir);
            this.Enabled = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            prompt = new Form();
            prompt.Parent = this.Parent;
            prompt.StartPosition = FormStartPosition.CenterParent;
            prompt.MaximizeBox = false;
            prompt.MinimizeBox = false;
            prompt.ShowIcon = false;
            prompt.AutoSize = false;
            prompt.Width = 190;
            prompt.Height = 125;
            prompt.FormBorderStyle = FormBorderStyle.FixedSingle;
            textBox = new TextBox() { Left = 10, Top = 35, Width = 150 };
            textBox.TabIndex = 0;
            textBox.Cursor = Cursors.Default;
            textBox.Multiline = false;
            textBox.AutoSize = false;
            textBox.Cursor = Cursors.IBeam;
            Label label = new Label() { Text = "Enter extension group name:", Left = 10, Top = 10, Width = 150 };
            Button cancel = new Button() { Text = "Cancel", Left = 110, Top = 60 };
            add = new Button() { Text = "Add", Left = 10, Top = 60 };
            prompt.AcceptButton = add;
            add.TabIndex = 1;
            cancel.TabIndex = 2;
            cancel.Click += new EventHandler(cancel_Click);
            add.Click += new EventHandler(add_Click);
            add.Enabled = false;
            textBox.TextChanged += new System.EventHandler(add_TextChanged);
            cancel.AutoSize = true;
            cancel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            cancel.AutoEllipsis = false;
            add.AutoSize = true;
            add.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            add.AutoEllipsis = false;
            prompt.Controls.Add(label);
            prompt.Controls.Add(cancel);
            prompt.Controls.Add(add);
            prompt.Controls.Add(textBox);
            prompt.ShowDialog();

        }

        private void saveBinary()
        {
            if (!lastIndex.Equals(""))
            {
                if (File.Exists(Directory.GetCurrentDirectory() + @"\" + lastIndex + ".bin"))
                {
                    File.Delete(Directory.GetCurrentDirectory() + @"\" + lastIndex + ".bin");
                }
                // Save Item
                SerializeObject(new Item(lastIndex, richTextBox1.Text), lastIndex + ".bin");
                richTextBox1.Text = "";

            }
            lastIndex = comboBox1.Text;
            checkAndLoadBinaries();
        }

        private void add_Click(object sender, EventArgs e)
        {
            // Add New Item
            saveBinary();
            SerializeObject(new Item(textBox.Text, ""), textBox.Text + ".bin");
            comboBox1.Items.Add(textBox.Text);
            comboBox1.SelectedIndex = comboBox1.Items.IndexOf(textBox.Text);
            lastIndex = comboBox1.Text;
            richTextBox1.Text = "";
            richTextBox1.Enabled = true;
            button4.Enabled = true;
            prompt.Close();
        }

        private void cancel_Click(object sender, EventArgs e)
        {
            prompt.Close();
        }

        private void comboBox1_SelectedIndexChangedConfirmed(object sender, EventArgs e)
        {
            saveBinary();
            if (File.Exists(Directory.GetCurrentDirectory() + @"\" + comboBox1.Text + ".bin"))
            {
                // Load new Item
                richTextBox1.Text = DeSerializeObject<Item>(comboBox1.Text + ".bin").Value;
            }

        }

        private void add_TextChanged(object sender, EventArgs e)
        {
            if (textBox.Text.Equals("")      ||
                textBox.Text.Length >= 248   ||
                textBox.Text.EndsWith(" ")   ||
                textBox.Text.EndsWith(".")   ||
                textBox.Text.Contains("<")   ||
                textBox.Text.Contains(">")   ||
                textBox.Text.Contains(":")   ||
                textBox.Text.Contains("\"")  ||
                textBox.Text.Contains(@"/")  ||
                textBox.Text.Contains(@"\")  ||
                textBox.Text.Contains("|")   ||
                textBox.Text.Contains("?")   ||
                textBox.Text.Contains("*")  ||
                textBox.Text.Equals("path")) add.Enabled = false;
            else add.Enabled = true;
        }
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            SerializeObject<String>(textBox1.Text, "path.bin");
            try
            {
                label1.Text = "Files Deleted: " + "0";
            }
            catch (ArgumentException er) { }

        }

        // Allow Combo Box to center aligned
        private void cbxDesign_DrawItem(object sender, DrawItemEventArgs e)
        {
            // By using Sender, one method could handle multiple ComboBoxes
            ComboBox cbx = sender as ComboBox;
            if (cbx != null)
            {
                // Always draw the background
                e.DrawBackground();

                // Drawing one of the items?
                if (e.Index >= 0)
                {
                    // Set the string alignment.  Choices are Center, Near and Far
                    StringFormat sf = new StringFormat();
                    sf.LineAlignment = StringAlignment.Center;
                    sf.Alignment = StringAlignment.Center;

                    // Set the Brush to ComboBox ForeColor to maintain any ComboBox color settings
                    // Assumes Brush is solid
                    Brush brush = new SolidBrush(cbx.ForeColor);

                    // If drawing highlighted selection, change brush
                    if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
                        brush = SystemBrushes.HighlightText;

                    // Draw the string
                    e.Graphics.DrawString(cbx.Items[e.Index].ToString(), cbx.Font, brush, e.Bounds, sf);
                }
            }
        }

        private void exitProgram(object sender, FormClosedEventArgs e)
        {
            if (comboBox1.Items.Count <= 0) return;
            if (File.Exists(Directory.GetCurrentDirectory() + @"\" + comboBox1.Text + ".bin"))
            {
                File.Delete(Directory.GetCurrentDirectory() + @"\" + comboBox1.Text + ".bin");
            }
            // Save Item
            SerializeObject(new Item(comboBox1.Text, richTextBox1.Text), comboBox1.Text + ".bin");
        }

        private void button4_Click(object sender, EventArgs e)
        {
            int currentIndex = comboBox1.SelectedIndex;
            if (File.Exists(Directory.GetCurrentDirectory() + @"\" + comboBox1.Text + ".bin"))
            {
                File.Delete(Directory.GetCurrentDirectory() + @"\" + comboBox1.Text + ".bin");
            }
            comboBox1.Items.RemoveAt(comboBox1.SelectedIndex);
            if (comboBox1.Items.Count <= 0)
            {
                button4.Enabled = false;
                richTextBox1.Enabled = false;
                return;
            }
            comboBox1.SelectedIndex = (currentIndex - 1);
            if (comboBox1.SelectedIndex < 0) comboBox1.SelectedIndex = 0;
            richTextBox1.Text = "";
            if (File.Exists(Directory.GetCurrentDirectory() + @"\" + comboBox1.Text + ".bin"))
            {
                // Load new Item
                richTextBox1.Text = DeSerializeObject<Item>(comboBox1.Text + ".bin").Value;
            }

        }

    }

    public class Item
    {
        public string Key { get; set; }
        public string Value { get; set; }

        public Item()
        {
            Key = "";
            Value = "";
        }

        public Item(string n, string f)
        {
            Key = n;
            Value = f;
        }
        public override string ToString()
        {
            return Key;
        }
    }
}

