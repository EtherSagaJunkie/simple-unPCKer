using System;
using System.IO;
using System.Windows.Forms;

namespace BeySoft
{
    public partial class MainForm : Form
    {
        private PckClass _pck;
        private const int TwoGb = 0x7FFFFF00;
        private const uint AlgoId = 0;

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // Set titlebar
            Text = $@"Simple UnPCKer : v{Program.Version}";

            // Set status info upon loading...
            lblStatus.Text =
                "It's not a bug, it's an undocumented feature!" +
                "\r\n\r\nDon't worry if it doesn't work right, if" +
                "\r\neverything did, I wouldn't have anything to do!";
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void SetFlags()
        {
            if (_pck.FileSize > TwoGb)
            {
                _pck.IsPck = false;
                _pck.IsPkx = true;
            }
            else
            {
                _pck.IsPck = true;
                _pck.IsPkx = false;
            }
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog open = new OpenFileDialog())
            {
                open.Filter = @"PCK Archive (*.pck)|*.pck|All Files (*.*)|*.*";
                open.Title = @"Open PCK Archive";

                if (open.ShowDialog() != DialogResult.OK || !File.Exists(open.FileName))
                    return;

                try
                {
                    Cursor = Cursors.AppStarting;

                    lblStatus.Text = @"Retrieving data...";
                    lblStatus.Refresh();

                    using (FileStream fs = File.OpenRead(open.FileName))
                    {
                        using (BinaryReader br = new BinaryReader(fs))
                        {
                            _pck = new PckClass(open.FileName);

                            fs.Seek(4, SeekOrigin.Begin);
                            uint filesize = br.ReadUInt32();

                            SetFlags();

                            txtFileName.Text = open.FileName;

                            if (_pck.OpenArchive(fs, br, AlgoId) == -1)
                            {
                                lblStatus.Text = "Unsupported format!\nWhen will it be supported?!";
                                Cursor = Cursors.Default;
                                return;
                            }
                        }
                    }

                    long cSize = 0;
                    long dSize = 0;

                    for (int i = 0; i < _pck.EntryCount; i++)
                    {
                        cSize += _pck.FileTable[i].CompressedSize;
                        dSize += _pck.FileTable[i].DecompressedSize;
                    }

                    _pck.CompressedSize = cSize;
                    _pck.DecompressedSize = dSize;

                    Text = $@"Simple UnPCKer : v{Program.Version}  ::  {_pck.GameName}";

                    lblStatus.Text =
                        "Version:  0x" + _pck.Version.ToString("X8") +
                        "\nEntry Count:  " + _pck.EntryCount +
                        "\nCompressed Size:  " + cSize.ToString("##,###,###,###") + " bytes" +
                        "\nDecompressed Size:  " + dSize.ToString("##,###,###,###") + " bytes";

                    Cursor = Cursors.Default;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("The following error occurred while opening:\n" + ex.Message);
                    Cursor = Cursors.Default;
                }
            }
        }

        private void btnExtract_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog folder = new FolderBrowserDialog())
            {
                if (folder.ShowDialog() != DialogResult.OK ||
                    !Directory.Exists(folder.SelectedPath))
                    return;

                btnOpen.Enabled = false;
                btnExtract.Enabled = false;
                btnExit.Enabled = false;

                lblStatus.Text =
                    "Extracting all files from archive.\n" +
                    "Depending on the size of the archive,\n" +
                    "this may take a few minutes...";
                lblStatus.Refresh();

                try
                {
                    Cursor = Cursors.AppStarting;

                    using (FileStream fs = File.OpenRead(txtFileName.Text))
                    {
                        using (BinaryReader br = new BinaryReader(fs))
                        {
                            _pck.ExtractArchive(folder.SelectedPath, fs, br, AlgoId);
                        }
                    }

                    lblStatus.Text =
                        "Finished extracting " + _pck.EntryCount + " files from\n" + _pck.PckName +
                        "\n\nPlease restart to extract more archives.";

                    Cursor = Cursors.Default;
                    MessageBox.Show(@"Extraction complete.");
                }
                catch (Exception ex)
                {
                    lblStatus.Text = "Oh shit! What happened?!\n" + ex.Message;
                    Cursor = Cursors.Default;
                }

                // force user to restart after extraction
                btnExit.Enabled = true;
            }
        }
    }
}
