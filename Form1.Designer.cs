namespace HytaleDownloader;

partial class Form1
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    ///  Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }

        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        hytale_location_selector = new System.Windows.Forms.Button();
        hytale_location_show = new System.Windows.Forms.TextBox();
        jre_location_show = new System.Windows.Forms.TextBox();
        jre_location_selector = new System.Windows.Forms.Button();
        progressBar1 = new System.Windows.Forms.ProgressBar();
        usernameb = new System.Windows.Forms.TextBox();
        label2 = new System.Windows.Forms.Label();
        uuid_shower = new System.Windows.Forms.TextBox();
        genuuid = new System.Windows.Forms.Button();
        playbtn = new System.Windows.Forms.Button();
        buylink = new System.Windows.Forms.LinkLabel();
        SuspendLayout();
        //
        // hytale_location_selector
        //
        hytale_location_selector.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right));
        hytale_location_selector.Location = new System.Drawing.Point(489, 12);
        hytale_location_selector.Name = "hytale_location_selector";
        hytale_location_selector.Size = new System.Drawing.Size(167, 37);
        hytale_location_selector.TabIndex = 0;
        hytale_location_selector.Text = "Select Hytale install location";
        hytale_location_selector.UseVisualStyleBackColor = true;
        hytale_location_selector.Click += Hytale_location_selectorOnClick;
        //
        // hytale_location_show
        //
        hytale_location_show.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right));
        hytale_location_show.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        hytale_location_show.Enabled = false;
        hytale_location_show.Location = new System.Drawing.Point(12, 20);
        hytale_location_show.Name = "hytale_location_show";
        hytale_location_show.Size = new System.Drawing.Size(471, 23);
        hytale_location_show.TabIndex = 1;
        //
        // jre_location_show
        //
        jre_location_show.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right));
        jre_location_show.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        jre_location_show.Enabled = false;
        jre_location_show.Location = new System.Drawing.Point(12, 63);
        jre_location_show.Name = "jre_location_show";
        jre_location_show.Size = new System.Drawing.Size(471, 23);
        jre_location_show.TabIndex = 2;
        //
        // jre_location_selector
        //
        jre_location_selector.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right));
        jre_location_selector.Location = new System.Drawing.Point(489, 55);
        jre_location_selector.Name = "jre_location_selector";
        jre_location_selector.Size = new System.Drawing.Size(167, 37);
        jre_location_selector.TabIndex = 3;
        jre_location_selector.Text = "Select JRE install location";
        jre_location_selector.UseVisualStyleBackColor = true;
        jre_location_selector.Click += Jre_location_selectorOnClick;
        //
        // progressBar1
        //
        progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right));
        progressBar1.Location = new System.Drawing.Point(12, 124);
        progressBar1.Name = "progressBar1";
        progressBar1.Size = new System.Drawing.Size(644, 21);
        progressBar1.TabIndex = 4;
        //
        // usernameb
        //
        usernameb.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left));
        usernameb.Location = new System.Drawing.Point(12, 95);
        usernameb.Name = "usernameb";
        usernameb.PlaceholderText = "In game username";
        usernameb.Size = new System.Drawing.Size(150, 23);
        usernameb.TabIndex = 6;
        usernameb.TextChanged += usernameb_TextChanged;
        //
        // label2
        //
        label2.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left));
        label2.AutoSize = true;
        label2.Location = new System.Drawing.Point(12, 185);
        label2.Name = "label2";
        label2.Size = new System.Drawing.Size(505, 15);
        label2.TabIndex = 7;
        label2.Text = ("You\'re playing in offline mode, no online features will be available during the gameplay, please");
        //
        // uuid_shower
        //
        uuid_shower.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right));
        uuid_shower.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        uuid_shower.Enabled = false;
        uuid_shower.Location = new System.Drawing.Point(321, 95);
        uuid_shower.Name = "uuid_shower";
        uuid_shower.PlaceholderText = "UUIDv4";
        uuid_shower.Size = new System.Drawing.Size(255, 23);
        uuid_shower.TabIndex = 8;
        uuid_shower.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
        //
        // genuuid
        //
        genuuid.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right));
        genuuid.Location = new System.Drawing.Point(582, 96);
        genuuid.Name = "genuuid";
        genuuid.Size = new System.Drawing.Size(74, 22);
        genuuid.TabIndex = 9;
        genuuid.Text = "Refresh";
        genuuid.UseVisualStyleBackColor = true;
        genuuid.Click += genuuid_Click;
        //
        // playbtn
        //
        playbtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right));
        playbtn.Enabled = false;
        playbtn.Location = new System.Drawing.Point(12, 151);
        playbtn.Name = "playbtn";
        playbtn.Size = new System.Drawing.Size(644, 31);
        playbtn.TabIndex = 10;
        playbtn.Text = "Play";
        playbtn.UseVisualStyleBackColor = true;
        playbtn.Click += PlaybtnOnClick;
        //
        // buylink
        //
        buylink.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right));
        buylink.AutoSize = true;
        buylink.Location = new System.Drawing.Point(523, 185);
        buylink.Name = "buylink";
        buylink.Size = new System.Drawing.Size(106, 15);
        buylink.TabIndex = 11;
        buylink.TabStop = true;
        buylink.Text = "buy the pre-access";
        buylink.LinkClicked += buylink_LinkClicked;
        //
        // Form1
        //
        AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        ClientSize = new System.Drawing.Size(668, 209);
        SizeGripStyle = SizeGripStyle.Hide;
        FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        Controls.Add(buylink);
        Controls.Add(playbtn);
        Controls.Add(genuuid);
        Controls.Add(uuid_shower);
        Controls.Add(label2);
        Controls.Add(usernameb);
        Controls.Add(progressBar1);
        Controls.Add(jre_location_selector);
        Controls.Add(jre_location_show);
        Controls.Add(hytale_location_show);
        Controls.Add(hytale_location_selector);
        Text = "Hytale Downloader";
        Load += Form1_Load;
        ResumeLayout(false);
        PerformLayout();
    }

    private System.Windows.Forms.LinkLabel buylink;

    private System.Windows.Forms.Button playbtn;

    private System.Windows.Forms.Button genuuid;

    private System.Windows.Forms.TextBox uuid_shower;

    private System.Windows.Forms.Label label2;

    private System.Windows.Forms.TextBox usernameb;

    private System.Windows.Forms.ProgressBar progressBar1;

    private System.Windows.Forms.TextBox jre_location_show;
    private System.Windows.Forms.Button jre_location_selector;

    private System.Windows.Forms.TextBox hytale_location_show;

    private System.Windows.Forms.Button hytale_location_selector;

    #endregion
}
