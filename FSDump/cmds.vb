Module cmds
    'this file only exists to make the other one look nicer.

    Declare Sub Sleep Lib "kernel32" Alias "Sleep" (ByVal dwMilliseconds As Long)

    Public HashChecked As Boolean = False
    Public ipsw As String
    Public outputdir As String = My.Computer.FileSystem.SpecialDirectories.Desktop + "\Telluride9A406.N94OS"
    Public ipswhash As String = "869caa17e6b3176efb11b5de653ec8330d43b176"
    Public tmp As String = My.Computer.FileSystem.SpecialDirectories.Temp
    Public vfdecryptkey As String = "a31ffd506c6711c5a0c52c9f0a2f7208a2f63ad9dd40506e70d80ea20a981eb1312bc774"

    'free bit of code for ya (the normal ShellWait doesn't work in command line VB apps, so i wrote my own. I never used it in this app though.
    Public Sub ShellWait(ByVal file As String, ByVal args As String)
        Dim proc As Process = New Process
        proc.StartInfo.Arguments = " " + args
        proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden
        proc.StartInfo.FileName = file
        proc.Start()
        Do Until proc.HasExited
            For i = 0 To 5000000
            Next
        Loop
        proc.WaitForExit()
    End Sub

    'Also another bit of code i wrote, a Delay code, Delay didn't work (again) in command line VB apps so i wrote my own.
    'make sure to have:
    ' Declare Sub Sleep Lib "kernel32" Alias "Sleep" (ByVal dwMilliseconds As Long)
    '(without the ' ) in your module somwhere, but not in a Sub or Function.
    'same for this bit:
    Public Sub Delay(ByVal seconds As Integer)
        Dim delaytime As Integer = seconds * 1000
        Sleep(delaytime)
    End Sub
    'then just use the code e.g Delay(3) = a delay for 3 seconds
    'neat huh?

    Public Function SHA1Hash(ByVal file As String) As String
        ' open file (as read-only)
        Using reader As New System.IO.FileStream(file, IO.FileMode.Open, IO.FileAccess.Read)
            Using sha1 As New System.Security.Cryptography.SHA1CryptoServiceProvider
                ' hash contents of this stream
                Dim hash() As Byte = sha1.ComputeHash(reader)
                ' return formatted hash
                Return ByteArrayToString(hash)
            End Using
        End Using
    End Function

    ' utility function to convert a byte array into a hex string
    Private Function ByteArrayToString(ByVal arrInput() As Byte) As String
        Dim sb As New System.Text.StringBuilder(arrInput.Length * 2)
        For i As Integer = 0 To arrInput.Length - 1
            sb.Append(arrInput(i).ToString("X2"))
        Next
        Return sb.ToString().ToLower
    End Function

    Public Sub Log(Optional ByVal item As String = "", Optional ByVal type As String = "Info", Optional ByVal LogNewLine As Boolean = False)
        Dim final As String

        If LogNewLine = True Then
            type = ""
            final = vbNewLine
        Else
            final = vbNewLine + "[" + type + "] " + item
        End If
        MakeTextFile(final, tmp + "\FSDumpLog.txt")
    End Sub

    Public Sub MakeTextFile(ByVal text As String, ByVal file As String)
        My.Computer.FileSystem.WriteAllText(file, text, True)
    End Sub

    Public Sub DownloadFile(ByVal ipsw As String, ByVal dir As String)
        Dim FileName = url_filename(ipsw)
        Console.WriteLine(" ")
        Console.WriteLine("[Info] Downloading " + FileName + "...")
        Console.WriteLine(" ")
        wget(ipsw, dir)
        ipsw = dir + "\" + FileName
    End Sub

    Public Sub dmg(ByVal key As String, ByVal file As String, ByVal outfile As String)
        ChDir(My.Application.Info.DirectoryPath + "\tmp")
        Dim decrypt As Process = New Process
        decrypt.StartInfo.Arguments = " extract " + file + " " + outfile + " -k " + key
        decrypt.StartInfo.WindowStyle = ProcessWindowStyle.Hidden
        decrypt.StartInfo.FileName = My.Application.Info.DirectoryPath + "\tmp\dmg.exe"
        decrypt.Start()
        decrypt.WaitForExit()
        'Log(LogNewLine:=True)
        'Log("dmg log")
        'Log(decrypt.StandardOutput.ReadToEnd)
        'Log(LogNewLine:=True)
    End Sub

    Public Sub wget(ByVal file As String, ByVal dir As String)
        Dim FileName = url_filename(file)
        ChDir(My.Application.Info.DirectoryPath + "\tmp")
        Dim wgetProc As Process = New Process
        wgetProc.StartInfo.UseShellExecute = False
        wgetProc.StartInfo.Arguments = " -O " + dir + "\" + FileName + " " + file
        wgetProc.StartInfo.RedirectStandardOutput = True
        wgetProc.StartInfo.FileName = My.Application.Info.DirectoryPath + "\tmp\wget.exe"
        wgetProc.Start()
        wgetProc.WaitForExit()
        Log(LogNewLine:=True)
        Log("wget log")
        Log(wgetProc.StandardOutput.ReadToEnd)
        Log(LogNewLine:=True)
    End Sub

    Private Function url_filename(ByVal file As String)
        Dim intPos As Int32
        intPos = file.LastIndexOfAny("/")
        intPos += 1
        Dim FileName As String = file.Substring(intPos, (Len(file) - intPos))
        Return FileName
    End Function

    Public Sub unzip(ByVal file As String, Optional ByVal dir As String = "", Optional ByVal overwrite As Boolean = False)
        ChDir(My.Application.Info.DirectoryPath + "\tmp")
        Dim args As String = " x "
        If Not dir = "" Then
            args = args + "-o" + dir + " "
        ElseIf overwrite = True Then
            args = args + "-y "
        End If
        args = args + file
        Dim sevenzip As Process = New Process
        sevenzip.StartInfo.Arguments = args
        'sevenzip.StartInfo.RedirectStandardOutput = True
        'sevenzip.StartInfo.UseShellExecute = False
        sevenzip.StartInfo.WindowStyle = ProcessWindowStyle.Hidden
        sevenzip.StartInfo.FileName = My.Application.Info.DirectoryPath + "\tmp\7z.exe"
        sevenzip.Start()
        Do Until sevenzip.HasExited
            For i = 0 To 5000000
            Next
        Loop
        sevenzip.WaitForExit()
        'Log(LogNewLine:=True)
        'Log("7z Log")
        'Log(sevenzip.StandardOutput.ReadToEnd)
        'Log(LogNewLine:=True)
    End Sub

    'ignore this shit, its for later.
    Public Function partial_zip_callback(ByVal exe As String)
        Dim i As Integer = 1
        Dim partial_zip As Process = New Process
        partial_zip.StartInfo.UseShellExecute = False
        partial_zip.StartInfo.WindowStyle = ProcessWindowStyle.Hidden
        partial_zip.StartInfo.RedirectStandardOutput = True
        partial_zip.StartInfo.FileName = " " + exe
        partial_zip.Start()
        'Console.Write("[                                                  ]")
        Do Until i = 100
            Console.Clear()
            Dim Prog As String = "[                                                  ]"
            Dim stdout As String = partial_zip.StandardOutput.ReadLine
            Console.Write(Prog)
            Console.CursorLeft = 1
            If stdout.Contains(i.ToString + "%") Then
                If i = 1 Then
                    Console.CursorLeft = 2
                    Console.Write("=>")
                ElseIf Not i = 1 Then
                    Console.CursorLeft = 1
                    Console.Write("=")
                End If
                Prog = Console.Read.ToString
                Prog.Remove(50 - i, i)
                i = i + 1
                stdout = ""
            End If
        Loop
        partial_zip.WaitForExit()
    End Function

    Public Sub Cleanup()
        If System.IO.Directory.Exists(tmp + "\fs") Then
            System.IO.Directory.Delete(tmp + "\fs", True)
        End If
        If System.IO.Directory.Exists(tmp + "\xtractedfs\5\Telluride9A406.N94OS\System") Then
            System.IO.Directory.Delete(tmp + "\xtractedfs\5\Telluride9A406.N94OS\System", True)
        End If
        If System.IO.Directory.Exists(tmp + "\xtractedfs\5\Telluride9A406.N94OS") Then
            System.IO.Directory.Delete(tmp + "\xtractedfs\5\Telluride9A406.N94OS", True)
        End If
        If System.IO.Directory.Exists(tmp + "\xtractedfs\5") Then
            System.IO.Directory.Delete(tmp + "\xtractedfs\5", True)
        End If
        If System.IO.Directory.Exists(tmp + "\xtractedfs") Then
            System.IO.Directory.Delete(tmp + "\xtractedfs", True)
        End If
        If System.IO.File.Exists(tmp + "\decrypted.038-3763-001.dmg.iso") Then
            System.IO.File.Delete(tmp + "\decrypted.038-3763-001.dmg.iso")
        End If
    End Sub

End Module
