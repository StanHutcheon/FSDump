Module main

    Sub Main()
        Log(TimeOfDay.Date + " " + TimeOfDay.TimeOfDay.ToString)
        Log("--System information--")
        Log(My.Computer.Info.AvailablePhysicalMemory, "System")
        Log(My.Computer.Info.AvailableVirtualMemory, "System")
        Log(My.Computer.Info.InstalledUICulture.ToString, "System")
        Log(My.Computer.Info.OSFullName, "System")
        Log(My.Computer.Info.OSPlatform, "System")
        Log(My.Computer.Info.OSVersion, "System")
        Log(My.Computer.Info.TotalPhysicalMemory, "System")
        Log(My.Computer.Info.TotalVirtualMemory, "System")
        Log(LogNewLine:=True)
        Log("FSDump 0.0.2-2")
        Log(LogNewLine:=True)

        Console.Write(" ")
        Console.Title = "FSDump 0.0.2-2 - iPhone4,1 iOS 5.0.1 9A406"
        Dim a_strArgs() As String

        Dim i As Integer

        a_strArgs = Split(Command$, " ")
        For i = LBound(a_strArgs) To UBound(a_strArgs)
            Select Case LCase(a_strArgs(i))
                Case "-h"
                    Console.WriteLine("usage: FSDump.exe [-i <ipsw>]")
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
                    If ipsw = "" Then
                        Console.WriteLine("  *Set default = Download IPSW")
                        Log("Used downloaded ipsw")
                        Log(LogNewLine:=True)

                    End If
                    Console.WriteLine(" ")
            End Select
        Next

        Console.WriteLine("--FSDump 0.0.2-2 for iPhone 4S - iOS 5.0.1--")
        Console.WriteLine(" ")
        Console.WriteLine("This app wouldn't have been made possible if it wasn't for these developers:")
        Console.WriteLine(" ")
        Console.WriteLine(" Igor Pavlov - 7-Zip")
        Console.WriteLine(" planetbeing - dmg (xpwn)")
        Console.WriteLine(" Apple - iOS")
        Console.WriteLine(" Microsoft - Visual Studio")
        Console.WriteLine(" ")

        'clean previous stuff
        Console.WriteLine("[Info] Cleaning up old files...")
        Console.WriteLine(" ")
        Try
            Cleanup()
        Catch ex As Exception
            Console.WriteLine("[Info - Error] Error code 1 'Failed to remove previously used directories (is 7z.exe still running?)'")
            Log(ex.ToString, "Error")

            Exit Sub
        End Try

        'xtra download stuff
        If ipsw = "" Then
            Try
                'Console.WriteLine("[Info] Downloading 038-3763-001.dmg from iPhone4,1_5.0.1_9A406_Restore.ipsw...")
                If System.IO.File.Exists(tmp + "\iPhone4,1_5.0.1_9A406_Restore.ipsw") Then
                    Console.WriteLine("[Info] '" + tmp + "\iPhone4,1_5.0.1_9A406_Restore.ipsw' already exists.")
                    Console.WriteLine("[Info] Checking IPSW...")
                    Log("IPSW already exists, checking SHA1 hash to see if its legit...")
                    Log(LogNewLine:=True)
                    If SHA1Hash(tmp + "\iPhone4,1_5.0.1_9A406_Restore.ipsw") = ipswhash Then
                        Console.WriteLine("[Info] IPSW SHA1 hash matched: " + ipswhash + "")
                        Log("IPSW is legit, skipping next SHA1 check...")
                        Log(LogNewLine:=True)
                        HashChecked = True
                    Else
                        Log("IPSW isn't legit. Nice try.")
                        Log(LogNewLine:=True)
                        DownloadFile("http://appldnld.apple.com/iPhone4//041-3417.20111215.Slnt4/iPhone4,1_5.0.1_9A406_Restore.ipsw", tmp)
                        Log("Downloaded IPSW")
                        Log(LogNewLine:=True)
                    End If
                Else
                    Log("IPSW isn't legit. Nice try.")
                    Log(LogNewLine:=True)
                    DownloadFile("http://appldnld.apple.com/iPhone4//041-3417.20111215.Slnt4/iPhone4,1_5.0.1_9A406_Restore.ipsw", tmp)
                    Log("Downloaded IPSW")
                    Log(LogNewLine:=True)
                End If
            Catch ex As Exception
                Console.WriteLine("[Info - Error] Error code 2 'Failed to download IPSW'")
                Log("Can't download IPSW, probably internet", "Error")
                Exit Sub
            End Try
            Console.WriteLine(" ")
            ipsw = tmp + "\iPhone4,1_5.0.1_9A406_Restore.ipsw"
            Log(ipsw)
            Log(LogNewLine:=True)

        End If

        'check ipsw hash
        If HashChecked = False Then
            Console.WriteLine("[Info] Checking IPSW...")
            Console.WriteLine(" ")
            If Not SHA1Hash(ipsw) = ipswhash Then
                Console.WriteLine("This is not a valid iPhone 4S 5.0.1 9A406 IPSW")
                Console.WriteLine("Make sure this is the 9A406 IPSW and not the 9A405 one as that does not have decrypted ramdisks (so no vfdecrypt key)")
                Log("Y U NO PICK IPHONE 4S IPSW??????????", "Error")
                Log(LogNewLine:=True)

                Exit Sub
            Else
                Console.WriteLine("[Info] IPSW SHA1 hash matched: " + ipswhash + "")
                Log("SHA1 matched! " + ipswhash)
                Log(LogNewLine:=True)
                Console.WriteLine(" ")
            End If
        End If

        Console.WriteLine("[Info] Unzipping IPSW...")
        Console.WriteLine(" ")
        Try
            unzip(ipsw, tmp + "\fs", True)
        Catch ex As Exception
            Console.WriteLine("Error code 3 'Failed to extract IPSW'")
            Log("Failed to unzip the ipsw provided, it cant be corrupt because it passed SHA1 checks, Probably unrelated 7z issue" + vbNewLine + ex.ToString, "Error")
            Log(LogNewLine:=True)

            Cleanup()
            Exit Sub
        End Try

        Console.WriteLine("[Info] Decrypting RootFS [038-3763-001.dmg]...")
        Console.WriteLine(" ")
        Try
            dmg(vfdecryptkey, tmp + "\fs\038-3763-001.dmg", tmp + "\decrypted.038-3763-001.dmg.iso")
        Catch ex As Exception
            Console.WriteLine("Error code 4 'Failed to decrypt firmware image'")
            Log("pile of horse shit" + vbNewLine + ex.ToString, "Error")
            Log(LogNewLine:=True)

            Cleanup()
            Exit Sub
        End Try


        'extract
        Console.WriteLine("[Info] Checking directories...")
        Console.WriteLine(" ")
        If System.IO.Directory.Exists(outputdir) Then
            'overwrite?
            Console.WriteLine("The directory '" + outputdir + "' already exists." + vbNewLine + "Would you like to overwrite? [y|n]:")
            Dim HasRead As Boolean = False
            Do
                Dim ans = Console.ReadLine
                If ans = "y" Or ans = "yes" Then
                    HasRead = True
                    Console.WriteLine("[Info] Deleting " + outputdir + "...")
                    Console.WriteLine(" ")
                    Try
                        My.Computer.FileSystem.DeleteDirectory(outputdir, FileIO.DeleteDirectoryOption.DeleteAllContents)
                        System.IO.Directory.Delete(outputdir, True)
                    Catch ex As Exception
                        Log(ex.ToString, "Error")
                        Log(LogNewLine:=True)

                    End Try
                    Console.WriteLine("[Info] Dumping filesystem [Telluride9A406.N94OS] - Takes quite a long time...")
                ElseIf ans = "n" Or ans = "no" Then
                    HasRead = True
                    Exit Sub
                End If
            Loop Until HasRead = True
        End If
        Try
            MkDir(outputdir)
            Delay(2)
            'ChDir(tmp)
            unzip(tmp + "\decrypted.038-3763-001.dmg.iso", outputdir)
            Log("unzipped ipsw yey ^^")
            Log(LogNewLine:=True)

            Console.ReadLine()
            Console.WriteLine("")
            Console.WriteLine("Dumped to: " + My.Computer.FileSystem.SpecialDirectories.Desktop + "\Telluride9A406.N94OS")
            Console.WriteLine(" ")
            Console.WriteLine("*note: all plists within the dump don't contain text")
            Console.WriteLine(" ")
            Console.WriteLine("[Info] Cleaning up files...")
            Cleanup()
        Catch ex As Exception
            Console.WriteLine("[Info - Error] Error code 5 'Failed to extract dmg '" + tmp + "\decrypted.038-3763-001.dmg.iso' to desktop (get it yourself?)")
            Log(ex.ToString, "Error")
            Log(LogNewLine:=True)

            Exit Sub
        End Try

        Log("***END LOG***")
        Log(LogNewLine:=True)

    End Sub

End Module
