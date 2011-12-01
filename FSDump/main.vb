Module main
    Declare Sub Sleep Lib "kernel32" Alias "Sleep" (ByVal dwMilliseconds As Long)

    Public ipsw As String
    Public tmp As String = My.Computer.FileSystem.SpecialDirectories.Temp
    Public vfdecryptexe As Process = New Process
    Public vfdecryptkey As String = "879132da717bbfec47df50307d759952e81947edb4faa6469a52f219853884f68eb8c7ce"
    Public HasRead As Boolean = False

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

    Sub Main()
        Console.Title = "FSDump - iPhone4,1 iOS 5.1"
        Dim a_strArgs() As String

        Dim i As Integer

        a_strArgs = Split(Command$, " ")
        For i = LBound(a_strArgs) To UBound(a_strArgs)
            Select Case LCase(a_strArgs(i))
                Case "-h"
                    Console.WriteLine("usage: -i <ipsw>")
                    Exit Sub
                Case "-i"
                    If i = UBound(a_strArgs) Then
                        Console.WriteLine("No ipsw specified. try -h for more information.")
                        Exit Sub
                    Else
                        i = i + 1
                    End If
                    If Left(a_strArgs(i), 1) = "-" Then
                        MsgBox("Invalid string.")
                    Else
                        ipsw = a_strArgs(i)
                    End If
                Case Else
                    Console.WriteLine("usage: -i <ipsw>")
                    Exit Sub
            End Select
        Next
        Console.WriteLine("--FSDump for iPhone 4S - iOS 5.1--")
        Console.WriteLine(" ")
        Console.WriteLine("This app wouldn't have been made possible if it wasn't for these developers:")
        Console.WriteLine(" ")
        Console.WriteLine(" Igor Pavlov - 7-Zip")
        Console.WriteLine(" planetbeing - vfdecrypt (xpwn)")
        Console.WriteLine(" Apple - iOS")
        Console.WriteLine(" Microsoft - Visual Studio")
        Console.WriteLine(" ")
        Console.WriteLine("Cleaning up old files...")
        Try
            Cleanup()
        Catch ex As Exception
            Console.WriteLine("Error code 1 'Failed to remove previously used directories'")
            Exit Sub
        End Try
        Console.WriteLine(" ")
        'extract
        Console.WriteLine("Unzipping ipsw...")
        Console.WriteLine(" ")
        Try
            'Using zip1 As ZipFile = ZipFile.Read(ipsw)
            'zip1.Extract("038-3074-006.dmg", tmp)
            'End Using
            unzip(ipsw, tmp + "\fs", True)
        Catch ex As Exception
            Console.WriteLine("Error code 2 'Failed to extract IPSW (corrupt file?)'")
            Exit Sub
        End Try
        If Not System.IO.File.Exists(tmp + "\fs\038-3074-006.dmg") Then
            Console.WriteLine("This isn't an iPhone 4S ipsw.")
            Exit Sub
        End If

        'decrypt
        Console.WriteLine("Decrypting RootFS [038-3074-006.dmg]...")
        Console.WriteLine(" ")
        Try
            vfdecrypt(vfdecryptkey, tmp + "\fs\038-3074-006.dmg", tmp + "\decrypted.038-3074-006.dmg")
        Catch ex As Exception
            Console.WriteLine("Error code 3 'Failed to decrypt firmware image'")
            Exit Sub
        End Try
        Delay(2)

        'extract again
        Console.WriteLine("Extracting RootFS [decrypted.038-3074-006.dmg]...")
        Console.WriteLine(" ")
        Try
            unzip(tmp + "\decrypted.038-3074-006.dmg", tmp + "\xtractedfs", True)
        Catch ex As Exception
            Console.WriteLine("Error code 4 'Failed to extract decrypted firmware image'")
            Exit Sub
        End Try
        Delay(2)

        'extract AGAIN
        Console.WriteLine("Extracting hfsx Filesystem [5.hfsx]... **this may take a while**")
        Console.WriteLine(" ")
        Try
            unzip(tmp + "\xtractedfs\5.hfsx", tmp + "\xtractedfs\5", True)
        Catch ex As Exception
            Console.WriteLine("Error code 5 'Failed to extract hfsx filesystem'")
            Exit Sub
        End Try
        Delay(2)

        'copy fs
        Console.WriteLine("Dumping filesystem [HoodooVail9B5117b.N94DeveloperOS]...")
        Console.WriteLine(" ")
        If System.IO.Directory.Exists(My.Computer.FileSystem.SpecialDirectories.Desktop + "\4S dump") Then
            Console.WriteLine("The directory '" + My.Computer.FileSystem.SpecialDirectories.Desktop + "\4S dump' already exists. Would you like to overwrite? [y|n]:")
            Do
                Dim ans = Console.ReadLine
                If ans = "y" Or ans = "yes" Then
                    HasRead = True
                    System.IO.Directory.Delete(My.Computer.FileSystem.SpecialDirectories.Desktop + "\4S dump", True)
                ElseIf ans = "n" Or ans = "no" Then
                    HasRead = True
                    Exit Sub
                End If
            Loop Until HasRead = True
            Console.WriteLine(" ")
        End If
        Try
            System.IO.Directory.Move(tmp + "\xtractedfs\5\HoodooVail9B5117b.N94DeveloperOS", My.Computer.FileSystem.SpecialDirectories.Desktop + "\4S dump")
            Console.WriteLine("Dumped to: " + My.Computer.FileSystem.SpecialDirectories.Desktop + "\4S Dump")
            Console.WriteLine(" ")
            Console.WriteLine("*note: all plists within the dump don't contain text")
            Console.WriteLine(" ")
            Console.WriteLine("Cleaning up files...")
            Cleanup()
        Catch ex As Exception
            Console.WriteLine("Error code 6 'Failed to move directory '" + tmp + "\xtractedfs\5\HoodooVail9B5117b.N94DeveloperOS' to desktop (get them yourself)")
            Exit Sub
        End Try
    End Sub

    Public Sub vfdecrypt(ByVal key As String, ByVal file As String, ByVal outfile As String)
        ChDir(My.Application.Info.DirectoryPath + "\tmp")
        Dim decrypt As Process = New Process
        decrypt.StartInfo.Arguments = " -i " + file + " -k " + key + " -o " + outfile
        decrypt.StartInfo.WindowStyle = ProcessWindowStyle.Hidden
        decrypt.StartInfo.FileName = My.Application.Info.DirectoryPath + "\tmp\vfdecrypt.exe"
        decrypt.Start()
        decrypt.WaitForExit()
    End Sub

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
        sevenzip.StartInfo.WindowStyle = ProcessWindowStyle.Hidden
        sevenzip.StartInfo.FileName = My.Application.Info.DirectoryPath + "\tmp\7z.exe"
        sevenzip.Start()
        Do Until sevenzip.HasExited
            For i = 0 To 5000000
            Next
        Loop
        sevenzip.WaitForExit()
    End Sub

    Public Sub Cleanup()
        If System.IO.Directory.Exists(tmp + "\fs") Then
            System.IO.Directory.Delete(tmp + "\fs", True)
        End If
        If System.IO.Directory.Exists(tmp + "\xtractedfs\5\HoodooVail9B5117b.N94DeveloperOS\System") Then
            System.IO.Directory.Delete(tmp + "\xtractedfs\5\HoodooVail9B5117b.N94DeveloperOS\System", True)
        End If
        If System.IO.Directory.Exists(tmp + "\xtractedfs\5\HoodooVail9B5117b.N94DeveloperOS") Then
            System.IO.Directory.Delete(tmp + "\xtractedfs\5\HoodooVail9B5117b.N94DeveloperOS", True)
        End If
        If System.IO.Directory.Exists(tmp + "\xtractedfs\5") Then
            System.IO.Directory.Delete(tmp + "\xtractedfs\5", True)
        End If
        If System.IO.Directory.Exists(tmp + "\xtractedfs") Then
            System.IO.Directory.Delete(tmp + "\xtractedfs", True)
        End If
        If System.IO.File.Exists(tmp + "\decrypted.038-3074-006.dmg") Then
            System.IO.File.Delete(tmp + "\decrypted.038-3074-006.dmg")
        End If
    End Sub

End Module
