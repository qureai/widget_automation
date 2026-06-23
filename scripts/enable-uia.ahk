; ============================================================================
;  enable-uia.ahk  —  Make AutoHotkey's UIAccess build able to drive the
;  elevated UAC dialog.
;
;  consent.exe (the UAC prompt) runs at System integrity. A normal — even
;  elevated — process is blocked by UIPI from sending input to it. The only
;  sanctioned bypass is a UIAccess application: an exe whose manifest sets
;  uiAccess="true", that is digitally signed by a cert in the Trusted Root
;  store, and that lives in a secure location (Program Files / System32).
;
;  AutoHotkey ships AutoHotkey64_UIA.exe but it isn't signed out of the box,
;  so the uiAccess flag is ignored. This script uses AHK's own EnableUIAccess
;  helper to (1) create/reuse a self-signed "AutoHotkey" code-signing cert in
;  LocalMachine\Root, (2) set uiAccess="true" in the exe manifest, and
;  (3) sign the exe — after which Windows honours its UIAccess privilege.
;
;  MUST be run ELEVATED (writes to Program Files + LocalMachine cert store).
; ============================================================================

#Requires AutoHotkey v2.0
#Include "C:\Program Files\AutoHotkey\UX\inc\EnableUIAccess.ahk"

base   := "C:\Program Files\AutoHotkey\v2\AutoHotkey64.exe"      ; clean source
target := "C:\Program Files\AutoHotkey\v2\AutoHotkey64_UIA.exe"  ; UIAccess copy
result := A_Temp "\enable-uia-result.txt"
Report(msg) => FileAppend(msg "`n", result)

try FileDelete(result)

if !FileExist(base) {
    Report("ERROR base exe not found: " base)
    ExitApp 1
}

try {
    ; Mirror AutoHotkey's own MakeUIA: start from a fresh copy of the clean
    ; base exe (its manifest has the requestedExecutionLevel node EnableUIAccess
    ; edits), then set uiAccess + sign. This also repairs a corrupted _UIA.exe.
    FileCopy(base, target, true)
    EnableUIAccess(target)
    err := EnableUIAccess_Verify(target)
    if (err = 0)
        Report("OK UIAccess enabled and signature verified: " target)
    else
        Report("WARN UIAccess applied but verify returned " err ": " target)
} catch as e {
    Report("ERROR " e.Message " | What=" e.What " | " e.Extra)
    ExitApp 1
}
