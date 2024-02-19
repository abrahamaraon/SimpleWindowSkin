/* 
 * NAME:   Simple Windows Skin
 * AUTHOR: Abraham Herrera Leiva
 * 
 * DESCRIPTION: This User-Control self-create a set of images to make appear like a
 *              Window Skin, those images are from 0 to 21, very easy to change to another
 *              images to set new skin.
 *              
 * DISCLAIMER:  This code as NOT waranty of any type and is shared "as is".
 * 
 * ** FEEL FREE TO EDIT, CLEAN UP, INCREASE FUNCTIONALITIES **
 * 
 * This is not a final work so any ideas please contact me to update this code.
 * 
 * 
 * 
 * 
 * HOW TO START: Compile this proyect, then add this control to you Form and change some properties
 *               like useAsWindowSkin=true, then you can use the Button Events (CloseButton_Click, ...)
 *               (Try to double click on title bar, I was planning to animate that)
 * 
 */
using ControlWindowSkin.Properties;
using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Resources;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ControlWindowSkin
{
    [Designer("System.Windows.Forms.Design.ParentControlDesigner, System.Design", typeof(IDesigner))]
    public partial class SimpleWindowSkin : UserControl
    {
        private PictureBox[] ImgWindowBox = new PictureBox[23];     // Array of PictureBoxes to load window skin
        private bool TheImgWasLoaded = false;                       // True if all images were loaded from resources
        private int marginStartImgTitleButtons = 4;                 // margin from ImgWindowBox(2).Left to start positioning the buttons from right to left
        private int marginImgTitleButtons = 4;                      // margin between title bar buttons
        private int lastHeightControl = 0;                          // set the last control height to perform the roll down
        private bool RolledUp { get; set; }                         // Set the control rolled up or down
        private bool isInDesignMode = false;                        // useless
        private Rectangle LastSize = Rectangle.Empty;               // save last position and size on container to restaurate
        private FormBorderStyle LastBorder = FormBorderStyle.None;  // save last container BorderStyle to restaure
        private bool useAsWindowSkin = false;                       // store user preference
        private bool useCloseButton = true;                         // store user preference
        private bool useMaxRestButton = true;                       // store user preference
        private bool useMinimizeButton = true;                      // store user preference
        private bool useHelpButton = true;                          // store user preference
        private bool useTopMostButton = true;                       // store user preference

        #region Declara las opciones de usuario del control
        [
            //DisplayName(""),
            Category("Ajustes"),
            DefaultValue(false),
            Description("Cambia el comportamiento del control a <skin>piel de ventana, esto modificará su tamaño y propiedades de su contenedor.")
        ]
        public bool UseAsWindowSkin
        {
            get { return this.useAsWindowSkin; }
            set {
                this.useAsWindowSkin = value;
                if (value == true)
                {
                    // modify!
                    LastSize = new Rectangle(this.Top, this.Left, this.Width, this.Height);
                    PrepareParentForm();
                }
                else
                {
                    // restaure!
                    RestaureParentForm();
                }
            }
        }
        [
            //DisplayName(""),
            Category("Ajustes"),
            DefaultValue(true),
            Description("Muestra u oculta el boton \"Cerrar\" de la barra de titulo del control")
        ]
        public bool UseCloseButton
        {
            get { return this.useCloseButton; }
            set {
                this.useCloseButton = value;
                SetImgProperties();
            }
        }
        [
            //DisplayName(""),
            Category("Ajustes"),
            DefaultValue(true),
            Description("Muestra u oculta los botones \"Maximizar\" y \"Restaurar\" de la barra de titulo del control")
        ]
        public bool UseMaxRestButton
        {
            get { return this.useMaxRestButton; }
            set {
                this.useMaxRestButton = value;
                SetImgProperties();
            }
        }
        [
            //DisplayName(""),
            Category("Ajustes"),
            DefaultValue(true),
            Description("Muestra u oculta el boton \"Minimizar\" de la barra de titulo del control")
        ]
        public bool UseMinimizeButton
        {
            get { return this.useMinimizeButton; }
            set {
                this.useMinimizeButton = value;
                SetImgProperties();
            }
        }
        [
            //DisplayName(""),
            Category("Ajustes"),
            DefaultValue(true),
            Description("Muestra u oculta el boton \"Ayuda\" de la barra de titulo del control")
        ]
        public bool UseHelpButton
        {
            get { return this.useHelpButton; }
            set {
                this.useHelpButton = value;
                SetImgProperties();
            }
        }
        [
            //DisplayName(""),
            Category("Ajustes"),
            DefaultValue(true),
            Description("Muestra u oculta el boton \"Siempre Arriba\" de la barra de titulo del control")
        ]
        public bool UseTopMostButton
        {
            get { return this.useTopMostButton; }
            set {
                this.useTopMostButton = value;
                SetImgProperties();
            }
        }
        #endregion

        private enum SWS_TITLE_BUTTONS : int
        {
            WS_NO_BUTTON = 0,
            WS_CLOSE_BUTTON = 8,
            WS_MAXIMIZE_BUTTON = 10,
            WS_RESTORE_BUTTON = 12,
            WS_MINIMIZE_BUTTON = 14,
            WS_HELP_BUTTON = 16,
            WS_TOPMOST_BUTTON = 18,
            WS_TOPMOST_ACTIVATED_BUTTON = 20,
        }

        #region Declare public control events
        public event EventHandler CloseButton_Click;
        public event EventHandler MaximizeButton_Click;
        public event EventHandler RestoreButton_Click;
        public event EventHandler MinimizeButton_Click;
        public event EventHandler HelpButton_Click;
        public event EventHandler TopMostButton_Click;
        public event EventHandler NotTopMostButton_Click;
        #endregion

        public SimpleWindowSkin()
        {
            InitializeComponent();
        }

        // remember this procedure to change it when you need make control skins
        /*
        private void MakeTransparentControls(Control parent)
        {
            if (parent.Controls != null && parent.Controls.Count > 0)
            {
                foreach (Control control in parent.Controls)
                {
                    if ((control is System.Windows.Forms.PictureBox) || (control is System.Windows.Forms.Label) || (control is System.Windows.Forms.GroupBox) || (control is System.Windows.Forms.CheckBox))
                        control.BackColor = Color.Beige;

                    if (control.Controls != null && control.Controls.Count > 0)
                        MakeTransparentControls(control);
                }
            }
        }
        */

        private void ResizeImgToNewDimension()
        {
            if (!TheImgWasLoaded) { return; }
            if (useAsWindowSkin)
            {
                this.Top = 0;
                this.Left = 0;
                this.Width = this.Parent.Width;
                this.Height = this.Parent.Height;
            }
            ImgWindowBox[1].Width = this.Width - ImgWindowBox[0].Width - ImgWindowBox[2].Width;
            ImgWindowBox[3].Height = this.Height - ImgWindowBox[0].Height - ImgWindowBox[5].Height;
            ImgWindowBox[4].Height = this.Height - ImgWindowBox[2].Height - ImgWindowBox[7].Height;
            ImgWindowBox[6].Width = this.Width - ImgWindowBox[5].Width - ImgWindowBox[7].Width;
        }

        #region Configuracion de todos los eventos del control
        private void SetImgEvents()
        {
            if (!TheImgWasLoaded) { return; }
            // Title bar
            ImgWindowBox[1].MouseDown += new System.Windows.Forms.MouseEventHandler(this.MoveControlOrParent);      // move the control or parent 
            ImgWindowBox[1].MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.ControlRollUpDown); // roll up or down the control
            for (int i = 8; i <= 20; i++)
            {
                ImgWindowBox[i].Tag = (SWS_TITLE_BUTTONS)i;
                ImgWindowBox[i].MouseDown += new MouseEventHandler(this.TitleButtonsEvent_MouseDownOrUp);
                ImgWindowBox[i].MouseUp += new MouseEventHandler(this.TitleButtonsEvent_MouseDownOrUp);
                ImgWindowBox[i].MouseCaptureChanged += new EventHandler(this.TitleButtonsEvent_MouseDownOrUp);
                ImgWindowBox[i].MouseClick += new MouseEventHandler(this.TitleButtonsEvent_Click);
            }
            // Borders
            ImgWindowBox[0].MouseDown += new MouseEventHandler(this.StartResize);
            ImgWindowBox[0].Tag = TYPE_SIZE.TS_LEFT_TOP;
            ImgWindowBox[2].MouseDown += new MouseEventHandler(this.StartResize);
            ImgWindowBox[2].Tag = TYPE_SIZE.TS_RIGHT_TOP;
            ImgWindowBox[3].MouseDown += new MouseEventHandler(this.StartResize);
            ImgWindowBox[3].Tag = TYPE_SIZE.TS_LEFT;
            ImgWindowBox[4].MouseDown += new MouseEventHandler(this.StartResize);
            ImgWindowBox[4].Tag = TYPE_SIZE.TS_RIGHT;
            ImgWindowBox[5].MouseDown += new MouseEventHandler(this.StartResize);
            ImgWindowBox[5].Tag = TYPE_SIZE.TS_LEFT_BOTTOM;
            ImgWindowBox[6].MouseDown += new MouseEventHandler(this.StartResize);
            ImgWindowBox[6].Tag = TYPE_SIZE.TS_BOTTOM;
            ImgWindowBox[7].MouseDown += new MouseEventHandler(this.StartResize);
            ImgWindowBox[7].Tag = TYPE_SIZE.TS_RIGHT_BOTTOM;
        }
        #endregion

        private void StartResize(object sender, MouseEventArgs e)
        {
            if (!useAsWindowSkin) { return; } // just resize parent - use only when useAsWindowSkin
            if ((e.Button == MouseButtons.Left) && (e.Clicks < 2))
            {
                if (this.Parent == null) { return; }
                IntPtr hWnd = this.Parent.Handle;
                PictureBox Sender = (PictureBox)sender;
                ReleaseCapture();
                SendMessage(hWnd, WM_SYSCOMMAND, (int)Sender.Tag, 0);
            }
        }

        private void TitleButtonsEvent_MouseDownOrUp(object sender, MouseEventArgs e)
        {
            if ((e.Button == MouseButtons.Left) & (sender.GetType() == typeof(PictureBox)))
            {
                PictureBox Sender = (PictureBox)sender;
                //ImgWindowBox[(int)Sender.Tag + 1].Visible = !ImgWindowBox[(int)Sender.Tag + 1].Visible;
                if (((int)Sender.Tag % 2) == 0)
                {
                    ImgWindowBox[(int)Sender.Tag].Image = ImgWindowBox[(int)Sender.Tag + 1].Image;
                    ImgWindowBox[(int)Sender.Tag].Tag = (int)Sender.Tag + 1;
                }
                else
                {
                    ResourceManager rm = Resources.ResourceManager;
                    ImgWindowBox[(int)Sender.Tag - 1].Image = (Bitmap)rm.GetObject("" + ((int)Sender.Tag - 1));
                    ImgWindowBox[(int)Sender.Tag - 1].Tag = (int)Sender.Tag - 1;
                }
            }
        }

        private void TitleButtonsEvent_MouseDownOrUp(object sender, EventArgs e)
        {
            // Fix for losing focus when mouse down (mouseUp event does not fires)
            if (sender.GetType() != typeof(PictureBox)) { return; }
            PictureBox Sender = (PictureBox)sender;
            if (((int)Sender.Tag % 2) != 0) // verify if the Img-object has the "pressed" picture
            {
                MouseEventArgs f = new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0);
                TitleButtonsEvent_MouseDownOrUp((object)sender, f);
            }
        }

        private void TitleButtonsEvent_Click(object sender, MouseEventArgs e)
        {
            if ((e.Button == MouseButtons.Left) & (sender.GetType() == typeof(PictureBox)))
            {
                PictureBox Sender = (PictureBox)sender;
                switch ((SWS_TITLE_BUTTONS)Sender.Tag)
                {
                    case SWS_TITLE_BUTTONS.WS_CLOSE_BUTTON:
                    case SWS_TITLE_BUTTONS.WS_CLOSE_BUTTON + 1:
                        if (this.CloseButton_Click != null) this.CloseButton_Click(this, new EventArgs());
                        Console.WriteLine("close button click!");
                        break;
                    case SWS_TITLE_BUTTONS.WS_MAXIMIZE_BUTTON:
                    case SWS_TITLE_BUTTONS.WS_MAXIMIZE_BUTTON + 1:
                        ImgWindowBox[(int)SWS_TITLE_BUTTONS.WS_MAXIMIZE_BUTTON].Visible = false;
                        ImgWindowBox[(int)SWS_TITLE_BUTTONS.WS_RESTORE_BUTTON].Visible = true;
                        if (this.MaximizeButton_Click != null) this.MaximizeButton_Click(this, new EventArgs());
                        Console.WriteLine("maximize click!");
                        break;
                    case SWS_TITLE_BUTTONS.WS_RESTORE_BUTTON:
                    case SWS_TITLE_BUTTONS.WS_RESTORE_BUTTON + 1:
                        ImgWindowBox[(int)SWS_TITLE_BUTTONS.WS_MAXIMIZE_BUTTON].Visible = true;
                        ImgWindowBox[(int)SWS_TITLE_BUTTONS.WS_RESTORE_BUTTON].Visible = false;
                        if (this.RestoreButton_Click != null) this.RestoreButton_Click(this, new EventArgs());
                        Console.WriteLine("restore click!");
                        break;
                    case SWS_TITLE_BUTTONS.WS_MINIMIZE_BUTTON:
                    case SWS_TITLE_BUTTONS.WS_MINIMIZE_BUTTON + 1:
                        Console.WriteLine("minimize click!");
                        if (this.MinimizeButton_Click != null) this.MinimizeButton_Click(this, new EventArgs());
                        break;
                    case SWS_TITLE_BUTTONS.WS_HELP_BUTTON:
                    case SWS_TITLE_BUTTONS.WS_HELP_BUTTON + 1:
                        if (this.HelpButton_Click != null) this.HelpButton_Click(this, new EventArgs());
                        Console.WriteLine("help click!");
                        break;
                    case SWS_TITLE_BUTTONS.WS_TOPMOST_BUTTON:
                    case SWS_TITLE_BUTTONS.WS_TOPMOST_BUTTON + 1:
                        ImgWindowBox[(int)SWS_TITLE_BUTTONS.WS_TOPMOST_BUTTON].Visible = false;
                        ImgWindowBox[(int)SWS_TITLE_BUTTONS.WS_TOPMOST_ACTIVATED_BUTTON].Visible = true;
                        if (this.TopMostButton_Click != null) this.TopMostButton_Click(this, new EventArgs());
                        Console.WriteLine("topMost click!");
                        break;
                    case SWS_TITLE_BUTTONS.WS_TOPMOST_ACTIVATED_BUTTON:
                    case SWS_TITLE_BUTTONS.WS_TOPMOST_ACTIVATED_BUTTON + 1:
                        ImgWindowBox[(int)SWS_TITLE_BUTTONS.WS_TOPMOST_BUTTON].Visible = true;
                        ImgWindowBox[(int)SWS_TITLE_BUTTONS.WS_TOPMOST_ACTIVATED_BUTTON].Visible = false;
                        if (this.NotTopMostButton_Click != null) this.NotTopMostButton_Click(this, new EventArgs());
                        Console.WriteLine("topMost activated click!");
                        break;
                    default:
                        break;
                }
            }
        }

        #region  Configuracion de todas las imagenes del control
        private void SetImgProperties()
        {
            if (!TheImgWasLoaded) { return; }

            // Set padding margins before positioning images (just for reference)
            this.Padding = new Padding(ImgWindowBox[0].Width + marginStartImgTitleButtons, ImgWindowBox[1].Height + marginStartImgTitleButtons, ImgWindowBox[2].Width + marginStartImgTitleButtons, ImgWindowBox[6].Height + marginStartImgTitleButtons);

            //Set control minimun size
            int minW = ImgWindowBox[0].Width + ImgWindowBox[1].Image.Width + ImgWindowBox[2].Width +
                       (useCloseButton ? ImgWindowBox[8].Width : 0) +
                       (useMaxRestButton ? ImgWindowBox[10].Width : 0) +
                       (useMinimizeButton ? ImgWindowBox[14].Width : 0) +
                       (useHelpButton ? ImgWindowBox[16].Width : 0) +
                       (useTopMostButton ? ImgWindowBox[18].Width : 0) +
                       marginStartImgTitleButtons;
            this.MinimumSize = new Size(minW, (ImgWindowBox[0].Height + marginStartImgTitleButtons + ImgWindowBox[5].Height));

            //Title bar images
            ImgWindowBox[0].Top = 0;
            ImgWindowBox[0].Left = 0;
            ImgWindowBox[0].Anchor = AnchorStyles.Left | AnchorStyles.Top;
            ImgWindowBox[0].Cursor = Cursors.SizeNWSE;
            ImgWindowBox[0].BringToFront();

            ImgWindowBox[1].Top = 0;
            ImgWindowBox[1].Left = ImgWindowBox[0].Width;
            ImgWindowBox[1].BackgroundImageLayout = ImageLayout.Tile;
            ImgWindowBox[1].Cursor = Cursors.Default;
            ImgWindowBox[1].Anchor = AnchorStyles.Left | AnchorStyles.Top;

            ImgWindowBox[2].Top = 0;
            ImgWindowBox[2].Left = this.Width - ImgWindowBox[2].Width;
            ImgWindowBox[2].Anchor = AnchorStyles.Right | AnchorStyles.Top;
            ImgWindowBox[2].Cursor = Cursors.SizeNESW;
            ImgWindowBox[2].BringToFront();

            //left and right images
            ImgWindowBox[3].Top = ImgWindowBox[0].Height;
            ImgWindowBox[3].Left = 0;
            ImgWindowBox[3].BackgroundImageLayout = ImageLayout.Tile;
            ImgWindowBox[3].Anchor = AnchorStyles.Left | AnchorStyles.Top;
            ImgWindowBox[3].Cursor = Cursors.SizeWE;

            ImgWindowBox[4].Top = ImgWindowBox[2].Height;
            ImgWindowBox[4].Left = this.Width - ImgWindowBox[4].Width;
            ImgWindowBox[4].BackgroundImageLayout = ImageLayout.Tile;
            ImgWindowBox[4].Anchor = AnchorStyles.Right | AnchorStyles.Top;
            ImgWindowBox[4].Cursor = Cursors.SizeWE;

            //Bottom images
            ImgWindowBox[5].Top = this.Height - ImgWindowBox[5].Height;
            ImgWindowBox[5].Left = 0;
            ImgWindowBox[5].Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
            ImgWindowBox[5].Cursor = Cursors.SizeNESW;
            ImgWindowBox[5].BringToFront();

            ImgWindowBox[6].Top = this.Height - ImgWindowBox[6].Height;
            ImgWindowBox[6].Left = ImgWindowBox[5].Width;
            ImgWindowBox[6].BackgroundImageLayout = ImageLayout.Tile;
            ImgWindowBox[6].Cursor = Cursors.SizeNS;
            ImgWindowBox[6].Anchor = AnchorStyles.Left | AnchorStyles.Bottom;

            ImgWindowBox[7].Top = this.Height - ImgWindowBox[7].Height;
            ImgWindowBox[7].Left = this.Width - ImgWindowBox[7].Width;
            ImgWindowBox[7].Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            ImgWindowBox[7].Cursor = Cursors.SizeNWSE;
            ImgWindowBox[7].BringToFront();

            // Default hide all button images
            for (int i = 8; i <= 21; i++)
            {
                ImgWindowBox[i].Visible = false;
            }

            int titleButtonsNextPositioning = ImgWindowBox[2].Left;
            int middleHeight = (ImgWindowBox[1].Height / 2) - (ImgWindowBox[8].Height / 2);
            if (useCloseButton)
            {
                //Close button
                ImgWindowBox[8].Visible = true;
                ImgWindowBox[8].Top = middleHeight;
                ImgWindowBox[8].Left = titleButtonsNextPositioning - ImgWindowBox[8].Width - marginStartImgTitleButtons;
                ImgWindowBox[8].Anchor = AnchorStyles.Right | AnchorStyles.Top;
                ImgWindowBox[8].BringToFront();

                //Close button pressed
                //ImgWindowBox[9].Visible = false;
                ImgWindowBox[9].Top = ImgWindowBox[8].Top;
                ImgWindowBox[9].Left = ImgWindowBox[8].Left;
                ImgWindowBox[9].Anchor = ImgWindowBox[8].Anchor;
                ImgWindowBox[9].BringToFront();

                titleButtonsNextPositioning = ImgWindowBox[8].Left;
            }

            if (useMaxRestButton)
            {
                //Maximize button
                ImgWindowBox[10].Visible = true;
                ImgWindowBox[10].Top = middleHeight;
                ImgWindowBox[10].Left = titleButtonsNextPositioning - ImgWindowBox[10].Width - marginImgTitleButtons;
                ImgWindowBox[10].Anchor = AnchorStyles.Right | AnchorStyles.Top;
                ImgWindowBox[10].BringToFront();

                //Maximize button pressed
                //ImgWindowBox[11].Visible = false;
                ImgWindowBox[11].Top = ImgWindowBox[10].Top;
                ImgWindowBox[11].Left = ImgWindowBox[10].Left;
                ImgWindowBox[11].Anchor = ImgWindowBox[10].Anchor;
                ImgWindowBox[11].BringToFront();

                //Restore button
                //ImgWindowBox[12].Visible = false;
                ImgWindowBox[12].Top = ImgWindowBox[10].Top;
                ImgWindowBox[12].Left = ImgWindowBox[10].Left;
                ImgWindowBox[12].Anchor = ImgWindowBox[10].Anchor;
                ImgWindowBox[12].BringToFront();

                //Restore button pressed
                //ImgWindowBox[13].Visible = false;
                ImgWindowBox[13].Top = ImgWindowBox[10].Top;
                ImgWindowBox[13].Left = ImgWindowBox[10].Left;
                ImgWindowBox[13].Anchor = ImgWindowBox[10].Anchor;
                ImgWindowBox[13].BringToFront();

                titleButtonsNextPositioning = ImgWindowBox[10].Left;
            }

            if (useMinimizeButton)
            {
                //Minimize button
                ImgWindowBox[14].Visible = true;
                ImgWindowBox[14].Top = middleHeight;
                ImgWindowBox[14].Left = titleButtonsNextPositioning - ImgWindowBox[14].Width - marginImgTitleButtons;
                ImgWindowBox[14].Anchor = AnchorStyles.Right | AnchorStyles.Top;
                ImgWindowBox[14].BringToFront();

                //Minimize button pressed
                //ImgWindowBox[15].Visible = false;
                ImgWindowBox[15].Top = ImgWindowBox[14].Top;
                ImgWindowBox[15].Left = ImgWindowBox[14].Left;
                ImgWindowBox[15].Anchor = ImgWindowBox[14].Anchor;
                ImgWindowBox[15].BringToFront();

                titleButtonsNextPositioning = ImgWindowBox[14].Left;
            }

            if (useHelpButton)
            {
                //Help button
                ImgWindowBox[16].Visible = true;
                ImgWindowBox[16].Top = middleHeight;
                ImgWindowBox[16].Left = titleButtonsNextPositioning - ImgWindowBox[16].Width - marginImgTitleButtons;
                ImgWindowBox[16].Anchor = AnchorStyles.Right | AnchorStyles.Top;
                ImgWindowBox[16].BringToFront();

                //Help button pressed
                //ImgWindowBox[17].Visible = false;
                ImgWindowBox[17].Top = ImgWindowBox[16].Top;
                ImgWindowBox[17].Left = ImgWindowBox[16].Left;
                ImgWindowBox[17].Anchor = ImgWindowBox[16].Anchor;
                ImgWindowBox[17].BringToFront();

                titleButtonsNextPositioning = ImgWindowBox[16].Left;
            }

            if (useTopMostButton)
            {
                //TopMost button
                middleHeight = (ImgWindowBox[1].Height / 2) - (ImgWindowBox[18].Height / 2); //TopMost button may be smaller
                ImgWindowBox[18].Visible = true;
                ImgWindowBox[18].Top = middleHeight;
                ImgWindowBox[18].Left = titleButtonsNextPositioning - ImgWindowBox[18].Width - marginImgTitleButtons;
                ImgWindowBox[18].Anchor = AnchorStyles.Right | AnchorStyles.Top;
                ImgWindowBox[18].BringToFront();

                //TopMost button pressed
                //ImgWindowBox[19].Visible = false;
                ImgWindowBox[19].Top = ImgWindowBox[18].Top;
                ImgWindowBox[19].Left = ImgWindowBox[18].Left;
                ImgWindowBox[19].Anchor = ImgWindowBox[18].Anchor;
                ImgWindowBox[19].BringToFront();

                //TopMost activated button
                //ImgWindowBox[20].Visible = false;
                ImgWindowBox[20].Top = ImgWindowBox[18].Top;
                ImgWindowBox[20].Left = ImgWindowBox[18].Left;
                ImgWindowBox[20].Anchor = ImgWindowBox[18].Anchor;
                ImgWindowBox[20].BringToFront();

                //TopMost activated button pressed
                //ImgWindowBox[21].Visible = false;
                ImgWindowBox[21].Top = ImgWindowBox[18].Top;
                ImgWindowBox[21].Left = ImgWindowBox[18].Left;
                ImgWindowBox[21].Anchor = ImgWindowBox[18].Anchor;
                ImgWindowBox[21].BringToFront();

                titleButtonsNextPositioning = ImgWindowBox[18].Left;
            }

            // -->
        }
        #endregion

        private void PrepareParentForm()
        {
            if (this.Parent == null) { return; }
            if (this.Parent.Handle == null)
            {
                throw new InvalidOperationException("the client has not a valid hWnd or Handler");
            }
            else
            {
                //Modify some visual configs from Parent Control
                Form MyForm = this.Parent.FindForm();
                LastBorder = MyForm.FormBorderStyle; // save last border style
                MyForm.FormBorderStyle = FormBorderStyle.None;
                MyForm.MinimumSize = this.MinimumSize;
                this.Top = 0;
                this.Left = 0;
                this.Width = this.Parent.Width;
                this.Height = this.Parent.Height;
            }
        }

        private void RestaureParentForm()
        {
            if (!TheImgWasLoaded) { return; }
            if (this.Parent == null) { return; }
            if (this.Parent.Handle == null)
            {
                throw new InvalidOperationException("the client has not a valid hWnd or Handler");
            }
            else
            {
                //Modify some visual configs from Parent Control
                Form MyForm = this.Parent.FindForm();
                MyForm.FormBorderStyle = LastBorder;
                if (LastSize != Rectangle.Empty)
                {
                    this.Top = LastSize.X;
                    this.Left = LastSize.Y;
                    this.Width = LastSize.Width;
                    this.Height = LastSize.Height;
                }
            }
        }

        private void LoadImgFromRes()
        {
            // Add picture boxes to control
            ResourceManager rm = Resources.ResourceManager;

            for (int i = 0; i < ImgWindowBox.Length; i++)
            {
                ImgWindowBox[i] = new PictureBox {
                    Image = (Bitmap)rm.GetObject("" + i),
                    BackgroundImage = (Bitmap)rm.GetObject("" + i),
                    SizeMode = PictureBoxSizeMode.AutoSize,
                    BackgroundImageLayout = ImageLayout.None,

                    // If you want to add it to the form:
                    Top = 20 * i,
                    Left = 20
                };

                this.Controls.Add(ImgWindowBox[i]);
            }
            TheImgWasLoaded = true;
        }

        private void SimpleWindowSkin_Resize(object sender, EventArgs e)
        {
            // Change the size of top, left, right and bottom images to enlarge
            ResizeImgToNewDimension();
        }

        private void SimpleWindowSkin_Load(object sender, EventArgs e)
        {
            isInDesignMode = (LicenseManager.UsageMode == LicenseUsageMode.Designtime);
            if (isInDesignMode)
            {
                this.BackColor = Color.Aqua;
                return;
            }
            LoadImgFromRes();
            SetImgProperties();
            SetImgEvents();
            ResizeImgToNewDimension();
            if (useAsWindowSkin)
            {
                PrepareParentForm();
                this.Parent.Resize += new EventHandler(SimpleWindowSkin_Resize);
            }
            lastHeightControl = this.Height;
        }

        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HT_CAPTION = 0x2;
        private const int WM_SYSCOMMAND = 0x0112;
        private enum TYPE_SIZE
        {
            TS_LEFT = 0xF001,
            TS_RIGHT = 0xF002,
            TS_TOP = 0xF003,
            TS_LEFT_TOP = 0xF004,
            TS_RIGHT_TOP = 0xF005,
            TS_BOTTOM = 0xF006,
            TS_LEFT_BOTTOM = 0xF007,
            TS_RIGHT_BOTTOM = 0xF008,
        }
        [DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();

        private void MoveControlOrParent(object sender, MouseEventArgs e)
        {
            if ((e.Button == MouseButtons.Left) && (e.Clicks < 2))
            {
                if ((this.Parent == null) & (useAsWindowSkin)) { return; }
                IntPtr hWnd = useAsWindowSkin ? this.Parent.Handle : this.Handle;
                ReleaseCapture();
                SendMessage(hWnd, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void ControlRollUpDown(object sender, MouseEventArgs e)
        {
            if (RolledUp)
            {
                this.Height = lastHeightControl;
                if (useAsWindowSkin)
                {
                    this.Parent.Height = lastHeightControl;
                    ResizeImgToNewDimension();
                }
            }
            else
            {
                int minControlHeight = ImgWindowBox[1].Height + ImgWindowBox[5].Height;
                lastHeightControl = this.Height;
                this.Height = minControlHeight;
                if (useAsWindowSkin)
                {
                    this.Parent.Height = minControlHeight;
                    ResizeImgToNewDimension();
                }
            }
            RolledUp = !RolledUp;
        }

    }
}
