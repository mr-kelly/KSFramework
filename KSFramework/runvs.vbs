filename = WScript.Arguments(0)
line = WScript.Arguments(1)
column = WScript.Arguments(2)

On Error Resume Next

MSVS_versions = Array _
( _
    "VisualStudio.DTE.7", _
    "VisualStudio.DTE.7.1", _
    "VisualStudio.DTE.8.0", _
    "VisualStudio.DTE.9.0", _
    "VisualStudio.DTE.10.0", _
    "VisualStudio.DTE.11.0", _
    "VisualStudio.DTE.12.0", _
    "VisualStudio.DTE.14.0" _
)

For each version in MSVS_versions
    Err.Clear
    Set dte = getObject(,version)
    If Err.Number = 0 Then
        Exit For
    End If
Next

If Err.Number <> 0 Then
    Set dte = WScript.CreateObject("VisualStudio.DTE")
    Err.Clear
End If


dte.MainWindow.Activate
dte.MainWindow.Visible = True
dte.UserControl = True

dte.ItemOperations.OpenFile filename
dte.ActiveDocument.Selection.MoveToLineAndOffset line, column + 1