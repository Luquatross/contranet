<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmMain
    Inherits System.Windows.Forms.Form

    'Das Formular überschreibt den Löschvorgang, um die Komponentenliste zu bereinigen.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Wird vom Windows Form-Designer benötigt.
    Private components As System.ComponentModel.IContainer

    'Hinweis: Die folgende Prozedur ist für den Windows Form-Designer erforderlich.
    'Das Bearbeiten ist mit dem Windows Form-Designer möglich.  
    'Das Bearbeiten mit dem Code-Editor ist nicht möglich.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.cmdSet = New System.Windows.Forms.Button
        Me.txtValue = New System.Windows.Forms.TextBox
        Me.Label1 = New System.Windows.Forms.Label
        Me.lbVars = New System.Windows.Forms.ListBox
        Me.lbMethod = New System.Windows.Forms.ListBox
        Me.Label2 = New System.Windows.Forms.Label
        Me.cmdRun = New System.Windows.Forms.Button
        Me.Label3 = New System.Windows.Forms.Label
        Me.Label4 = New System.Windows.Forms.Label
        Me.ddRequestMethod = New System.Windows.Forms.ComboBox
        Me.Button1 = New System.Windows.Forms.Button
        Me.SuspendLayout()
        '
        'cmdSet
        '
        Me.cmdSet.Location = New System.Drawing.Point(536, 28)
        Me.cmdSet.Name = "cmdSet"
        Me.cmdSet.Size = New System.Drawing.Size(75, 23)
        Me.cmdSet.TabIndex = 0
        Me.cmdSet.Text = "&Set"
        Me.cmdSet.UseVisualStyleBackColor = True
        '
        'txtValue
        '
        Me.txtValue.Location = New System.Drawing.Point(142, 31)
        Me.txtValue.Name = "txtValue"
        Me.txtValue.Size = New System.Drawing.Size(388, 20)
        Me.txtValue.TabIndex = 1
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(13, 13)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(35, 13)
        Me.Label1.TabIndex = 2
        Me.Label1.Text = "Name"
        '
        'lbVars
        '
        Me.lbVars.FormattingEnabled = True
        Me.lbVars.Location = New System.Drawing.Point(16, 30)
        Me.lbVars.Name = "lbVars"
        Me.lbVars.Size = New System.Drawing.Size(120, 147)
        Me.lbVars.Sorted = True
        Me.lbVars.TabIndex = 3
        '
        'lbMethod
        '
        Me.lbMethod.FormattingEnabled = True
        Me.lbMethod.Items.AddRange(New Object() {"album.addTags", "album.getInfo", "album.removeTag", "artist.addTags", "artist.getEvents", "artist.getInfo", "artist.getSimilar", "artist.getTopAlbums", "artist.getTopFans", "artist.getTopTags", "artist.getTopTracks", "artist.removeTag", "artist.search", "artist.share", "auth.getMobileSession", "auth.getSession", "auth.getToken", "event.attend", "event.getInfo", "geo.getEvents", "geo.getTopArtists", "geo.getTopTracks", "group.getWeeklyAlbumChart", "group.getWeeklyArtistChart", "group.getWeeklyChartList", "group.getWeeklyTrackChart", "playlist.fetch", "tag.getSimilar", "tag.getTopAlbums", "tag.getTopArtists", "tag.getTopTags", "tag.getTopTracks", "tag.search", "tasteometer.compare", "track.addTags", "track.ban", "track.getSimilar", "track.getTopFans", "track.getTopTags", "track.love", "track.removeTag", "track.search", "track.share", "user.getEvents", "user.getFriends", "user.getNeighbours", "user.getPlaylists", "user.getRecentTracks", "user.getTopAlbums", "user.getTopArtists", "user.getTopTags", "user.getTopTracks", "user.getWeeklyAlbumChart", "user.getWeeklyArtistChart", "user.getWeeklyChartList", "user.getWeeklyTrackChart"})
        Me.lbMethod.Location = New System.Drawing.Point(16, 199)
        Me.lbMethod.Name = "lbMethod"
        Me.lbMethod.Size = New System.Drawing.Size(120, 95)
        Me.lbMethod.Sorted = True
        Me.lbMethod.TabIndex = 4
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(13, 183)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(43, 13)
        Me.Label2.TabIndex = 5
        Me.Label2.Text = "Method"
        '
        'cmdRun
        '
        Me.cmdRun.Location = New System.Drawing.Point(281, 247)
        Me.cmdRun.Name = "cmdRun"
        Me.cmdRun.Size = New System.Drawing.Size(75, 23)
        Me.cmdRun.TabIndex = 6
        Me.cmdRun.Text = "&Run"
        Me.cmdRun.UseVisualStyleBackColor = True
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Font = New System.Drawing.Font("Microsoft Sans Serif", 15.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label3.Location = New System.Drawing.Point(181, 171)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(370, 25)
        Me.Label3.TabIndex = 7
        Me.Label3.Text = "See debug window for text output!"
        '
        'Label4
        '
        Me.Label4.AutoSize = True
        Me.Label4.Location = New System.Drawing.Point(13, 301)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(88, 13)
        Me.Label4.TabIndex = 9
        Me.Label4.Text = "Request method:"
        '
        'ddRequestMethod
        '
        Me.ddRequestMethod.DisplayMember = ".SelectedItem.Text"
        Me.ddRequestMethod.FormattingEnabled = True
        Me.ddRequestMethod.Items.AddRange(New Object() {"REST", "Xml-RPC"})
        Me.ddRequestMethod.Location = New System.Drawing.Point(16, 317)
        Me.ddRequestMethod.Name = "ddRequestMethod"
        Me.ddRequestMethod.Size = New System.Drawing.Size(121, 21)
        Me.ddRequestMethod.TabIndex = 10
        '
        'Button1
        '
        Me.Button1.BackColor = System.Drawing.Color.FromArgb(CType(CType(128, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(128, Byte), Integer))
        Me.Button1.ForeColor = System.Drawing.Color.Black
        Me.Button1.Location = New System.Drawing.Point(281, 95)
        Me.Button1.Name = "Button1"
        Me.Button1.Size = New System.Drawing.Size(75, 57)
        Me.Button1.TabIndex = 11
        Me.Button1.Text = "Ask for granting permission"
        Me.Button1.UseVisualStyleBackColor = False
        '
        'frmMain
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(623, 389)
        Me.Controls.Add(Me.Button1)
        Me.Controls.Add(Me.ddRequestMethod)
        Me.Controls.Add(Me.Label4)
        Me.Controls.Add(Me.Label3)
        Me.Controls.Add(Me.cmdRun)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.lbMethod)
        Me.Controls.Add(Me.lbVars)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.txtValue)
        Me.Controls.Add(Me.cmdSet)
        Me.Name = "frmMain"
        Me.Text = "Form1"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents cmdSet As System.Windows.Forms.Button
    Friend WithEvents txtValue As System.Windows.Forms.TextBox
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents lbVars As System.Windows.Forms.ListBox
    Friend WithEvents lbMethod As System.Windows.Forms.ListBox
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents cmdRun As System.Windows.Forms.Button
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents Label4 As System.Windows.Forms.Label
    Friend WithEvents ddRequestMethod As System.Windows.Forms.ComboBox
    Friend WithEvents Button1 As System.Windows.Forms.Button

End Class
