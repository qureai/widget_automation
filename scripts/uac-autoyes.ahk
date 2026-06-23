; ============================================================================
;  uac-autoyes.ahk  —  Auto-click "Yes" on the Windows UAC consent dialog
;  that appears while the BDD framework installs the Qure Widget MSI.
;
;  AutoHotkey v2.   Requires (one-time, via scripts/install-uac-task.ps1 +
;  manual secure-desktop disable):
;    1. Secure desktop disabled:  PromptOnSecureDesktop = 0
;    2. Launched as the UIAccess build (AutoHotkey64_UIA.exe) via ShellExecute,
;       so it may send input to the System-integrity consent.exe window.
;
;  Diagnostic log: %TEMP%\uac-autoyes.log
; ============================================================================

#Requires AutoHotkey v2.0
#SingleInstance Force

; ---- Config ----------------------------------------------------------------
TIMEOUT_MS := 180000   ; watch for up to 3 min, then exit
POLL_MS    := 200
logFile    := A_Temp "\uac-autoyes.log"
; ----------------------------------------------------------------------------

Log(msg) {
    global logFile
    try FileAppend(FormatTime(, "yyyy-MM-dd HH:mm:ss") " | " msg "`n", logFile)
}

; Never let an unexpected error pop a (hidden) dialog and freeze the watcher.
OnError (e, *) => (Log("ERROR " e.Message), -1)

Log("=== watcher started | A_IsAdmin=" A_IsAdmin)

SetTitleMatchMode 2       ; substring title match
DetectHiddenWindows false ; only real, visible prompts
startTick := A_TickCount

Loop {
    if (A_TickCount - startTick > TIMEOUT_MS) {
        Log("timeout reached, exiting")
        ExitApp
    }

    ; Match the UAC consent dialog by its window title ONLY. The Win11 XAML
    ; host can also leave behind an empty-title phantom window; matching the
    ; title avoids acting on it.
    hwnd := WinExist("User Account Control")
    if (hwnd) {
        title := ""
        try title := WinGetTitle("ahk_id " hwnd)
        Log("found hwnd=" hwnd " title='" title "'")

        approved := false
        try approved := ApproveUac(hwnd)
        catch as e
            Log("approve threw: " e.Message)

        if WinWaitClose("ahk_id " hwnd, , 5) {
            Log("dialog closed -> SUCCESS")
            ExitApp
        }
        Log("dialog still open after approve attempt; will retry")
    }

    Sleep POLL_MS
}

; --- Press "Yes". Every step is guarded so nothing can hang the watcher. -----
ApproveUac(hwnd) {
    win := "ahk_id " hwnd
    Log("approving " win)

    try WinActivate(win)
    try WinWaitActive(win, , 2)

    ; "Yes" sits to the LEFT of the default-focused "No"; move left, confirm.
    ; (The XAML buttons have no reliable control name, so ControlClick is
    ;  unreliable — keyboard navigation is what works.)
    try {
        Send "{Left}"
        Sleep 120
        Send "{Enter}"
        Log("{Left}{Enter} sent")
    } catch as e {
        Log("send failed: " e.Message)
    }
    return true
}
